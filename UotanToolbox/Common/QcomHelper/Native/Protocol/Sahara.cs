using System;
using System.IO;
using System.Text;
using UotanToolbox.Common.QcomHelper.Connection;
using UotanToolbox.Common.QcomHelper.Models;

namespace UotanToolbox.Common.QcomHelper.Native.Protocol
{


    /// <summary>
    /// Sahara模式常量
    /// </summary>
    public static class SaharaModes
    {
        public const uint SAHARA_MODE_IMAGE_TX_PENDING = 0x0;
        public const uint SAHARA_MODE_IMAGE_TX_COMPLETE = 0x1;
        public const uint SAHARA_MODE_MEMORY_DEBUG = 0x2;
        public const uint SAHARA_MODE_COMMAND = 0x3;
    }
    /// <summary>
    /// Sahara客户端类，用于与高通设备进行Sahara协议通信
    /// </summary>
    /// <remarks>
    /// 初始化Sahara客户端
    /// </remarks>
    /// <param name="device">设备通信类</param>
    public class SaharaClient(DeviceClass device)
    {
        private bool bit64 = false;
        private ulong hwid = 0;
        private string serial = "Unknown";
        private string programmer = "firehose";
        private int maxCommandLen = 0;
        private byte[] buffer = new byte[4096];

        public bool Bit64 => bit64;
        public ulong HwId => hwid;
        public string Serial => serial;
        public string Programmer => programmer;

        /// <summary>
        /// 连接到设备并进行初始握手
        /// </summary>
        public bool Connect()
        {
            if (!device.Connected)
            {
                bool connected = device.Connect();
                if (!connected)
                    return false;
            }
            return SendHello();
        }

        /// <summary>
        /// 发送Sahara Hello命令
        /// </summary>
        private bool SendHello()
        {
            try
            {
                var hello = new Sahara.SaharaPacketHello
                {
                    Header = new Sahara.SaharaPacketHeader
                    {
                        Command = Sahara.SaharaCommandId.SAHARA_HELLO_ID,
                        Length = 0x20 // 32字节
                    },
                    Version = 2,
                    VersionSupported = 1,
                    CmdPacketLength = 0x1000, // 最大数据包长度
                    Mode = SaharaModes.SAHARA_MODE_IMAGE_TX_PENDING
                };

                // 将结构体转换为字节数组
                byte[] helloBytes = Sahara.StructureToByteArray(hello);

                // 发送命令
                device.Write(helloBytes);

                // 接收响应
                byte[] response = device.Read(48);

                if (response != null && response.Length >= 0x20)
                {
                    uint cmdId = BitConverter.ToUInt32(response, 0);
                    uint length = BitConverter.ToUInt32(response, 4);

                    if (cmdId == Sahara.SaharaCommandId.SAHARA_HELLO_RESP_ID)
                    {
                        var helloResp = Sahara.ByteArrayToStructure<Sahara.SaharaPacketHelloResp>(response);
                        uint version = helloResp.Version;
                        uint minVersion = helloResp.VersionSupported;
                        maxCommandLen = (int)BitConverter.ToUInt32(response, 16); // 读取原始字节中的值
                        uint mode = helloResp.Mode;
                        //Console.WriteLine($"Sahara: device version {version}, min version {minVersion}, mode {mode}");
                        return GetDeviceInfo();
                    }
                }
                //Console.WriteLine("Failed to receive valid Sahara Hello response");
                return false;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error in Sahara Hello: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取设备信息
        /// </summary>
        private bool GetDeviceInfo()
        {
            try
            {
                byte[] response = device.Read(16);
                if (response == null || response.Length < 8)
                    return false;
                uint cmdId = BitConverter.ToUInt32(response, 0);
                uint length = BitConverter.ToUInt32(response, 4);
                if (cmdId == Sahara.SaharaCommandId.SAHARA_READ_DATA_ID)
                {
                    bit64 = false;
                    // 需要完整读取请求
                    if (response.Length < 20)
                    {
                        byte[] rest = device.Read((int)(length - response.Length));
                        byte[] full = new byte[response.Length + rest.Length];
                        Array.Copy(response, 0, full, 0, response.Length);
                        Array.Copy(rest, 0, full, response.Length, rest.Length);
                        response = full;
                    }

                    uint imageId = BitConverter.ToUInt32(response, 8);
                    uint offset = BitConverter.ToUInt32(response, 12);
                    uint sizeInBytes = BitConverter.ToUInt32(response, 16);
                    //Console.WriteLine($"32-bit read request: imageId={imageId}, offset={offset:X}, size={sizeInBytes}");
                    return UploadImage(imageId, offset, sizeInBytes);
                }
                else if (cmdId == Sahara.SaharaCommandId.SAHARA_64_BITS_MEMORY_READ_ID)
                {
                    bit64 = true;
                    if (response.Length < 24)
                    {
                        byte[] rest = device.Read((int)(length - response.Length));
                        byte[] full = new byte[response.Length + rest.Length];
                        Array.Copy(response, 0, full, 0, response.Length);
                        Array.Copy(rest, 0, full, response.Length, rest.Length);
                        response = full;
                    }

                    uint imageId = BitConverter.ToUInt32(response, 8);
                    ulong offset = BitConverter.ToUInt64(response, 12);
                    uint sizeInBytes = BitConverter.ToUInt32(response, 20);

                    //Console.WriteLine($"64-bit read request: imageId={imageId}, offset={offset:X}, size={sizeInBytes}");

                    // 上传程序逻辑
                    return Upload64Image(imageId, offset, sizeInBytes);
                }

                //Console.WriteLine($"Unexpected command: {cmdId:X}");
                return false;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error getting device info: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 上传32位模式的引导程序
        /// </summary>
        private bool UploadImage(uint imageId, uint offset, uint size)
        {
            try
            {
                // 查找合适的引导程序
                string loaderPath = FindLoader(imageId);
                if (string.IsNullOrEmpty(loaderPath))
                {
                    //Console.WriteLine($"Could not find loader for imageId {imageId}");
                    return false;
                }

                // 读取引导程序文件
                byte[] loader = File.ReadAllBytes(loaderPath);

                // 上传引导程序
                if (offset + size > loader.Length)
                {
                    //Console.WriteLine("Requested image portion exceeds loader size");
                    return false;
                }

                // 截取请求的部分
                byte[] portion = new byte[size];
                Array.Copy(loader, offset, portion, 0, size);

                // 发送给设备
                device.Write(portion);

                // 等待更多命令
                return WaitForNextCommand();
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error uploading image: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 上传64位模式的引导程序
        /// </summary>
        private bool Upload64Image(uint imageId, ulong offset, uint size)
        {
            try
            {
                // 查找合适的引导程序
                string loaderPath = FindLoader(imageId);
                if (string.IsNullOrEmpty(loaderPath))
                {
                    //Console.WriteLine($"Could not find loader for imageId {imageId}");
                    return false;
                }

                // 读取引导程序文件
                byte[] loader = File.ReadAllBytes(loaderPath);

                // 上传引导程序
                if (offset + size > (ulong)loader.Length)
                {
                    //Console.WriteLine("Requested image portion exceeds loader size");
                    return false;
                }

                // 截取请求的部分
                byte[] portion = new byte[size];
                Array.Copy(loader, (long)offset, portion, 0, size);

                // 发送给设备
                device.Write(portion);

                // 等待更多命令
                return WaitForNextCommand();
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error uploading 64-bit image: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 等待设备的下一个命令
        /// </summary>
        private bool WaitForNextCommand()
        {
            try
            {
                byte[] response = device.Read(16);
                if (response == null || response.Length < 8)
                    return false;

                uint cmdId = BitConverter.ToUInt32(response, 0);
                uint length = BitConverter.ToUInt32(response, 4);

                if (cmdId == Sahara.SaharaCommandId.SAHARA_READ_DATA_ID)
                {
                    // 另一个读取请求
                    if (response.Length < length)
                    {
                        byte[] rest = device.Read((int)(length - response.Length));
                        byte[] full = new byte[response.Length + rest.Length];
                        Array.Copy(response, 0, full, 0, response.Length);
                        Array.Copy(rest, 0, full, response.Length, rest.Length);
                        response = full;
                    }

                    uint imageId = BitConverter.ToUInt32(response, 8);
                    uint offset = BitConverter.ToUInt32(response, 12);
                    uint sizeInBytes = BitConverter.ToUInt32(response, 16);

                    return UploadImage(imageId, offset, sizeInBytes);
                }
                else if (cmdId == Sahara.SaharaCommandId.SAHARA_64_BITS_MEMORY_READ_ID)
                {
                    // 64位读取请求
                    if (response.Length < length)
                    {
                        byte[] rest = device.Read((int)(length - response.Length));
                        byte[] full = new byte[response.Length + rest.Length];
                        Array.Copy(response, 0, full, 0, response.Length);
                        Array.Copy(rest, 0, full, response.Length, rest.Length);
                        response = full;
                    }

                    uint imageId = BitConverter.ToUInt32(response, 8);
                    ulong offset = BitConverter.ToUInt64(response, 12);
                    uint sizeInBytes = BitConverter.ToUInt32(response, 20);

                    return Upload64Image(imageId, offset, sizeInBytes);
                }
                else if (cmdId == Sahara.SaharaCommandId.SAHARA_END_IMAGE_TX_ID)
                {
                    // 图像传输结束
                    //Console.WriteLine("Image transfer complete");

                    // 发送Done命令
                    return SendDone();
                }
                else if (cmdId == Sahara.SaharaCommandId.SAHARA_CMD_READY_ID)
                {
                    // 设备准备好接收命令
                    //Console.WriteLine("Device ready for commands");

                    // 获取设备信息
                    hwid = GetMsmHwId();
                    serial = GetSerial();

                    //Console.WriteLine($"Device HwId: {hwid:X}, Serial: {serial}");
                    return true;
                }

                //Console.WriteLine($"Unexpected command: {cmdId:X}");
                return false;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error waiting for command: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 发送Done命令
        /// </summary>
        private bool SendDone()
        {
            try
            {
                byte[] cmd = new byte[8];
                BitConverter.GetBytes(Sahara.SaharaCommandId.SAHARA_DONE_ID).CopyTo(cmd, 0);
                BitConverter.GetBytes((uint)8).CopyTo(cmd, 4);

                device.Write(cmd);

                // 等待Done响应
                byte[] response = device.Read(8);
                if (response == null || response.Length < 8)
                    return false;

                uint cmdId = BitConverter.ToUInt32(response, 0);

                if (cmdId == Sahara.SaharaCommandId.SAHARA_DONE_RESP_ID)
                {
                    //Console.WriteLine("Done command acknowledged");
                    return true;
                }

                //Console.WriteLine($"Unexpected response to Done: {cmdId:X}");
                return false;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error sending Done: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 切换Sahara模式
        /// </summary>
        public bool SwitchMode(uint mode)
        {
            try
            {
                // 构建模式切换命令
                byte[] cmd = new byte[12];
                BitConverter.GetBytes(Sahara.SaharaCommandId.SAHARA_CMD_SWITCH_MODE_ID).CopyTo(cmd, 0);
                BitConverter.GetBytes((uint)12).CopyTo(cmd, 4);
                BitConverter.GetBytes(mode).CopyTo(cmd, 8);

                // 发送命令
                device.Write(cmd);

                // 等待响应
                byte[] response = device.Read(16);
                if (response == null || response.Length < 8)
                    return false;

                uint cmdId = BitConverter.ToUInt32(response, 0);

                if (cmdId == Sahara.SaharaCommandId.SAHARA_CMD_EXEC_ID)
                {
                    uint status = BitConverter.ToUInt32(response, 8);
                    if (status == 0)
                    {
                        //Console.WriteLine($"Successfully switched to mode {mode}");
                        return true;
                    }

                    //Console.WriteLine($"Mode switch failed with status {status}");
                }
                else if (cmdId == Sahara.SaharaCommandId.SAHARA_CMD_READY_ID)
                {
                    // 有些设备会直接返回CMD_READY
                    //Console.WriteLine("Device ready after mode switch");
                    return true;
                }

                //Console.WriteLine($"Unexpected response to mode switch: {cmdId:X}");
                return false;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error switching mode: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 进入命令模式
        /// </summary>
        public bool EnterCommandMode()
        {
            return SwitchMode(SaharaModes.SAHARA_MODE_COMMAND);
        }

        /// <summary>
        /// 发送复位命令
        /// </summary>
        public bool Reset()
        {
            try
            {
                byte[] cmd = new byte[8];
                BitConverter.GetBytes(Sahara.SaharaCommandId.SAHARA_RESET_ID).CopyTo(cmd, 0);
                BitConverter.GetBytes((uint)8).CopyTo(cmd, 4);

                device.Write(cmd);

                // 设备应该重启，不需要响应
                //Console.WriteLine("Reset command sent");
                return true;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error sending reset: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取流信息 (完整设备信息)
        /// </summary>
        public bool StreamingInfo()
        {
            // 已经在Connect过程中获取了基本信息
            // 如果需要更多信息，可以在这里实现额外的查询
            //Console.WriteLine($"HwId: {hwid:X}, Serial: {serial}, 64bit: {bit64}");
            return true;
        }

        /// <summary>
        /// 查找匹配的引导程序
        /// </summary>
        private string FindLoader(uint imageId)
        {
            // 这里应该有更复杂的逻辑来查找合适的引导程序
            // 作为简化示例，我们返回一个预设的路径
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string loaderPath = Path.Combine(appPath, "Loaders", $"prog_emmc_firehose_{imageId:X}.mbn");

            if (File.Exists(loaderPath))
                return loaderPath;

            // 通用引导程序
            loaderPath = Path.Combine(appPath, "Loaders", "prog_emmc_firehose.mbn");
            if (File.Exists(loaderPath))
                return loaderPath;

            return null;
        }

        /// <summary>
        /// 获取MSM硬件ID
        /// </summary>
        private ulong GetMsmHwId()
        {
            try
            {
                // 发送命令以获取硬件ID
                // 这通常需要设备处于命令模式
                byte[] cmd = new byte[8];
                BitConverter.GetBytes((uint)1).CopyTo(cmd, 0); // 命令ID 1表示获取HWID
                BitConverter.GetBytes((uint)8).CopyTo(cmd, 4);

                device.Write(cmd);

                // 读取响应
                byte[] response = device.Read(16);
                if (response == null || response.Length < 16)
                    return 0;

                // 解析HWID - 示例实现，实际可能需要根据设备调整
                return BitConverter.ToUInt64(response, 8);
            }
            catch
            {
                // 如果发生错误，返回0
                return 0;
            }
        }

        /// <summary>
        /// 获取设备序列号
        /// </summary>
        private string GetSerial()
        {
            try
            {
                // 发送命令以获取序列号
                // 这通常需要设备处于命令模式
                byte[] cmd = new byte[8];
                BitConverter.GetBytes((uint)2).CopyTo(cmd, 0); // 命令ID 2表示获取序列号
                BitConverter.GetBytes((uint)8).CopyTo(cmd, 4);

                device.Write(cmd);

                // 读取响应
                byte[] response = device.Read(24);
                if (response == null || response.Length < 16)
                    return "Unknown";

                // 解析序列号 - 示例实现，实际可能需要根据设备调整
                uint serialLen = BitConverter.ToUInt32(response, 8);
                if (serialLen > 0 && response.Length >= 12 + serialLen)
                {
                    return Encoding.ASCII.GetString(response, 12, (int)serialLen);
                }

                return "Unknown";
            }
            catch
            {
                // 如果发生错误，返回Unknown
                return "Unknown";
            }
        }
    }
}