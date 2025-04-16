using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GptPartitionStruct = DiskPartition.Models.GptPartitionEntry;
using GptStruct = DiskPartition.Models.GuidPartitionTable;

namespace DiskPartition.Gpt
{
    public class GuidPartitionTable
    {
        public int SectorSize;
        private readonly GptStruct _gpt;
        private readonly IReadOnlyCollection<PartitionEntry> _partitions;
        public bool HasValidSignature()
            => new string(_gpt.Signature).Equals("EFI PART", StringComparison.Ordinal);

        public ulong PrimaryHeaderLocation
            => _gpt.PrimaryHeaderLocation;

        public ulong SecondaryHeaderLocation
            => _gpt.SecondaryHeaderLocation;

        public ulong FirstUsableLba
            => _gpt.FirstUsableLba;

        public ulong LastUsableLba
            => _gpt.LastUsableLba;

        public Guid DiskGuid
            => _gpt.DiskGuid;

        public IReadOnlyCollection<PartitionEntry> Partitions
            => _partitions;

        internal GuidPartitionTable(int sectorSize,
            GptStruct gpt,
            IEnumerable<GptPartitionStruct> partitions)
        {
            SectorSize = sectorSize;
            _gpt = gpt;

            _partitions = new ReadOnlyCollection<PartitionEntry>([.. partitions.Select(p => new PartitionEntry(p))]);
        }
    }
}
