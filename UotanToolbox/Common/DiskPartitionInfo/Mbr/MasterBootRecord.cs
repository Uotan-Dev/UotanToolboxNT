using System;
using System.Collections.Generic;
using System.Linq;
using DiskPartitionInfo.Models;

namespace DiskPartitionInfo.Mbr
{
    public class MasterBootRecord
    {
        private readonly ClassicalMasterBootRecord _mbr;

        public IReadOnlyCollection<byte> BootstrapCode
            => Array.AsReadOnly(_mbr.BootstrapCodeArea);

        public IReadOnlyCollection<byte> BootSignature
            => Array.AsReadOnly(_mbr.BootSignature);

        public IReadOnlyCollection<PartitionEntry> Partitions
            => Array.AsReadOnly(_mbr.PartitionEntries
                .Select(p => new PartitionEntry(p))
                .ToArray());

        /// <summary>
        /// Checks whether the MBR looks like a protective MBR for drives with GPT.
        /// </summary>
        public bool IsProtectiveMbr
            => _mbr.PartitionEntries[0].PartitionType == 0xEE;

        /// <summary>
        /// Checks whether the MBR boot signature is valid (0x55 0xAA).
        /// </summary>
        public bool IsBootSignatureValid
            => _mbr.BootSignature[0] == 0x55
            && _mbr.BootSignature[1] == 0xAA;

        internal MasterBootRecord(ClassicalMasterBootRecord mbr)
        {
            _mbr = mbr;
        }
    }
}
