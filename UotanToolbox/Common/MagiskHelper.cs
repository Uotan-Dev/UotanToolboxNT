using SukiUI.Controls;
using System;
using System.IO;
using System.Linq;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Common
{
    internal class MagiskHelper
    {
        public static bool CheckComponentFiles(string magisk_path, string arch)
        {
            string compPathBase = System.IO.Path.Combine(magisk_path, "lib");
            string archSubfolder = arch switch
            {
                "aarch64" => "arm64-v8a",
                "armeabi" => "armeabi-v7a",
                "X86" => "x86",
                "X86-64" => "x86_64",
                _ => throw new ArgumentException($"未知架构：{arch}")
            };
            string compPath = System.IO.Path.Combine(compPathBase, archSubfolder);
            string[] commonFiles = { "libmagiskpolicy.so", "libmagiskinit.so", "libmagiskboot.so", "libbusybox.so" };
            string specificFiles = arch switch
            {
                "aarch64" => "libmagisk64.so",
                "armeabi" => "libmagisk32.so",
                "X86" => "libmagisk32.so",
                "X86-64" => "libmagisk64.so",
                _ => throw new InvalidOperationException() // 前面不出问题，这个东西应该不会被抛出
            };
            commonFiles = commonFiles.Concat(new[] { specificFiles }).ToArray();
            var results = FileHelper.CheckFilesExistInDirectory(compPath, commonFiles);
            bool allFilesExist = results.Values.All(result => result);
            return allFilesExist;
        }
        public static bool boot_img_pre(string boot_path)
        {
            try
            {
                File.Copy(boot_path, System.IO.Path.Combine(Global.boot_tmp, "stock_boot.img"), true);
                File.Copy(System.IO.Path.Combine(Global.boot_tmp, "ramdisk.cpio"), System.IO.Path.Combine(Global.boot_tmp, "ramdisk.cpio.orig"), true);
                return true;
            }
            catch (Exception e)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"模式0预处理出错 {e.Message}"));
                return false;
            }
        }
        public static bool patched_img_pre(string boot_path)
        {
            try
            {
                File.Copy(System.IO.Path.Combine(Global.boot_tmp, "ramdisk", ".backup", ".magisk", "config.orig"), System.IO.Path.Combine(Global.boot_tmp, "config.orig"));
                return true;
            }
            catch (Exception e)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"模式1预处理出错 {e.Message}"));
                return false;
            }
        }
        public static bool comp_copy(string compPath)
        {
            try
            {
                File.Copy(System.IO.Path.Combine(Global.magisk_tmp, "assets", "stub.xz"), System.IO.Path.Combine(Global.boot_tmp, "stub.xz"), true);
                File.Copy(System.IO.Path.Combine(compPath, "libmagiskinit.so"), System.IO.Path.Combine(Global.boot_tmp, "magiskinit"), true);
                if (File.Exists(System.IO.Path.Combine((compPath), "magisk32.xz")))
                {
                    File.Copy(System.IO.Path.Combine(compPath, "magisk32.xz"), System.IO.Path.Combine(Global.boot_tmp, "magisk32.xz"), true);
                }
                if (File.Exists(System.IO.Path.Combine((compPath), "magisk64.xz")))
                {
                    File.Copy(System.IO.Path.Combine(compPath, "magisk64.xz"), System.IO.Path.Combine(Global.boot_tmp, "magisk64.xz"), true);
                }
                return true;
            }
            catch (Exception e)
            {
                SukiHost.ShowDialog(new ConnectionDialog("Failed to copy components to boot partition" + e));
                return false;
            }
        }
        public static string dtb_detect(string path)
        {
            if (File.Exists(System.IO.Path.Combine(Global.boot_tmp, "dtb")))
            {
                return "dtb";
            }
            if (File.Exists(System.IO.Path.Combine(Global.boot_tmp, "kernel_dtb")))
            {
                return "kernel_dtb";
            }
            if (File.Exists(System.IO.Path.Combine(Global.boot_tmp, "extra")))
            {
                return "extra";
            }
            return null;
        }
        public static bool clean_boot(string path)
        {
            try
            {
                if (File.Exists(System.IO.Path.Combine(path, "magisk64.xz")))
                {
                    FileHelper.WipeFile(System.IO.Path.Combine(path, "magisk64.xz"));
                }
                if (File.Exists(System.IO.Path.Combine(path, "magisk32.xz")))
                {
                    FileHelper.WipeFile(System.IO.Path.Combine(path, "magisk32.xz"));
                }
                if (File.Exists(System.IO.Path.Combine(path, "magiskinit")))
                {
                    FileHelper.WipeFile(System.IO.Path.Combine(path, "magiskinit"));
                }
                if (File.Exists(System.IO.Path.Combine(path, "stub.xz")))
                {
                    FileHelper.WipeFile(System.IO.Path.Combine(path, "stub.xz"));
                }
                if (File.Exists(System.IO.Path.Combine(path, "ramdisk.cpio.orig")))
                {
                    FileHelper.WipeFile(System.IO.Path.Combine(path, "ramdisk.cpio.orig"));
                }
                if (File.Exists(System.IO.Path.Combine(path, "config")))
                {
                    FileHelper.WipeFile(System.IO.Path.Combine(path, "config"));
                }
                if (File.Exists(System.IO.Path.Combine(path, "stock_boot.img")))
                {
                    FileHelper.WipeFile(System.IO.Path.Combine(path, "stock_boot.img"));
                }
                if (File.Exists(System.IO.Path.Combine(path, "cpio")))
                {
                    FileHelper.WipeFile(System.IO.Path.Combine(path, "cpio"));
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
