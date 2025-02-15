using System;
using System.Runtime.InteropServices;

namespace DiskPartitionInfo.Models
{
    [StructLayout(LayoutKind.Sequential, Size = 512, Pack = 1)]
    internal readonly struct GuidPartitionTable
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly char[] Signature;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly byte[] Revision;

        public readonly uint HeaderSize;

        public readonly uint HeaderCrc32;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly byte[] Reserved;

        public readonly ulong PrimaryHeaderLocation;

        public readonly ulong SecondaryHeaderLocation;

        public readonly ulong FirstUsableLba;

        public readonly ulong LastUsableLba;

        public readonly Guid DiskGuid;

        public readonly ulong PartitionsArrayLba;

        public readonly uint PartitionsCount;

        public readonly uint PartitionEntryLength;

        public readonly uint PartitionsArrayCrc32;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 420)]
        public readonly byte[] Reserved2;
    }
}
