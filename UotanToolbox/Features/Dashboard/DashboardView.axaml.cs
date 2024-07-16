using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Features.Dashboard;

public partial class DashboardView : UserControl
{
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public AvaloniaList<string> SimpleUnlock = ["oem unlock", "oem unlock-go", "flashing unlock", "flashing unlock_critical"];
    public AvaloniaList<string> Arch = ["aarch64", "armeabi", "X86-64", "X86"];

    public DashboardView()
    {
        InitializeComponent();
        SimpleContent.ItemsSource = SimpleUnlock;
        ArchList.ItemsSource = Arch;
    }
    public void patch_busy(bool is_busy)
    {
        if (is_busy)
        {
            BusyPatch.IsBusy = true;
            PanelPatch.IsEnabled = false;
        }
        else
        {
            BusyPatch.IsBusy = false;
            PanelPatch.IsEnabled = true;
        }
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

    private async void Unlock(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Fastboot"))
            {
                BusyUnlock.IsBusy = true;
                UnlockPanel.IsEnabled = false;
                if (!string.IsNullOrEmpty(UnlockFile.Text) && !string.IsNullOrEmpty(UnlockCode.Text))
                {
                    SukiHost.ShowDialog(new PureDialog("请勿同时填写两种方式！"), allowBackgroundClose: true);
                }
                else if (!string.IsNullOrEmpty(UnlockFile.Text) && string.IsNullOrEmpty(UnlockCode.Text))
                {
                    await CallExternalProgram.Fastboot($"-s {Global.thisdevice} flash unlock \"{UnlockFile.Text}\"");
                    string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem unlock-go");
                    if (output.Contains("OKAY"))
                    {
                        SukiHost.ShowDialog(new PureDialog("解锁成功!"), allowBackgroundClose: true);
                    }
                    else
                    {
                        SukiHost.ShowDialog(new PureDialog("解锁失败！"), allowBackgroundClose: true);
                    }
                }
                else if (string.IsNullOrEmpty(UnlockFile.Text) && !string.IsNullOrEmpty(UnlockCode.Text))
                {
                    string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem unlock {UnlockCode.Text}");
                    if (output.Contains("OKAY"))
                    {
                        SukiHost.ShowDialog(new PureDialog("解锁成功！"), allowBackgroundClose: true);
                    }
                    else
                    {
                        SukiHost.ShowDialog(new PureDialog("解锁失败！"), allowBackgroundClose: true);
                    }
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请选择解锁文件,或输入解锁码！"), allowBackgroundClose: true);
                }
                BusyUnlock.IsBusy = false;
                UnlockPanel.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请进入Fastboot模式！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
    }

    private async void Lock(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Fastboot"))
            {
                BusyUnlock.IsBusy = true;
                UnlockPanel.IsEnabled = false;
                await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem lock-go");
                string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} flashing lock");
                if (output.Contains("OKAY"))
                {
                    SukiHost.ShowDialog(new PureDialog("回锁成功！"), allowBackgroundClose: true);
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("回锁失败！"), allowBackgroundClose: true);
                }
                BusyUnlock.IsBusy = false;
                UnlockPanel.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请进入Fastboot模式！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
    }

    private async void BaseUnlock(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Fastboot"))
            {
                BusyBaseUnlock.IsBusy = true;
                BaseUnlockPanel.IsEnabled = false;
                if (SimpleContent.SelectedItem != null)
                {
                    var newDialog = new ConnectionDialog("该功能仅支持部分品牌设备！\n\r执行后您的设备应当出现确认解锁提示，\n\r若未出现则为您的设备不支持该操作。");
                    await SukiHost.ShowDialogAsync(newDialog);
                    if (newDialog.Result == true)
                    {
                        await CallExternalProgram.Fastboot($"-s {Global.thisdevice} {SimpleContent.SelectedItem}");
                        SukiHost.ShowDialog(new PureDialog("执行完成，请查看您的设备！"), allowBackgroundClose: true);
                    }
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请选择解锁命令！"), allowBackgroundClose: true);
                }
                BusyBaseUnlock.IsBusy = false;
                BaseUnlockPanel.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请进入Fastboot模式！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
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

    private async Task FlashRec(string shell)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Fastboot"))
            {
                BusyFlash.IsBusy = true;
                FlashRecovery.IsEnabled = false;
                if (!string.IsNullOrEmpty(RecFile.Text))
                {
                    string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} {shell} \"{RecFile.Text}\"");
                    if (!output.Contains("FAILED") && !output.Contains("error"))
                    {
                        var newDialog = new ConnectionDialog("刷入成功！是否重启到Recovery？");
                        await SukiHost.ShowDialogAsync(newDialog);
                        if (newDialog.Result == true)
                        {
                            output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem reboot-recovery");
                            if (output.Contains("unknown command"))
                            {
                                await CallExternalProgram.Fastboot($"-s {Global.thisdevice} flash misc bin/img/misc.img");
                                await CallExternalProgram.Fastboot($"-s {Global.thisdevice} reboot");
                            }
                        }
                    }
                    else
                    {
                        SukiHost.ShowDialog(new PureDialog("刷入失败！"), allowBackgroundClose: true);
                    }
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请选择Recovery文件！"), allowBackgroundClose: true);
                }
                BusyFlash.IsBusy = false;
                FlashRecovery.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请进入Fastboot模式！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
    }

    private async void FlashToRec(object sender, RoutedEventArgs args)
    {
        await FlashRec("flash recovery");
    }

    private async void FlashToRecA(object sender, RoutedEventArgs args)
    {
        await FlashRec("flash recovery_a");
    }

    private async void FlashToRecB(object sender, RoutedEventArgs args)
    {
        await FlashRec("flash recovery_b");
    }

    private async void BootRec(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Fastboot"))
            {
                BusyFlash.IsBusy = true;
                FlashRecovery.IsEnabled = false;
                if (!string.IsNullOrEmpty(RecFile.Text))
                {
                    string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} boot \"{RecFile.Text}\"");
                    if (output.Contains("Finished"))
                    {
                        SukiHost.ShowDialog(new PureDialog("启动成功！"), allowBackgroundClose: true);
                    }
                    else
                    {
                        SukiHost.ShowDialog(new PureDialog("启动失败！"), allowBackgroundClose: true);
                    }
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请选择Recovery文件！"), allowBackgroundClose: true);
                }
                BusyFlash.IsBusy = false;
                FlashRecovery.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请进入Fastboot模式！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
    }

    private async void FlashToBootA(object sender, RoutedEventArgs args)
    {
        await FlashRec("flash boot_a");
    }

    private async void FlashToBootB(object sender, RoutedEventArgs args)
    {
        await FlashRec("flash boot_b");
    }

    private async void OpenMagiskFile(object sender, RoutedEventArgs args)
    {
        patch_busy(true);
        var topLevel = TopLevel.GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false
        });
        if (files.Count == 0)
        {
            patch_busy(false);
            return;
        }
        MagiskFile.Text = Uri.UnescapeDataString(StringHelper.FilePath(files[0].Path.ToString()));
        ZipInfo.tmp_path = Path.Combine(Global.tmp_path, "Zip-" + StringHelper.RandomString(8));
        bool istempclean = FileHelper.ClearFolder(ZipInfo.tmp_path);
        if (istempclean)
        {
            string outputzip = await CallExternalProgram.SevenZip($"x \"{MagiskFile.Text}\" -o\"{ZipInfo.tmp_path}\" -y");
            string pattern_MAGISK_VER = @"MAGISK_VER='([^']+)'";
            string pattern_MAGISK_VER_CODE = @"MAGISK_VER_CODE=(\d+)";
            string Magisk_sh_path = Path.Combine(ZipInfo.tmp_path, "assets", "util_functions.sh");
            string MAGISK_VER = StringHelper.FileRegex(Magisk_sh_path, pattern_MAGISK_VER, 1);
            string MAGISK_VER_CODE = StringHelper.FileRegex(Magisk_sh_path, pattern_MAGISK_VER_CODE, 1);
            if ((MAGISK_VER != null) & (MAGISK_VER_CODE != null))
            {
                string BOOT_PATCH_PATH = Path.Combine(ZipInfo.tmp_path, "assets", "boot_patch.sh");
                string md5 = FileHelper.Md5Hash(BOOT_PATCH_PATH);
                bool Magisk_Valid = BootPatchHelper.Magisk_Validation(md5, MAGISK_VER);
                if (Magisk_Valid)
                {
                    File.Copy(Path.Combine(ZipInfo.tmp_path, "lib", "armeabi-v7a", "libmagisk32.so"), Path.Combine(ZipInfo.tmp_path, "lib", "arm64-v8a", "libmagisk32.so"));
                    File.Copy(Path.Combine(ZipInfo.tmp_path, "lib", "x86", "libmagisk32.so"), Path.Combine(ZipInfo.tmp_path, "lib", "x86_64", "libmagisk32.so"));
                    ZipInfo.userful = true;
                }
                patch_busy(false);
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("未能获取到有效Magisk版本号"), allowBackgroundClose: true);
                patch_busy(false);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("清理临时目录出错"), allowBackgroundClose: true);
            patch_busy(false);
        }

    }

    private async void OpenBootFile(object sender, RoutedEventArgs args)
    {
        patch_busy(true);
        var topLevel = TopLevel.GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false
        });
        if (files.Count == 0)
        {
            patch_busy(false);
            return;
        }
        BootFile.Text = Uri.UnescapeDataString(StringHelper.FilePath(files[0].Path.ToString()));
        BootInfo.SHA1 = FileHelper.SHA1Hash(BootFile.Text);
        if (BootInfo.SHA1 == null)
        {
            patch_busy(false);
            return;
        }
        //在临时目录创建临时boot目录，这破东西跨平台解压各种问题，直接即用即丢了
        BootInfo.tmp_path = Path.Combine(Global.tmp_path, "Boot-" + StringHelper.RandomString(8));
        string workpath = BootInfo.tmp_path;
        if (FileHelper.ClearFolder(workpath))
        {
            string osVersionPattern = @"OS_VERSION\s+\[(.*?)\]";
            string osPatchLevelPattern = @"OS_PATCH_LEVEL\s+\[(.*?)\]";
            (string mb_output, Global.mb_exitcode) = await CallExternalProgram.MagiskBoot($"unpack \"{BootFile.Text}\"", BootInfo.tmp_path);
            if (mb_output.Contains("error"))
            {
                SukiHost.ShowDialog(new PureDialog("请选择有效Boot文件"), allowBackgroundClose: true);
                patch_busy(false);
                return;
            }
            BootInfo.os_version = StringHelper.StringRegex(mb_output, osVersionPattern, 1);
            BootInfo.patch_level = StringHelper.StringRegex(mb_output, osPatchLevelPattern, 1);
            BootPatchHelper.dtb_detect();
            await BootPatchHelper.kernel_detect();
            await BootPatchHelper.ramdisk_detect();
            SukiHost.ShowDialog(new PureDialog($"Boot内检测到\nArch:{BootInfo.arch}\nOS:{BootInfo.os_version}\nPatch_level:{BootInfo.patch_level}\nRamdisk:{BootInfo.have_ramdisk}\nKMI:{BootInfo.kmi}"), allowBackgroundClose: true);
            ArchList.SelectedItem = BootInfo.arch;
            patch_busy(false);
        }
    }
    private async void StartPatch(object sender, RoutedEventArgs args)
    {
        if (!BootInfo.userful | !ZipInfo.userful | !BootInfo.have_ramdisk)
        {
            SukiHost.ShowDialog(new PureDialog("请选择有效的面具与镜像文件"), allowBackgroundClose: true);
            return;
        }
        if (!BootPatchHelper.CheckComponentFiles(ZipInfo.tmp_path, ArchList.SelectedItem.ToString()))
        {
            SukiHost.ShowDialog(new PureDialog("文件预处理时出错！"), allowBackgroundClose: true);
            return;
        }
        patch_busy(true);
        //设置环境变量
        EnvironmentVariable.KEEPVERITY = KEEPVERITY.IsChecked.ToString().ToLower();
        EnvironmentVariable.KEEPFORCEENCRYPT = KEEPFORCEENCRYPT.IsChecked.ToString().ToLower();
        EnvironmentVariable.PATCHVBMETAFLAG = PATCHVBMETAFLAG.IsChecked.ToString().ToLower();
        EnvironmentVariable.RECOVERYMODE = RECOVERYMODE.IsChecked.ToString().ToLower();
        EnvironmentVariable.LEGACYSAR = LEGACYSAR.IsChecked.ToString().ToLower();
        string archSubfolder = ArchList.SelectedItem.ToString() switch
        {
            "aarch64" => "arm64-v8a",
            "armeabi" => "armeabi-v7a",
            "X86" => "x86",
            "X86-64" => "x86_64",
            _ => throw new ArgumentException($"未知架构：{ArchList.SelectedItem}")
        };
        string compPath = Path.Combine(Path.Combine(ZipInfo.tmp_path, "lib"), archSubfolder);
        try
        {
            File.Copy(Path.Combine((compPath), "libmagisk32.so"), Path.Combine((compPath), "magisk32"), true);
            await CallExternalProgram.MagiskBoot($"compress=xz magisk32 magisk32.xz", compPath);
            if (File.Exists(Path.Combine((compPath), "libmagisk64.so")))
            {
                File.Copy(Path.Combine((compPath), "libmagisk64.so"), Path.Combine((compPath), "magisk64"), true);
                await CallExternalProgram.MagiskBoot($"compress=xz magisk64 magisk64.xz", compPath);
            }
        }
        catch (Exception ex)
        {
            SukiHost.ShowDialog(new ErrorDialog("magisk组件预处理时 " + ex));
            patch_busy(false);
            return;
        }
        (string mb_output, int exitcode) = await CallExternalProgram.MagiskBoot($"compress=xz stub.apk stub.xz", Path.Combine(ZipInfo.tmp_path, "assets"));
        if (mb_output.Contains("error"))
        {
            SukiHost.ShowDialog(new PureDialog("压缩stub.apk时出错"), allowBackgroundClose: true);
            patch_busy(false);
            return;
        }
        (mb_output, exitcode) = await CallExternalProgram.MagiskBoot($"cpio ramdisk.cpio test", BootInfo.tmp_path);
        int mode_code = exitcode & 3;
        switch (mode_code)
        {
            case 0:
                if (!BootPatchHelper.boot_img_pre(BootFile.Text))
                {
                    return;
                }
                break;
            case 1:
                try
                {
                    File.Copy(Path.Combine(BootInfo.tmp_path, "ramdisk", ".backup", ".magisk"), Path.Combine(BootInfo.tmp_path, "comfig.orig"), true);
                    (mb_output, exitcode) = await CallExternalProgram.MagiskBoot($"cpio ramdisk.cpio restore", BootInfo.tmp_path);
                    File.Copy(Path.Combine(BootInfo.tmp_path, "ramdisk.cpio"), Path.Combine(BootInfo.tmp_path, "ramdisk.cpio.orig"), true);
                    File.Delete(Path.Combine(BootInfo.tmp_path, "stock_boot.img"));
                    break;
                }
                catch (Exception e)
                {
                    SukiHost.ShowDialog(new ErrorDialog("1文件预处理时出错！" + e));
                    return;
                }
            case 2:
                SukiHost.ShowDialog(new ErrorDialog("镜像被未支持软件修补，请选择原生镜像！"));
                return;
            default:
                SukiHost.ShowDialog(new ErrorDialog("magiskboot检验出错"));
                return;
        }
        //patch ramdisk.cpio
        string config_path = Path.Combine(BootInfo.tmp_path, "config");
        File.WriteAllText(config_path, "");
        File.AppendAllText(config_path, $"KEEPVERITY={KEEPVERITY.IsChecked.ToString().ToLower()}\n");
        File.AppendAllText(config_path, $"KEEPFORCEENCRYPT={KEEPFORCEENCRYPT.IsChecked.ToString().ToLower()}\n");
        File.AppendAllText(config_path, $"RECOVERYMODE={RECOVERYMODE.IsChecked.ToString().ToLower()}\n");
        File.AppendAllText(config_path, $"SHA1={BootInfo.SHA1}\n");
        string allowedChars = "abcdef0123456789";
        Random random = new Random();
        string randomStr = new string(Enumerable.Repeat(allowedChars, 16)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        string configContent = $"RANDOMSEED=0x{randomStr}";
        File.AppendAllText(config_path, configContent + Environment.NewLine);
        bool success = await BootPatchHelper.ramdisk_patch(compPath);
        if (!success)
        {
            patch_busy(false);
            return;
        }
        //以上完成ramdisk.cpio的修补
        try
        {
            success = await BootPatchHelper.dtb_patch();
            if (!success)
            {
                patch_busy(false);
                return;
            }
            success = await BootPatchHelper.kernel_patch((bool)LEGACYSAR.IsChecked);
            if (BootPatchHelper.CleanBoot(BootInfo.tmp_path))
            {
                (mb_output, exitcode) = await CallExternalProgram.MagiskBoot($"repack \"{BootFile.Text}\"", BootInfo.tmp_path);
                File.Copy(Path.Combine(BootInfo.tmp_path, "new-boot.img"), Path.Combine(Path.GetDirectoryName(BootFile.Text), "boot_patched_" + randomStr + ".img"), true);
                SukiHost.ShowDialog(new PureDialog("面具修补完成"), allowBackgroundClose: true);
                patch_busy(false);
                FileHelper.OpenFolder(Path.GetDirectoryName(BootFile.Text));
                ZipInfo.userful = false;
                BootInfo.userful = false;
                BootFile.Text = null;
                MagiskFile.Text = null;
                ArchList.SelectedItem = null;
                return;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("清理打包目录失败"), allowBackgroundClose: true);
                patch_busy(false);
                return;
            }
        }
        catch (Exception ex)
        {
            SukiHost.ShowDialog(new ErrorDialog(ex.Message));
            patch_busy(false);
            return;
        }
    }

    private async void OpenAFDI(object sender, RoutedEventArgs args)
    {
        if (Global.System == "Windows")
        {
            if (RuntimeInformation.OSArchitecture == Architecture.X64)
            {
                Process.Start(@"Drive\adb.exe");
            }
            else if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                string drvpath = String.Format($"{Global.runpath}/Drive/adb/*.inf");
                string shell = String.Format("/add-driver {0} /subdirs /install", drvpath);
                string drvlog = await CallExternalProgram.Pnputil(shell);
                FileHelper.Write($"{Global.log_path}/drive.txt", drvlog);
                if (drvlog.Contains("成功"))
                {
                    SukiHost.ShowDialog(new PureDialog("安装完成！"), allowBackgroundClose: true);
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("安装失败！"), allowBackgroundClose: true);
                }
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("当前设备无需进行此操作！"), allowBackgroundClose: true);
        }
    }

    private async void Open9008DI(object sender, RoutedEventArgs args)
    {
        if (Global.System == "Windows")
        {
            if (RuntimeInformation.OSArchitecture == Architecture.X64)
            {
                Process.Start(@"Drive\Qualcomm_HS-USB_Driver.exe");
            }
            else if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                string drvpath = String.Format($"{Global.runpath}/drive/9008/*.inf");
                string shell = String.Format("/add-driver {0} /subdirs /install", drvpath);
                string drvlog = await CallExternalProgram.Pnputil(shell);
                FileHelper.Write($"{Global.log_path}/drive.txt", drvlog);
                if (drvlog.Contains("成功"))
                {
                    SukiHost.ShowDialog(new PureDialog("安装完成！"), allowBackgroundClose: true);
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("安装失败！"), allowBackgroundClose: true);
                }
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("当前设备无需进行此操作！"), allowBackgroundClose: true);
        }
    }

    private async void OpenUSBP(object sender, RoutedEventArgs args)
    {
        if (Global.System == "Windows")
        {
            string cmd = @"drive\USB3.bat";
            ProcessStartInfo cmdshell = null;
            cmdshell = new ProcessStartInfo(cmd)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process f = Process.Start(cmdshell);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SukiHost.ShowDialog(new PureDialog("执行完成！"), allowBackgroundClose: true);
            });
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("当前设备无需进行此操作！"), allowBackgroundClose: true);
        }
    }

    private async void FlashMagisk(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Recovery")
            {
                if (MagiskFile.Text != null)
                {
                    BusyInstall.IsBusy = true;
                    InstallZIP.IsEnabled = false;
                    if (TWRPInstall.IsChecked == true)
                    {
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} push {MagiskFile.Text} /tmp/magisk.apk");
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp install /tmp/magisk.apk");
                    }
                    else if (ADBSideload.IsChecked == true)
                    {
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} sideload \"{MagiskFile.Text}\"");
                    }
                    SukiHost.ShowDialog(new PureDialog("执行完成！"), allowBackgroundClose: true);
                    BusyInstall.IsBusy = false;
                    InstallZIP.IsEnabled = true;
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请在右侧选择Magisk文件！"), allowBackgroundClose: true);
                }
            }
            else if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                if (MagiskFile.Text != null)
                {
                    BusyInstall.IsBusy = true;
                    InstallZIP.IsEnabled = false;
                    var newDialog = new ConnectionDialog("检测到当前为系统模式，是否推送Magisk应用？");
                    await SukiHost.ShowDialogAsync(newDialog);
                    if (newDialog.Result == true)
                    {
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} push \"{MagiskFile.Text}\" /sdcard/magisk.apk");
                        SukiHost.ShowDialog(new PureDialog("已推送至根目录，请自行安装。"), allowBackgroundClose: true);
                    }
                    BusyInstall.IsBusy = false;
                    InstallZIP.IsEnabled = true;
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请在右侧选择Magisk文件！"), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请进入Recovery模式！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
    }

    private async void DisableOffRec(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Recovery")
            {
                BusyInstall.IsBusy = true;
                InstallZIP.IsEnabled = false;
                if (TWRPInstall.IsChecked == true)
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} push {Global.runpath}/ZIP/DisableAutoRecovery.zip /tmp/");
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp install /tmp/DisableAutoRecovery.zip");
                }
                else if (ADBSideload.IsChecked == true)
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} sideload ZIP/DisableAutoRecovery.zip");
                }
                SukiHost.ShowDialog(new PureDialog("执行完成！"), allowBackgroundClose: true);
                BusyInstall.IsBusy = false;
                InstallZIP.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请进入Recovery模式！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
    }

    private async void SyncAB(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Recovery")
            {
                BusyInstall.IsBusy = true;
                InstallZIP.IsEnabled = false;
                if (TWRPInstall.IsChecked == true)
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} push {Global.runpath}/ZIP/copy-partitions.zip /tmp/");
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp install /tmp/copy-partitions.zip");
                }
                else if (ADBSideload.IsChecked == true)
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} sideload ZIP/copy-partitions.zip");
                }
                SukiHost.ShowDialog(new PureDialog("执行完成！"), allowBackgroundClose: true);
                BusyInstall.IsBusy = false;
                InstallZIP.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请进入Recovery模式！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
    }
}