using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UotanToolbox.Utilities
{
    public static class UrlUtilities
    {
        public static void OpenURL(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _ = Process.Start(new ProcessStartInfo(url.Replace("&", "^&")) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _ = Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _ = Process.Start("open", url);
            }
        }
    }
}