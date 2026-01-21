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
        public static PatchInfo Zipinfo = new PatchInfo("", "", false, PatchMode.None);
        public static BootInfo Bootinfo = new BootInfo("", "", "", false, false, "", "", "", "", false, false, false, "", "", "");
        public static int mb_exitcode, cpio_exitcode, load_times;
        public static string SetBoot = null;
        public static bool SetResize = false;
        //分区表储存
        public static string sdatable = "";
        public static string sdbtable = "";
        public static string sdctable = "";
        public static string sddtable = "";
        public static string sdetable = "";
        public static string sdftable = "";
        public static string sdgtable = "";
        public static string sdhtable = "";
        public static string emmcrom = "";
        //工具箱版本
        public static string currentVersion = "3.3.5";
        public static bool isLightThemeChanged = false;
        //主页的Dialog
        public static ISukiDialogManager MainDialogManager;
        public static ISukiToastManager MainToastManager;

        public static string BootPatchPath { get; internal set; }
        public static string MagiskAPKPath { get; internal set; }
    }
    public class BootInfo(string sha1, string path, string tempPath, bool isUseful, bool gki2, string version, string kmi, string osversion, string patchlevel, bool haveramdisk, bool havekernel, bool havedtb, string dtbname, string arch, string compress)
    {
        public string SHA1 { get; set; } = sha1;
        public string Path { get; set; } = path;
        public string TempPath { get; set; } = tempPath;
        public bool IsUseful { get; set; } = isUseful;
        public bool GKI2 { get; set; } = gki2;
        public string Version { get; set; } = version;
        public string KMI { get; set; } = kmi;
        public string OSVersion { get; set; } = osversion;
        public string PatchLevel { get; set; } = patchlevel;
        public bool HaveRamdisk { get; set; } = haveramdisk;
        public bool HaveKernel { get; set; } = havekernel;
        public bool HaveDTB { get; set; } = havedtb;
        public string DTBName { get; set; } = dtbname;
        public string Arch { get; set; } = arch;
        public string Compress { get; set; } = compress;
    }
    public class PatchInfo(string path, string tempPath, bool isUseful, PatchMode mode)
    {
        public string Path { get; set; } = path;
        public string TempPath { get; set; } = tempPath;
        public bool IsUseful { get; set; } = isUseful;
        public PatchMode Mode { get; set; } = mode;
    }
    public class EnvironmentVariable
    {
        public static bool KEEPVERITY = true;
        public static bool KEEPFORCEENCRYPT = true;
        public static bool PATCHVBMETAFLAG = false;
        public static bool RECOVERYMODE = false;
        public static bool LEGACYSAR = true;
        public static string PREINITDEVICE = "";
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