using System.Runtime.InteropServices;

namespace WuXingLibrary.code.module;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SimpleResponse
{
    public byte uResponse;
}
