using DiskPartitionInfo.Extensions;
using DiskPartitionInfo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using GuidPartitionTable = DiskPartitionInfo.Gpt.GuidPartitionTable;

namespace DiskPartitionInfo.FluentApi
{
    internal class GptWriter : IGptWriter, IGptWriterLocation
    {
        // 定义两种扇区大小
        private const int StandardSectorSize = 512;
        private const int AdvancedSectorSize = 4096;

        private bool _usePrimary = true;
        private int _sectorSize = StandardSectorSize; // 默认使用标准扇区大小

        /// <inheritdoc/>
        public IGptWriter Primary()
        {
            _usePrimary = true;
            return this;
        }

        /// <inheritdoc/>
        public IGptWriter Secondary()
        {
            _usePrimary = false;
            return this;
        }

        /// <summary>
        /// 设置要使用的扇区大小
        /// </summary>
        /// <param name="sectorSize">扇区大小，通常是512或4096字节</param>
        /// <returns>写入器实例</returns>
        public IGptWriter WithSectorSize(int sectorSize)
        {
            if (sectorSize != StandardSectorSize && sectorSize != AdvancedSectorSize)
            {
                throw new ArgumentException($"扇区大小必须为{StandardSectorSize}或{AdvancedSectorSize}", nameof(sectorSize));
            }

            _sectorSize = sectorSize;
            return this;
        }

        /// <inheritdoc/>
        public void ToPath(string path, GuidPartitionTable gpt)
        {
            using var stream = File.OpenWrite(path);
            ToStream(stream, gpt);
        }

        /// <inheritdoc/>
        public void ToStream(Stream stream, GuidPartitionTable gpt)
        {
            if (!gpt.HasValidSignature())
            {
                throw new ArgumentException("提供的GPT分区表签名无效", nameof(gpt));
            }

            // 保存原始流位置以便于恢复
            long originalPosition = stream.Position;

            try
            {
                // 将GPT分区表中的每个分区条目转换为可写结构体
                var writablePartitions = new List<WritableGptPartitionEntry>();
                foreach (var partition in gpt.Partitions)
                {
                    writablePartitions.Add(WritableGptPartitionEntry.FromPartitionEntry(partition));
                }

                // 创建一个新的可写GPT头结构体并手动设置其属性
                var writableGpt = new WritableGuidPartitionTable
                {
                    // 设置EFI PART签名
                    Signature = "EFI PART".ToCharArray(),
                    // 版本通常为1.0（00 00 01 00）
                    Revision = new byte[] { 0, 0, 1, 0 },
                    HeaderSize = 92, // GPT头部的标准大小为92字节
                    // HeaderCrc32会在后面计算
                    HeaderCrc32 = 0,
                    Reserved = new byte[4],
                    PrimaryHeaderLocation = gpt.PrimaryHeaderLocation,
                    SecondaryHeaderLocation = gpt.SecondaryHeaderLocation,
                    FirstUsableLba = gpt.FirstUsableLba,
                    LastUsableLba = gpt.LastUsableLba,
                    DiskGuid = gpt.DiskGuid,
                    // 分区表通常从LBA 2开始
                    PartitionsArrayLba = 2,
                    // 设置分区数量
                    PartitionsCount = (uint)gpt.Partitions.Count,
                    // 每个分区项的大小，标准为128字节
                    PartitionEntryLength = 128,
                    // PartitionsArrayCrc32将在后面计算
                    PartitionsArrayCrc32 = 0,
                    // 保留字段，填充为0
                    Reserved2 = new byte[420]
                };

                // 确定写入的位置
                long headerPosition;
                long partitionsPosition;
                if (_usePrimary)
                {
                    // 主GPT位于LBA1（即第二个扇区）
                    headerPosition = _sectorSize;
                    writableGpt.PrimaryHeaderLocation = 1;

                    // 计算备份GPT位置
                    stream.Seek(0, SeekOrigin.End);
                    long endPosition = stream.Position;
                    writableGpt.SecondaryHeaderLocation = (ulong)((endPosition / _sectorSize) - 1);

                    // 主GPT的分区表通常在LBA2
                    partitionsPosition = 2 * _sectorSize;
                    writableGpt.PartitionsArrayLba = 2;
                }
                else
                {
                    // 备份GPT位于磁盘末尾
                    stream.Seek(0, SeekOrigin.End);
                    long endPosition = stream.Position;
                    headerPosition = endPosition - _sectorSize;

                    // 设置备份GPT的位置
                    writableGpt.SecondaryHeaderLocation = (ulong)(headerPosition / _sectorSize);
                    writableGpt.PrimaryHeaderLocation = 1;

                    // 备份GPT的分区表通常在末尾前
                    // 标准是33个LBA位置（即32个分区表扇区 + 1个GPT头扇区）
                    partitionsPosition = headerPosition - (32 * _sectorSize);
                    writableGpt.PartitionsArrayLba = (ulong)(partitionsPosition / _sectorSize);
                }

                // 计算分区表的CRC32校验和
                uint partitionsArrayCrc32 = writableGpt.CalculatePartitionsArrayCrc32(writablePartitions);

                // 生成包含正确CRC32校验和的GPT头字节数组
                byte[] gptHeaderBytes = writableGpt.ToBytes(partitionsArrayCrc32);

                // 写入GPT头部
                stream.Seek(headerPosition, SeekOrigin.Begin);
                stream.Write(gptHeaderBytes, 0, gptHeaderBytes.Length);

                // 写入分区表
                stream.Seek(partitionsPosition, SeekOrigin.Begin);
                foreach (var partition in writablePartitions)
                {
                    byte[] partitionBytes = partition.StructToBytes();
                    stream.Write(partitionBytes, 0, partitionBytes.Length);
                }

                // 确保流刷新到磁盘
                stream.Flush();
            }
            finally
            {
                // 恢复流的原始位置
                stream.Position = originalPosition;
            }
        }
    }
}