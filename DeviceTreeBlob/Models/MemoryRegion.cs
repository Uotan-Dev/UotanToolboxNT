namespace DeviceTreeNode.Models
{
    public class MemoryRegion
    {
        public IntPtr StartingAddress { get; }
        public ulong? Size { get; }

        public MemoryRegion(IntPtr address, ulong? size)
        {
            StartingAddress = address;
            Size = size;
        }

        public ulong EndAddress => Size.HasValue ?
            (ulong)StartingAddress.ToInt64() + Size.Value : (ulong)StartingAddress.ToInt64();

        public bool Contains(IntPtr address)
        {
            ulong addr = (ulong)address.ToInt64();
            ulong start = (ulong)StartingAddress.ToInt64();

            return addr >= start && (!Size.HasValue || addr < start + Size.Value);
        }

        public override string ToString() => Size.HasValue ?
            $"MemoryRegion[0x{StartingAddress.ToInt64():X}-0x{EndAddress:X}]" :
            $"MemoryRegion[0x{StartingAddress.ToInt64():X}]";
    }
}
