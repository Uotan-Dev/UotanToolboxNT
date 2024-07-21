using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System;
using SukiUI.Controls;
using UotanToolbox.Features.Components;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace UotanToolbox.Common.PatchHelper
{
    internal class BootDetect
    {
        private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
        public static async Task<BootInfo> Boot_Detect(string path)
        {
            BootInfo bootinfo= new BootInfo("","","",false,false,"","","","",false,false,false,"","");
            bootinfo.Path = path;
            bootinfo.SHA1 = await FileHelper.SHA1HashAsync(bootinfo.Path);
            bootinfo.TempPath = Path.Combine(Global.tmp_path, "Boot-" + StringHelper.RandomString(8));
            bool istempclean = FileHelper.ClearFolder(bootinfo.TempPath);
            if (!istempclean)
            {
                throw new Exception("fatal error!");
            }
            string osVersionPattern = @"OS_VERSION\s+\[(.*?)\]";
            string osPatchLevelPattern = @"OS_PATCH_LEVEL\s+\[(.*?)\]";
            (string mb_output, Global.mb_exitcode) = await CallExternalProgram.MagiskBoot($"unpack \"{path}\"", bootinfo.TempPath);
            if (Global.mb_exitcode != 0)
            {
                throw new Exception(GetTranslation("Basicflash_SelectBoot")+mb_output);
            }
            bootinfo.OSVersion = Regex.Match(mb_output, osVersionPattern).Groups[1].Value;
            bootinfo.PatchLevel = Regex.Match(mb_output, osPatchLevelPattern).Groups[1].Value;
            bootinfo.IsUseful = true;
            (bootinfo.HaveDTB,bootinfo.DTBName) = await Task.Run(() => dtb_detect(bootinfo.TempPath));
            (bootinfo.Version, bootinfo.KMI,bootinfo.HaveKernel,bootinfo.GKI2)  =await Task.Run(() => kernel_detect(bootinfo.TempPath));
            (bootinfo.HaveRamdisk, bootinfo.Arch)  = await ramdisk_detect(bootinfo.TempPath);
            return bootinfo;
        }
        private static (bool,string) dtb_detect(string temp)
        {
            if (File.Exists(Path.Combine(temp, "dtb")))
            {
                return (true, "dtb");
            }
            else if (File.Exists(Path.Combine(temp, "kernel_dtb")))
            {
                return (true, "kernel_dtb");
            }
            else if (File.Exists(Path.Combine(temp, "extra")))
            {
                return (true, "extra");
            }
            else
            {
                return (false, "");
            }
        }

        private static (string,string,bool,bool) kernel_detect(string temp)
        {
            string kmi="", version ="";
            bool have_kernel =false, gki2 = false;
            if (File.Exists(Path.Combine(temp, "kernel")))
            {
                have_kernel = true;
                version = FileHelper.ReadKernelVersion(Path.Combine(temp, "kernel"));
                kmi = StringHelper.ExtractKMI(version);
                if (!String.IsNullOrEmpty(kmi))
                {
                    gki2 = true;
                }
            }
            return (version, kmi, have_kernel, gki2);
        }

        public static async Task<(bool,string)> ramdisk_detect(string tmp_path)
        {
            bool have_ramdisk = false;
            string init_info ="";
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
                    Directory.CreateDirectory(workpath);
                }
                (string outputcpio, Global.cpio_exitcode) = await CallExternalProgram.MagiskBoot($"cpio \"{cpio_file}\" extract", workpath);
                string initPath = Path.Join(ramdisk_path, "init");
                if (File.Exists(Path.Join(ramdisk_path, "/system/bin/init"))) 
                {
                    initPath = Path.Join(ramdisk_path, "/system/bin/init");
                }
                init_info = await CallExternalProgram.File($"\"{initPath}\"");
                arch = ArchDetect(init_info);
            }
            return (have_ramdisk,arch);
        }
        private static readonly Dictionary<string, string> ArchMappings = new Dictionary<string, string>
        {
            {"ARM aarch64", "aarch64"},
            {"X86-64", "X86-64"},
            {"ARM,", "armeabi"},
            {"Intel 80386", "X86"}
        };

        private static string ArchDetect(string init_info)
        {
            foreach (var entry in ArchMappings)
            {
                if (init_info.Contains(entry.Key))
                    return entry.Value;
            }
            throw new Exception("wrong elf bin file");
        }
    }
}
