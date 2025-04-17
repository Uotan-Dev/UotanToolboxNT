using System.Runtime.InteropServices;

namespace EDLLibrary.code.module;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct sahara_end_transfer_packet
{
    public uint Command;

    public uint Length;

    public uint Image_id;

    public uint Status;
}
