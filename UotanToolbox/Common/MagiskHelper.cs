using Avalonia.Controls.Shapes;
using SukiUI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Common
{
    internal class MagiskHelper
    {
        public static bool CheckComponentFiles(string magisk_path ,string arch)
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
                "aarch64" => "libmagisk64.so" ,
                "armeabi" => "libmagisk32.so" ,
                "X86" => "libmagisk32.so" ,
                "X86-64" => "libmagisk64.so" ,
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
                File.Copy(boot_path,System.IO.Path.Combine(Global.boot_tmp,"stock_boot.img"),true);
                    File.Copy(System.IO.Path.Combine(Global.boot_tmp, "ramdisk.cpio"), System.IO.Path.Combine(Global.boot_tmp,"ramdisk.cpio.orig"), true);
                return true;
            }
            catch (Exception e) 
            {
                SukiHost.ShowDialog(new ConnectionDialog($"模式0预处理出错 {e.Message}"), allowBackgroundClose: true);
                return false;
            }
        }
        public static bool patched_img_pre(string boot_path)
        {
            try
            {
                File.Copy( System.IO.Path.Combine(Global.boot_tmp, "ramdisk",".backup",".magisk","config.orig"), System.IO.Path.Combine(Global.boot_tmp, "config.orig"));
                return true;
            }
            catch (Exception e)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"模式1预处理出错 {e.Message}"), allowBackgroundClose: true);
                return false;
            }
        }
    }
}
