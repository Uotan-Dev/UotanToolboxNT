using System.Runtime.InteropServices;

namespace WuXingLibrary.code.module;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct sahara_hello_packet
{
    public uint Command;

    public uint Length;

    public uint Version;

    public uint Version_min;

    public uint Max_Command_Length;

    public uint Mode;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public uint[] Reserved;
}
