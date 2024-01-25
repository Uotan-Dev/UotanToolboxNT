using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SukiUI.Demo.Utilities
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