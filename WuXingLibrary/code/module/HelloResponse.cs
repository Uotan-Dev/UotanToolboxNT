using System.Runtime.InteropServices;

namespace WuXingLibrary.code.module;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct HelloResponse
{
    public byte uResponse;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] uMagicNumber;

    public byte uVersionNumber;

    public byte uCompatibleVersion;

    public uint uMaxBlockSize;

    public uint uFlashBaseAddress;

    public byte uFlashIdLength;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
    public byte[] uVariantBuffer;
}
