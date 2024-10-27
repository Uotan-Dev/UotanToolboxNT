using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UotanToolbox.Common.PatchHelper
{
    internal class BootDetect
    {
        private static string GetTranslation(string key)
        {
            return FeaturesHelper.GetTranslation(key);
        }

        public static async Task<BootInfo> Boot_Detect(string path)
        {
            BootInfo bootinfo = new BootInfo("", "", "", false, false, "", "", "", "", false, false, false, "", "", "")
            {
                Path = path
            };
            bootinfo.SHA1 = await FileHelper.SHA1HashAsync(bootinfo.Path);
            bootinfo.TempPath = Path.Combine(Global.tmp_path, "Boot-" + StringHelper.RandomString(8));
            bool istempclean = FileHelper.ClearFolder(bootinfo.TempPath);
            if (!istempclean)
            {
                throw new Exception(GetTranslation("Basicflash_FatalError"));
            }
            string osVersionPattern = @"OS_VERSION\s+\[(.*?)\]";
            string osPatchLevelPattern = @"OS_PATCH_LEVEL\s+\[(.*?)\]";
            string osKernelFMTPattern = @"KERNEL_FMT\s*\[([^\]]*)\]";
            (string mb_output, Global.mb_exitcode) = await CallExternalProgram.MagiskBoot($"unpack \"{path}\"", bootinfo.TempPath);
            if (Global.mb_exitcode != 0)
            {
                throw new Exception(GetTranslation("Basicflash_SelectBoot") + mb_output);
            }
            bootinfo.OSVersion = Regex.Match(mb_output, osVersionPattern).Groups[1].Value;
            bootinfo.PatchLevel = Regex.Match(mb_output, osPatchLevelPattern).Groups[1].Value;
            bootinfo.Compress = Regex.Match(mb_output, osKernelFMTPattern).Groups[1].Value;
            bootinfo.IsUseful = true;
            (bootinfo.HaveDTB, bootinfo.DTBName) = await Task.Run(() => dtb_detect(bootinfo.TempPath));
            (bootinfo.Version, bootinfo.KMI, bootinfo.HaveKernel, bootinfo.GKI2, bootinfo.Arch) = await kernel_detect(bootinfo.TempPath);
            (bootinfo.HaveRamdisk, bootinfo.Arch) = await ramdisk_detect(bootinfo.TempPath);
            return bootinfo;
        }
        private static (bool, string) dtb_detect(string temp)
        {
            return File.Exists(Path.Combine(temp, "dtb"))
                ? (true, "dtb")
                : File.Exists(Path.Combine(temp, "kernel_dtb"))
                    ? (true, "kernel_dtb")
                    : File.Exists(Path.Combine(temp, "extra")) ? (true, "extra") : (false, "");
        }

        private static async Task<(string, string, bool, bool, string)> kernel_detect(string temp)
        {
            string kmi = "", version = "", arch = "";
            bool have_kernel = false, gki2 = false;
            if (File.Exists(Path.Combine(temp, "kernel")))
            {
                have_kernel = true;
                version = FileHelper.ReadKernelVersion(Path.Combine(temp, "kernel"));
                kmi = StringHelper.ExtractKMI(version);
                if (!string.IsNullOrEmpty(kmi))
                {
                    gki2 = true;
                }
                string kernel_info = await CallExternalProgram.File($"\"{Path.Combine(temp, "kernel")}\"");
                arch = ArchDetect(kernel_info);
            }
            return (version, kmi, have_kernel, gki2, arch);
        }

        public static async Task<(bool, string)> ramdisk_detect(string tmp_path)
        {
            bool have_ramdisk = false;
            string arch = "";
            if (File.Exists(Path.Combine(tmp_path, "ramdisk.cpio")))
            {
                have_ramdisk = true;
                string workpath = tmp_path;
                string cpio_file = Path.Combine(tmp_path, "ramdisk.cpio");
                string ramdisk_path = Path.Combine(tmp_path, "ramdisk");
                //适配Windows的抽象magiskboot（使用cygwin），其他平台都是原生编译的，可以直接用参数提取ramdisk
                if (Global.System != "Windows")
                {
                    workpath = Path.Combine(tmp_path, "ramdisk");
                    _ = Directory.CreateDirectory(workpath);
                }

                (_, int exitcode) = await CallExternalProgram.MagiskBoot($"cpio \"{cpio_file}\" test", workpath);
                if (exitcode != 0)
                {
                    throw new Exception("Do not support magisk patched boot.img");
                }
                (_, exitcode) = await CallExternalProgram.MagiskBoot($"cpio \"{cpio_file}\" \"exists kernelsu.ko\"", workpath);
                if (exitcode == 0)
                {
                    throw new Exception("Do not support kernelsu patched boot.img");
                }
                (_, _) = await CallExternalProgram.MagiskBoot($"cpio \"{cpio_file}\" extract", workpath);
                if (Global.System == "macOS")
                {
                    ramdisk_path = Path.Join("/private", ramdisk_path);
                }
                string initPath = Path.Join(ramdisk_path, "init");
                if (File.Exists(Path.Join(ramdisk_path, "/system/bin/init")))
                {
                    initPath = Path.Join(ramdisk_path, "/system/bin/init");
                }
                string init_info = await CallExternalProgram.File($"\"{initPath}\"");
                arch = ArchDetect(init_info);
            }
            return (have_ramdisk, arch);
        }
        private static readonly Dictionary<string, string> ArchMappings = new Dictionary<string, string>
        {
            {"ARM aarch64", "aarch64"},
            {"X86-64", "X86-64"},
            {"ARM,", "armeabi"},
            {"Intel 80386", "X86"},
            {"ARM64","aarch64" }
        };

        private static string ArchDetect(string init_info)
        {
            foreach (KeyValuePair<string, string> entry in ArchMappings)
            {
                if (init_info.Contains(entry.Key))
                {
                    return entry.Value;
                }
            }
            throw new Exception(GetTranslation("Basicflash_ELFError"));
        }
    }
}
