using System;

namespace DiskPartitionInfo.Util
{
    /// <summary>
    /// 用于计算CRC32校验和的工具类
    /// </summary>
    internal static class Crc32
    {
        private static readonly uint[] Table = new uint[256];
        private const uint Polynomial = 0xEDB88320;

        static Crc32()
        {
            // 初始化CRC32表
            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 1) == 1)
                        crc = (crc >> 1) ^ Polynomial;
                    else
                        crc >>= 1;
                }
                Table[i] = crc;
            }
        }

        /// <summary>
        /// 计算字节数组的CRC32校验和
        /// </summary>
        /// <param name="bytes">要计算校验和的字节数组</param>
        /// <param name="offset">起始偏移量</param>
        /// <param name="count">要计算的字节数</param>
        /// <returns>CRC32校验和</returns>
        public static uint Compute(byte[] bytes, int offset, int count)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            uint crc = 0xFFFFFFFF;
            for (int i = offset; i < offset + count; i++)
            {
                byte index = (byte)((crc & 0xFF) ^ bytes[i]);
                crc = (crc >> 8) ^ Table[index];
            }
            return ~crc;
        }

        /// <summary>
        /// 计算字节数组的CRC32校验和
        /// </summary>
        /// <param name="bytes">要计算校验和的字节数组</param>
        /// <returns>CRC32校验和</returns>
        public static uint Compute(byte[] bytes)
        {
            return Compute(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// 计算字节数组的CRC32校验和，但排除指定区域（例如用于计算GPT头时排除CRC32字段）
        /// </summary>
        /// <param name="bytes">要计算校验和的字节数组</param>
        /// <param name="excludeOffset">要排除区域的起始偏移量</param>
        /// <param name="excludeSize">要排除区域的字节数</param>
        /// <returns>CRC32校验和</returns>
        public static uint ComputeWithExclusion(byte[] bytes, int excludeOffset, int excludeSize)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            // 如果排除区域无效或超出范围，则计算整个数组
            if (excludeOffset < 0 || excludeOffset >= bytes.Length ||
                excludeSize <= 0 || excludeOffset + excludeSize > bytes.Length)
            {
                return Compute(bytes);
            }

            // 计算排除区域之前的部分
            uint crc = 0xFFFFFFFF;
            for (int i = 0; i < excludeOffset; i++)
            {
                byte index = (byte)((crc & 0xFF) ^ bytes[i]);
                crc = (crc >> 8) ^ Table[index];
            }

            // 跳过排除区域
            for (int i = excludeOffset + excludeSize; i < bytes.Length; i++)
            {
                byte index = (byte)((crc & 0xFF) ^ bytes[i]);
                crc = (crc >> 8) ^ Table[index];
            }

            return ~crc;
        }
    }
}