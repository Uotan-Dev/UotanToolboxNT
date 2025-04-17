using System.Runtime.InteropServices;

namespace EDLLibrary.code.module;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct sahara_64b_readdata_packet
{
    public uint Command;

    public uint Length;

    public ulong Image_id;

    public ulong Offset;

    public ulong SLength;
}
