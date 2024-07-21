using SukiUI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Common
{
    internal class BootPatchHelper
    {
        private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
        public static readonly Dictionary<string, string> ArchMappings = new Dictionary<string, string>
        {
            {"ARM aarch64", "aarch64"},
            {"X86-64", "X86-64"},
            {"ARM,", "armeabi"},
            {"Intel 80386", "X86"}
        };

        public static (bool, string) ArchDetect(string init_info)
        {
            foreach (var entry in ArchMappings)
            {
                if (init_info.Contains(entry.Key))
                    return (true, entry.Value);
            }
            return (false, null);
        }
        public static bool boot_img_pre(string boot_path)
        {
            File.Copy(boot_path, Path.Combine(BootInfo.tmp_path, "stock_boot.img"), true);
            File.Copy(Path.Combine(BootInfo.tmp_path, "ramdisk.cpio"), Path.Combine(BootInfo.tmp_path, "ramdisk.cpio.orig"), true);
            return true;
        }
        /// <summary>
        /// 对面具修补后的Boot预处理，尝试恢复原来的boot映像
        /// </summary>
        /// <param name="boot_path">源boot地址，但是实际上没被用到</param>
        /// <returns>预处理是否成功</returns>
        public static bool patched_img_pre(string boot_path)
        {
            File.Copy(Path.Combine(BootInfo.tmp_path, "ramdisk", ".backup", ".magisk", "config.orig"), Path.Combine(BootInfo.tmp_path, "config.orig"));
            return true;
        }
        /// <summary>
        /// 将组件文件从面具文件夹中复制到boot文件夹
        /// </summary>
        /// <param name="compPath">组件路径</param>
        /// <returns>是否成功</returns>
        public static bool comp_copy(string compPath)
        {
            //File.Copy(Path.Combine(ZipInfo.Temppath, "assets", "stub.xz"), Path.Combine(BootInfo.tmp_path, "stub.xz"), true);
            File.Copy(Path.Combine(compPath, "libmagiskinit.so"), Path.Combine(BootInfo.tmp_path, "magiskinit"), true);
            File.Copy(Path.Combine(compPath, "magisk32.xz"), Path.Combine(BootInfo.tmp_path, "magisk32.xz"), true);
            if (File.Exists(Path.Combine((compPath), "magisk64.xz")))
            {
                File.Copy(Path.Combine(compPath, "magisk64.xz"), Path.Combine(BootInfo.tmp_path, "magisk64.xz"), true);
            }
            return true;
        }
        public static async Task<(bool, string)> boot_detect(string boot_path)
        {
            BootInfo.SHA1 = await FileHelper.SHA1HashAsync(boot_path);
            if (BootInfo.SHA1 == null)
            {
                return (false, null);
            }
            //在临时目录创建临时boot目录，这破东西跨平台解压各种问题，直接即用即丢了
            BootInfo.tmp_path = Path.Combine(Global.tmp_path, "Boot-" + StringHelper.RandomString(8));
            string workpath = BootInfo.tmp_path;
            if (FileHelper.ClearFolder(workpath))
            {
                string osVersionPattern = @"OS_VERSION\s+\[(.*?)\]";
                string osPatchLevelPattern = @"OS_PATCH_LEVEL\s+\[(.*?)\]";
                (string mb_output, Global.mb_exitcode) = await CallExternalProgram.MagiskBoot($"unpack \"{boot_path}\"", BootInfo.tmp_path);
                if (mb_output.Contains("error"))
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_SelectBoot")), allowBackgroundClose: true);
                    return (false, null);
                }
                BootInfo.os_version = Regex.Match(mb_output, osVersionPattern).Groups[1].Value;
                BootInfo.patch_level = Regex.Match(mb_output, osPatchLevelPattern).Groups[1].Value;
                await Task.Run(() => dtb_detect());
                await Task.Run(() => kernel_detect());
                await ramdisk_detect();
                //SukiHost.ShowDialog(new PureDialog($"{GetTranslation("Basicflash_DetectdBoot")}\nArch:{BootInfo.arch}\nOS:{BootInfo.os_version}\nPatch_level:{BootInfo.patch_level}\nRamdisk:{BootInfo.have_ramdisk}\nKMI:{BootInfo.kmi}"), allowBackgroundClose: true);
                return (true, BootInfo.arch);
            }
            return (false, null);
        }
        /// <summary>
        /// 检测Boot文件夹下是否存在dtb文件
        /// </summary>
        public static void dtb_detect()
        {
            if (File.Exists(Path.Combine(BootInfo.tmp_path, "dtb")))
            {
                BootInfo.have_dtb = true;
                BootInfo.dtb_name = "dtb";
            }
            else if (File.Exists(Path.Combine(BootInfo.tmp_path, "kernel_dtb")))
            {
                BootInfo.have_dtb = true;
                BootInfo.dtb_name = "kernel_dtb";
            }
            else if (File.Exists(Path.Combine(BootInfo.tmp_path, "extra")))
            {
                BootInfo.have_dtb = true;
                BootInfo.dtb_name = "extra";
            }
            else
            {
                BootInfo.have_dtb = false;
                BootInfo.dtb_name = "";
            }
        }

        /// <summary>
        /// 检测Boot文件夹下是否存在kernel文件
        /// </summary>
        public static void kernel_detect()
        {
            if (File.Exists(Path.Combine(BootInfo.tmp_path, "kernel")))
            {
                string comp_path = Path.Combine(BootInfo.tmp_path, "kernel-component"); //kernel内解压出来的文件路径
                BootInfo.have_kernel = true;
                //await CallExternalProgram.SevenZip($"e -t#:e -aoa -o{comp_path} kernel -y");
                //string[] gz_names = FileHelper.FindConfigGzFiles(comp_path);
                //string[] decompress_file_names = await FileHelper.DecompressConfigGzFiles(gz_names);
                BootInfo.version = ReadKernelVersion(Path.Combine(BootInfo.tmp_path, "kernel"));
                BootInfo.kmi = ExtractKMI(BootInfo.version);
                if (!String.IsNullOrEmpty(BootInfo.kmi))
                {
                    BootInfo.gki2 = true;
                }
            }
            else
            {
                BootInfo.have_kernel = false;
            }
        }

        public static async Task<bool> ramdisk_detect()
        {
            if (File.Exists(Path.Combine(BootInfo.tmp_path, "ramdisk.cpio")))
            {
                BootInfo.have_ramdisk = true;
                string workpath = BootInfo.tmp_path;
                string cpio_file = Path.Combine(BootInfo.tmp_path, "ramdisk.cpio");
                string ramdisk_path = Path.Combine(BootInfo.tmp_path, "ramdisk");
                //适配Windows的抽象magiskboot（使用cygwin），其他平台都是原生编译的，可以直接用参数提取ramdisk
                if (Global.System != "Windows")
                {
                    workpath = Path.Combine(BootInfo.tmp_path, "ramdisk");
                    Directory.CreateDirectory(workpath);
                }
                (string outputcpio, Global.cpio_exitcode) = await CallExternalProgram.MagiskBoot($"cpio \"{cpio_file}\" extract", workpath);
                string initPath = Path.Combine(ramdisk_path, "init");
                string init_info = await CallExternalProgram.File($"\"{initPath}\"");
                (BootInfo.userful, BootInfo.arch) = ArchDetect(init_info);
                if (!BootInfo.userful)
                {
                    string tmp_initPath = await read_symlink(initPath);
                    initPath = Path.Join(ramdisk_path, tmp_initPath);
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        initPath = Path.Join("/private", initPath);
                    }
                    init_info = await CallExternalProgram.File(initPath);
                    SukiHost.ShowDialog(new PureDialog(init_info), allowBackgroundClose: true);
                    (BootInfo.userful, BootInfo.arch) = ArchDetect(init_info);
                    if (!BootInfo.userful)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 进行ramdisk修补
        /// </summary>
        /// <param name="compPath">组件目录</param>
        /// <returns>是否成功</returns>
        public static async Task<bool> ramdisk_patch(string compPath)
        {
            string mb_output;
            int exitcode;
            if (comp_copy(compPath))
            {
                (mb_output, exitcode) = await CallExternalProgram.MagiskBoot("cpio ramdisk.cpio \"add 0750 init magiskinit\" \"mkdir 0750 overlay.d\" \"mkdir 0750 overlay.d/sbin\" \"add 0644 overlay.d/sbin/magisk32.xz magisk32.xz\" ", BootInfo.tmp_path);
                if (exitcode != 0)
                {
                    return false;
                }
                (mb_output, exitcode) = await CallExternalProgram.MagiskBoot("cpio ramdisk.cpio \"add 0644 overlay.d/sbin/stub.xz stub.xz\" \"patch\" \"backup ramdisk.cpio.orig\" \"mkdir 000 .backup\" \"add 000 .backup/.magisk config\"", BootInfo.tmp_path);
                if (exitcode != 0)
                {
                    return false;
                }
            }
            if (File.Exists(Path.Combine((compPath), "magisk64.xz")))
            {
                (mb_output, exitcode) = await CallExternalProgram.MagiskBoot("cpio ramdisk.cpio \"add 0644 overlay.d/sbin/magisk64.xz magisk64.xz\"", BootInfo.tmp_path);
                if (exitcode != 0)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 进行面具修补流程中的kernel修补
        /// </summary>
        /// <param name="LEGACYSAR">legacysar是否选中</param>
        /// <returns></returns>
        public static async Task<bool> kernel_patch(bool LEGACYSAR)
        {
            if (BootInfo.have_kernel)
            {
                bool kernel_patched = false;
                (string mb_output, int exitcode) = await CallExternalProgram.MagiskBoot($"hexpatch kernel 49010054011440B93FA00F71E9000054010840B93FA00F7189000054001840B91FA00F7188010054 A1020054011440B93FA00F7140020054010840B93FA00F71E0010054001840B91FA00F7181010054", BootInfo.tmp_path);
                if (exitcode == 0)
                {
                    kernel_patched = true;
                }
                (mb_output, exitcode) = await CallExternalProgram.MagiskBoot($"hexpatch kernel 821B8012 E2FF8F12", BootInfo.tmp_path);
                if (exitcode == 0)
                {
                    kernel_patched = true;
                }
                if (LEGACYSAR)
                {
                    (mb_output, exitcode) = await CallExternalProgram.MagiskBoot($"hexpatch kernel 736B69705F696E697472616D667300 77616E745F696E697472616D667300", BootInfo.tmp_path);
                    if (exitcode == 0)
                    {
                        kernel_patched = true;
                    }
                }
                if (!kernel_patched)
                {
                    File.Delete(Path.Combine(BootInfo.tmp_path, "kernel"));
                }
            }
            return true;
        }
        public async static Task<bool> dtb_patch()
        {
            if (BootInfo.have_dtb)
            {
                (string mb_output, int exitcode) = await CallExternalProgram.MagiskBoot($"dtb {BootInfo.dtb_name} test", BootInfo.tmp_path);
                if (exitcode != 0)
                {
                    SukiHost.ShowDialog(new PureDialog("dtb验证失败"), allowBackgroundClose: true);
                    return false;
                }
                (mb_output, exitcode) = await CallExternalProgram.MagiskBoot($"dtb {BootInfo.dtb_name} patch", BootInfo.tmp_path);
            }
            return true;
        }

        /// <summary>
        /// 打包之前清理boot文件夹，避免不必要的报错
        /// </summary>
        /// <param name="path">boot文件夹路径</param>
        /// <returns>是否成功</returns>
        public static bool CleanBoot(string path)
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
                return true;
            }
            catch
            {
                return false;
            }
        }


    }
}
