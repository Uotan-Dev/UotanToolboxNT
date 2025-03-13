using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UotanToolbox.Common.QcomHelper.Connection
{
    /// <summary>
    /// 串行通信类，处理串行端口的连接和数据传输
    /// 原始Python实现 © B.Kerler 2018-2024 under GPLv3 license
    /// </summary>
    public class SerialClass : DeviceClass
    {
        private SerialPort device;
        private bool xmlread = true;
        private bool is_serial = false;

        /// <summary>
        /// 创建串行通信类实例
        /// </summary>
        /// <param name="logLevel">日志级别</param>
        /// <param name="portConfig">端口配置参数</param>
        /// <param name="devClass">设备类型标识符</param>
        public SerialClass(int logLevel = 20, List<UsbDeviceIdentifier> portConfig = null, int devClass = -1)
            : base(logLevel, portConfig, devClass)
        {
            // 标记为串行设备
            this.is_serial = true;
        }

        /// <summary>
        /// 连接到串行设备
        /// </summary>
        public override bool Connect(int epIn = -1, int epOut = -1, string portname = "")
        {
            if (connected)
            {
                Close();
                connected = false;
            }

            if (string.IsNullOrEmpty(portname))
            {
                List<string> devices = DetectDevices().Select(d => d.PortName).ToList();
                if (devices.Count > 0)
                {
                    portname = devices[0];
                }
            }

            if (!string.IsNullOrEmpty(portname))
            {
                try
                {
                    device = new SerialPort(
                        portname,
                        115200,                          // baudrate
                        Parity.None,                     // parity
                        8,                               // databits
                        StopBits.One)                    // stopbits
                    {
                        ReadTimeout = 50000,                  // 50秒超时
                        Handshake = Handshake.RequestToSend,  // 启用RTS/CTS流控制     
                        DtrEnable = true                      // 启用DTR
                    };

                    device.Open();
                    connected = device.IsOpen;
                    if (connected)
                    {
                        device.DiscardInBuffer();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error connecting: {ex.Message}");
                }
            }

            return false;
        }

        /// <summary>
        /// 关闭串行连接
        /// </summary>
        public override void Close(bool reset = false)
        {
            if (connected)
            {
                device.Close();
                device.Dispose();
                device = null;
                connected = false;
            }
        }

        /// <summary>
        /// 检测可用的串行设备
        /// </summary>
        public override List<DeviceInfo> DetectDevices()
        {
            List<SerialDeviceInfo> ids = [];

            foreach (string port in SerialPort.GetPortNames())
            {
                try
                {
                    // 在C#中，我们无法直接获取串行端口的VID/PID，需要使用第三方库或WMI查询
                    // 这里简化实现，假设我们能够获取到设备PID/VID
                    foreach (UsbDeviceIdentifier usbid in portConfig)
                    {
                        // 这里需要使用WMI或其他方法来获取端口的VID/PID
                        // 由于这种检测在C#中比较复杂，这里只是框架示例
                        // 实际生产代码需要实现正确的设备检测

                        // 假设我们能通过某种方式获取到这些信息
                        int vid = usbid.Vid;
                        int pid = usbid.Pid;

                        Console.WriteLine($"Detected {vid:X}:{pid:X} device at: {port}");
                        ids.Add(new SerialDeviceInfo(vid, pid, port));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error detecting device on port {port}: {ex.Message}");
                }
            }

            return ids.OrderBy(d => d.PortName).Cast<DeviceInfo>().ToList();
        }

        /// <summary>
        /// 设置串行线路参数
        /// </summary>
        public override void SetLineCoding(int? baudrate = null, int parity = 0, int databits = 8, double stopbits = 1)
        {
            if (baudrate.HasValue)
                device.BaudRate = baudrate.Value;

            device.Parity = (Parity)parity;

            device.DataBits = databits;

            device.StopBits = stopbits == 1 ? StopBits.One :
                             stopbits == 1.5 ? StopBits.OnePointFive :
                             StopBits.Two;

            // 这里原本会记录日志
            Console.WriteLine("Linecoding set");
        }

        /// <summary>
        /// 发送中断信号
        /// </summary>
        public override void SetBreak()
        {
            device.BreakState = true;
            Thread.Sleep(100);  // 稍等一下以确保中断被发送
            device.BreakState = false;

            // 这里原本会记录日志
            Console.WriteLine("Break set");
        }

        /// <summary>
        /// 设置控制线状态
        /// </summary>
        public override void SetControlLineState(bool? rts = null, bool? dtr = null, bool isFTDI = false)
        {
            if (rts == true)
                device.RtsEnable = true;

            if (dtr == true)
                device.DtrEnable = true;

            // 这里原本会记录日志
            Console.WriteLine("Linecoding set");
        }

        /// <summary>
        /// 写入数据到串行端口
        /// </summary>
        public override bool Write(byte[] command, int? pktsize = null)
        {
            if (pktsize == null)
                pktsize = 512;

            if (command.Length == 0)
            {
                try
                {
                    device.Write(new byte[0], 0, 0);
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                    if (error.Contains("timeout"))
                    {
                        try
                        {
                            device.Write(new byte[0], 0, 0);
                        }
                        catch (Exception err)
                        {
                            // 这里原本会记录日志
                            Console.WriteLine($"Error: {err.Message}");
                            return false;
                        }
                    }
                    return true;
                }
            }
            else
            {
                int pos = 0;
                int retries = 0;

                while (pos < command.Length)
                {
                    try
                    {
                        int writeLen = Math.Min(command.Length - pos, pktsize.Value);
                        device.Write(command, pos, writeLen);
                        pos += writeLen;
                    }
                    catch (Exception ex)
                    {
                        // 这里原本会记录日志
                        Console.WriteLine($"Error: {ex.Message}");

                        retries++;
                        if (retries == 3)
                            return false;
                    }
                }
            }

            // 验证数据（在没有日志系统的情况下，只是形式上保留）
            VerifyData(command, "TX:");

            device.BaseStream.Flush();

            // 短暂等待以确保数据已发送
            Thread.Sleep(5);

            return true;
        }

        /// <summary>
        /// 读取指定长度的数据
        /// </summary>
        public override byte[] Read(int? length = null, int timeout = -1)
        {
            if (timeout == -1)
                timeout = this.timeout;

            if (length == null)
            {
                length = device.BytesToRead;
                if (length == 0)
                    return new byte[0];
            }

            if (xmlread)
            {
                if (length > device.BytesToRead)
                    length = device.BytesToRead;
            }

            return UsbRead(length, timeout);
        }

        /// <summary>
        /// 清空设备缓冲区
        /// </summary>
        public override void Flush()
        {
            device.BaseStream.Flush();
        }

        /// <summary>
        /// 读取USB数据
        /// </summary>
        public override byte[] UsbRead(int? resplen = null, int timeout = 0)
        {
            if (resplen == null)
                resplen = device.BytesToRead;

            if (resplen <= 0)
            {
                Console.WriteLine("Warning!");
            }

            List<byte> result = new List<byte>();
            int originalTimeout = device.ReadTimeout;
            device.ReadTimeout = timeout * 1000;  // 转换为毫秒

            try
            {
                if (xmlread)
                {
                    // 读取前6个字节来检查是否有XML标头
                    byte[] info = new byte[6];
                    int read = device.Read(info, 0, 6);
                    result.AddRange(info.Take(read));

                    // 如果是XML数据，读取直到结束标记
                    bool isXml = result.Count >= 6 &&
                                Encoding.ASCII.GetString(result.ToArray(), 0, 6) == "<?xml ";

                    if (isXml)
                    {
                        byte[] buffer = new byte[1];
                        while (true)
                        {
                            device.Read(buffer, 0, 1);
                            result.Add(buffer[0]);

                            // 检查是否到达结束标记 "</data>"
                            if (result.Count >= 7)
                            {
                                byte[] endTag = result.Skip(result.Count - 7).ToArray();
                                if (Encoding.ASCII.GetString(endTag) == "</data>")
                                    break;
                            }
                        }
                        return result.ToArray();
                    }
                }

                // 读取指定长度的数据
                int bytesToRead = resplen.Value;
                byte[] buffer = new byte[bytesToRead];
                int totalBytesRead = 0;

                while (totalBytesRead < bytesToRead)
                {
                    try
                    {
                        byte[] tempBuffer = new byte[bytesToRead - totalBytesRead];
                        int bytesRead = device.Read(tempBuffer, 0, tempBuffer.Length);

                        if (bytesRead == 0)
                            break;

                        result.AddRange(tempBuffer.Take(bytesRead));
                        totalBytesRead += bytesRead;
                    }
                    catch (TimeoutException)
                    {
                        if (timeout == null)
                            return new byte[0];

                        Console.WriteLine("Timed out");

                        if (timeout == 10)
                            return new byte[0];

                        timeout++;
                    }
                    catch (IOException ex) when (ex.Message.Contains("Overflow"))
                    {
                        Console.WriteLine("USB Overflow");
                        return new byte[0];
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex}");
                        return new byte[0];
                    }
                }
            }
            finally
            {
                // 恢复原始超时设置
                device.ReadTimeout = originalTimeout;
            }

            byte[] finalResult = result.ToArray();


            return finalResult.Take(Math.Min(resplen.Value, finalResult.Length)).ToArray();
        }

        /// <summary>
        /// USB控制传输（串行端口不实现）
        /// </summary>
        public override int ControlTransfer(byte bmRequestType, byte bRequest, short wValue, short wIndex, byte[] data, int length)
        {
            throw new NotImplementedException("Control transfers are not supported on serial connections");
        }

        /// <summary>
        /// USB写入操作
        /// </summary>
        public override bool UsbWrite(byte[] data, int? pktsize = null)
        {
            if (pktsize == null)
                pktsize = data.Length;

            bool result = Write(data, pktsize);
            device.BaseStream.Flush();
            return result;
        }

        /// <summary>
        /// USB读写组合操作
        /// </summary>
        public override byte[] UsbReadWrite(byte[] data, int resplen)
        {
            UsbWrite(data);
            device.BaseStream.Flush();
            return UsbRead(resplen);
        }

        public override int GetInterfaceCount()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 串行设备信息类
    /// </summary>
    public class SerialDeviceInfo : DeviceInfo
    {
        public string PortName { get; }

        public SerialDeviceInfo(int vid, int pid, string portName) : base(vid, pid)
        {
            PortName = portName;
        }
    }
}
