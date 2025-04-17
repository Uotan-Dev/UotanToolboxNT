using System.Runtime.InteropServices;

namespace EDLLibrary.code.module;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct sahara_done_response
{
    public uint Command;

    public uint Length;

    public uint Status;
}
