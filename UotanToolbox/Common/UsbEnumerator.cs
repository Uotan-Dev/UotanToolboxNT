using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UotanToolbox.Common
{
    /// <summary>
    /// Enumerates present devices on the USB bus in-process via SetupAPI and
    /// renders text compatible with `devcon find usb*`:
    ///   USB\VID_XXXX&amp;PID_XXXX\instance
    ///       Name: &lt;friendly name or description&gt;
    /// This replaces the external devcon.exe: devcon.exe requires elevation,
    /// and it deadlocked with many USB devices when the caller read
    /// stdout/stderr sequentially (issue #101).
    /// </summary>
    /// <remarks>
    /// Scope: every present devnode under the USB enumerator (USB\*), which is
    /// the same set `devcon find usb*` matches for this codebase's consumers.
    /// This is enumerator based, not class based, so the Qualcomm QDLoader
    /// 9008/900E/901D/9091 and SPRD U2S Diag serial ports parsed by
    /// StringHelper.COMDevices are included even though they belong to the
    /// Ports device class. Unlike the WinUSB API, this also covers devices not
    /// driven by winusb.sys. No elevation is required.
    /// </remarks>
    internal static class UsbEnumerator
    {
        private const uint DIGCF_PRESENT = 0x00000002;
        private const uint DIGCF_ALLCLASSES = 0x00000004;
        private const uint SPDRP_DEVICEDESC = 0x00000000;
        private const uint SPDRP_FRIENDLYNAME = 0x0000000C;

        private static readonly IntPtr InvalidHandle = new(-1);

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVINFO_DATA
        {
            public int cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevs(IntPtr classGuid, string enumerator, IntPtr hwndParent, uint flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet, uint memberIndex, ref SP_DEVINFO_DATA deviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetupDiGetDeviceInstanceId(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, StringBuilder deviceInstanceId, int deviceInstanceIdSize, out int requiredSize);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetupDiGetDeviceRegistryProperty(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, uint property, out uint propertyRegDataType, byte[] propertyBuffer, int propertyBufferSize, out int requiredSize);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

        /// <summary>
        /// Returns text equivalent to `devcon find usb*`. An empty string means
        /// enumeration failed (callers treat it as "no matching device").
        /// </summary>
        public static Task<string> FindUsbDevicesAsync() => Task.Run(FindUsbDevices);

        private static string FindUsbDevices()
        {
            // ClassGuid = NULL, Enumerator = "USB", DIGCF_PRESENT | DIGCF_ALLCLASSES:
            // every present devnode under the USB enumerator, i.e. all USB\*
            // devices regardless of their device class.
            IntPtr set = SetupDiGetClassDevs(IntPtr.Zero, "USB", IntPtr.Zero, DIGCF_PRESENT | DIGCF_ALLCLASSES);
            if (set == IntPtr.Zero || set == InvalidHandle)
            {
                return string.Empty;
            }

            try
            {
                StringBuilder output = new();
                StringBuilder idBuffer = new(512);
                byte[] nameBuffer = new byte[2048];
                for (uint index = 0; ; index++)
                {
                    SP_DEVINFO_DATA info = new() { cbSize = Marshal.SizeOf<SP_DEVINFO_DATA>() };
                    if (!SetupDiEnumDeviceInfo(set, index, ref info))
                    {
                        break; // ERROR_NO_MORE_ITEMS (or an unexpected error): stop
                    }

                    _ = idBuffer.Clear();
                    if (!SetupDiGetDeviceInstanceId(set, ref info, idBuffer, idBuffer.Capacity, out _))
                    {
                        continue;
                    }

                    _ = output.Append(idBuffer).Append('\n');

                    string name = ReadDescription(set, ref info, nameBuffer);
                    if (!string.IsNullOrEmpty(name))
                    {
                        _ = output.Append("    Name: ").Append(name).Append('\n');
                    }
                }

                return output.ToString();
            }
            catch
            {
                return string.Empty;
            }
            finally
            {
                _ = SetupDiDestroyDeviceInfoList(set);
            }
        }

        private static string ReadDescription(IntPtr set, ref SP_DEVINFO_DATA info, byte[] buffer)
        {
            // devcon prints the friendly name when present, else the description
            uint[] properties = [SPDRP_FRIENDLYNAME, SPDRP_DEVICEDESC];
            foreach (uint property in properties)
            {
                if (SetupDiGetDeviceRegistryProperty(set, ref info, property, out _, buffer, buffer.Length, out int requiredSize) && requiredSize > 0)
                {
                    int byteLength = Math.Min(requiredSize, buffer.Length);
                    string value = Encoding.Unicode.GetString(buffer, 0, byteLength).TrimEnd('\0');
                    if (value.Length > 0)
                    {
                        return value;
                    }
                }
            }

            return string.Empty;
        }
    }
}
