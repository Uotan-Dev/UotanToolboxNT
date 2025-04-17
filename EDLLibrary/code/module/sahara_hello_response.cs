using System.Runtime.InteropServices;

namespace EDLLibrary.code.module;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct sahara_hello_response
{
    public uint Command;

    public uint Length;

    public uint Version;

    public uint Version_min;

    public uint Status;

    public uint Mode;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public uint[] Reserved;
}
