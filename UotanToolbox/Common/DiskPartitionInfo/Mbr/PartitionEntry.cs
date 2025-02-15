using System;
using System.Collections.Generic;
using DiskPartitionInfo.Models;

namespace DiskPartitionInfo.Mbr
{
    /// <summary>
    /// Represents the MBR partition entry.
    /// </summary>
    public class PartitionEntry
    {
        private readonly MbrPartitionEntry _partitionEntry;

        public byte Status
            => _partitionEntry.Status;

        /// <summary>
        /// Checks whether the partition hss the active/bootable flag.
        /// </summary>
        public bool IsActive
            => (_partitionEntry.Status & 0x80) != 0;

        public byte PartitionType
            => _partitionEntry.PartitionType;

        /// <summary>
        /// CHS address of the first absolute sector in partition.
        /// See https://en.wikipedia.org/wiki/Master_boot_record#PTE for more info.
        /// </summary>
        public IReadOnlyCollection<byte> FirstBootableSector
            => Array.AsReadOnly(_partitionEntry.FirstBootableSector);

        /// <summary>
        /// CHS address of the last absolute sector in partition.
        /// See https://en.wikipedia.org/wiki/Master_boot_record#PTE for more info.
        /// </summary>
        public IReadOnlyCollection<byte> LastBootableSector
            => Array.AsReadOnly(_partitionEntry.LastBootableSector);

        /// <summary>
        /// LBA of first absolute sector in the partition.
        /// </summary>
        public uint FirstPartitionSector
            => _partitionEntry.FirstAbsoluteSector;

        /// <summary>
        /// Number of sectors in the partition.
        /// </summary>
        public uint Length
            => _partitionEntry.SectorsCount;

        internal PartitionEntry(MbrPartitionEntry partitionEntry)
        {
            _partitionEntry = partitionEntry;
        }
    }
}
