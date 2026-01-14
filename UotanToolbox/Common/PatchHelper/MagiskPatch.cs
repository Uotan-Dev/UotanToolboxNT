using MagiskPatcher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace UotanToolbox.Common.PatchHelper
{

    internal class MagiskPatch
    {
        private static string GetTranslation(string key)
        {
            return FeaturesHelper.GetTranslation(key);
        }

        public static async Task<string> Magisk_Patch_Mouzei(PatchInfo zipInfo, BootInfo bootInfo)
        {
            ArgumentNullException.ThrowIfNull(zipInfo);
            ArgumentNullException.ThrowIfNull(bootInfo);

            if (!bootInfo.HaveRamdisk)
            {
                throw new Exception(GetTranslation("Basicflash_BootWrong"));
            }

            string allowedChars = "abcdef0123456789";
            Random random = new Random();
            string randomStr = new string([.. Enumerable.Repeat(allowedChars, 16).Select(s => s[random.Next(s.Length)])]);
            string csvConfPath = Global.BootPatchPath;
            string cpuType = "";
            Dictionary<string, string> ArchMappings = new Dictionary<string, string>
            {
                {"aarch64", "arm_64"},
                {"X86-64", "x86_64"},
                {"armeabi", "arm_32"},
                {"X86", "x86_32"}
                //理论上说，可以兼容RV
            };

            foreach (KeyValuePair<string, string> entry in ArchMappings)
            {
                if (bootInfo.Arch.Contains(entry.Key))
                {
                    cpuType = entry.Value;
                }
            }

            var config = new PatcherConfig
            {
                MagiskZipPath = zipInfo.Path,
                OrigFilePath = bootInfo.Path,
                WorkDir = bootInfo.TempPath,
                ZipToolPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                            ? Path.Combine(Global.bin_path, "7z", "7za.exe")
                            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) | (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                            ? Path.Combine(Global.bin_path, "7zza")
                            : throw new PlatformNotSupportedException("This function only supports Windows,macOS and Linux."),
                MagiskbootPath = Path.Combine(Global.bin_path, "magiskboot"),
                CsvConfPath = csvConfPath,
                CpuType = cpuType,
                Flag_KEEPVERITY = EnvironmentVariable.KEEPVERITY,
                Flag_KEEPFORCEENCRYPT = EnvironmentVariable.KEEPFORCEENCRYPT,
                Flag_RECOVERYMODE = EnvironmentVariable.RECOVERYMODE,
                Flag_PATCHVBMETAFLAG = EnvironmentVariable.PATCHVBMETAFLAG,
                Flag_LEGACYSAR = EnvironmentVariable.LEGACYSAR,
                Flag_PREINITDEVICE = EnvironmentVariable.PREINITDEVICE,
                NewFilePath = Path.Combine(Path.GetDirectoryName(bootInfo.Path), "magisk_patched_" + randomStr + ".img"),
                CleanupAfterComplete = true
            };

            ValidationResult validation = config.Validate();
            if (!validation.IsValid)
            {
                throw new Exception(string.Join(Environment.NewLine, validation.Errors));
            }

            PatchResult patchResult = await Task.Run(() => new MagiskPatcherCore(config).Patch()).ConfigureAwait(false);

            if (!patchResult.IsSuccess)
            {
                string fallbackMessage = $"{GetTranslation("Common_Error")}: Magisk patch failed.";
                string message = string.IsNullOrWhiteSpace(patchResult.ErrorMessage) ? fallbackMessage : patchResult.ErrorMessage!;
                if (patchResult.Exception != null)
                {
                    throw new Exception(message, patchResult.Exception);
                }
                throw new Exception(message);
            }

            return patchResult.Details.NewFilePath;
        }



        public static async Task<string> Magisk_Patch(PatchInfo zipInfo, BootInfo bootInfo)
        {
            if (bootInfo.HaveRamdisk == false)
            {
                throw new Exception(GetTranslation("Basicflash_BootWrong"));
            }
            await comp_copy(zipInfo, bootInfo);
            (string mb_output, int exitcode) = await CallExternalProgram.MagiskBoot($"cpio ramdisk.cpio test", bootInfo.TempPath);
            int mode_code = exitcode & 3;
            switch (mode_code)
            {
                case 0:
                    boot_img_pre(bootInfo);
                    break;
                case 1:
                    await patched_img_pre(bootInfo);
                    break;
                case 2:
                    throw new Exception(GetTranslation("Basicflash_UnsupportImage"));
                default:
                    throw new Exception(GetTranslation("Basicflash_CheckError"));
            }
            string config_path = Path.Combine(bootInfo.TempPath, "config");
            File.WriteAllText(config_path, "");
            File.AppendAllText(config_path, $"KEEPVERITY={EnvironmentVariable.KEEPVERITY}\n");
            File.AppendAllText(config_path, $"KEEPFORCEENCRYPT={EnvironmentVariable.KEEPFORCEENCRYPT}\n");
            File.AppendAllText(config_path, $"RECOVERYMODE={EnvironmentVariable.RECOVERYMODE}\n");
            File.AppendAllText(config_path, $"SHA1={bootInfo.SHA1}\n");
            string allowedChars = "abcdef0123456789";
            Random random = new Random();
            string randomStr = new string([.. Enumerable.Repeat(allowedChars, 16).Select(s => s[random.Next(s.Length)])]);
            string configContent = $"RANDOMSEED=0x{randomStr}";
            File.AppendAllText(config_path, configContent + Environment.NewLine);
            await ramdisk_patch(bootInfo);
            await dtb_patch(bootInfo);
            await kernel_patch(bootInfo, EnvironmentVariable.LEGACYSAR);
            CleanBoot(bootInfo.TempPath);
            (mb_output, exitcode) = await CallExternalProgram.MagiskBoot($"repack \"{bootInfo.Path}\"", bootInfo.TempPath);
            string newboot = Path.Combine(Path.GetDirectoryName(bootInfo.Path), "magisk_patched_" + randomStr + ".img");
            File.Copy(Path.Combine(bootInfo.TempPath, "new-boot.img"), newboot, true);
            return newboot;
        }
        private static void boot_img_pre(BootInfo bootinfo)
        {
            File.Copy(bootinfo.Path, Path.Combine(bootinfo.TempPath, "stock_boot.img"), true);
            File.Copy(Path.Combine(bootinfo.TempPath, "ramdisk.cpio"), Path.Combine(bootinfo.TempPath, "ramdisk.cpio.orig"), true);
        }
        private static async Task patched_img_pre(BootInfo bootinfo)
        {
            File.Copy(Path.Combine(bootinfo.TempPath, "ramdisk", ".backup", ".magisk"), Path.Combine(bootinfo.TempPath, "comfig.orig"), true);
            (_, _) = await CallExternalProgram.MagiskBoot($"cpio ramdisk.cpio restore", bootinfo.TempPath);
            File.Copy(Path.Combine(bootinfo.TempPath, "ramdisk.cpio"), Path.Combine(bootinfo.TempPath, "ramdisk.cpio.orig"), true);
            File.Delete(Path.Combine(bootinfo.TempPath, "stock_boot.img"));
        }
        private static async Task comp_copy(PatchInfo zipInfo, BootInfo bootinfo)
        {
            try
            {
                string archSubfolder = bootinfo.Arch switch
                {
                    "aarch64" => "arm64-v8a",
                    "armeabi" => "armeabi-v7a",
                    "X86" => "x86",
                    "X86-64" => "x86_64",
                    _ => throw new ArgumentException($"{GetTranslation("Basicflash_UnknowArch")}{bootinfo.Arch}")
                };
                string compPath = Path.Join(zipInfo.TempPath, "lib", archSubfolder);
                List<Task> copyTasks =
                [
                    Task.Run(() => File.Copy(Path.Combine(zipInfo.TempPath, "assets", "stub.xz"), Path.Combine(bootinfo.TempPath, "stub.xz"), true)),
                    Task.Run(() => File.Copy(Path.Combine(compPath, "init"), Path.Combine(bootinfo.TempPath, "init"), true)),
                    Task.Run(() => File.Copy(Path.Combine(compPath, "magisk32.xz"), Path.Combine(bootinfo.TempPath, "magisk32.xz"), true)),
                ];

                if (File.Exists(Path.Combine(compPath, "magisk64.xz")))
                {
                    copyTasks.Add(Task.Run(() => File.Copy(Path.Combine(compPath, "magisk64.xz"), Path.Combine(bootinfo.TempPath, "magisk64.xz"), true)));
                }
                await Task.WhenAll(copyTasks);
            }
            catch (Exception ex)
            {
                throw new Exception("comp_copy failed: " + ex.Message);
            }

        }
        private static async Task ramdisk_patch(BootInfo bootInfo)
        {
            string mb_output;
            int exitcode;
            (mb_output, exitcode) = await CallExternalProgram.MagiskBoot("cpio ramdisk.cpio \"add 0750 init init\" \"mkdir 0750 overlay.d\" \"mkdir 0750 overlay.d/sbin\" \"add 0644 overlay.d/sbin/magisk32.xz magisk32.xz\" ", bootInfo.TempPath);
            if (exitcode != 0)
            {
                throw new Exception("ramdisk_patch_1 failed: " + mb_output);
            }
            (mb_output, exitcode) = await CallExternalProgram.MagiskBoot("cpio ramdisk.cpio \"add 0644 overlay.d/sbin/stub.xz stub.xz\" \"patch\" \"backup ramdisk.cpio.orig\" \"mkdir 000 .backup\" \"add 000 .backup/.magisk config\"", bootInfo.TempPath);
            if (exitcode != 0)
            {
                throw new Exception("ramdisk_patch_2 failed: " + mb_output);
            }

            if (File.Exists(Path.Combine(bootInfo.TempPath, "magisk64.xz")))
            {
                (mb_output, exitcode) = await CallExternalProgram.MagiskBoot("cpio ramdisk.cpio \"add 0644 overlay.d/sbin/magisk64.xz magisk64.xz\"", bootInfo.TempPath);
                if (exitcode != 0)
                {
                    throw new Exception("ramdisk_patch_3 failed: " + mb_output);
                }
            }
        }
        private static async Task kernel_patch(BootInfo bootInfo, bool LEGACYSAR)
        {
            if (bootInfo.HaveKernel)
            {
                bool kernel_patched = false;
                (_, int exitcode) = await CallExternalProgram.MagiskBoot($"hexpatch kernel 49010054011440B93FA00F71E9000054010840B93FA00F7189000054001840B91FA00F7188010054 A1020054011440B93FA00F7140020054010840B93FA00F71E0010054001840B91FA00F7181010054", bootInfo.TempPath);
                if (exitcode == 0)
                {
                    kernel_patched = true;
                }
                (_, exitcode) = await CallExternalProgram.MagiskBoot($"hexpatch kernel 821B8012 E2FF8F12", bootInfo.TempPath);
                if (exitcode == 0)
                {
                    kernel_patched = true;
                }
                if (LEGACYSAR)
                {
                    (_, exitcode) = await CallExternalProgram.MagiskBoot($"hexpatch kernel 736B69705F696E697472616D667300 77616E745F696E697472616D667300", bootInfo.TempPath);
                    if (exitcode == 0)
                    {
                        kernel_patched = true;
                    }
                }
                if (!kernel_patched)
                {
                    File.Delete(Path.Combine(bootInfo.TempPath, "kernel"));
                }
            }
        }
        private static async Task dtb_patch(BootInfo bootInfo)
        {
            if (bootInfo.HaveDTB)
            {
                (string mb_output, int exitcode) = await CallExternalProgram.MagiskBoot($"dtb {bootInfo.DTBName} test", bootInfo.TempPath);
                if (exitcode != 0)
                {
                    throw new Exception("dtb_patch_1 failed: " + mb_output);
                }
                (_, _) = await CallExternalProgram.MagiskBoot($"dtb {bootInfo.DTBName} patch", bootInfo.TempPath);
            }
        }
        private static void CleanBoot(string path)
        {
            string[] filesToDelete =
                [
                "magisk64.xz",
                "magisk32.xz",
                "magiskinit",
                "stub.xz",
                "ramdisk.cpio.orig",
                "config",
                "stock_boot.img",
                "cpio",
                "init",
                "init.xz",
                ".magisk",
                ".rmlist"
                ];
            try
            {
                foreach (string file in filesToDelete)

                {
                    string filePath = Path.Combine(path, file);
                    if (File.Exists(filePath))
                    {
                        _ = FileHelper.WipeFile(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("bootclean failed: " + ex.Message);
            }
        }
    }
}
