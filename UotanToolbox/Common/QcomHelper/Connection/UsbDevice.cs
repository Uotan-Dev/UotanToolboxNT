/*
using Avalonia.Logging;
using Avalonia.OpenGL;
using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;
using Splat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

/// <summary>
/// C# implementation of the USB communication class
/// Originally ported from B.Kerler's Python implementation (2018-2024)
/// Under GPLv3 license
/// </summary>
namespace EdlClient.Library.Connection
{
    public static class UsbConst
    {
        // Direction constants
        public const byte USB_DIR_OUT = 0;  // to device
        public const byte USB_DIR_IN = 0x80;  // to host

        // USB types, the second of three bRequestType fields
        public const byte USB_TYPE_MASK = (0x03 << 5);
        public const byte USB_TYPE_STANDARD = (0x00 << 5);
        public const byte USB_TYPE_CLASS = (0x01 << 5);
        public const byte USB_TYPE_VENDOR = (0x02 << 5);
        public const byte USB_TYPE_RESERVED = (0x03 << 5);

        // USB recipients, the third of three bRequestType fields
        public const byte USB_RECIP_MASK = 0x1f;
        public const byte USB_RECIP_DEVICE = 0x00;
        public const byte USB_RECIP_INTERFACE = 0x01;
        public const byte USB_RECIP_ENDPOINT = 0x02;
        public const byte USB_RECIP_OTHER = 0x03;
        // From Wireless USB 1.0
        public const byte USB_RECIP_PORT = 0x04;
        public const byte USB_RECIP_RPIPE = 0x05;

        public const int MAX_USB_BULK_BUFFER_SIZE = 16384;
    }

    public class CdcCommands
    {
        public static readonly Dictionary<string, byte> Values = new Dictionary<string, byte>
        {
            { "SEND_ENCAPSULATED_COMMAND", 0x00 },
            { "GET_ENCAPSULATED_RESPONSE", 0x01 },
            { "SET_COMM_FEATURE", 0x02 },
            { "GET_COMM_FEATURE", 0x03 },
            { "CLEAR_COMM_FEATURE", 0x04 },
            { "SET_LINE_CODING", 0x20 },
            { "GET_LINE_CODING", 0x21 },
            { "SET_CONTROL_LINE_STATE", 0x22 },
            { "SEND_BREAK", 0x23 }  // wValue is break time
        };
    }

    public class UsbClass
    {
        private string serialNumber;
        private UsbDevice device;
        private UsbEndpointReader endpointReader;
        private UsbEndpointWriter endpointWriter;
        private byte[] buffer;
        private bool isSerial = false;
        private int maxPacketSize;
        private UsbConfigInfo configuration;
        private bool connected = false;

        // Needed for serial communication
        private int baudrate;
        private int databits;
        private int parity;
        private double stopbits;

        public UsbClass(LogLevel logLevel = LogLevel.Info, List<UsbDeviceIdentifier> portConfig = null,
                      int devClass = -1, string serialNumber = null)
            : base(logLevel, portConfig, devClass)
        {
            this.serialNumber = serialNumber;
            LoadWindowsDll();
            buffer = new byte[1048576]; // 1MB buffer like in Python
        }

        private void LoadWindowsDll()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    // Add DLL directory to search path
                    string windowsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Windows");
                    if (Directory.Exists(windowsDir))
                    {
                        Environment.SetEnvironmentVariable("PATH",
                            windowsDir + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH"));
                    }
                }
                catch (Exception)
                {
                    // Ignore errors in DLL path setup
                }
            }
        }

        public int GetInterfaceCount()
        {
            if (Vid != null)
            {
                // Find device by VID/PID
                UsbDeviceFinder finder = new UsbDeviceFinder((int)Vid, (int)Pid);
                device = UsbDevice.OpenUsbDevice(finder);

                if (device == null)
                {
                    Debug("Couldn't detect the device. Is it connected?");
                    return 0;
                }

                try
                {
                    device.SetConfiguration(1);
                }
                catch (Exception err)
                {
                    Debug(err.ToString());
                }

                configuration = device.Configs[0];
                Debug(2, configuration.ToString());
                return configuration.InterfaceInfoList.Count;
            }
            else
            {
                Logger.Error("No device detected. Is it connected?");
            }
            return 0;
        }

        public void SetLineCoding(int? baudrate = null, int parity = 0, int databits = 8, double stopbits = 1)
        {
            Dictionary<double, int> sbits = new Dictionary<double, int>
            {
                { 1, 0 },
                { 1.5, 1 },
                { 2, 2 }
            };

            HashSet<int> dbits = new HashSet<int> { 5, 6, 7, 8, 16 };
            HashSet<int> pmodes = new HashSet<int> { 0, 1, 2, 3, 4 };
            HashSet<int> brates = new HashSet<int> {
                300, 600, 1200, 2400, 4800, 9600, 14400,
                19200, 28800, 38400, 57600, 115200, 230400
            };

            if (stopbits != 0)
            {
                if (!sbits.ContainsKey(stopbits))
                {
                    string valid = string.Join(", ", sbits.Keys.OrderBy(k => k));
                    throw new ArgumentException("Valid stopbits are " + valid);
                }
                this.stopbits = stopbits;
            }
            else
            {
                this.stopbits = 0;
            }

            if (databits != 0)
            {
                if (!dbits.Contains(databits))
                {
                    string valid = string.Join(", ", dbits.OrderBy(d => d));
                    throw new ArgumentException("Valid databits are " + valid);
                }
                this.databits = databits;
            }
            else
            {
                this.databits = 0;
            }

            if (parity != 0)
            {
                if (!pmodes.Contains(parity))
                {
                    string valid = string.Join(", ", pmodes.OrderBy(p => p));
                    throw new ArgumentException("Valid parity modes are " + valid);
                }
                this.parity = parity;
            }
            else
            {
                this.parity = 0;
            }

            if (baudrate.HasValue)
            {
                if (!brates.Contains(baudrate.Value))
                {
                    List<int> brs = brates.OrderBy(br => br).ToList();
                    List<int> dif = brs.Select(br => Math.Abs(br - baudrate.Value)).ToList();
                    int bestIndex = dif.IndexOf(dif.Min());
                    int best = brs[bestIndex];
                    throw new ArgumentException($"Invalid baudrates, nearest valid is {best}");
                }
                this.baudrate = baudrate.Value;
            }

            byte[] linecode = new byte[7];
            linecode[0] = (byte)(this.baudrate & 0xff);
            linecode[1] = (byte)((this.baudrate >> 8) & 0xff);
            linecode[2] = (byte)((this.baudrate >> 16) & 0xff);
            linecode[3] = (byte)((this.baudrate >> 24) & 0xff);
            linecode[4] = (byte)sbits[this.stopbits];
            linecode[5] = (byte)this.parity;
            linecode[6] = (byte)this.databits;

            int txdir = 0;  // 0:OUT, 1:IN
            int req_type = 1;  // 0:std, 1:class, 2:vendor
            int recipient = 1;  // 0:device, 1:interface, 2:endpoint, 3:other
            req_type = (txdir << 7) + (req_type << 5) + recipient;

            int wlen = ControlTransfer(
                (byte)req_type,
                CdcCommands.Values["SET_LINE_CODING"],
                0,
                1,
                linecode,
                linecode.Length);

            Debug($"Linecoding set, {wlen}b sent");
        }

        public void SetBreak()
        {
            int txdir = 0;  // 0:OUT, 1:IN
            int req_type = 1;  // 0:std, 1:class, 2:vendor
            int recipient = 1;  // 0:device, 1:interface, 2:endpoint, 3:other
            int req_type_val = (txdir << 7) + (req_type << 5) + recipient;

            int wlen = ControlTransfer(
                (byte)req_type_val,
                CdcCommands.Values["SEND_BREAK"],
                0,
                1,
                null,
                0);

            Debug($"Break set, {wlen}b sent");
        }

        private void Debug(string v)
        {
            throw new NotImplementedException();
        }

        public void SetControlLineState(bool? rts = null, bool? dtr = null, bool isFTDI = false)
        {
            int ctrlstate = (rts == true ? 2 : 0) + (dtr == true ? 1 : 0);
            if (isFTDI)
            {
                ctrlstate += (dtr.HasValue ? (1 << 8) : 0);
                ctrlstate += (rts.HasValue ? (2 << 8) : 0);
            }

            int txdir = 0;  // 0:OUT, 1:IN
            int req_type = isFTDI ? 2 : 1;  // 0:std, 1:class, 2:vendor
            int recipient = isFTDI ? 0 : 1;  // 0:device, 1:interface, 2:endpoint, 3:other
            int req_type_val = (txdir << 7) + (req_type << 5) + recipient;

            int wlen = ControlTransfer(
                (byte)req_type_val,
                isFTDI ? (byte)1 : CdcCommands.Values["SET_CONTROL_LINE_STATE"],
                (short)ctrlstate,
                1,
                null,
                0);

            Debug($"Linecoding set, {wlen}b sent");
        }

        public void Flush()
        {
            // No implementation needed
        }

        public bool Connect(int epIn = -1, int epOut = -1, string portname = "")
        {
            if (connected)
            {
                Close();
                connected = false;
            }

            device = null;
            endpointReader = null;
            endpointWriter = null;

            // Find all USB devices
            UsbRegDeviceList allDevices = UsbDevice.AllDevices;
            foreach (UsbRegistry regDevice in allDevices)
            {
                UsbDevice dev;
                if (!regDevice.Open(out dev)) continue;

                foreach (UsbDeviceIdentifier usbid in PortConfig)
                {
                    if (dev.UsbRegistryInfo.Pid == usbid.Pid && dev.UsbRegistryInfo.Vid == usbid.Vid)
                    {
                        if (serialNumber != null && dev.Info.SerialString != serialNumber)
                        {
                            continue;
                        }

                        device = dev;
                        Vid = (uint)device.UsbRegistryInfo.Vid;
                        Pid = (uint)device.UsbRegistryInfo.Pid;
                        serialNumber = device.Info.SerialString;
                        Interface = usbid.Interface;
                        break;
                    }
                }

                if (device != null) break;
            }

            if (device == null)
            {
                Debug("Couldn't detect the device. Is it connected?");
                return false;
            }

            try
            {
                configuration = device.Configs[0];
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Configuration not set"))
                {
                    device.SetConfiguration(1);
                    configuration = device.Configs[0];
                }
                // Handle permission errors
                if (e is System.UnauthorizedAccessException)
                {
                    // Try different approach here if needed
                }
            }

            if (configuration == null)
            {
                Error("Couldn't get device configuration.");
                return false;
            }

            if (Interface > configuration.InterfaceInfoList.Count)
            {
                Console.WriteLine($"Invalid interface, max number is {configuration.InterfaceInfoList.Count}");
                return false;
            }

            // Find the appropriate interface and endpoints
            foreach (UsbInterfaceInfo itf in configuration.InterfaceInfoList)
            {
                if (DevClass == -1)
                {
                    DevClass = 0xFF;
                }

                if (itf.Descriptor.Class == DevClass)
                {
                    if (Interface == -1 || Interface == itf.Descriptor.InterfaceID)
                    {
                        Interface = itf.Descriptor.InterfaceID;

                        foreach (UsbEndpointInfo ep in itf.EndpointInfoList)
                        {
                            if ((ep.Descriptor.EndpointID & LibUsbDotNet.Main.EndpointType.TypeMask) ==
                                LibUsbDotNet.Main.EndpointType.Bulk)
                            {
                                if ((ep.Descriptor.EndpointID & LibUsbDotNet.Main.EndpointType.DirMask) ==
                                    LibUsbDotNet.Main.EndpointType.DirIn)
                                {
                                    // Input endpoint
                                    if (epIn == -1 || ep.Descriptor.EndpointID == (epIn & 0xF))
                                    {
                                        endpointReader = device.OpenEndpointReader(
                                            (ReadEndpointID)ep.Descriptor.EndpointID);
                                        maxPacketSize = ep.Descriptor.MaxPacketSize;
                                    }
                                }
                                else
                                {
                                    // Output endpoint
                                    if (epOut == -1 || ep.Descriptor.EndpointID == (epOut & 0xF))
                                    {
                                        endpointWriter = device.OpenEndpointWriter(
                                            (WriteEndpointID)ep.Descriptor.EndpointID);
                                    }
                                }
                            }
                        }
                        break;
                    }
                }
            }

            if (endpointReader != null && endpointWriter != null)
            {
                // Claim the interface
                try
                {
                    if (device.IsKernelDriverLoaded(0))
                    {
                        Debug("Detaching kernel driver");
                        device.DetachKernelDriver(0);
                    }
                }
                catch (Exception err)
                {
                    Debug("No kernel driver supported: " + err.ToString());
                }

                try
                {
                    device.ClaimInterface(0);
                }
                catch
                {
                    // Just continue if claiming interface fails
                }

                connected = true;
                return true;
            }

            Console.WriteLine("Couldn't find CDC interface. Aborting.");
            connected = false;
            return false;
        }

        public void Close(bool reset = false)
        {
            if (connected)
            {
                try
                {
                    if (reset)
                    {
                        device.ResetDevice();
                    }

                    if (!device.IsKernelDriverLoaded(Interface))
                    {
                        // Do NOT uncomment:
                        // device.AttachKernelDriver(Interface);
                        device.AttachKernelDriver(0);
                    }
                }
                catch (Exception err)
                {
                    Debug(err.ToString());
                }

                device.Close();
                device = null;

                if (reset)
                {
                    Thread.Sleep(2000);
                }

                connected = false;
            }
        }

        public bool Write(byte[] command, int? pktsize = null)
        {
            if (pktsize == null)
            {
                pktsize = UsbConst.MAX_USB_BULK_BUFFER_SIZE;
            }

            if (command.Length == 0)
            {
                try
                {
                    int written;
                    ErrorCode ec = endpointWriter.Write(new byte[0], 0, 0, 1000, out written);
                }
                catch (Exception)
                {
                    try
                    {
                        int written;
                        ErrorCode ec = endpointWriter.Write(new byte[0], 0, 0, 1000, out written);
                    }
                    catch (Exception err)
                    {
                        Debug(err.ToString());
                        return false;
                    }
                }
                return true;
            }
            else
            {
                int pos = 0;
                int retryCount = 0;

                while (pos < command.Length)
                {
                    try
                    {
                        int writeLen = Math.Min(command.Length - pos, pktsize.Value);
                        int written;
                        ErrorCode ec = endpointWriter.Write(command, pos, writeLen, 1000, out written);

                        if (written <= 0)
                        {
                            Info(written.ToString());
                        }

                        pos += writeLen;
                    }
                    catch (Exception err)
                    {
                        Debug(err.ToString());
                        retryCount++;

                        if (retryCount == 3)
                        {
                            return false;
                        }
                    }
                }
            }

            VerifyData(command, "TX:");
            return true;
        }

        public byte[] UsbRead(int? resplen = null, int timeout = 0)
        {
            if (timeout == 0)
            {
                timeout = 1;
            }

            if (resplen == null)
            {
                resplen = maxPacketSize;
            }

            if (resplen <= 0)
            {
                Info("Warning!");
            }

            byte[] result = new byte[resplen.Value];
            int resultPos = 0;
            LogLevel loglevel = LogLevel;

            while (resultPos < resplen)
            {
                try
                {
                    int len;
                    ErrorCode ec = endpointReader.Read(buffer, 0, resplen.Value, 1000 * timeout, out len);

                    if (len > 0)
                    {
                        if (resultPos + len > result.Length)
                        {
                            // Resize result buffer if needed
                            Array.Resize(ref result, resultPos + len);
                        }

                        Array.Copy(buffer, 0, result, resultPos, len);
                        resultPos += len;
                    }

                    if (len == maxPacketSize)
                    {
                        break;
                    }
                }
                catch (TimeoutException)
                {
                    if (timeout == null)
                    {
                        return new byte[0];
                    }

                    Debug("Timed out");

                    if (timeout == 10)
                    {
                        return new byte[0];
                    }

                    timeout++;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Overflow"))
                    {
                        Error("USB Overflow");
                        return new byte[0];
                    }
                    else
                    {
                        Info(ex.ToString());
                        return new byte[0];
                    }
                }
            }

            if (loglevel == LogLevel.Debug)
            {
                string callerName = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name;
                Debug($"{callerName}:0x{result.Length:X}");

                if (LogLevel == LogLevel.Debug)
                {
                    VerifyData(result, "RX:");
                }
            }

            // Resize array to actual data length
            if (result.Length > resultPos)
            {
                Array.Resize(ref result, resultPos);
            }

            return result;
        }

        private int ControlTransfer(byte bmRequestType, byte bRequest, short wValue, short wIndex,
                                  byte[] data, int length)
        {
            UsbSetupPacket setupPacket = new UsbSetupPacket(
                bmRequestType,
                bRequest,
                wValue,
                wIndex,
                (short)(data != null ? data.Length : 0));

            int transferLength = 0;
            ErrorCode ec;

            if (data == null || data.Length == 0)
            {
                ec = device.ControlTransfer(ref setupPacket, null, 0, out transferLength);
            }
            else
            {
                ec = device.ControlTransfer(ref setupPacket, data, data.Length, out transferLength);
            }

            if (ec != ErrorCode.None)
            {
                throw new Exception($"Control transfer failed: {ec}");
            }

            return transferLength;
        }

        public class DeviceInfo
        {
            public int Vid { get; set; }
            public int Pid { get; set; }

            public DeviceInfo(int vid, int pid)
            {
                Vid = vid;
                Pid = pid;
            }
        }

        public List<DeviceInfo> DetectDevices()
        {
            List<DeviceInfo> result = new List<DeviceInfo>();
            UsbRegDeviceList allDevices = UsbDevice.AllDevices;

            foreach (UsbRegistry regDevice in allDevices)
            {
                result.Add(new DeviceInfo(regDevice.Vid, regDevice.Pid));
            }

            return result;
        }

        public bool UsbWrite(byte[] data, int? pktsize = null)
        {
            if (pktsize == null)
            {
                pktsize = data.Length;
            }

            bool result = Write(data, pktsize);
            return result;
        }

        public byte[] UsbReadWrite(byte[] data, int resplen)
        {
            UsbWrite(data);
            byte[] result = UsbRead(resplen);
            return result;
        }

        private void VerifyData(byte[] data, string prefix)
        {
            // Implementation would depend on your logging system
            if (LogLevel == LogLevel.Debug)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(prefix);

                for (int i = 0; i < Math.Min(data.Length, 16); i++)
                {
                    sb.Append($"{data[i]:X2} ");
                }

                if (data.Length > 16)
                {
                    sb.Append("...");
                }

                Debug(sb.ToString());
            }
        }
    }

    public class UsbDeviceIdentifier
    {
        public int Vid { get; set; }
        public int Pid { get; set; }
        public int Interface { get; set; }

        public UsbDeviceIdentifier(int vid, int pid, int interfaceNum)
        {
            Vid = vid;
            Pid = pid;
            Interface = interfaceNum;
        }
    }
}*/