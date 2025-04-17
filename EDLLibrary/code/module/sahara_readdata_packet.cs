using System.Runtime.InteropServices;

namespace EDLLibrary.code.module;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct sahara_readdata_packet
{
    public uint Command;

    public uint Length;

    public uint Image_id;

    public uint Offset;

    public uint SLength;
}
