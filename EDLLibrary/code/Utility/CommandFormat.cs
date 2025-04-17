using System;
using System.Runtime.InteropServices;

namespace EDLLibrary.code.Utility;

public class CommandFormat
{
    public static byte[] StructToBytes(object structObj)
    {
        int num = 48;
        byte[] array = new byte[num];
        IntPtr intPtr = Marshal.AllocHGlobal(num);
        Marshal.StructureToPtr(structObj, intPtr, fDeleteOld: false);
        Marshal.Copy(intPtr, array, 0, num);
        Marshal.FreeHGlobal(intPtr);
        return array;
    }

    public static byte[] StructToBytes(object structObj, int length)
    {
        byte[] array = new byte[length];
        IntPtr intPtr = Marshal.AllocHGlobal(length);
        Marshal.StructureToPtr(structObj, intPtr, fDeleteOld: false);
        Marshal.Copy(intPtr, array, 0, length);
        Marshal.FreeHGlobal(intPtr);
        return array;
    }

    public static object BytesToStuct(byte[] bytes, Type type)
    {
        int num = Marshal.SizeOf(type);
        if (num > bytes.Length)
        {
            return null;
        }
        IntPtr intPtr = Marshal.AllocHGlobal(num);
        Marshal.Copy(bytes, 0, intPtr, num);
        object result = Marshal.PtrToStructure(intPtr, type);
        Marshal.FreeHGlobal(intPtr);
        return result;
    }
}
