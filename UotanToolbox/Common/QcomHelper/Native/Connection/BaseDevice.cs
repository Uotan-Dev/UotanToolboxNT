using System;
using System.Collections.Generic;

namespace UotanToolbox.Common.QcomHelper.Connection
{
    /// <summary>
    /// 设备通信抽象基类
    /// 原始Python实现 © B.Kerler 2018-2024 under GPLv3 license
    /// </summary>
    public abstract class DeviceClass
    {
        // 设备通信基本属性
        protected bool connected = false;
        protected int timeout = 1000;
        protected int maxsize = 512;

        // 设备标识和配置
        protected uint? vid = null;
        protected uint? pid = null;
        protected int devclass = -1;

        // 串行通信参数
        protected int baudrate;
        protected int databits;
        protected double stopbits;
        protected int parity;

        // 设备实例和配置
        protected object device = null;
        protected object configuration = null;

        // 其他通信参数
        protected List<UsbDeviceIdentifier> portConfig;
        protected bool xmlread = true;

        /// <summary>
        /// 设备是否已连接
        /// </summary>
        public bool Connected => connected;

        /// <summary>
        /// 设备厂商ID
        /// </summary>
        public uint? Vid => vid;

        /// <summary>
        /// 设备产品ID
        /// </summary>
        public uint? Pid => pid;

        /// <summary>
        /// 设备类型
        /// </summary>
        public int DevClass
        {
            get => devclass;
            protected set => devclass = value;
        }

        /// <summary>
        /// 初始化设备通信类
        /// </summary>
        /// <param name="portConfig">端口配置</param>
        /// <param name="devClass">设备类型标识符</param>
        public DeviceClass(List<UsbDeviceIdentifier> portConfig = null, int devClass = -1)
        {
            this.portConfig = portConfig ?? new List<UsbDeviceIdentifier>();
            this.devclass = devClass;
        }

        #region 抽象方法 - 必须由子类实现

        /// <summary>
        /// 连接到设备
        /// </summary>
        public abstract bool Connect(int epIn = -1, int epOut = -1, string portname = "");

        /// <summary>
        /// 关闭设备连接
        /// </summary>
        public abstract void Close(bool reset = false);

        /// <summary>
        /// 刷新设备缓冲区
        /// </summary>
        public abstract void Flush();

        /// <summary>
        /// 检测可用设备
        /// </summary>
        public abstract List<DeviceInfo> DetectDevices();

        /// <summary>
        /// 获取设备接口数量
        /// </summary>
        public abstract int GetInterfaceCount();

        /// <summary>
        /// 设置串行线路编码参数
        /// </summary>
        public abstract void SetLineCoding(int? baudrate = null, int parity = 0, int databits = 8, double stopbits = 1);

        /// <summary>
        /// 发送中断信号
        /// </summary>
        public abstract void SetBreak();

        /// <summary>
        /// 设置控制线状态
        /// </summary>
        public abstract void SetControlLineState(bool? rts = null, bool? dtr = null, bool isFTDI = false);

        /// <summary>
        /// 写入命令到设备
        /// </summary>
        public abstract bool Write(byte[] command, int? pktsize = null);

        /// <summary>
        /// USB写入操作
        /// </summary>
        public abstract bool UsbWrite(byte[] data, int? pktsize = null);

        /// <summary>
        /// USB读取操作
        /// </summary>
        public abstract byte[] UsbRead(int? resplen = null, int timeout = 0);

        /// <summary>
        /// USB控制传输
        /// </summary>
        public abstract int ControlTransfer(byte bmRequestType, byte bRequest, short wValue, short wIndex, byte[] data, int length);

        /// <summary>
        /// USB读写组合操作
        /// </summary>
        public abstract byte[] UsbReadWrite(byte[] data, int resplen);

        #endregion

        #region 已实现的数据读取方法

        /// <summary>
        /// 从设备读取数据
        /// </summary>
        public virtual byte[] Read(int? length = null, int timeout = -1)
        {
            if (timeout == -1)
                timeout = this.timeout;

            if (length == null)
                length = this.maxsize;

            return UsbRead(length, timeout);
        }

        /// <summary>
        /// 读取指定数量的4字节整数
        /// </summary>
        public virtual uint[] ReadDWord(int count = 1, bool little = false)
        {
            uint[] result = new uint[count];
            byte[] data = UsbRead(4 * count);

            if (data.Length < 4 * count)
                Array.Resize(ref result, data.Length / 4);

            for (int i = 0; i < result.Length; i++)
            {
                if (little)
                {
                    result[i] = BitConverter.ToUInt32(data, i * 4);
                }
                else
                {
                    // 处理大端字节序
                    byte[] temp = new byte[4];
                    Array.Copy(data, i * 4, temp, 0, 4);
                    Array.Reverse(temp);
                    result[i] = BitConverter.ToUInt32(temp, 0);
                }
            }

            return result;
        }

        /// <summary>
        /// 读取指定数量的2字节整数
        /// </summary>
        public virtual ushort[] ReadWord(int count = 1, bool little = false)
        {
            ushort[] result = new ushort[count];

            for (int i = 0; i < count; i++)
            {
                byte[] data = UsbRead(2);
                if (data.Length < 2)
                    break;

                if (little)
                {
                    result[i] = BitConverter.ToUInt16(data, 0);
                }
                else
                {
                    // 处理大端字节序
                    byte[] temp = new byte[2];
                    Array.Copy(data, 0, temp, 0, 2);
                    Array.Reverse(temp);
                    result[i] = BitConverter.ToUInt16(temp, 0);
                }
            }

            return result;
        }

        /// <summary>
        /// 读取指定数量的字节
        /// </summary>
        public virtual byte[] ReadByte(int count = 1)
        {
            return UsbRead(count);
        }

        /// <summary>
        /// 验证数据(移除了日志记录功能)
        /// </summary>
        public virtual byte[] VerifyData(byte[] data, string prefix = "RX:")
        {
            return data;
        }

        #endregion
    }

    /// <summary>
    /// USB设备标识符
    /// </summary>
    public class UsbDeviceIdentifier
    {
        public int Vid { get; }
        public int Pid { get; }
        public int Interface { get; }

        public UsbDeviceIdentifier(int vid, int pid, int interfaceNum)
        {
            Vid = vid;
            Pid = pid;
            Interface = interfaceNum;
        }
    }

    /// <summary>
    /// 设备信息类
    /// </summary>
    public class DeviceInfo
    {
        public int Vid { get; }
        public int Pid { get; }

        public DeviceInfo(int vid, int pid)
        {
            Vid = vid;
            Pid = pid;
        }
    }

}
