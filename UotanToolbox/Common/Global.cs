using Avalonia.Collections;
using SukiUI.Dialogs;
using SukiUI.Toasts;

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
        public static string serviceID = null;
        public static string password = null;
        public static bool root = true;
        public static AvaloniaList<string> deviceslist;
        public static string thisdevice = null;
        public static ZipInfo Zipinfo = new ZipInfo("", "", "", "", "", false, PatchMode.None, "");
        public static BootInfo Bootinfo = new BootInfo("", "", "", false, false, "", "", "", "", false, false, false, "", "", "");
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
        public static string currentVersion = "3.3.5";
        public static bool isLightThemeChanged = false;
        //主页的Dialog
        public static ISukiDialogManager MainDialogManager;
        public static ISukiToastManager MainToastManager;
    }
    public class BootInfo
    {
        public string SHA1 { get; set; }
        public string Path { get; set; }
        public string TempPath { get; set; }
        public bool IsUseful { get; set; }
        public bool GKI2 { get; set; }
        public string Version { get; set; }
        public string KMI { get; set; }
        public string OSVersion { get; set; }
        public string PatchLevel { get; set; }
        public bool HaveRamdisk { get; set; }
        public bool HaveKernel { get; set; }
        public bool HaveDTB { get; set; }
        public string DTBName { get; set; }
        public string Arch { get; set; }
        public string Compress { get; set; }
        public BootInfo(string sha1, string path, string tempPath, bool isUseful, bool gki2, string version, string kmi, string osversion, string patchlevel, bool haveramdisk, bool havekernel, bool havedtb, string dtbname, string arch, string compress)
        {
            SHA1 = sha1;
            Path = path;
            TempPath = tempPath;
            IsUseful = isUseful;
            GKI2 = gki2;
            Version = version;
            KMI = kmi;
            OSVersion = osversion;
            PatchLevel = patchlevel;
            HaveRamdisk = haveramdisk;
            HaveKernel = havekernel;
            HaveDTB = havedtb;
            DTBName = dtbname;
            Arch = arch;
            Compress = compress;
        }
    }
    public class ZipInfo
    {
        public string Path { get; set; }
        public string SHA1 { get; set; }
        public string Version { get; set; }
        public string KMI { get; set; }
        public string TempPath { get; set; }
        public bool IsUseful { get; set; }
        public PatchMode Mode { get; set; }
        public string SubSHA1 { get; set; }

        public ZipInfo(string path, string sha1, string version, string kmi, string tempPath, bool isUseful, PatchMode mode, string subSHA1)
        {
            Path = path;
            SHA1 = sha1;
            Version = version;
            KMI = kmi;
            TempPath = tempPath;
            IsUseful = isUseful;
            Mode = mode;
            SubSHA1 = subSHA1;
        }
    }
    public class EnvironmentVariable
    {
        public static bool KEEPVERITY = true;
        public static bool KEEPFORCEENCRYPT = true;
        public static bool PATCHVBMETAFLAG = false;
        public static bool RECOVERYMODE = false;
        public static bool LEGACYSAR = true;
    }

    public static class GlobalData
    {
        public static MainViewModel MainViewModelInstance { get; set; }
    }
    public enum PatchMode
    {
        Magisk,
        Apatch,
        GKI,
        LKM,
        None
    }

}