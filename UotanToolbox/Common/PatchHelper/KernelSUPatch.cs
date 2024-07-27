using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UotanToolbox.Common.PatchHelper
{
    internal class KernelSUPatch
    {
        private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
        public async static Task<string> GKI_Patch(ZipInfo zipInfo, BootInfo bootInfo)
        {
            if (bootInfo.HaveKernel == false)
            {
                throw new Exception(GetTranslation("Basicflash_BootWong"));
            }
            if (zipInfo.KMI != bootInfo.KMI)
            {
                throw new Exception("error zip kernel kmi");
            }
            File.Copy(Path.Combine(zipInfo.TempPath, "Image"), Path.Combine(bootInfo.TempPath, "kernel"), true);
            CleanBoot(bootInfo.TempPath);
            (string mb_output, int exitcode) = await CallExternalProgram.MagiskBoot($"repack \"{bootInfo.Path}\"", bootInfo.TempPath);
            if (exitcode != 0)
            {
                throw new Exception("ksu_pack_1 failed: " + mb_output);
            }
            string allowedChars = "abcdef0123456789";
            Random random = new Random();
            string randomStr = new string(Enumerable.Repeat(allowedChars, 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            string newboot = Path.Combine(Path.GetDirectoryName(bootInfo.Path), "lkm_patched_" + randomStr + ".img");
            File.Copy(Path.Combine(bootInfo.TempPath, "new-boot.img"), newboot, true);
            return newboot;
        }
        public async static Task<string> LKM_Patch(ZipInfo zipInfo, BootInfo bootInfo)
        {
            if (bootInfo.HaveRamdisk == false)
            {
                throw new Exception(GetTranslation("Basicflash_BootWong"));
            }
            if (zipInfo.KMI != bootInfo.KMI)
            {
                throw new Exception("error zip kernel kmi");
            }
            File.Copy(Path.Combine(zipInfo.TempPath, "kernelsu.ko"), Path.Combine(bootInfo.TempPath, "kernelsu.ko"), true);
            string archSubfolder = bootInfo.Arch switch
            {
                "aarch64" => "arm64-v8a",
                "X86-64" => "x86_64",
                _ => throw new ArgumentException($"{GetTranslation("Basicflash_UnknowArch")}{bootInfo.Arch}")
            };
            File.Copy(Path.Combine(Global.bin_path, "ksud", archSubfolder, "init"), Path.Combine(bootInfo.TempPath, "init"));
            (string mb_output, int exitcode) = await CallExternalProgram.MagiskBoot($"cpio ramdisk.cpio \"cp init init.real\" \"add 0755 ksuinit init\" \"add 0755 kernelsu.ko kernelsu.ko\"", bootInfo.TempPath);
            if (exitcode != 0)
            {
                throw new Exception("lkm_patch failed: " + mb_output);
            }
            CleanBoot(bootInfo.TempPath);
            (mb_output, exitcode) = await CallExternalProgram.MagiskBoot($"repack \"{bootInfo.Path}\"", bootInfo.TempPath);
            if (exitcode != 0)
            {
                throw new Exception("lkm_pack failed: " + mb_output);
            }
            string allowedChars = "abcdef0123456789";
            Random random = new Random();
            string randomStr = new string(Enumerable.Repeat(allowedChars, 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            string newboot = Path.Combine(Path.GetDirectoryName(bootInfo.Path), "lkm_patched_" + randomStr + ".img");
            File.Copy(Path.Combine(bootInfo.TempPath, "new-boot.img"), newboot, true);
            return newboot;
        }
        private static void CleanBoot(string path)
        {
            string[] filesToDelete =
                {
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
                };
            try
            {
                foreach (string file in filesToDelete)

                {
                    string filePath = Path.Combine(path, file);
                    if (File.Exists(filePath))
                    {
                        FileHelper.WipeFile(filePath);
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
