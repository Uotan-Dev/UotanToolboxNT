namespace DeviceTreeNode.Nodes
{
    public class MemoryReservation
    {
        public IntPtr Address { get; }
        public ulong Size { get; }

        public MemoryReservation(IntPtr address, ulong size)
        {
            Address = address;
            Size = size;
        }

        public override string ToString() =>
            $"MemoryReservation[Address=0x{Address.ToInt64():X}, Size=0x{Size:X}]";
    }
}
