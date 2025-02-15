using System;
using System.Runtime.InteropServices;

namespace DiskPartitionInfo.Models
{
    [StructLayout(LayoutKind.Sequential, Size = 128, Pack = 1, CharSet = CharSet.Unicode)]
    internal readonly struct GptPartitionEntry
    {
        public readonly Guid PartitionType;

        public readonly Guid PartitionGuid;

        public readonly ulong FirstLba;

        public readonly ulong LastLba;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] AttributeFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)]
        public readonly string Name;
    }
}
