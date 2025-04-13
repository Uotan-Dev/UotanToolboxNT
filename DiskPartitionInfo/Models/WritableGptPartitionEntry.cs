using System;
using System.Runtime.InteropServices;

namespace DiskPartitionInfo.Models
{
    [StructLayout(LayoutKind.Sequential, Size = 128, Pack = 1, CharSet = CharSet.Unicode)]
    internal struct WritableGptPartitionEntry
    {
        public Guid PartitionType;

        public Guid PartitionGuid;

        public ulong FirstLba;

        public ulong LastLba;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] AttributeFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)]
        public string Name;

        /// <summary>
        /// 从PartitionEntry创建一个可写的分区项结构体
        /// </summary>
        /// <param name="entry">源分区项</param>
        /// <returns>可写分区项结构体</returns>
        public static WritableGptPartitionEntry FromPartitionEntry(Gpt.PartitionEntry entry)
        {
            var result = new WritableGptPartitionEntry
            {
                PartitionType = entry.Type,
                PartitionGuid = entry.Guid,
                FirstLba = entry.FirstLba,
                LastLba = entry.LastLba,
                Name = entry.Name,
                AttributeFlags = new byte[8]
            };

            // 设置属性标志
            if (entry.IsRequired)
                result.AttributeFlags[0] |= 0x01;
            if (entry.ShouldNotHaveDriveLetterAssigned)
                result.AttributeFlags[7] |= 0x80;
            if (entry.IsHidden)
                result.AttributeFlags[7] |= 0x40;
            if (entry.IsShadowCopy)
                result.AttributeFlags[7] |= 0x20;
            if (entry.IsReadOnly)
                result.AttributeFlags[7] |= 0x10;

            return result;
        }
    }
}