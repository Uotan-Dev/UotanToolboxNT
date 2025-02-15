using System.Collections.Generic;
using System.IO;
using DiskPartitionInfo.Extensions;
using DiskPartitionInfo.Gpt;
using GptPartitionStruct = DiskPartitionInfo.Models.GptPartitionEntry;
using GptStruct = DiskPartitionInfo.Models.GuidPartitionTable;

namespace DiskPartitionInfo.FluentApi
{
    internal partial class GptReader : IGptReader, IGptReaderLocation
    {
        private const int SectorSize = 512;

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
            if (_usePrimary)
                stream.Seek(SectorSize, SeekOrigin.Begin);
            else
                stream.Seek(-1 * SectorSize, SeekOrigin.End);

            var gpt = ReadGpt(stream);

            stream.Seek((long) gpt.PartitionsArrayLba * SectorSize, SeekOrigin.Begin);

            var partitions = ReadPartitions(stream, gpt);

            return new GuidPartitionTable(gpt, partitions);
        }

        private static GptStruct ReadGpt(Stream stream)
        {
            var gptData = new byte[SectorSize];
            stream.Read(buffer: gptData, offset: 0, count: SectorSize);

            return gptData.ToStruct<GptStruct>();
        }

        private static List<GptPartitionStruct> ReadPartitions(Stream stream, GptStruct gpt)
        {
            var partitions = new List<GptPartitionStruct>();

            for (var i = 0; i < gpt.PartitionsCount; ++i)
            {
                var partition = new byte[gpt.PartitionEntryLength];

                stream.Read(buffer: partition, offset: 0, count: (int) gpt.PartitionEntryLength);
                partitions.Add(partition.ToStruct<GptPartitionStruct>());
            }

            return partitions;
        }
    }
}
