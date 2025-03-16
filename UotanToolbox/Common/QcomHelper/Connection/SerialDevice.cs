using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace UotanToolbox.Common.QcomHelper.Connection
{
    /// <summary>
    /// 串行通信类，处理串行端口的连接和数据传输
    /// 原始Python实现 © B.Kerler 2018-2024 under GPLv3 license
    /// </summary>
    public class SerialClass : DeviceClass
    {
        private new SerialPort device;
        private new readonly bool xmlread = true;
        private readonly bool isSerial = true;

        /// <summary>
        /// 创建串行通信类实例
        /// </summary>
        /// <param name="portConfig">端口配置参数</param>
        /// <param name="devClass">设备类型标识符</param>
        public SerialClass(List<UsbDeviceIdentifier> portConfig = null, int devClass = -1)
            : base(portConfig, devClass)
        {
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
                // 无法读取VID/PID，简单获取可用端口
                string[] availablePorts = SerialPort.GetPortNames();
                if (availablePorts.Length > 0)
                {
                    portname = availablePorts[0];
                }
            }

            if (!string.IsNullOrEmpty(portname))
            {
                device = new SerialPort
                {
                    PortName = portname,
                    BaudRate = 115200,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    ReadTimeout = 50000,         // 50秒
                    WriteTimeout = 50000,
                    Handshake = Handshake.RequestToSend,  // rtscts=True
                    DtrEnable = true             // dsrdtr=True
                };

                try
                {
                    // 重置输入缓冲区处理
                    device.Open();

                    // 清空输入缓冲区（等同于Python的tcflush）
                    if (device.IsOpen)
                        device.DiscardInBuffer();

                    connected = device.IsOpen;

                    if (connected)
                        return true;
                }
                catch (Exception)
                {
                    // 已删除日志输出
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
            List<DeviceInfo> ids = new List<DeviceInfo>();

            // C#无法直接获取串行端口VID/PID，返回一个空列表
            // 实际上，可以使用WMI查询或第三方库获取相关信息，但超出了本示例范围

            return ids;
        }

        /// <summary>
        /// 获取设备接口数量
        /// </summary>
        public override int GetInterfaceCount()
        {
            // 串行设备不需要此功能
            return 1;
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

            // 转换停止位
            if (stopbits == 1)
                device.StopBits = StopBits.One;
            else if (stopbits == 1.5)
                device.StopBits = StopBits.OnePointFive;
            else if (stopbits == 2)
                device.StopBits = StopBits.Two;
        }

        /// <summary>
        /// 发送中断信号
        /// </summary>
        public override void SetBreak()
        {
            device.BreakState = true;
            Thread.Sleep(100); // 确保中断被发送
            device.BreakState = false;
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
                        catch
                        {
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
                        pos += pktsize.Value;
                    }
                    catch
                    {
                        retries++;
                        if (retries == 3)
                            return false;
                    }
                }
            }

            // 验证数据（无日志功能）
            VerifyData(command, "TX:");

            device.BaseStream.Flush();
            Thread.Sleep(5); // 等同于Python的sleep(0.005)

            return true;
        }

        /// <summary>
        /// 读取数据方法
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
        /// 刷新设备缓冲区
        /// </summary>
        public override void Flush()
        {
            device.BaseStream.Flush();
        }

        /// <summary>
        /// USB读取操作
        /// </summary>
        public override byte[] UsbRead(int? resplen = null, int timeout = 0)
        {
            if (resplen == null)
                resplen = device.BytesToRead;

            if (resplen <= 0)
            {
                // 无日志输出
                return new byte[0];
            }

            List<byte> result = new List<byte>();
            int originalTimeout = device.ReadTimeout;

            try
            {
                device.ReadTimeout = timeout * 1000; // 转换为毫秒

                if (xmlread)
                {
                    // 读取前6个字节来检查是否有XML标头
                    byte[] headerInfo = new byte[6]; // 改名为headerInfo避免冲突
                    int read = device.Read(headerInfo, 0, 6);
                    result.AddRange(headerInfo.Take(read));

                    // 检查是否是XML数据
                    bool isXml = result.Count >= 6 &&
                               Encoding.ASCII.GetString([.. result], 0, Math.Min(6, result.Count)).Contains("<?xml ");

                    if (isXml)
                    {
                        byte[] tempByte = new byte[1]; // 改名为tempByte避免冲突

                        // 读取直到找到XML结束标记
                        while (!result.Contains((byte)'<') ||
                               !(result.Count >= 7 && Encoding.ASCII.GetString(
                                   result.Skip(result.Count - 7).ToArray()) == "</data>"))
                        {
                            device.Read(tempByte, 0, 1);
                            result.Add(tempByte[0]);
                        }

                        return result.ToArray();
                    }
                }

                // 常规读取
                int bytesToRead = resplen.Value;
                byte[] readBuffer = new byte[bytesToRead]; // 改名为readBuffer避免冲突
                int totalBytesRead = 0;

                while (totalBytesRead < bytesToRead)
                {
                    try
                    {
                        int bytesRead = device.Read(readBuffer, 0, bytesToRead - totalBytesRead);

                        if (bytesRead == 0)
                            break;

                        result.AddRange(readBuffer.Take(bytesRead));
                        totalBytesRead += bytesRead;
                    }
                    catch (TimeoutException)
                    {
                        if (timeout == 0)
                            return new byte[0];

                        if (timeout == 10)
                            return new byte[0];

                        timeout++;
                    }
                    catch (IOException ex) when (ex.Message.Contains("Overflow"))
                    {
                        return new byte[0];
                    }
                    catch (Exception)
                    {
                        return new byte[0];
                    }
                }
            }
            finally
            {
                // 恢复原始超时设置
                device.ReadTimeout = originalTimeout;
            }

            // 无日志输出
            return [.. result.ToArray().Take(Math.Min(resplen.Value, result.Count))];
        }

        /// <summary>
        /// USB控制传输
        /// </summary>
        public override int ControlTransfer(byte bmRequestType, byte bRequest, short wValue, short wIndex, byte[] data, int length)
        {
            // 串行设备不支持控制传输
            throw new NotImplementedException("ControlTransfer not supported on serial devices");
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
    }
}