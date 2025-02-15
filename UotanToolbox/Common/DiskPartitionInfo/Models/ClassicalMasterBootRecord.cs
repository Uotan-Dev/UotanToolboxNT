using System.Runtime.InteropServices;

namespace DiskPartitionInfo.Models
{
    [StructLayout(LayoutKind.Sequential, Size = 512, Pack = 1)]
    internal readonly struct ClassicalMasterBootRecord
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 446)]
        public readonly byte[] BootstrapCodeArea;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly MbrPartitionEntry[] PartitionEntries;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly byte[] BootSignature;
    }
}
