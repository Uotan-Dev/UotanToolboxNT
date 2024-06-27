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
                Process.Start(new ProcessStartInfo(url.Replace("&", "^&")) { UseShellExecute = true });
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Process.Start("xdg-open", url);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                Process.Start("open", url);
        }
    }
}