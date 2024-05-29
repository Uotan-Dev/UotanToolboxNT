using Avalonia.Collections;

namespace UotanToolbox.Common
{
    internal class Global
    {
        public static string runpath = null;
        public static string System = "Windows";
        public static AvaloniaList<string> deviceslist;
        public static string thisdevice = null;
    }

    public static class GlobalData
    {
        public static MainViewModel MainViewModelInstance { get; set; }
    }

}