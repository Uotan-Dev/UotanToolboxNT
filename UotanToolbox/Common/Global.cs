﻿using Avalonia.Collections;

namespace UotanToolbox.Common
{
    internal class Global
    {
        public static bool checkdevice = true;
        public static string runpath = null;
        public static string bin_path = null;
        public static string tmp_path = null;
        public static string log_path = null;
        public static string backup_path = null;
        public static string System = "Windows";
        public static AvaloniaList<string> deviceslist;
        public static string thisdevice = null;
        public static int mb_exitcode, cpio_exitcode, load_times;
        //分区表储存
        public static string sdatable = "";
        public static string sdbtable = "";
        public static string sdctable = "";
        public static string sddtable = "";
        public static string sdetable = "";
        public static string sdftable = "";
        public static string emmcrom = "";

        //工具箱版本
        public static string currentVersion = "819a867";
    }
    public class BootInfo
    {
        public static string SHA1 = "";
        public static string tmp_path = "";
        public static bool userful = false;
        public static bool gki2 = false;
        public static string kmi = "";
        public static string os_version = "";
        public static string patch_level = "";
        public static bool have_ramdisk = false;
        public static bool have_kernel = false;
        public static bool have_dtb = false;
        public static string dtb_name = "";
        public static string arch = "aarch64";
    }
    public class ZipInfo
    {
        public static string SHA1 = "";
        public static string ver = "";
        public static string tmp_path = "";
        public static bool userful = false;
        public static bool is_magisk = false;
        public static string patch_SHA1 = "";
    }
    public class EnvironmentVariable
    {
        public static string KEEPVERITY = "true";
        public static string KEEPFORCEENCRYPT = "true";
        public static string PATCHVBMETAFLAG = "false";
        public static string RECOVERYMODE = "false";
        public static string LEGACYSAR = "true";
    }

    public static class GlobalData
    {
        public static MainViewModel MainViewModelInstance { get; set; }
    }
    enum PatchMode
    {
        Magisk,
        apatch,
        kernelsu
    }

}