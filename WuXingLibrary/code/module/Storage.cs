using System.Runtime.InteropServices;

namespace WuXingLibrary.code.module;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct Storage
{
    public static string ufs = "ufs";

    public static string emmc = "emmc";
}
