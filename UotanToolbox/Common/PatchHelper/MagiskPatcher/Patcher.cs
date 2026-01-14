using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UotanToolbox.Common;
using static MagiskPatcher.Tool;

namespace MagiskPatcher
{
    internal static partial class Patcher
    {
        public static string? MagiskZipPath;
        public static string? OrigFilePath;
        public static string? NewFilePath;
        public static string? WorkDir;
        public static string? ZipToolPath;
        public static string? MagiskbootPath;
        public static string? CsvConfPath;
        public static bool? InstFullMagsikAPP;
        public static string? ChkNewFileSize;
        public static bool? CleanupAfterComplete;
        public static string? SaveSomeOutputInfoToBat;
        public static string? CpuType;
        public static bool? Flag_KEEPVERITY;
        public static bool? Flag_KEEPFORCEENCRYPT;
        public static bool? Flag_RECOVERYMODE;
        public static bool? Flag_PATCHVBMETAFLAG;
        public static bool? Flag_LEGACYSAR;
        public static string? Flag_PREINITDEVICE;
        public static string MagiskVersion => MagiskVer;
        public static uint OriginalFileSize => OrigFileSize;
        public static uint PatchedFileSize => NewFileSize;

        //版本号
        public static string Version = "2025.12.3-1";
        //csv配置文件涉及的参数
        static string Comment = "";
        static List<string> RequiredFiles = [];
        static bool RecoveryModeSupport = false;
        static bool AutoSetRecoveryModeWhenRecoveryDtboFound = false;
        static bool TwoStageInitSupport = false;
        static bool SupportPatchVbmetaFlag = false;
        static bool SupportPreInitDevice = false;
        static bool SupportLegacySarFlag = false;
        static bool SupportPatchVendorBoot = false;

        static bool CheckRamdiskStatus_AllowMissingRamdisk = false;
        static bool AonlySARRamdiskSpecialHandling = false;
        static bool SupportSonyInitReal = false;

        static bool ReadFromConfigOrig = false;
        static bool AddPatchVbmetaFlagToConfig = true;
        static bool AddRandomSeedToConfig = false;

        static bool RmInitZygoteRcInRamdisk = false;
        static string RamdiskPatchOption = "1";
        static bool EnableSkip64FlagForOption2 = false;
        static bool EnableSkip32FlagForOption2 = false;
        static bool CompressRamdisk = false;

        static bool EditHeaderToSetSELinuxPermissive = false;

        static string DtbPatchingCondition = "none";
        static string[] SupportedDtbFiles = [];
        static bool TestDtb = false;

        static bool PatchKernel_PatchVivoDoMountCheck = false;
        static bool PatchKernel_RemoveSamsungDefexA8Variant = false;
        static bool PatchKernel_RemoveSamsungDefexN9Variant = false;
        static bool PatchKernel_RemoveSamsungDefex = false;
        static bool PatchKernel_DisableSamsungPROCA = false;
        static bool RmKernelIfUnpatched = false;
        //环境变量（可能涉及的）
        static bool KEEPVERITY;
        static bool KEEPFORCEENCRYPT;
        static bool PATCHVBMETAFLAG;
        //处理器
        static string CpuArch = "";
        static Dictionary<string, bool> CpuBitSupport = [];
        //面具版本号
        static string MagiskVer = "unknown";
        static string MagiskVerCode = "unknown";
        //程序生成的（需要清理的）文件名
        static List<string> FilesForCleanup = [];
        //boot文件大小
        static uint OrigFileSize;
        static uint NewFileSize;

        public static void Run()
        {
            Info("Magisk patch : Start");
            //处理参数和加载csv配置
            Info("Parse args and load csv config : Start");
            ArgsParser();
            Info("Parse args and load csv config : Done");
            //清理工作目录
            Cleanup();
            //准备Magisk组件
            Info("Prepare Magisk files : Start");
            PrepareMagiskFiles(CpuArch, CpuBitSupport);
            Info("Prepare Magisk files : Done");
            //设置标志（使用默认值以防止可空类型导致的异常）
            KEEPVERITY = Flag_KEEPVERITY ?? true;
            KEEPFORCEENCRYPT = Flag_KEEPFORCEENCRYPT ?? true;
            bool RECOVERYMODE = Flag_RECOVERYMODE ?? false;
            PATCHVBMETAFLAG = Flag_PATCHVBMETAFLAG ?? false;
            string PREINITDEVICE = "";
            if (SupportPreInitDevice) { PREINITDEVICE = Flag_PREINITDEVICE ?? ""; }
            bool LEGACYSAR;
            if (SupportLegacySarFlag) { LEGACYSAR = Flag_LEGACYSAR ?? false; } else { LEGACYSAR = true; }
            bool SYSTEM_ROOT = LEGACYSAR;
            bool TWOSTAGEINIT = false;
            //bool CHROMEOS = false; //暂不支持
            int STATUS = 0;
            string SHA1 = "";
            string SKIP64 = "";
            if (EnableSkip64FlagForOption2 && CpuBitSupport["64"] == false) { SKIP64 = "#"; }
            string SKIP32 = "";
            if (EnableSkip32FlagForOption2 && CpuBitSupport["32"] == false) { SKIP32 = "#"; }
            string INIT = "init";
            string SKIP_BACKUP = "";
            string RAMDISK = "";
            bool VENDORBOOT = false;
            string CompressRamdiskBackups = "";
            //检查必需文件
            Info("Check files : Start");
            for (int i = RequiredFiles.Count - 1; i >= 0; i--)
            {
                string fileName = RequiredFiles[i];
                if (fileName.Contains("{CpuBit}"))
                {
                    RequiredFiles.RemoveAt(i);  // 先移除当前项
                    if (SKIP64 == "") { RequiredFiles.Add(fileName.Replace("{CpuBit}", "64")); }
                    if (SKIP32 == "") { RequiredFiles.Add(fileName.Replace("{CpuBit}", "32")); }
                }
            }
            foreach (string fileName in RequiredFiles)
            {
                if (!File.Exists($@"{WorkDir}\{fileName}")) { Error($"File not exist: {fileName}"); }
            }
            if (!string.IsNullOrEmpty(NewFilePath) && File.Exists(NewFilePath)) { File.Delete(NewFilePath); }
            if (!string.IsNullOrEmpty(SaveSomeOutputInfoToBat) && File.Exists(SaveSomeOutputInfoToBat)) { File.Delete(SaveSomeOutputInfoToBat); }
            Info("Check files : Done");
            //解包原boot
            Info("Unpack boot : Start");
            int unpackReturnCode = MagiskBoot($"unpack -h \"{OrigFilePath}\"");
            switch (unpackReturnCode)
            {
                case 0:
                    Info("Unpack boot : Info : Normal boot image detected");
                    break;
                case 2:
                    Info("Unpack boot : Info : ChromeOS boot image detected");
                    Error("Unpack boot : Error : ChromeOS is currently not supported");
                    break;
                case 3 when SupportPatchVendorBoot:
                    Info("Unpack boot : Info : Vendor boot image detected");
                    VENDORBOOT = true;
                    break;
                default:
                    Error($"Unpack boot : Error: Unable to unpack boot image({unpackReturnCode})");
                    break;
            }
            Info("Unpack boot : Done");
            //设置 RECOVERYMODE 标志
            if (RecoveryModeSupport && AutoSetRecoveryModeWhenRecoveryDtboFound && File.Exists($@"{WorkDir}\recovery_dtbo"))
            {
                RECOVERYMODE = true;
                Info($"Set RECOVERYMODE flag : Done : {RECOVERYMODE}");
            }
            //查找ramdisk
            Info("Find ramdisk : Start");
            if (File.Exists($@"{WorkDir}\ramdisk.cpio")) { RAMDISK = "ramdisk.cpio"; }
            if (SupportPatchVendorBoot)
            {
                if (File.Exists($@"{WorkDir}\vendor_ramdisk\init_boot.cpio")) { RAMDISK = @"vendor_ramdisk\init_boot.cpio"; goto FindRamdiskDone; }
                if (File.Exists($@"{WorkDir}\vendor_ramdisk\ramdisk.cpio")) { RAMDISK = @"vendor_ramdisk\ramdisk.cpio"; goto FindRamdiskDone; }
            }
        FindRamdiskDone:
            if (RAMDISK != "") { Info($"Find ramdisk : Done : {RAMDISK}"); } else { Info($"Find ramdisk : Done : No ramdisk found"); }
            //检查ramdisk状态
            Info("Check ramdisk status : Start");
            if (RAMDISK != "") //ramdisk存在
            {
                STATUS = MagiskBoot($@"cpio {RAMDISK} test");
                SKIP_BACKUP = "";
            }
            else //ramdisk不存在
            {
                if (CheckRamdiskStatus_AllowMissingRamdisk)
                {
                    Info("Check ramdisk status : Info : No ramdisk found, will create one from scratch. Could be stock A only legacy SAR, or some Android 13 GKIs");
                    RAMDISK = "ramdisk.cpio";
                    STATUS = 0;
                    SKIP_BACKUP = "#";
                }
                else
                {
                    Error("Check ramdisk status : Error : No ramdisk found");
                }
            }
            switch (STATUS)
            {
                case 0:
                    Info("Check ramdisk status : Done: Stock boot image detected");
                    break;
                case 1:
                    Info("Check ramdisk status : Done: Magisk patched boot image detected");
                    break;
                default:
                    Error("Check ramdisk status : Error: Boot image patched by unsupported programs. Please restore back to stock boot image");
                    break;
            }
            //计算SHA1
            Info($"Get stock boot sha1 : Start");
            SHA1 = "";
            if (STATUS == 0)
            {
                Info("Get stock boot sha1 : Info : From stock boot");
                MagiskBoot($"sha1 \"{OrigFilePath}\"");
                SHA1 = Tool.GetLineFromString(Tool.RemoveEmptyLines(CmdOutput), 1) ?? "";
            }
            if (STATUS == 1)
            {
                if (ReadFromConfigOrig)
                {
                    MagiskBoot($@"extract .backup/.magisk config.orig");
                    if (File.Exists($@"{WorkDir}\config.orig"))
                    {
                        Info("Get stock boot sha1 : Info : From config.orig");
                        SHA1 = Tool.GetStrFromText(File.ReadAllText($@"{WorkDir}\config.orig"), "SHA1=", '=', 2);
                        FilesForCleanup.AddRange(new string[] { "config.orig" });
                    }
                }
                else
                {
                    Info($"Get stock boot sha1 : Info : From {RAMDISK}");
                    MagiskBoot($@"cpio {RAMDISK} sha1");
                    SHA1 = Tool.GetLineFromString(Tool.RemoveEmptyLines(CmdOutput), 1) ?? "";
                }
            }
            Info($"Get stock boot sha1 : Done : {SHA1}");
            //若STATUS=1且ReadFromConfigOrig，则从config.orig读取PREINITDEVICE
            if (STATUS == 1 && ReadFromConfigOrig)
            {
                MagiskBoot($@"extract .backup/.magisk config.orig");
                if (File.Exists($@"{WorkDir}\config.orig"))
                {
                    PREINITDEVICE = Tool.GetStrFromText(File.ReadAllText($@"{WorkDir}\config.orig"), "PREINITDEVICE=", '=', 2);
                    Info($"Read PREINITDEVICE from config.orig : Done : {PREINITDEVICE}");
                    FilesForCleanup.AddRange(new string[] { "config.orig" });
                }
            }
            //若STATUS=1，则还原ramdisk
            if (STATUS == 1)
            {
                Info("Restore ramdisk : Start");
                if (MagiskBoot($@"cpio {RAMDISK} restore") == 0)
                {
                    Info("Restore ramdisk : Done");
                }
                else
                {
                    Error("Restore ramdisk : Error : Failed");
                }
            }
            //备份ramdisk
            if (File.Exists($@"{WorkDir!}\{RAMDISK}"))
            {
                Info("Backup ramdisk : Start");
                File.Copy($@"{WorkDir!}\{RAMDISK}", $@"{WorkDir!}\ramdisk.cpio.orig", overwrite: true);
                FilesForCleanup.AddRange(new string[] { "ramdisk.cpio.orig" });
                Info("Backup ramdisk : Done");
            }
            //针对AonlySAR的ramdisk的特殊处理
            if (AonlySARRamdiskSpecialHandling && STATUS == 1)
            {
                Info("Aonly SAR ramdisk special handling : Start");
                if (MagiskBoot($"cpio {RAMDISK} \"exists init.rc\"") == 0)
                {
                    Info("Aonly SAR ramdisk special handling : Info : Normal boot image");
                }
                else
                {
                    Info($"Aonly SAR ramdisk special handling : Info : A only system-as-root. Delete {RAMDISK} and ramdisk.cpio.orig");
                    File.Delete($@"{WorkDir}\{RAMDISK}");
                    File.Delete($@"{WorkDir!}\ramdisk.cpio.orig");
                }
                Info("Aonly SAR ramdisk special handling : Done");
            }
            //设置 TWOSTAGEINIT 标志
            if (TwoStageInitSupport && (STATUS & 8) != 0)
            {
                TWOSTAGEINIT = true;
                Environment.SetEnvironmentVariable("TWOSTAGEINIT", $"{TWOSTAGEINIT}");
                Info($"Set TWOSTAGEINIT flag : Done : {TWOSTAGEINIT}");
            }
            //索尼init.real支持
            INIT = "init";
            if (SupportSonyInitReal && ((STATUS & 4) != 0))
            {
                INIT = "init.real";
            }
            Info($"Determine target init : Done : {INIT}");
            //生成config
            Info("Write config : Start");
            string configText = "";
            configText += $"KEEPVERITY={KEEPVERITY.ToString().ToLower()}\n";
            configText += $"KEEPFORCEENCRYPT={KEEPFORCEENCRYPT.ToString().ToLower()}\n";
            if (SupportPatchVbmetaFlag && AddPatchVbmetaFlagToConfig) { configText += $"PATCHVBMETAFLAG={PATCHVBMETAFLAG.ToString().ToLower()}\n"; }
            if (RecoveryModeSupport) { configText += $"RECOVERYMODE={RECOVERYMODE.ToString().ToLower()}\n"; }
            if (SupportPatchVendorBoot) { configText += $"VENDORBOOT={VENDORBOOT.ToString().ToLower()}\n"; }
            if (SupportPreInitDevice && PREINITDEVICE != "" && PREINITDEVICE != null) { configText += $"PREINITDEVICE={PREINITDEVICE.ToString()}\n"; }
            if (SHA1 != "" && SHA1 != null) { configText += $"SHA1={SHA1.ToString().ToLower()}\n"; }
            if (AddRandomSeedToConfig) { configText += $"RANDOMSEED=0x{Tool.GenerateRandomString("abcdef0123456789", 16)}\n"; }
            // library should not write to console directly; let host log it
            MagiskPatcherCore.Logger?.Debug(configText);
            WriteToFile($@"{WorkDir}\config", configText, false, false, new UTF8Encoding(false));
            FilesForCleanup.AddRange(new string[] { "config" });
            Info("Write config : Done");
            //删除ramdisk中的init.zygote*.rc
            if (RmInitZygoteRcInRamdisk && File.Exists($@"{WorkDir}\{RAMDISK}"))
            {
                Info($"Delete init.zygote*.rc in ramdisk : Start");
                if (MagiskBoot($"cpio {RAMDISK} \"rm init.zygote32.rc\" \"rm init.zygote64_32.rc\"") == 0)
                {
                    Info($"Delete init.zygote*.rc in ramdisk : Done");
                }
                else
                {
                    Info($"Delete init.zygote*.rc in ramdisk : Info : Failed");
                }
            }
            //确定修补ramdisk时是否压缩ramdisk\.backup内的部分文件（如init压缩为init.xz）
            //4372656174652072616D6469736B206261636B7570732066726F6D204F524947 : Create ramdisk backups from ORIG
            //4372656174652072616D6469736B206261636B7570732066726F6D204F5249472C2073706563696679205B2D6E5D20746F20736B697020636F6D7072657373696F6E : Create ramdisk backups from ORIG, specify [-n] to skip compression
            Info($"Check if magiskboot compresses ramdisk backups : Start");
            if (ContainsHexPattern($@"{WorkDir}\magiskboot", "4372656174652072616D6469736B206261636B7570732066726F6D204F524947"))
            {
                if (ContainsHexPattern($@"{WorkDir}\magiskboot", "4372656174652072616D6469736B206261636B7570732066726F6D204F5249472C2073706563696679205B2D6E5D20746F20736B697020636F6D7072657373696F6E"))
                {
                    CompressRamdiskBackups = ""; //压缩
                    Info($"Check if magiskboot compresses ramdisk backups : Done : True. Compression enabled");
                }
                else
                {
                    CompressRamdiskBackups = "-n"; //不压缩
                    Info($"Check if magiskboot compresses ramdisk backups : Done : False. Compression disabled");
                }
            }
            else
            {
                CompressRamdiskBackups = ""; //压缩
                Info($"Check if magiskboot compresses ramdisk backups : Done : Unknown. Compression enabled by default");
            }
            //修补ramdisk(若ramdisk不存在则生成一个)
            Info($"Patch ramdisk : Start : Option {RamdiskPatchOption}");
            string SKIP_APK = "";
            if (!File.Exists($@"{WorkDir}\stub.xz")) { SKIP_APK = "#"; }
            string command = "";
            if (RamdiskPatchOption == "1")
            {
                command = $"cpio {RAMDISK} \"add 0750 {INIT} magiskinit\"                                                                                                                                                                                                                                                                                                                                                                                                             \"patch\" \"{SKIP_BACKUP} backup ramdisk.cpio.orig {CompressRamdiskBackups}\" \"mkdir 000 .backup\" \"add 000 .backup/.magisk config\"";
            }
            else if (RamdiskPatchOption == "2")
            {
                command = $"cpio {RAMDISK} \"add 0750 {INIT} magiskinit\" \"mkdir 0750 overlay.d\" \"mkdir 0750 overlay.d/sbin\" \"{SKIP32} add 0644 overlay.d/sbin/magisk32.xz magisk32.xz\" \"{SKIP64} add 0644 overlay.d/sbin/magisk64.xz magisk64.xz\" \"{SKIP_APK} add 0644 overlay.d/sbin/stub.xz stub.xz\"                                                                                                                                                                     \"patch\" \"{SKIP_BACKUP} backup ramdisk.cpio.orig {CompressRamdiskBackups}\" \"mkdir 000 .backup\" \"add 000 .backup/.magisk config\"";
            }
            else if (RamdiskPatchOption == "2-mod1")
            {
                command = $"cpio {RAMDISK} \"add 0750 {INIT} magiskinit\" \"mkdir 0750 overlay.d\" \"mkdir 0750 overlay.d/sbin\" \"{SKIP32} add 0644 overlay.d/sbin/magisk32.xz magisk32.xz\" \"{SKIP64} add 0644 overlay.d/sbin/magisk64.xz magisk64.xz\" \"{SKIP_APK} add 0644 overlay.d/sbin/stub.xz stub.xz\"                                                   \"add 0644 overlay.d/sbin/busybox.xz busybox.xz\" \"add 0644 overlay.d/sbin/util_functions.xz util_functions.xz\" \"patch\" \"{SKIP_BACKUP} backup ramdisk.cpio.orig {CompressRamdiskBackups}\" \"mkdir 000 .backup\" \"add 000 .backup/.magisk config\"";
            }
            else if (RamdiskPatchOption == "3")
            {
                command = $"cpio {RAMDISK} \"add 0750 {INIT} magiskinit\" \"mkdir 0750 overlay.d\" \"mkdir 0750 overlay.d/sbin\" \"add 0644 overlay.d/sbin/magisk.xz magisk.xz\"                                                                           \"{SKIP_APK} add 0644 overlay.d/sbin/stub.xz stub.xz\"                                                                                                                                                                     \"patch\" \"{SKIP_BACKUP} backup ramdisk.cpio.orig {CompressRamdiskBackups}\" \"mkdir 000 .backup\" \"add 000 .backup/.magisk config\"";
            }
            else if (RamdiskPatchOption == "4")
            {
                command = $"cpio {RAMDISK} \"add 0750 {INIT} magiskinit\" \"mkdir 0750 overlay.d\" \"mkdir 0750 overlay.d/sbin\" \"add 0644 overlay.d/sbin/magisk.xz magisk.xz\"                                                                           \"{SKIP_APK} add 0644 overlay.d/sbin/stub.xz stub.xz\" \"add 0644 overlay.d/sbin/init-ld.xz init-ld.xz\"                                                                                                                   \"patch\" \"{SKIP_BACKUP} backup ramdisk.cpio.orig {CompressRamdiskBackups}\" \"mkdir 000 .backup\" \"add 000 .backup/.magisk config\"";
            }
            else
            {
                Error($"Patch ramdisk : Error : Unsupported option {RamdiskPatchOption}");
            }
            if (MagiskBoot(command) == 0)
            {
                Info($"Patch ramdisk : Done");
            }
            else
            {
                Error($"Patch ramdisk : Error : Unable to patch ramdisk");
            }
            //压缩ramdisk
            if (CompressRamdisk && ((STATUS & 4) != 0))
            {
                Info($"Compress ramdisk : Start");
                if (MagiskBoot($"cpio {RAMDISK} compress") == 0)
                {
                    Info($"Compress ramdisk : Done");
                }
                else
                {
                    Error($"Compress ramdisk : Error : Failed");
                }
            }
            //设置header selinux permissive
            if (EditHeaderToSetSELinuxPermissive)
            {
                Info($"Edit header to set SELinux permissive : Start");
                if (EditHeaderToSetSELinuxPermissive_Run())
                {
                    Info($"Edit header to set SELinux permissive : Done");
                }
                else
                {
                    Error($"Edit header to set SELinux permissive : Error : Failed");
                }
            }
            //修补dtb
            if ((DtbPatchingCondition == "none") || (DtbPatchingCondition == "KEEPVERITY=false" && KEEPVERITY == false))
            {
                Info($"Patch dtb fstab : Start");
                foreach (string fileName in SupportedDtbFiles)
                {
                    if (File.Exists($@"{WorkDir}\{fileName}"))
                    {
                        if (TestDtb)
                        {
                            if (MagiskBoot($"dtb {fileName} test") == 0)
                            {
                                Info($"Patch dtb fstab : Info : Successfully tested {fileName}");
                            }
                            else
                            {
                                Error($"Patch dtb fstab : Error : Failed to test {fileName}. Boot image was patched by old (unsupported) Magisk. Please try again with *unpatched* boot image");
                            }
                        }
                        if (MagiskBoot($"dtb {fileName} patch") == 0)
                        {
                            Info($"Patch dtb fstab : Info : Successfully patched {fileName}");
                        }
                        else
                        {
                            Info($"Patch dtb fstab : Info : Failed to patch {fileName}");
                        }
                    }
                }
                Info($"Patch dtb fstab : Done");
            }
            else
            {
                Info($"Patch dtb fstab : Skipped");
            }
            //修补kernel
            if (File.Exists($@"{WorkDir}\kernel"))
            {
                bool PATCHEDKERNEL = false;
                Info("Patch kernel : Start");
                //修补kernel-修补vivo do_mount_check
                if (PatchKernel_PatchVivoDoMountCheck)
                {
                    if (MagiskBoot($"hexpatch kernel 0092CFC2C9CDDDDA00 0092CFC2C9CEC0DB00") == 0)
                    {
                        PATCHEDKERNEL = true;
                        Info("Patch kernel : Info : Successfully patched vivo do_mount_check by wuxianlin");
                    }
                }
                //修补kernel-移除三星RKP
                if (MagiskBoot($"hexpatch kernel 49010054011440B93FA00F71E9000054010840B93FA00F7189000054001840B91FA00F7188010054 A1020054011440B93FA00F7140020054010840B93FA00F71E0010054001840B91FA00F7181010054") == 0)
                {
                    PATCHEDKERNEL = true;
                    Info("Patch kernel : Info : Successfully removed Samsung RKP");
                }
                //修补kernel-移除三星defex-A8_variant
                if (PatchKernel_RemoveSamsungDefexA8Variant)
                {
                    if (MagiskBoot($"hexpatch kernel 006044B91F040071802F005460DE41F9 006044B91F00006B802F005460DE41F9") == 0)
                    {
                        PATCHEDKERNEL = true;
                        Info("Patch kernel : Info : Successfully removed Samsung defex A8 variant");
                    }
                }
                //修补kernel-移除三星defex-N9_variant
                if (PatchKernel_RemoveSamsungDefexN9Variant)
                {
                    if (MagiskBoot($"hexpatch kernel 603A46B91F0400710030005460C642F9 603A46B91F00006B0030005460C642F9") == 0)
                    {
                        PATCHEDKERNEL = true;
                        Info("Patch kernel : Info : Successfully removed Samsung defex N9 variant");
                    }
                }
                //修补kernel-移除三星defex
                if (PatchKernel_RemoveSamsungDefex)
                {
                    if (MagiskBoot($"hexpatch kernel 821B8012 E2FF8F12") == 0)
                    {
                        PATCHEDKERNEL = true;
                        Info("Patch kernel : Info : Successfully removed Samsung defex");
                    }
                }
                //修补kernel-禁用三星PROCA
                if (PatchKernel_DisableSamsungPROCA)
                {
                    if (MagiskBoot($"hexpatch kernel 70726F63615F636F6E66696700 70726F63615F6D616769736B00") == 0)
                    {
                        PATCHEDKERNEL = true;
                        Info("Patch kernel : Info : Successfully disable Samsung PROCA");
                    }
                }
                //修补kernel-强制开启rootfs  skip_initramfs -> want_initramfs
                if (LEGACYSAR)
                {
                    if (MagiskBoot($"hexpatch kernel 736B69705F696E697472616D667300 77616E745F696E697472616D667300") == 0)
                    {
                        PATCHEDKERNEL = true;
                        Info("Patch kernel : Info : Successfully force kernel to load rootfs (for legacy SAR devices)");
                    }
                }
                //If the kernel doesn't need to be patched at all, keep raw kernel to avoid bootloops on some weird devices
                if (RmKernelIfUnpatched && !PATCHEDKERNEL)
                {
                    Info("Patch kernel : Info : Kernel is unpatched. Delete kernel");
                    File.Delete($@"{WorkDir}\kernel");
                }
                Info("Patch kernel : Done");
            }
            //打包boot
            Info($"Repack boot : Start");
            if (MagiskBoot($"repack \"{OrigFilePath!}\" \"{NewFilePath!}\"") != 0)
            {
                Error("Repack boot : Error : Unable to repack boot image");
            }
            Info($"Repack boot : Done");
            //检查boot大小
            NewFileSize = (uint)new FileInfo(NewFilePath!).Length;
            Info($"[NewFileSize]{NewFileSize}");
            if (!String.IsNullOrEmpty(ChkNewFileSize))
            {
                Info($"Check new file size : Start");
                if (NewFileSize > uint.Parse(ChkNewFileSize)) { Error($"Check new file size : Error : The new file size {NewFileSize} exceeds the specified maximum {ChkNewFileSize}."); }
                Info($"Check new file size : Done");
            }
            //打印完成信息
            PrintDone();
            Info($"New boot : {NewFilePath}");
            //保存一些输出信息到bat
            if (!string.IsNullOrEmpty(SaveSomeOutputInfoToBat))
            {
                Info($"Save some output info to bat : {SaveSomeOutputInfoToBat}");
                WriteToFile(SaveSomeOutputInfoToBat!, $"" +
                    $"set \"MagiskPatcher_MagiskVer={MagiskVer}\"\r\n" +
                    $"set \"MagiskPatcher_MagiskVerCode={MagiskVerCode}\"\r\n" +
                    $"set \"MagiskPatcher_OrigFilePath={OrigFilePath}\"\r\n" +
                    $"set \"MagiskPatcher_OrigFileSize={OrigFileSize}\"\r\n" +
                    $"set \"MagiskPatcher_NewFilePath={NewFilePath}\"\r\n" +
                    $"set \"MagiskPatcher_NewFileSize={NewFileSize}\"\r\n"
                    , false, true, Encoding.GetEncoding("GB2312"));
            }
            //清理
            if (CleanupAfterComplete == true) { Cleanup(); }
            Info("Magisk patch : Done");
        }


        //参数处理
        static void ArgsParser()
        {
            //原文件路径【必需】
            if (string.IsNullOrEmpty(OrigFilePath) || !File.Exists(OrigFilePath)) { Error($"File not found : {OrigFilePath}"); }
            OrigFilePath = Path.GetFullPath(OrigFilePath!);
            Info($"[OrigFilePath]{OrigFilePath}");
            //原文件大小
            OrigFileSize = (uint)new FileInfo(OrigFilePath).Length;
            Info($"[OrigFileSize]{OrigFileSize}");
            //工作目录
            if (string.IsNullOrEmpty(WorkDir))
            {
                WorkDir = Environment.CurrentDirectory;
            }
            else
            {
                WorkDir = Path.GetFullPath(WorkDir!);
                if (WorkDir.EndsWith("\\")) { WorkDir = WorkDir.TrimEnd('\\'); }
                if (!Directory.Exists(WorkDir)) { Error($"Directory not found : {WorkDir}"); }
            }
            Info($"[WorkDir]{WorkDir}");
            //CSV路径
            if (string.IsNullOrEmpty(CsvConfPath))
            {
                // 优先查找可执行文件同级目录的配置文件
                var exePath = Path.Combine(AppContext.BaseDirectory, "MagiskPatcher.csv");
                if (File.Exists(exePath))
                {
                    CsvConfPath = exePath;
                }
                else
                {
                    // 回退到当前工作目录
                    CsvConfPath = Path.Combine(Environment.CurrentDirectory, "MagiskPatcher.csv");
                }
            }
            CsvConfPath = Path.GetFullPath(CsvConfPath!);
            if (!File.Exists(CsvConfPath)) { Error($"File not found : {CsvConfPath}"); }
            Info($"[CsvConfPath]{CsvConfPath}");
            //开机时安装完整MagiskAPP
            if (InstFullMagsikAPP == null) { InstFullMagsikAPP = false; }
            Info($"[InstFullMagsikAPP]{InstFullMagsikAPP}");
            //检查新文件大小
            if (!String.IsNullOrEmpty(ChkNewFileSize))
            {
                if (ChkNewFileSize.ToLower() == "{origsize}") { ChkNewFileSize = OrigFileSize.ToString(); }
                if (uint.TryParse(ChkNewFileSize, out uint result))
                {
                    ChkNewFileSize = result.ToString();
                }
                else
                {
                    Error($"Parameter {ChkNewFileSize} is not a uint.");
                }
            }
            Info($"[ChkNewFileSize]{ChkNewFileSize}");
            //处理器
            if (string.IsNullOrEmpty(CpuType))
            {
                CpuType = "arm_64";
            }
            if (CpuType == "arm_64")
            {
                CpuArch = "arm";
                CpuBitSupport.Add("64", true);
                CpuBitSupport.Add("32", true);
            }
            else if (CpuType == "arm_32")
            {
                CpuArch = "arm";
                CpuBitSupport.Add("64", false);
                CpuBitSupport.Add("32", true);
            }
            else if (CpuType == "x86_64")
            {
                CpuArch = "x86";
                CpuBitSupport.Add("64", true);
                CpuBitSupport.Add("32", true);
            }
            else if (CpuType == "x86_32")
            {
                CpuArch = "x86";
                CpuBitSupport.Add("64", false);
                CpuBitSupport.Add("32", true);
            }
            else if (CpuType == "riscv_64")
            {
                CpuArch = "riscv";
                CpuBitSupport.Add("64", true);
                CpuBitSupport.Add("32", true);
            }
            else
            {
                Error($"Unsupported CPU type: {CpuType}");
            }
            Info($"[CpuType]{CpuType}");
            //修补选项
            if (Flag_KEEPVERITY == null) { Flag_KEEPVERITY = true; }
            if (Flag_KEEPFORCEENCRYPT == null) { Flag_KEEPFORCEENCRYPT = true; }
            if (Flag_RECOVERYMODE == null) { Flag_RECOVERYMODE = false; }
            if (Flag_PATCHVBMETAFLAG == null) { Flag_PATCHVBMETAFLAG = false; }
            if (Flag_LEGACYSAR == null) { Flag_LEGACYSAR = false; }
            Info($"[Flag_KEEPVERITY]{Flag_KEEPVERITY}");
            Info($"[Flag_KEEPFORCEENCRYPT]{Flag_KEEPFORCEENCRYPT}");
            Info($"[Flag_RECOVERYMODE]{Flag_RECOVERYMODE}");
            Info($"[Flag_PATCHVBMETAFLAG]{Flag_PATCHVBMETAFLAG}");
            Info($"[Flag_LEGACYSAR]{Flag_LEGACYSAR}");
            Info($"[Flag_PREINITDEVICE]{Flag_PREINITDEVICE}");
            //结束后清理
            if (CleanupAfterComplete == null) { CleanupAfterComplete = true; }
            Info($"[CleanupAfterComplete]{CleanupAfterComplete}");
            //保存一些输出信息到bat
            if (string.IsNullOrEmpty(SaveSomeOutputInfoToBat))
            {
                SaveSomeOutputInfoToBat = "";
            }
            else
            {
                SaveSomeOutputInfoToBat = Path.GetFullPath(SaveSomeOutputInfoToBat);
                if (!Directory.Exists(Path.GetDirectoryName(SaveSomeOutputInfoToBat))) { Error($"Directory not found : {Path.GetDirectoryName(SaveSomeOutputInfoToBat)}"); }
            }
            Info($"[SaveSomeOutputInfoToBat]{SaveSomeOutputInfoToBat}");
            //解压util_functions.sh和boot_patch.sh
            if (File.Exists($@"{WorkDir}\boot_patch.sh")) { File.Delete($@"{WorkDir}\boot_patch.sh"); }
            if (File.Exists($@"{WorkDir}\util_functions.sh")) { File.Delete($@"{WorkDir}\util_functions.sh"); }
            ZipTool($@"e -aoa -o.\ -slp -y -ir!assets\util_functions.sh -ir!common\util_functions.sh -ir!assets\boot_patch.sh -ir!common\boot_patch.sh " + $"\"{MagiskZipPath}\"");
            if (!File.Exists($@"{WorkDir}\boot_patch.sh")) { Error($"Failed to extract boot_patch.sh from {MagiskZipPath}"); }
            if (!File.Exists($@"{WorkDir}\util_functions.sh")) { Error($"Failed to extract util_functions.sh from {MagiskZipPath}"); }
            FilesForCleanup.AddRange(new string[] { "util_functions.sh", "boot_patch.sh" });
            //读取修补脚本md5，确定所需步骤
            Info("Load csv config : Start");
            if (LoadCsvConf(CsvConfPath, CalculateFileMD5($@"{WorkDir}\boot_patch.sh")))
            {
                Info($"Load csv config : Info :{Comment}");
                Info("Load csv config : Done");
            }
            else
            {
                Error("Load csv config : Error : This version of Magisk is currently not supported. Please feedback to developer");
            }
            //读取面具版本号
            Info("Read Magisk version : Start");
            MagiskVer = GetStrFromText(File.ReadAllText($@"{WorkDir}\util_functions.sh"), "MAGISK_VER=", '\'', 2);
            MagiskVerCode = GetStrFromText(File.ReadAllText($@"{WorkDir}\util_functions.sh"), "MAGISK_VER_CODE=", '=', 2);
            Info($"MagiskVer : {MagiskVer}");
            Info($"MagiskVerCode : {MagiskVerCode}");
            Info("Read Magisk version : Done");
            //新文件路径（对于自动默认的新文件名，此处仅初步命名，在打包boot步骤扩展为正式命名）
            if (string.IsNullOrEmpty(NewFilePath))
            {
                NewFilePath = $@"{Path.GetDirectoryName(OrigFilePath)}\{Path.GetFileNameWithoutExtension(OrigFilePath)}_MagiskPatched_{MagiskVer}_{MagiskVerCode}_{DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss.fff")}{Path.GetExtension(OrigFilePath)}";
            }
            else
            {
                NewFilePath = Path.GetFullPath(NewFilePath);
                if (!Directory.Exists(Path.GetDirectoryName(NewFilePath))) { Error($"Directory not found : {Path.GetDirectoryName(NewFilePath)}"); }
            }
            Info($"[NewFilePath]{NewFilePath}");
        }


        //加载csv配置文件
        static bool LoadCsvConf(string filePath, string md5)
        {
            //读取整个filePath
            string csvText = File.ReadAllText(filePath);
            //获取含有[patchShMd5]的行
            string optionLine = "";
            var optionLine_MatchingLines = csvText.Split('\r', '\n')
                           .Where(line => line.Contains("[patchShMd5]"));
            foreach (var line in optionLine_MatchingLines)
            {
                optionLine = line;
            }
            //按,[]分割
            char[] separators = [',', '[', ']'];
            string[] options = optionLine.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            //创建字典，绑定索引和值
            Dictionary<string, int> optionDict = [];
            for (int i = 0; i < options.Length; i++)
            {
                optionDict.Add(options[i], i);
            }
            //获取含有[md5]的行
            string md5Line = "";
            var md5Line_MatchingLines = csvText.Split('\r', '\n')
                           .Where(line => line.Contains($"[{md5}]"));
            foreach (var line in md5Line_MatchingLines)
            {
                md5Line = line;
            }
            if (string.IsNullOrEmpty(md5Line)) { return false; } //找不到目标MD5，不支持该版本
            //按,分割
            string[] values = md5Line.Split(',');
            //赋值
            Comment = values[optionDict["comment"]];
            RequiredFiles = values[optionDict["RequiredFiles"]].Split(';').ToList();
            RecoveryModeSupport = bool.Parse(values[optionDict["RecoveryModeSupport"]]);
            AutoSetRecoveryModeWhenRecoveryDtboFound = bool.Parse(values[optionDict["AutoSetRecoveryModeWhenRecoveryDtboFound"]]);
            TwoStageInitSupport = bool.Parse(values[optionDict["TwoStageInitSupport"]]);
            SupportPatchVbmetaFlag = bool.Parse(values[optionDict["SupportPatchVbmetaFlag"]]);
            SupportPreInitDevice = bool.Parse(values[optionDict["SupportPreInitDevice"]]);
            SupportLegacySarFlag = bool.Parse(values[optionDict["SupportLegacySarFlag"]]);
            SupportPatchVendorBoot = bool.Parse(values[optionDict["SupportPatchVendorBoot"]]);
            CheckRamdiskStatus_AllowMissingRamdisk = bool.Parse(values[optionDict["CheckRamdiskStatus_AllowMissingRamdisk"]]);
            AonlySARRamdiskSpecialHandling = bool.Parse(values[optionDict["AonlySARRamdiskSpecialHandling"]]);
            SupportSonyInitReal = bool.Parse(values[optionDict["SupportSonyInitReal"]]);
            ReadFromConfigOrig = bool.Parse(values[optionDict["ReadFromConfigOrig"]]);
            AddPatchVbmetaFlagToConfig = bool.Parse(values[optionDict["AddPatchVbmetaFlagToConfig"]]);
            AddRandomSeedToConfig = bool.Parse(values[optionDict["AddRandomSeedToConfig"]]);
            RmInitZygoteRcInRamdisk = bool.Parse(values[optionDict["RmInitZygoteRcInRamdisk"]]);
            RamdiskPatchOption = values[optionDict["RamdiskPatchOption"]];
            EnableSkip64FlagForOption2 = bool.Parse(values[optionDict["EnableSkip64FlagForOption2"]]);
            EnableSkip32FlagForOption2 = bool.Parse(values[optionDict["EnableSkip32FlagForOption2"]]);
            CompressRamdisk = bool.Parse(values[optionDict["CompressRamdisk"]]);
            EditHeaderToSetSELinuxPermissive = bool.Parse(values[optionDict["EditHeaderToSetSELinuxPermissive"]]);
            DtbPatchingCondition = values[optionDict["DtbPatchingCondition"]];
            SupportedDtbFiles = values[optionDict["SupportedDtbFiles"]].Split(';');
            TestDtb = bool.Parse(values[optionDict["TestDtb"]]);
            PatchKernel_PatchVivoDoMountCheck = bool.Parse(values[optionDict["PatchKernel_PatchVivoDoMountCheck"]]);
            PatchKernel_RemoveSamsungDefexA8Variant = bool.Parse(values[optionDict["PatchKernel_RemoveSamsungDefexA8Variant"]]);
            PatchKernel_RemoveSamsungDefexN9Variant = bool.Parse(values[optionDict["PatchKernel_RemoveSamsungDefexN9Variant"]]);
            PatchKernel_RemoveSamsungDefex = bool.Parse(values[optionDict["PatchKernel_RemoveSamsungDefex"]]);
            PatchKernel_DisableSamsungPROCA = bool.Parse(values[optionDict["PatchKernel_DisableSamsungPROCA"]]);
            RmKernelIfUnpatched = bool.Parse(values[optionDict["RmKernelIfUnpatched"]]);
            return true;
        }


        //清理
        static void Cleanup()
        {
            Info("Cleanup working directory : Start");
            if (MagiskBoot($@"cleanup") != 0) { Error("Cleanup working directory : Error : Failed"); }
            foreach (var fileName in FilesForCleanup)
            {
                if (File.Exists($@"{WorkDir}\{fileName}")) { File.Delete($@"{WorkDir}\{fileName}"); }
            }
            Info("Cleanup working directory : Done");
        }


        //调用magiskboot
        static int MagiskBoot(string arguments)
        {
            (string output, int returnCode) = CallExternalProgram.MagiskBoot(arguments, WorkDir).GetAwaiter().GetResult();
            return returnCode;
        }


        //调用7z
        static int ZipTool(string arguments)
        {
            int exitCode = RunCommand(WorkDir!, ZipToolPath!, arguments);
            MagiskPatcherCore.Logger?.Debug(CmdOutput);
            return exitCode;
        }


        //准备Magisk组件
        static void PrepareMagiskFiles(string CpuArch, Dictionary<string, bool> CpuBitSupport)
        {
            if (CpuArch == "arm" && CpuBitSupport["64"])
            {
                ZipTool($@"e -aoa -o.\ -slp -y -ir!lib\armeabi-v7a\libmagiskinit.so                                  -ir!lib\armeabi-v7a\libmagisk32.so -ir!lib\armeabi-v7a\libmagisk64.so -ir!arm\magiskinit                                     -ir!lib\armeabi-v7a\libbusybox.so -ir!lib\armeabi-v7a\libmagiskboot.so -ir!arm\magiskboot " + $"\"{MagiskZipPath}\"");
                ZipTool($@"e -aoa -o.\ -slp -y -ir!lib\arm64-v8a\libmagiskinit.so   -ir!lib\arm64-v8a\libmagisk.so                                      -ir!lib\arm64-v8a\libmagisk64.so   -ir!arm\magiskinit64 -ir!lib\arm64-v8a\libinit-ld.so   -ir!lib\arm64-v8a\libbusybox.so   -ir!lib\arm64-v8a\libmagiskboot.so                      " + $"\"{MagiskZipPath}\"");
            }
            if (CpuArch == "arm" && CpuBitSupport["32"] && !CpuBitSupport["64"])
            {
                ZipTool($@"e -aoa -o.\ -slp -y -ir!lib\armeabi-v7a\libmagiskinit.so -ir!lib\armeabi-v7a\libmagisk.so -ir!lib\armeabi-v7a\libmagisk32.so                                    -ir!arm\magiskinit   -ir!lib\armeabi-v7a\libinit-ld.so -ir!lib\armeabi-v7a\libbusybox.so -ir!lib\armeabi-v7a\libmagiskboot.so -ir!arm\magiskboot " + $"\"{MagiskZipPath}\"");
            }
            if (CpuArch == "x86" && CpuBitSupport["64"])
            {
                ZipTool($@"e -aoa -o.\ -slp -y -ir!lib\x86\libmagiskinit.so                                          -ir!lib\x86\libmagisk32.so         -ir!lib\x86\libmagisk64.so         -ir!x86\magiskinit                                     -ir!lib\x86\libbusybox.so         -ir!lib\x86\libmagiskboot.so         -ir!x86\magiskboot " + $"\"{MagiskZipPath}\"");
                ZipTool($@"e -aoa -o.\ -slp -y -ir!lib\x86_64\libmagiskinit.so      -ir!lib\x86_64\libmagisk.so                                         -ir!lib\x86_64\libmagisk64.so      -ir!x86\magiskinit64 -ir!lib\x86_64\libinit-ld.so      -ir!lib\x86_64\libbusybox.so      -ir!lib\x86_64\libmagiskboot.so                         " + $"\"{MagiskZipPath}\"");
            }
            if (CpuArch == "x86" && CpuBitSupport["32"] && !CpuBitSupport["64"])
            {
                ZipTool($@"e -aoa -o.\ -slp -y -ir!lib\x86\libmagiskinit.so         -ir!lib\x86\libmagisk.so         -ir!lib\x86\libmagisk32.so                                            -ir!x86\magiskinit   -ir!lib\x86\libinit-ld.so         -ir!lib\x86\libbusybox.so         -ir!lib\x86\libmagiskboot.so         -ir!x86\magiskboot " + $"\"{MagiskZipPath}\"");
            }
            if (CpuArch == "riscv" && CpuBitSupport["64"])
            {
                ZipTool($@"e -aoa -o.\ -slp -y -ir!lib\riscv64\libmagiskinit.so     -ir!lib\riscv64\libmagisk.so                                                                                                -ir!lib\riscv64\libinit-ld.so     -ir!lib\riscv64\libbusybox.so     -ir!lib\riscv64\libmagiskboot.so                        " + $"\"{MagiskZipPath}\"");
            }
            ZipTool($@"e -aoa -o.\ -slp -y -ir!assets\stub.apk -ir!assets\util_functions.sh -ir!common\util_functions.sh " + $"\"{MagiskZipPath}\"");
            if (File.Exists($@"{WorkDir}\magiskinit64"))
            {
                if (File.Exists($@"{WorkDir}\magiskinit")) { File.Delete($@"{WorkDir}\magiskinit"); }
                File.Move($@"{WorkDir}\magiskinit64", $@"{WorkDir}\magiskinit");
            }
            if (File.Exists($@"{WorkDir}\libmagiskinit.so"))
            {
                if (File.Exists($@"{WorkDir}\magiskinit")) { File.Delete($@"{WorkDir}\magiskinit"); }
                File.Move($@"{WorkDir}\libmagiskinit.so", $@"{WorkDir}\magiskinit");
            }
            if (File.Exists($@"{WorkDir}\libmagiskboot.so"))
            {
                if (File.Exists($@"{WorkDir}\magiskboot")) { File.Delete($@"{WorkDir}\magiskboot"); }
                File.Move($@"{WorkDir}\libmagiskboot.so", $@"{WorkDir}\magiskboot");
            }
            if (File.Exists($@"{WorkDir}\libmagisk.so"))
            {
                MagiskBoot($@"compress=xz libmagisk.so magisk.xz");
            }
            if (File.Exists($@"{WorkDir}\libmagisk32.so"))
            {
                MagiskBoot($@"compress=xz libmagisk32.so magisk32.xz");
            }
            if (File.Exists($@"{WorkDir}\libmagisk64.so"))
            {
                MagiskBoot($@"compress=xz libmagisk64.so magisk64.xz");
            }
            if (RequiredFiles.Contains("stub.xz"))
            {
                if (InstFullMagsikAPP == true) { File.Copy(MagiskZipPath!, $@"{WorkDir!}\stub.apk", true); }
                if (File.Exists($@"{WorkDir!}\stub.apk")) { MagiskBoot($@"compress=xz stub.apk stub.xz"); }
            }
            if (File.Exists($@"{WorkDir}\libinit-ld.so"))
            {
                MagiskBoot($@"compress=xz libinit-ld.so init-ld.xz");
            }
            if (RequiredFiles.Contains("busybox.xz") && File.Exists($@"{WorkDir}\libbusybox.so")) //Kitsune-27005-a497a13b-mod
            {
                if (File.Exists($@"{WorkDir}\busybox")) { File.Delete($@"{WorkDir}\busybox"); }
                File.Move($@"{WorkDir}\libbusybox.so", $@"{WorkDir}\busybox");
                MagiskBoot($@"compress=xz busybox busybox.xz");
                FilesForCleanup.AddRange(new string[] { "busybox", "busybox.xz" });
            }
            if (RequiredFiles.Contains("util_functions.xz") && File.Exists($@"{WorkDir}\util_functions.sh")) //Kitsune-27005-a497a13b-mod
            {
                MagiskBoot($@"compress=xz util_functions.sh util_functions.xz");
                FilesForCleanup.AddRange(new string[] { "util_functions.sh", "util_functions.xz" });
            }
            FilesForCleanup.AddRange(new string[] { "libmagiskinit.so", "libmagisk.so", "magisk.xz", "libmagisk32.so", "magisk32.xz", "libmagisk64.so", "magisk64.xz", "magiskinit", "magiskinit64", "libinit-ld.so", "init-ld.xz", "stub.apk", "stub.xz", "libbusybox.so", "libmagiskboot.so", "magiskboot" });
        }


        //编辑header设置selinux为permissive
        private static bool EditHeaderToSetSELinuxPermissive_Run()
        {
            string filePath = $@"{WorkDir}\header";

            try
            {
                // 读取文件内容
                string[] lines = File.ReadAllLines(filePath);
                bool modified = false;

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("cmdline="))
                    {
                        // 执行所有替换操作
                        string processed = lines[i];

                        // 移除所有androidboot.selinux=...模式
                        processed = Regex.Replace(processed, @"androidboot\.selinux=[^ ]*", "");

                        // 将多个空格替换为单个空格
                        processed = Regex.Replace(processed, @" +", " ");

                        // 移除行首和行尾空格
                        processed = processed.Trim();

                        // 添加androidboot.selinux=permissive
                        processed += " androidboot.selinux=permissive";

                        lines[i] = processed;
                        modified = true;
                    }
                }

                if (modified)
                {
                    // 写回文件
                    File.WriteAllLines(filePath, lines);
                    MagiskPatcherCore.Logger?.Info("+ Successfully set SELinux to permissive");
                    return true;
                }
                else
                {
                    MagiskPatcherCore.Logger?.Warn("- No line starting with 'cmdline=' was found");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MagiskPatcherCore.Logger?.Warn($"- Failed to set SELinux to permissive: {ex.Message}");
                return false;
            }
        }


        //打印完成信息
        private static void PrintDone()
        {
            var ascii = $@"
  /######  /## /##                      
 /##__  ##| ##| ##                      
| ##  \ ##| ##| ##                      
| ########| ##| ##    Magisk Patcher    
| ##__  ##| ##| ##    {Version}
| ##  | ##| ##| ##                      
| ##  | ##| ##| ##    酷安@某贼         
|__/  |__/|__/|__/    xda@SYXZ          

       /##                              
      | ##                              
  /#######  /######  /#######   /###### 
 /##__  ## /##__  ##| ##__  ## /##__  ##
| ##  | ##| ##  \ ##| ##  \ ##| ########
| ##  | ##| ##  | ##| ##  | ##| ##_____/
|  #######|  ######/| ##  | ##|  #######
 \_______/ \______/ |__/  |__/ \_______/
";
            MagiskPatcherCore.Logger?.Info(ascii);
        }


        private static void Error(string info)
        {
            var message = $"[Error]{info}";
            MagiskPatcherCore.Logger?.Error(message);
            throw new InvalidOperationException(message);
        }
        private static void Info(string info)
        {
            MagiskPatcherCore.Logger?.Info($"[Info]{info}");
        }
    }
}
