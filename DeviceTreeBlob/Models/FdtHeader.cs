using DeviceTreeNode.Core;

namespace DeviceTreeNode.Models
{
    public class FdtHeader
    {
        // FDT头部魔数
        private const uint FDT_MAGIC = 0xd00dfeed;

        public uint Magic { get; }
        public uint TotalSize { get; }
        public uint StructOffset { get; }
        public uint StringsOffset { get; }
        public uint MemoryReservationOffset { get; }
        public uint Version { get; }
        public uint LastCompatibleVersion { get; }
        public uint BootCpuId { get; }
        public uint StringsSize { get; }
        public uint StructSize { get; }

        private FdtHeader(
            uint magic, uint totalSize,
            uint structOffset, uint stringsOffset, uint memReservOffset,
            uint version, uint lastCompatVersion,
            uint bootCpuId, uint stringsSize, uint structSize)
        {
            Magic = magic;
            TotalSize = totalSize;
            StructOffset = structOffset;
            StringsOffset = stringsOffset;
            MemoryReservationOffset = memReservOffset;
            Version = version;
            LastCompatibleVersion = lastCompatVersion;
            BootCpuId = bootCpuId;
            StringsSize = stringsSize;
            StructSize = structSize;
        }

        public static FdtHeader FromBytes(FdtData data)
        {
            uint? magic = data.ReadUInt32();
            uint? totalSize = data.ReadUInt32();
            uint? structOffset = data.ReadUInt32();
            uint? stringsOffset = data.ReadUInt32();
            uint? memReservOffset = data.ReadUInt32();
            uint? version = data.ReadUInt32();
            uint? lastCompatVersion = data.ReadUInt32();
            uint? bootCpuId = data.ReadUInt32();
            uint? stringsSize = data.ReadUInt32();
            uint? structSize = data.ReadUInt32();

            if (!magic.HasValue || !totalSize.HasValue || !structOffset.HasValue ||
                !stringsOffset.HasValue || !memReservOffset.HasValue || !version.HasValue ||
                !lastCompatVersion.HasValue || !bootCpuId.HasValue || !stringsSize.HasValue ||
                !structSize.HasValue)
                return null;

            return new FdtHeader(
                magic.Value, totalSize.Value,
                structOffset.Value, stringsOffset.Value, memReservOffset.Value,
                version.Value, lastCompatVersion.Value,
                bootCpuId.Value, stringsSize.Value, structSize.Value
            );
        }

        public bool ValidMagic => Magic == FDT_MAGIC;
    }
}
