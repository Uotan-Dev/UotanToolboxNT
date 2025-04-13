using DiskPartitionInfo.Extensions;
using DiskPartitionInfo.Util;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DiskPartitionInfo.Models
{
    [StructLayout(LayoutKind.Sequential, Size = 512, Pack = 1)]
    internal struct WritableGuidPartitionTable
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public char[] Signature;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Revision;

        public uint HeaderSize;

        public uint HeaderCrc32;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Reserved;

        public ulong PrimaryHeaderLocation;

        public ulong SecondaryHeaderLocation;

        public ulong FirstUsableLba;

        public ulong LastUsableLba;

        public Guid DiskGuid;

        public ulong PartitionsArrayLba;

        public uint PartitionsCount;

        public uint PartitionEntryLength;

        public uint PartitionsArrayCrc32;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 420)]
        public byte[] Reserved2;

        /// <summary>
        /// GPT头CRC32校验和字段的偏移量
        /// </summary>
        private const int HeaderCrc32Offset = 16;

        /// <summary>
        /// GPT头CRC32校验和字段的大小（字节数）
        /// </summary>
        private const int HeaderCrc32Size = 4;

        /// <summary>
        /// 从GuidPartitionTable创建一个可写的分区表结构体
        /// </summary>
        /// <param name="gpt">源分区表</param>
        /// <returns>可写分区表结构体</returns>
        public static WritableGuidPartitionTable FromGuidPartitionTable(GuidPartitionTable gpt)
        {
            return new WritableGuidPartitionTable
            {
                // 设置EFI PART签名
                Signature = "EFI PART".ToCharArray(),
                // 版本通常为1.0（00 00 01 00）
                Revision = new byte[] { 0, 0, 1, 0 },
                HeaderSize = 92, // GPT头部的标准大小为92字节
                // HeaderCrc32会在后面计算
                HeaderCrc32 = 0,
                Reserved = new byte[4],
                PrimaryHeaderLocation = 1, // 通常是LBA 1
                SecondaryHeaderLocation = gpt.SecondaryHeaderLocation,
                FirstUsableLba = gpt.FirstUsableLba,
                LastUsableLba = gpt.LastUsableLba,
                DiskGuid = gpt.DiskGuid,
                // 分区表通常从LBA 2开始
                PartitionsArrayLba = 2,
                // 设置分区数量
                PartitionsCount = gpt.PartitionsCount,
                // 每个分区项的大小，标准为128字节
                PartitionEntryLength = 128,
                // PartitionsArrayCrc32将在后面计算
                PartitionsArrayCrc32 = 0,
                // 保留字段，填充为0
                Reserved2 = new byte[420]
            };
        }

        /// <summary>
        /// 计算分区表的CRC32校验和
        /// </summary>
        /// <param name="partitionEntries">分区条目</param>
        /// <returns>分区表的CRC32校验和</returns>
        public uint CalculatePartitionsArrayCrc32(IReadOnlyCollection<WritableGptPartitionEntry> partitionEntries)
        {
            if (partitionEntries == null || partitionEntries.Count == 0)
                return 0;

            // 将所有分区条目合并为一个字节数组
            byte[] allPartitionBytes = new byte[partitionEntries.Count * 128]; // 每个分区条目128字节
            int offset = 0;

            foreach (var entry in partitionEntries)
            {
                byte[] entryBytes = entry.StructToBytes();
                Buffer.BlockCopy(entryBytes, 0, allPartitionBytes, offset, entryBytes.Length);
                offset += entryBytes.Length;
            }

            // 计算CRC32校验和
            return Crc32.Compute(allPartitionBytes);
        }

        /// <summary>
        /// 创建包含已计算CRC32校验和的GPT字节数组
        /// </summary>
        /// <param name="partitionsArrayCrc32">分区表的CRC32校验和</param>
        /// <returns>包含完整GPT头信息的字节数组</returns>
        public byte[] ToBytes(uint partitionsArrayCrc32)
        {
            // 设置分区表CRC32
            this.PartitionsArrayCrc32 = partitionsArrayCrc32;

            // 先将结构体转换为字节数组
            byte[] gptBytes = this.StructToBytes();

            // 临时将HeaderCrc32字段设为0进行计算
            Buffer.BlockCopy(BitConverter.GetBytes(0u), 0, gptBytes, HeaderCrc32Offset, HeaderCrc32Size);

            // 计算HeaderCrc32（排除HeaderCrc32字段本身）
            uint headerCrc32 = Crc32.ComputeWithExclusion(gptBytes.AsSpan(0, (int)HeaderSize).ToArray(), HeaderCrc32Offset, HeaderCrc32Size);

            // 将计算好的HeaderCrc32写入到正确位置
            Buffer.BlockCopy(BitConverter.GetBytes(headerCrc32), 0, gptBytes, HeaderCrc32Offset, HeaderCrc32Size);

            return gptBytes;
        }
    }
}