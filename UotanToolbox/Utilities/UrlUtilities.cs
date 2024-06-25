using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UotanToolbox.Common;

namespace UotanToolbox.Utilities
{
    public static class UrlUtilities
    {
        public static void OpenURL(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    WorkingDirectory = Path.Combine(Global.bin_path, "platform-tools"),
                    UseShellExecute = true
                });
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Process.Start(new ProcessStartInfo
                {
                    FileName = "/usr/bin/gnome-terminal",  // 可以根据实际情况选择合适的终端程序
                    Arguments = $"--working-directory={Path.Combine(Global.bin_path, "platform-tools", "adb")}",
                    UseShellExecute = false
                });
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                Process.Start("open", "-a Terminal " + Path.Combine(Global.bin_path, "platform-tools", "adb"));
        }
    }
}