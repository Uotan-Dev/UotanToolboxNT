using System;
using System.Linq;
using GptPartitionStruct = DiskPartitionInfo.Models.GptPartitionEntry;

namespace DiskPartitionInfo.Gpt
{
    public class PartitionEntry
    {
        private readonly GptPartitionStruct _partition;

        public Guid Type
            => _partition.PartitionType;

        public Guid Guid
            => _partition.PartitionGuid;

        public ulong FirstLba
            => _partition.FirstLba;

        public ulong LastLba
            => _partition.LastLba;

        public string Name
            => _partition.Name;

        public bool IsRequired
            => (_partition.AttributeFlags.First() & 0x01) != 0;

        public bool ShouldNotHaveDriveLetterAssigned
            => (_partition.AttributeFlags.Last() & 0x80) != 0;

        public bool IsHidden
            => (_partition.AttributeFlags.Last() & 0x40) != 0;

        public bool IsShadowCopy
            => (_partition.AttributeFlags.Last() & 0x20) != 0;

        public bool IsReadOnly
            => (_partition.AttributeFlags.Last() & 0x10) != 0;

        internal PartitionEntry(GptPartitionStruct partition)
        {
            _partition = partition;
        }
    }
}
