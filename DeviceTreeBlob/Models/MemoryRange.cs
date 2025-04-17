namespace DeviceTreeNode.Models
{
    public class MemoryRange
    {
        public IntPtr PhysicalAddress { get; }
        public IntPtr VirtualAddress { get; }
        public ulong Size { get; }

        public MemoryRange(IntPtr physAddr, IntPtr virtAddr, ulong size)
        {
            PhysicalAddress = physAddr;
            VirtualAddress = virtAddr;
            Size = size;
        }

        public override string ToString() =>
            $"MemoryRange[Physical=0x{PhysicalAddress.ToInt64():X}, Virtual=0x{VirtualAddress.ToInt64():X}, Size=0x{Size:X}]";
    }
}
