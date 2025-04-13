using DiskPartitionInfo.Extensions;
using DiskPartitionInfo.Gpt;
using System;
using System.Collections.Generic;
using System.IO;
using GptPartitionStruct = DiskPartitionInfo.Models.GptPartitionEntry;
using GptStruct = DiskPartitionInfo.Models.GuidPartitionTable;

namespace DiskPartitionInfo.FluentApi
{
    internal partial class GptReader : IGptReader, IGptReaderLocation
    {
        // 定义两种扇区大小
        private const int StandardSectorSize = 512;
        private const int AdvancedSectorSize = 4096;
        private readonly int[] _sectorSizes = [AdvancedSectorSize, StandardSectorSize];

        private bool _usePrimary = true;

        /// <inheritdoc/>
        public IGptReader Primary()
        {
            _usePrimary = true;
            return this;
        }

        /// <inheritdoc/>
        public IGptReader Secondary()
        {
            _usePrimary = false;
            return this;
        }

        /// <inheritdoc/>
        public GuidPartitionTable FromPath(string path)
        {
            using var stream = File.OpenRead(path);

            return FromStream(stream);
        }

        /// <inheritdoc/>
        public GuidPartitionTable FromStream(Stream stream)
        {
            // 保存原始流位置以便于恢复
            long originalPosition = stream.Position;
            Exception? lastException = null;

            // 尝试使用不同的扇区大小读取GPT
            foreach (int sectorSize in _sectorSizes)
            {
                try
                {
                    // 重置流位置
                    stream.Position = originalPosition;

                    // 根据使用主分区表还是备份分区表决定定位位置
                    if (_usePrimary)
                        stream.Seek(sectorSize, SeekOrigin.Begin);
                    else
                        stream.Seek(-1 * sectorSize, SeekOrigin.End);

                    var gpt = ReadGpt(stream, sectorSize);

                    // 验证GPT签名有效性
                    if (!new string(gpt.Signature).Equals("EFI PART", StringComparison.Ordinal))
                    {
                        continue; // 签名无效，尝试下一种扇区大小
                    }

                    stream.Seek((long)gpt.PartitionsArrayLba * sectorSize, SeekOrigin.Begin);

                    var partitions = ReadPartitions(stream, gpt);

                    // 成功读取到有效的GPT
                    return new GuidPartitionTable(gpt, partitions);
                }
                catch (Exception ex)
                {
                    // 记录最后一次异常，如果所有尝试都失败则抛出
                    lastException = ex;
                }
            }

            // 如果所有尝试都失败，抛出最后一次异常
            if (lastException != null)
                throw new InvalidOperationException("无法使用任何扇区大小读取有效的GPT分区表", lastException);

            throw new InvalidOperationException("无法读取有效的GPT分区表");
        }

        private static GptStruct ReadGpt(Stream stream, int sectorSize)
        {
            var gptData = new byte[sectorSize];
            stream.Read(buffer: gptData, offset: 0, count: sectorSize);

            return gptData.ToStruct<GptStruct>();
        }

        private static List<GptPartitionStruct> ReadPartitions(Stream stream, GptStruct gpt)
        {
            var partitions = new List<GptPartitionStruct>();

            for (var i = 0; i < gpt.PartitionsCount; ++i)
            {
                var partition = new byte[gpt.PartitionEntryLength];

                stream.Read(buffer: partition, offset: 0, count: (int)gpt.PartitionEntryLength);
                partitions.Add(partition.ToStruct<GptPartitionStruct>());
            }

            return partitions;
        }
    }
}
