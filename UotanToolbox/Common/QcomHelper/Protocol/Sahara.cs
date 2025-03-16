using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UotanToolbox.Common.QcomHelper.Connection;
using UotanToolbox.Common.QcomHelper.Models;

namespace UotanToolbox.Common.QcomHelper.Protocol
{
    /// <summary>
    /// Sahara协议客户端
    /// </summary>
    /// <remarks>
    /// 创建Sahara协议客户端
    /// </remarks>
    public class SaharaClient(DeviceClass device)
    {
        private byte[] buffer = new byte[0x400];
        private uint version;
        private uint minVersion;
        private uint maxCommandPacketSize;
        private bool is64Bit = false;

        public string SerialNumber { get; private set; }
        public ulong HardwareId { get; private set; }
        public bool Bit64 => is64Bit;
        public string Programmer { get; private set; }

        /// <summary>
        /// 连接到设备并初始化
        /// </summary>
        public bool Connect()
        {
            try
            {
                // 发送Sahara Hello命令
                SendHello(1, 1);

                // 接收设备响应
                var response = GetResponse();
                if (response == null || response.Length < 0x20)
                {
                    //Console.WriteLine("没有收到有效的Sahara响应");
                    return false;
                }

                // 解析Hello响应
                uint cmd = BitConverter.ToUInt32(response, 0);
                if (cmd == (uint)Sahara.SaharaCommandId.SAHARA_HELLO_RESP_ID)
                {
                    version = BitConverter.ToUInt32(response, 4);
                    minVersion = BitConverter.ToUInt32(response, 8);
                    maxCommandPacketSize = BitConverter.ToUInt32(response, 12);
                    uint mode = BitConverter.ToUInt32(response, 16);

                    //Console.WriteLine($"设备状态: {mode}, 版本: {version}, 最小版本: {minVersion}");
                    //Console.WriteLine($"最大数据包大小: {maxCommandPacketSize} bytes");
                    return GetDeviceInfo();
                }
                else
                {
                    //Console.WriteLine($"收到意外命令: 0x{cmd:X}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"连接错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 发送Sahara Hello命令
        /// </summary>
        private void SendHello(uint version, uint minVersion)
        {
            byte[] cmd = new byte[0x20]; // Sahara Hello命令长度

            // 命令ID
            BitConverter.GetBytes((uint)Sahara.SaharaCommandId.SAHARA_HELLO_ID).CopyTo(cmd, 0);

            // 命令长度
            BitConverter.GetBytes((uint)cmd.Length).CopyTo(cmd, 4);

            // 版本号
            BitConverter.GetBytes(version).CopyTo(cmd, 8);

            // 最小版本号
            BitConverter.GetBytes(minVersion).CopyTo(cmd, 12);

            // 最大数据包大小
            BitConverter.GetBytes((uint)0x400).CopyTo(cmd, 16);

            // 模式 (只支持图像传输)
            BitConverter.GetBytes((uint)Sahara.SaharaMode.SAHARA_MODE_IMAGE_TX_PENDING).CopyTo(cmd, 20);

            device.Write(cmd);
        }

        /// <summary>
        /// 获取设备响应
        /// </summary>
        private byte[] GetResponse()
        {
            byte[] header = device.Read(8);
            if (header == null || header.Length < 8)
            {
                return null;
            }

            uint cmd = BitConverter.ToUInt32(header, 0);
            uint length = BitConverter.ToUInt32(header, 4);

            // 读取剩余数据
            if (length > 8)
            {
                byte[] data = device.Read((int)(length - 8));
                byte[] response = new byte[length];
                Array.Copy(header, response, header.Length);
                Array.Copy(data, 0, response, header.Length, data.Length);
                return response;
            }

            return header;
        }

        /// <summary>
        /// 获取设备信息
        /// </summary>
        private bool GetDeviceInfo()
        {
            try
            {
                // 不断处理设备请求，直到完成
                while (true)
                {
                    byte[] response = GetResponse();
                    if (response == null)
                    {
                        return false;
                    }

                    uint cmd = BitConverter.ToUInt32(response, 0);

                    switch (cmd)
                    {
                        case (uint)Sahara.SaharaCommandId.SAHARA_READ_DATA_ID:
                            {
                                uint imageId = BitConverter.ToUInt32(response, 8);
                                uint offset = BitConverter.ToUInt32(response, 12);
                                uint length = BitConverter.ToUInt32(response, 16);

                                // 加载适当的引导程序并上传
                                bool result = UploadImage(imageId, offset, length);
                                if (!result)
                                    return false;
                                break;
                            }

                        case (uint)Sahara.SaharaCommandId.SAHARA_64_BITS_MEMORY_READ_ID:
                            {
                                is64Bit = true;
                                ulong address = BitConverter.ToUInt64(response, 8);
                                ulong length = BitConverter.ToUInt64(response, 16);

                                // 读取64位设备内存
                                // 此处应实现64位内存读取逻辑
                                //Console.WriteLine($"64位内存读取请求: 地址 0x{address:X}, 长度 {length}");
                                return true;
                            }

                        case (uint)Sahara.SaharaCommandId.SAHARA_CMD_READY_ID:
                            //Console.WriteLine("设备已准备好接收命令");
                            return true;

                        case (uint)Sahara.SaharaCommandId.SAHARA_END_IMAGE_TX_ID:
                            //Console.WriteLine("镜像传输结束");
                            return true;

                        default:
                            //Console.WriteLine($"收到未知命令: 0x{cmd:X}");
                            return false;
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"获取设备信息错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 上传指定的镜像到设备
        /// </summary>
        private bool UploadImage(uint imageId, uint offset, uint length)
        {
            // 此处应该实现加载并上传指定的引导程序
            // 实际实现中，应该从已知位置加载引导程序文件

            //Console.WriteLine($"需要上传镜像 ID: {imageId}, 偏移量: 0x{offset:X}, 长度: {length}");

            // 这里是一个简单的例子，实际实现需要更复杂的逻辑
            byte[] fakeImage = new byte[length];
            for (int i = 0; i < length; i++)
            {
                fakeImage[i] = (byte)(i & 0xFF);
            }

            // 将数据写入设备
            device.Write(fakeImage);

            return true;
        }

        /// <summary>
        /// 切换到指定模式
        /// </summary>
        public bool SwitchMode(uint mode)
        {
            try
            {
                byte[] cmd = new byte[16];

                // 命令ID
                BitConverter.GetBytes((uint)Sahara.SaharaCommandId.SAHARA_CMD_SWITCH_MODE_ID).CopyTo(cmd, 0);

                // 命令长度
                BitConverter.GetBytes((uint)cmd.Length).CopyTo(cmd, 4);

                // 目标模式
                BitConverter.GetBytes(mode).CopyTo(cmd, 8);

                device.Write(cmd);

                byte[] response = GetResponse();
                if (response == null)
                {
                    return false;
                }

                uint respCmd = BitConverter.ToUInt32(response, 0);
                if (respCmd == (uint)Sahara.SaharaCommandId.SAHARA_CMD_EXEC_ID)
                {
                    uint status = BitConverter.ToUInt32(response, 8);
                    return status == (uint)Sahara.SaharaStatus.SAHARA_STATUS_SUCCESS;
                }

                return false;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"切换模式错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取设备流信息
        /// </summary>
        public bool StreamingInfo()
        {
            try
            {
                // 尝试连接设备
                if (!Connect())
                {
                    return false;
                }

                // 尝试切换到内存调试模式
                if (SwitchMode((uint)Sahara.SaharaMode.SAHARA_MODE_MEMORY_DEBUG))
                {
                    // 获取设备硬件ID和序列号
                    HardwareId = GetMsmHwId();
                    SerialNumber = GetSerialNum();

                    //Console.WriteLine($"硬件ID: 0x{HardwareId:X}, 序列号: {SerialNumber}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"获取设备信息错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取MSM硬件ID
        /// </summary>
        private ulong GetMsmHwId()
        {
            // 这里应该实现实际读取硬件ID的逻辑
            // 作为示例，返回一个固定值
            return 0x009480E100000000;
        }

        /// <summary>
        /// 获取设备序列号
        /// </summary>
        private string GetSerialNum()
        {
            // 这里应该实现实际读取序列号的逻辑
            // 作为示例，返回一个固定值
            return "0123456789ABCDEF";
        }
    }
}