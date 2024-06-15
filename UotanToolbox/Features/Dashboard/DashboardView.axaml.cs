using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Controls;
using System;
using System.Diagnostics;
using System.IO;
using UotanToolbox.Common;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Features.Dashboard;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }

    private async void OpenUnlockFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            UnlockFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    private async void OpenRecFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            RecFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    private async void OpenMagiskFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            MagiskFile.Text = StringHelper.FilePath(files[0].Path.ToString());
            string outputpath = Path.Combine(Global.runpath,"Temp","Magisk");
            bool istempclean =FileHelper.DeleteDirectory(outputpath);
            if (istempclean)
            {
                string outputzip = await CallExternalProgram.SevenZip($"x \"{MagiskFile.Text}\" -o\"{outputpath}\" -y");
                string pattern_MAGISK_VER = @"MAGISK_VER='([^']+)'";
                string pattern_MAGISK_VER_CODE = @"MAGISK_VER_CODE=(\d+)";
                string Magisk_sh_path = Path.Combine(outputpath, "assets", "util_functions.sh");
                string MAGISK_VER = StringHelper.FileRegex(Magisk_sh_path, pattern_MAGISK_VER, 1);
                string MAGISK_VER_CODE = StringHelper.FileRegex(Magisk_sh_path, pattern_MAGISK_VER_CODE, 1);
                if ((MAGISK_VER != null) &(MAGISK_VER_CODE != null))
                {
                    string BOOT_PATCH_PATH = Path.Combine(outputpath, "assets", "boot_patch.sh");
                    string md5 = FileHelper.Md5Hash(BOOT_PATCH_PATH);
                    //SukiHost.ShowDialog(new ConnectionDialog(md5), allowBackgroundClose: true);
                    bool Magisk_Valid = StringHelper.Magisk_Validation(md5, MAGISK_VER, MAGISK_VER_CODE);
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog("未能获取到有效Magisk版本号"), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("清理临时目录出错"), allowBackgroundClose: true);
            }
        }
    }

    private async void OpenBootFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            string outputpath = Path.Combine(Global.runpath, "Temp", "Boot");
            bool istempclean = FileHelper.DeleteDirectory(outputpath);
            try
            {
                File.Delete(outputpath);
                if (!Directory.Exists(outputpath))
                {
                    Directory.CreateDirectory(outputpath);
                }
            }
            catch (Exception ex)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"创建临时文件夹时发生错误: {ex.Message}"), allowBackgroundClose: true);
                return;
            }

            BootFile.Text = StringHelper.FilePath(files[0].Path.ToString());
            if (istempclean)
            {
                string magiskbootpath;
                if (Global.System == "Windows")
                {
                    magiskbootpath = Path.Combine(Global.bin_path, "magiskboot.exe");
                    outputpath = Path.Combine(outputpath, "magiskboot.exe");
                }
                else
                {
                    magiskbootpath = Path.Combine(Global.bin_path, "magiskboot");
                    outputpath = Path.Combine(outputpath, "magiskboot");
                }
                try 
                { 
                    File.Copy(magiskbootpath, outputpath, true); 
                }
                catch (Exception ex)
                {
                    SukiHost.ShowDialog(new ConnectionDialog($"复制文件时发生错误: {ex.Message}"), allowBackgroundClose: true);
                    return;
                }
                string mb_output = await CallExternalProgram.MagiskBoot($"unpack \"{BootFile.Text}\"");
                SukiHost.ShowDialog(new ConnectionDialog(mb_output), allowBackgroundClose: true);
            }
        }
    }

    private async void OpenAFDI(object sender, RoutedEventArgs args)
    {
        if (Global.System == "Windows")
        {
            Process.Start(@"drive\adb.exe");
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SukiHost.ShowDialog(new ConnectionDialog("当前设备无需进行此操作！"), allowBackgroundClose: true);
            });
        }
    }

    private async void Open9008DI(object sender, RoutedEventArgs args)
    {
        if (Global.System == "Windows")
        {
            Process.Start(@"drive\Qualcomm_HS-USB_Driver.exe");
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SukiHost.ShowDialog(new ConnectionDialog("当前设备无需进行此操作！"), allowBackgroundClose: true);
            });
        }
    }

    private async void OpenUSBP(object sender, RoutedEventArgs args)
    {
        if (Global.System == "Windows")
        {
            string cmd = @"drive\USB3.bat";
            ProcessStartInfo? cmdshell = null;
            cmdshell = new ProcessStartInfo(cmd)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process? f = Process.Start(cmdshell);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SukiHost.ShowDialog(new ConnectionDialog("执行完成！"), allowBackgroundClose: true);
            });
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SukiHost.ShowDialog(new ConnectionDialog("当前设备无需进行此操作！"), allowBackgroundClose: true);
            });
        }
    }
}