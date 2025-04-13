using System.Runtime.InteropServices;

namespace DiskPartitionInfo.Models
{
    [StructLayout(LayoutKind.Sequential, Size = 16, Pack = 1)]
    internal readonly struct MbrPartitionEntry
    {
        public readonly byte Status;

        /// <summary>
        /// CHS address
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] FirstBootableSector;

        public readonly byte PartitionType;

        /// <summary>
        /// CHS address
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] LastBootableSector;

        public readonly uint FirstAbsoluteSector;

        public readonly uint SectorsCount;
    }
}
