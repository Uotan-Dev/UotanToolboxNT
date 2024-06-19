using Avalonia.Collections;

namespace UotanToolbox.Common
{
    internal class Global
    {
        public static string runpath = null;
        public static string bin_path = null;
        public static string tmp_path = null;
        public static string magisk_tmp, boot_tmp, boot_sha1 = null;
        public static string System = "Windows";
        public static AvaloniaList<string> deviceslist;
        public static string thisdevice = null;
        public static int mb_exitcode, cpio_exitcode;
        public static bool is_magisk_ok, is_boot_ok = false;
        //分区表储存
        public static string sdatable = "";
        public static string sdbtable = "";
        public static string sdctable = "";
        public static string sddtable = "";
        public static string sdetable = "";
        public static string sdftable = "";
        public static string emmcrom = "";
    }

    public static class GlobalData
    {
        public static MainViewModel MainViewModelInstance { get; set; }
    }

}