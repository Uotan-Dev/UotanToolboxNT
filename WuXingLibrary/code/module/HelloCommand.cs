using System.Runtime.InteropServices;

namespace WuXingLibrary.code.module;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct HelloCommand
{
    public byte uCommand;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] uMagicNumber;

    public byte uVersionNumber;

    public byte uCompatibleVersion;

    public byte uFeatureBits;
}
