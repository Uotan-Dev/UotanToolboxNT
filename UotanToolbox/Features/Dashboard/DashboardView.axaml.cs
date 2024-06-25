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
using System.Threading;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Features.Dashboard;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
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
        MagiskFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        Global.magisk_tmp = Path.Combine(Global.tmp_path, "Magisk-" + StringHelper.RandomString(8));
        bool istempclean = FileHelper.ClearFolder(Global.magisk_tmp);
        if (istempclean)
        {
            string outputzip = await CallExternalProgram.SevenZip($"x \"{MagiskFile.Text}\" -o\"{Global.magisk_tmp}\" -y");
            string pattern_MAGISK_VER = @"MAGISK_VER='([^']+)'";
            string pattern_MAGISK_VER_CODE = @"MAGISK_VER_CODE=(\d+)";
            string Magisk_sh_path = Path.Combine(Global.magisk_tmp, "assets", "util_functions.sh");
            string MAGISK_VER = StringHelper.FileRegex(Magisk_sh_path, pattern_MAGISK_VER, 1);
            string MAGISK_VER_CODE = StringHelper.FileRegex(Magisk_sh_path, pattern_MAGISK_VER_CODE, 1);
            if ((MAGISK_VER != null) & (MAGISK_VER_CODE != null))
            {
                string BOOT_PATCH_PATH = Path.Combine(Global.magisk_tmp, "assets", "boot_patch.sh");
                string md5 = FileHelper.Md5Hash(BOOT_PATCH_PATH);
                bool Magisk_Valid = MagiskHelper.Magisk_Validation(md5, MAGISK_VER);
                if (Magisk_Valid)
                {
                    Global.is_magisk_ok = true;
                }
                patch_busy(false);
            }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("未能获取到有效Magisk版本号"));
                patch_busy(false);
            }
        }
        else
        {
            SukiHost.ShowDialog(new ConnectionDialog("清理临时目录出错"));
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
        BootFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        Global.boot_sha1 = FileHelper.SHA1Hash(BootFile.Text);
        //在临时目录创建临时boot目录，这破东西跨平台解压各种问题，直接即用即丢了
        Global.boot_tmp = Path.Combine(Global.tmp_path, "Boot-" + StringHelper.RandomString(8));
        string workpath = Global.boot_tmp; // 不这样搞会报错，莫名其妙
        if (FileHelper.ClearFolder(workpath))
        {
            (string mb_output, Global.mb_exitcode) = await CallExternalProgram.MagiskBoot($"unpack \"{BootFile.Text}\"", Global.boot_tmp);
            if (mb_output.Contains("error"))
            {
                SukiHost.ShowDialog(new ConnectionDialog("解包失败"));
                patch_busy(false);
                return;
            }
            string cpio_path = Path.Combine(Global.boot_tmp, "ramdisk.cpio");
            string ramdisk = Path.Combine(Global.boot_tmp, "ramdisk");
            //适配Windows的抽象magiskboot（使用cygwin），其他平台都是原生编译的，可以直接用参数提取ramdisk
            if (Global.System != "Windows")
            {
                workpath = Path.Combine(Global.boot_tmp, "ramdisk");
                Directory.CreateDirectory(workpath);
            }
            (string outputcpio, Global.cpio_exitcode) = await CallExternalProgram.MagiskBoot($"cpio \"{cpio_path}\" extract", workpath);
            string init_info = await CallExternalProgram.File($"\"{Path.Combine(ramdisk, "init")}\"");
            //下面是根据镜像的init架构来推定整个Boot.img文件的架构，但是逻辑写的相当的屎，你有更好的想法可以来改
            if (init_info.Contains("ARM aarch64"))
            {
                SukiHost.ShowDialog(new ConnectionDialog("检测到可用AArch64镜像"));
                ArchList.SelectedItem = "aarch64";
                Global.is_boot_ok = true;
                patch_busy(false);
            }
            else if (init_info.Contains("X86-64"))
            {
                SukiHost.ShowDialog(new ConnectionDialog("检测到可用X86-64镜像"));
                ArchList.SelectedItem = "X86-64";
                Global.is_boot_ok = true;
                patch_busy(false);
            }
            else if (init_info.Contains("ARM,"))
            {
                SukiHost.ShowDialog(new ConnectionDialog("检测到可用ARM镜像"));
                ArchList.SelectedItem = "armeabi";
                Global.is_boot_ok = true;
                patch_busy(false);
            }
            else if (init_info.Contains(" Intel 80386"))
            {
                SukiHost.ShowDialog(new ConnectionDialog("检测到可用X86镜像"));
                ArchList.SelectedItem = "X86";
                Global.is_boot_ok = true;
                patch_busy(false);
            }
            //有些设备的init路径是/bin/init而不是/init,在这里再做一次检测
            init_info = await CallExternalProgram.File($"\"{Path.Combine(ramdisk, "system", "bin", "init")}\"");
            if (init_info.Contains("ARM aarch64"))
            {
                SukiHost.ShowDialog(new ConnectionDialog("检测到可用AArch64镜像"));
                ArchList.SelectedItem = "aarch64";
                Global.is_boot_ok = true;
                patch_busy(false);
            }
            else if (init_info.Contains("X86-64"))
            {
                SukiHost.ShowDialog(new ConnectionDialog("检测到可用X86-64镜像"));
                ArchList.SelectedItem = "X86-64";
                Global.is_boot_ok = true;
                patch_busy(false);
            }
            else if (init_info.Contains("ARM,"))
            {
                SukiHost.ShowDialog(new ConnectionDialog("检测到可用ARM镜像"));
                ArchList.SelectedItem = "armeabi";
                Global.is_boot_ok = true;
                patch_busy(false);
            }
            else if (init_info.Contains(" Intel 80386"))
            {
                SukiHost.ShowDialog(new ConnectionDialog("检测到可用X86镜像"));
                ArchList.SelectedItem = "X86";
                Global.is_boot_ok = true;
                patch_busy(false);
            }
        }
    }
    private async void StartPatch(object sender, RoutedEventArgs args)
    {
        if (!Global.is_boot_ok | !Global.is_magisk_ok)
        {
            SukiHost.ShowDialog(new ConnectionDialog("请选择有效的面具与镜像文件"));
            return;
        }
        if (!MagiskHelper.CheckComponentFiles(Global.magisk_tmp, ArchList.SelectedItem.ToString()))
        {
            SukiHost.ShowDialog(new ConnectionDialog("文件预处理时出错！"));
            return;
        }
        patch_busy(true);
        //设置环境变量
        string env_KEEPVERITY = KEEPVERITY.IsChecked.ToString().ToLower();
        string env_KEEPFORCEENCRYPT = KEEPFORCEENCRYPT.IsChecked.ToString().ToLower();
        string env_PATCHVBMETAFLAG = PATCHVBMETAFLAG.IsChecked.ToString().ToLower();
        string env_RECOVERYMODE = RECOVERYMODE.IsChecked.ToString().ToLower();
        string env_LEGACYSAR = LEGACYSAR.IsChecked.ToString().ToLower();

        string compPathBase = System.IO.Path.Combine(Global.magisk_tmp, "lib");
        string archSubfolder = ArchList.SelectedItem.ToString() switch
        {
            "aarch64" => "arm64-v8a",
            "armeabi" => "armeabi-v7a",
            "X86" => "x86",
            "X86-64" => "x86_64",
            _ => throw new ArgumentException($"未知架构：{ArchList.SelectedItem.ToString()}")
        };
        string archSubfolder2 = ArchList.SelectedItem.ToString() switch
        {
            "aarch64" => "armeabi-v7a",
            "X86-64" => "x86",
            _ => ""
        };
        string compPath = System.IO.Path.Combine(compPathBase, archSubfolder);
        string sub_compPath = System.IO.Path.Combine(compPathBase, archSubfolder2);
        if (archSubfolder2 != "")
        {
            try
            {
                File.Copy(Path.Combine((sub_compPath), "libmagisk32.so"), Path.Combine((compPath), "libmagisk32.so"), true);
            }
            catch (Exception ex)
            {
                SukiHost.ShowDialog(new ConnectionDialog("64位magisk32组件预处理时 " + ex));
                patch_busy(false);
                return;
            }
        }
        if (File.Exists(Path.Combine((compPath), "libmagisk32.so")))
        {
            try
            {
                File.Copy(Path.Combine((compPath), "libmagisk32.so"), Path.Combine((compPath), "magisk32"), true);
                await CallExternalProgram.MagiskBoot($"compress=xz magisk32 magisk32.xz", compPath);
            }
            catch (Exception ex)
            {
                SukiHost.ShowDialog(new ConnectionDialog("magisk32组件预处理时 " + ex));
                patch_busy(false);
                return;
            }
        }
        if (File.Exists(Path.Combine((compPath), "libmagisk64.so")))
        {
            try
            {
                File.Copy(Path.Combine((compPath), "libmagisk64.so"), Path.Combine((compPath), "magisk64"), true);
                await CallExternalProgram.MagiskBoot($"compress=xz magisk64 magisk64.xz", compPath);
            }
            catch (Exception ex)
            {
                SukiHost.ShowDialog(new ConnectionDialog("magisk32组件预处理时 " + ex));
                patch_busy(false);
                return;
            }
        }
        (string mb_output, int exitcode) = await CallExternalProgram.MagiskBoot($"compress=xz stub.apk stub.xz", Path.Combine(Global.magisk_tmp, "assets"));
        if (mb_output.Contains("error"))
        {
            SukiHost.ShowDialog(new ConnectionDialog("压缩stub.apk时出错"));
            patch_busy(false);
            return;
        }
        (mb_output, exitcode) = await CallExternalProgram.MagiskBoot($"cpio ramdisk.cpio test", Global.boot_tmp);
        int mode_code = exitcode & 3;
        switch (mode_code)
        {
            case 0:
                try
                {
                    File.Copy(BootFile.Text, Path.Combine(Global.boot_tmp, "stock_boot.img"), true);
                    File.Copy(Path.Combine(Global.boot_tmp, "ramdisk.cpio"), Path.Combine(Global.boot_tmp, "ramdisk.cpio.orig"), true);
                    break;
                }
                catch (Exception e)
                {
                    SukiHost.ShowDialog(new ConnectionDialog("0文件预处理时出错！" + e));
                    break;
                }
            case 1:
                try
                {
                    File.Copy(Path.Combine(Global.boot_tmp, "ramdisk", ".backup", ".magisk"), Path.Combine(Global.boot_tmp, "comfig.orig"), true);
                    (mb_output, exitcode) = await CallExternalProgram.MagiskBoot($"cpio ramdisk.cpio restore", Global.boot_tmp);
                    File.Copy(Path.Combine(Global.boot_tmp, "ramdisk.cpio"), Path.Combine(Global.boot_tmp, "ramdisk.cpio.orig"), true);
                    File.Delete(Path.Combine(Global.boot_tmp, "stock_boot.img"));
                    break;
                }
                catch (Exception e)
                {
                    SukiHost.ShowDialog(new ConnectionDialog("1文件预处理时出错！" + e));
                    break;
                }
            case 2:
                SukiHost.ShowDialog(new ConnectionDialog("镜像被未支持软件修补，请选择原生镜像！"), allowBackgroundClose: true);
                break;
            default:
                SukiHost.ShowDialog(new ConnectionDialog("magiskboot检验出错"));
                break;
        }
        //patch ramdisk.cpio
        string config_path = Path.Combine(Global.boot_tmp, "config");
        File.WriteAllText(config_path, "");
        File.AppendAllText(config_path, $"KEEPVERITY={KEEPVERITY.IsChecked.ToString().ToLower()}\n");
        File.AppendAllText(config_path, $"KEEPFORCEENCRYPT={KEEPFORCEENCRYPT.IsChecked.ToString().ToLower()}\n");
        File.AppendAllText(config_path, $"RECOVERYMODE={RECOVERYMODE.IsChecked.ToString().ToLower()}\n");
        File.AppendAllText(config_path, $"SHA1={Global.boot_sha1}\n");
        string allowedChars = "abcdef0123456789";
        Random random = new Random();
        string randomStr = new string(Enumerable.Repeat(allowedChars, 16)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        string configContent = $"RANDOMSEED=0x{randomStr}";
        File.AppendAllText(config_path, configContent + Environment.NewLine);
        if (MagiskHelper.comp_copy(compPath))
        {
            (mb_output, exitcode) = await CallExternalProgram.MagiskBoot("cpio ramdisk.cpio \"add 0750 init magiskinit\" \"mkdir 0750 overlay.d\" \"mkdir 0750 overlay.d/sbin\" \"add 0644 overlay.d/sbin/magisk32.xz magisk32.xz\" ", Global.boot_tmp);
            (mb_output, exitcode) = await CallExternalProgram.MagiskBoot("cpio ramdisk.cpio \"add 0644 overlay.d/sbin/stub.xz stub.xz\" \"patch\" \"backup ramdisk.cpio.orig\" \"mkdir 000 .backup\" \"add 000 .backup/.magisk config\"", Global.boot_tmp, env_KEEPVERITY, env_KEEPFORCEENCRYPT, env_PATCHVBMETAFLAG, env_RECOVERYMODE, env_LEGACYSAR);
        }
        if (File.Exists(Path.Combine((compPath), "magisk64.xz")))
        {
            (mb_output, exitcode) = await CallExternalProgram.MagiskBoot("cpio ramdisk.cpio \"add 0644 overlay.d/sbin/magisk64.xz magisk64.xz\"", Global.boot_tmp);
        }
        //以上完成ramdisk.cpio的修补
        string dtb_name = MagiskHelper.dtb_detect(Global.boot_tmp);
        if (dtb_name != null)
        {
            (mb_output, exitcode) = await CallExternalProgram.MagiskBoot($"dtb {dtb_name} test", Global.boot_tmp);
            if (exitcode != 0)
            {
                SukiHost.ShowDialog(new ConnectionDialog("dtb验证失败"));
                patch_busy(false);
                return;
            }
            (mb_output, exitcode) = await CallExternalProgram.MagiskBoot($"dtb {dtb_name} patch", Global.boot_tmp, env_KEEPVERITY, env_KEEPFORCEENCRYPT, env_PATCHVBMETAFLAG, env_RECOVERYMODE, env_LEGACYSAR);
        }
        bool kernel_patched = false;
        if (File.Exists(Path.Combine(Global.boot_tmp, "kernel")))
        {
            (mb_output, exitcode) = await CallExternalProgram.MagiskBoot($"hexpatch kernel 49010054011440B93FA00F71E9000054010840B93FA00F7189000054001840B91FA00F7188010054 A1020054011440B93FA00F7140020054010840B93FA00F71E0010054001840B91FA00F7181010054", Global.boot_tmp);
            if (exitcode == 0)
            {
                kernel_patched = true;
            }
            (mb_output, exitcode) = await CallExternalProgram.MagiskBoot($"hexpatch kernel 821B8012 E2FF8F12", Global.boot_tmp);
            if (exitcode == 0)
            {
                kernel_patched = true;
            }
            if ((bool)LEGACYSAR.IsChecked)
            {
                (mb_output, exitcode) = await CallExternalProgram.MagiskBoot($"hexpatch kernel 736B69705F696E697472616D667300 77616E745F696E697472616D667300", Global.boot_tmp);
                if (exitcode == 0)
                {
                    kernel_patched = true;
                }
            }
            if (!kernel_patched)
            {
                try
                {
                    File.Delete(Path.Combine(Global.boot_tmp, "kernel"));
                }
                catch (Exception ex)
                {
                    SukiHost.ShowDialog(new ConnectionDialog("kernel删除失败" + ex));
                    patch_busy(false);
                    return;
                }

            }
        }
        try
        {
            if (MagiskHelper.CleanBoot(Global.boot_tmp))
            {
                (mb_output, exitcode) = await CallExternalProgram.MagiskBoot($"repack \"{BootFile.Text}\"", Global.boot_tmp, env_KEEPVERITY, env_KEEPFORCEENCRYPT, env_PATCHVBMETAFLAG, env_RECOVERYMODE, env_LEGACYSAR);
                File.Copy(Path.Combine(Global.boot_tmp, "new-boot.img"), Path.Combine(Path.GetDirectoryName(BootFile.Text), "boot_patched_" + randomStr + ".img"), true);
                SukiHost.ShowDialog(new ConnectionDialog("面具修补完成"));
                patch_busy(false);
                FileHelper.OpenFolder(Path.GetDirectoryName(BootFile.Text));
                Global.is_boot_ok = false;
                Global.is_magisk_ok = false;
                BootFile.Text = null;
                MagiskFile.Text = null;
                ArchList.SelectedItem = null;
                return;
            }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("清理打包目录失败"));
                patch_busy(false);
                return;
            }
        }
        catch (Exception ex)
        {
            SukiHost.ShowDialog(new ConnectionDialog(ex.Message));
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
                string drvpath = String.Format(@"{0}\Drive\adb\*.inf", Global.runpath);
                string shell = String.Format("/add-driver {0} /subdirs /install", drvpath);
                string drvlog = await CallExternalProgram.Pnputil(shell);
                FileHelper.Write($"{Global.runpath}/Log/drive.txt", drvlog);
                if (drvlog.Contains("成功"))
                {
                    SukiHost.ShowDialog(new ConnectionDialog("安装完成！"));
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog("安装失败！"));
                }
            }
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SukiHost.ShowDialog(new ConnectionDialog("当前设备无需进行此操作！"));
            });
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
                string drvpath = String.Format(@"{0}\drive\9008\*.inf", Global.runpath);
                string shell = String.Format("/add-driver {0} /subdirs /install", drvpath);
                string drvlog = await CallExternalProgram.Pnputil(shell);
                FileHelper.Write($"{Global.runpath}/Log/drive.txt", drvlog);
                if (drvlog.Contains("成功"))
                {
                    SukiHost.ShowDialog(new ConnectionDialog("安装完成！"));
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog("安装失败！"));
                }
            }
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SukiHost.ShowDialog(new ConnectionDialog("当前设备无需进行此操作！"));
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
                SukiHost.ShowDialog(new ConnectionDialog("执行完成！"));
            });
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SukiHost.ShowDialog(new ConnectionDialog("当前设备无需进行此操作！"));
            });
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
                    if (TWRPInstall.IsChecked == true)
                    {
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} push {MagiskFile.Text} /tmp/");
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp install /tmp/{Path.GetFileNameWithoutExtension(MagiskFile.Text)}.");
                    }
                    else if (ADBSideload.IsChecked == true)
                    {
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} sideload \"{MagiskFile.Text}\"");
                    }
                    SukiHost.ShowDialog(new ConnectionDialog("执行完成！"));
                    BusyInstall.IsBusy = false;
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog("请在右侧选择Magisk文件！"));
                }
            }
            else if (sukiViewModel.Status == "系统")
            {
                if (MagiskFile.Text != null)
                {
                    BusyInstall.IsBusy = true;
                    var newDialog = new ConnectionDialog("检测到当前为系统模式，是否推送Magisk应用？");
                    await SukiHost.ShowDialogAsync(newDialog);
                    if (newDialog.Result == true)
                    {
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} push {MagiskFile.Text} /sdcard");
                        SukiHost.ShowDialog(new ConnectionDialog("已推送至根目录，请自行安装。"));
                    }
                    BusyInstall.IsBusy = false;
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog("请在右侧选择Magisk文件！"));
                }
            }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("请进入Recovery模式！"));
            }
        }
        else
        {
            SukiHost.ShowDialog(new ConnectionDialog("设备未连接！"));
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
                if (TWRPInstall.IsChecked == true)
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} push {Global.runpath}/ZIP/DisableAutoRecovery.zip /tmp/");
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp install /tmp/DisableAutoRecovery.zip");
                }
                else if (ADBSideload.IsChecked == true)
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} sideload ZIP/DisableAutoRecovery.zip");
                }
                SukiHost.ShowDialog(new ConnectionDialog("执行完成！"));
                BusyInstall.IsBusy = false;
            }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("请进入Recovery模式！"));
            }
        }
        else
        {
            SukiHost.ShowDialog(new ConnectionDialog("设备未连接！"));
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
                if (TWRPInstall.IsChecked == true)
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} push {Global.runpath}/ZIP/copy-partitions.zip /tmp/");
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp install /tmp/copy-partitions.zip");
                }
                else if (ADBSideload.IsChecked == true)
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} sideload ZIP/copy-partitions.zip");
                }
                SukiHost.ShowDialog(new ConnectionDialog("执行完成！"));
                BusyInstall.IsBusy = false;
            }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("请进入Recovery模式！"));
            }
        }
        else
        {
            SukiHost.ShowDialog(new ConnectionDialog("设备未连接！"));
        }
    }
}