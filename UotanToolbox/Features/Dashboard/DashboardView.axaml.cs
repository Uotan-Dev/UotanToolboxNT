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
                if (string.IsNullOrEmpty(RecFile.Text))
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
                if (string.IsNullOrEmpty(RecFile.Text))
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
        Global.zip_tmp = Path.Combine(Global.tmp_path, "Magisk-" + StringHelper.RandomString(8));
        bool istempclean = FileHelper.ClearFolder(Global.zip_tmp);
        if (istempclean)
        {
            string outputzip = await CallExternalProgram.SevenZip($"x \"{MagiskFile.Text}\" -o\"{Global.zip_tmp}\" -y");
            string pattern_MAGISK_VER = @"MAGISK_VER='([^']+)'";
            string pattern_MAGISK_VER_CODE = @"MAGISK_VER_CODE=(\d+)";
            string Magisk_sh_path = Path.Combine(Global.zip_tmp, "assets", "util_functions.sh");
            string MAGISK_VER = StringHelper.FileRegex(Magisk_sh_path, pattern_MAGISK_VER, 1);
            string MAGISK_VER_CODE = StringHelper.FileRegex(Magisk_sh_path, pattern_MAGISK_VER_CODE, 1);
            if ((MAGISK_VER != null) & (MAGISK_VER_CODE != null))
            {
                string BOOT_PATCH_PATH = Path.Combine(Global.zip_tmp, "assets", "boot_patch.sh");
                string md5 = FileHelper.Md5Hash(BOOT_PATCH_PATH);
                bool Magisk_Valid = BootPatchHelper.Magisk_Validation(md5, MAGISK_VER);
                if (Magisk_Valid)
                {
                    Global.is_zip_ok = true;
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
        Global.boot_sha1 = FileHelper.SHA1Hash(BootFile.Text);
        if (Global.boot_sha1 == null)
        {
            patch_busy(false);
            return;
        }
        //在临时目录创建临时boot目录，这破东西跨平台解压各种问题，直接即用即丢了
        Global.boot_tmp = Path.Combine(Global.tmp_path, "Boot -" + StringHelper.RandomString(8));
        string workpath = Global.boot_tmp;
        if (FileHelper.ClearFolder(workpath))
        {
            (string mb_output, Global.mb_exitcode) = await CallExternalProgram.MagiskBoot($"unpack \"{BootFile.Text}\"", Global.boot_tmp);
            if (mb_output.Contains("error"))
            {
                SukiHost.ShowDialog(new PureDialog("解包失败"), allowBackgroundClose: true);
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
            string init_path = BootPatchHelper.CheckInitPath(ramdisk);
            string init_info = await CallExternalProgram.File($"\"{init_path}\"");
            (bool valid, string arch) = BootPatchHelper.ArchDetect(init_info);
            if (valid)
            {
                SukiHost.ShowDialog(new PureDialog($"检测到可用{arch}镜像"), allowBackgroundClose: true);
                ArchList.SelectedItem = arch;
                Global.is_boot_ok = true;
                patch_busy(false);
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(init_info), allowBackgroundClose: true);
                patch_busy(false);
            }
        }
    }
    private async void StartPatch(object sender, RoutedEventArgs args)
    {
        if (!Global.is_boot_ok | !Global.is_zip_ok)
        {
            SukiHost.ShowDialog(new PureDialog("请选择有效的面具与镜像文件"), allowBackgroundClose: true);
            return;
        }
        if (!BootPatchHelper.CheckComponentFiles(Global.zip_tmp, ArchList.SelectedItem.ToString()))
        {
            SukiHost.ShowDialog(new PureDialog("文件预处理时出错！"), allowBackgroundClose: true);
            return;
        }
        patch_busy(true);
        //设置环境变量
        string env_KEEPVERITY = KEEPVERITY.IsChecked.ToString().ToLower();
        string env_KEEPFORCEENCRYPT = KEEPFORCEENCRYPT.IsChecked.ToString().ToLower();
        string env_PATCHVBMETAFLAG = PATCHVBMETAFLAG.IsChecked.ToString().ToLower();
        string env_RECOVERYMODE = RECOVERYMODE.IsChecked.ToString().ToLower();
        string env_LEGACYSAR = LEGACYSAR.IsChecked.ToString().ToLower();
        string compPathBase = System.IO.Path.Combine(Global.zip_tmp, "lib");
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
        string compPath = Path.Combine(compPathBase, archSubfolder);
        string sub_compPath = Path.Combine(compPathBase, archSubfolder2);
        if (archSubfolder2 != "")
        {
            try
            {
                File.Copy(Path.Combine((sub_compPath), "libmagisk32.so"), Path.Combine((compPath), "libmagisk32.so"), true);
            }
            catch (Exception ex)
            {
                SukiHost.ShowDialog(new ErrorDialog("64位magisk32组件预处理时 " + ex));
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
                SukiHost.ShowDialog(new ErrorDialog("magisk32组件预处理时 " + ex));
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
                SukiHost.ShowDialog(new ErrorDialog("magisk32组件预处理时 " + ex));
                patch_busy(false);
                return;
            }
        }
        (string mb_output, int exitcode) = await CallExternalProgram.MagiskBoot($"compress=xz stub.apk stub.xz", Path.Combine(Global.zip_tmp, "assets"));
        if (mb_output.Contains("error"))
        {
            SukiHost.ShowDialog(new PureDialog("压缩stub.apk时出错"), allowBackgroundClose: true);
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
                    SukiHost.ShowDialog(new ErrorDialog("0文件预处理时出错！" + e));
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
                    SukiHost.ShowDialog(new ErrorDialog("1文件预处理时出错！" + e));
                    break;
                }
            case 2:
                SukiHost.ShowDialog(new PureDialog("镜像被未支持软件修补，请选择原生镜像！"), allowBackgroundClose: true);
                break;
            default:
                SukiHost.ShowDialog(new PureDialog("magiskboot检验出错"), allowBackgroundClose: true);
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
        if (BootPatchHelper.comp_copy(compPath))
        {
            (mb_output, exitcode) = await CallExternalProgram.MagiskBoot("cpio ramdisk.cpio \"add 0750 init magiskinit\" \"mkdir 0750 overlay.d\" \"mkdir 0750 overlay.d/sbin\" \"add 0644 overlay.d/sbin/magisk32.xz magisk32.xz\" ", Global.boot_tmp);
            (mb_output, exitcode) = await CallExternalProgram.MagiskBoot("cpio ramdisk.cpio \"add 0644 overlay.d/sbin/stub.xz stub.xz\" \"patch\" \"backup ramdisk.cpio.orig\" \"mkdir 000 .backup\" \"add 000 .backup/.magisk config\"", Global.boot_tmp, env_KEEPVERITY, env_KEEPFORCEENCRYPT, env_PATCHVBMETAFLAG, env_RECOVERYMODE, env_LEGACYSAR);
        }
        if (File.Exists(Path.Combine((compPath), "magisk64.xz")))
        {
            (mb_output, exitcode) = await CallExternalProgram.MagiskBoot("cpio ramdisk.cpio \"add 0644 overlay.d/sbin/magisk64.xz magisk64.xz\"", Global.boot_tmp);
        }
        //以上完成ramdisk.cpio的修补
        string dtb_name = BootPatchHelper.dtb_detect(Global.boot_tmp);
        if (dtb_name != null)
        {
            (mb_output, exitcode) = await CallExternalProgram.MagiskBoot($"dtb {dtb_name} test", Global.boot_tmp);
            if (exitcode != 0)
            {
                SukiHost.ShowDialog(new PureDialog("dtb验证失败"), allowBackgroundClose: true);
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
                    SukiHost.ShowDialog(new ErrorDialog("kernel删除失败" + ex));
                    patch_busy(false);
                    return;
                }

            }
        }
        try
        {
            if (BootPatchHelper.CleanBoot(Global.boot_tmp))
            {
                (mb_output, exitcode) = await CallExternalProgram.MagiskBoot($"repack \"{BootFile.Text}\"", Global.boot_tmp, env_KEEPVERITY, env_KEEPFORCEENCRYPT, env_PATCHVBMETAFLAG, env_RECOVERYMODE, env_LEGACYSAR);
                File.Copy(Path.Combine(Global.boot_tmp, "new-boot.img"), Path.Combine(Path.GetDirectoryName(BootFile.Text), "boot_patched_" + randomStr + ".img"), true);
                SukiHost.ShowDialog(new PureDialog("面具修补完成"), allowBackgroundClose: true);
                patch_busy(false);
                FileHelper.OpenFolder(Path.GetDirectoryName(BootFile.Text));
                Global.is_boot_ok = false;
                Global.is_zip_ok = false;
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
                string drvpath = String.Format(@"{0}\Drive\adb\*.inf", Global.runpath);
                string shell = String.Format("/add-driver {0} /subdirs /install", drvpath);
                string drvlog = await CallExternalProgram.Pnputil(shell);
                FileHelper.Write($"{Global.runpath}/Log/drive.txt", drvlog);
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
                string drvpath = String.Format(@"{0}\drive\9008\*.inf", Global.runpath);
                string shell = String.Format("/add-driver {0} /subdirs /install", drvpath);
                string drvlog = await CallExternalProgram.Pnputil(shell);
                FileHelper.Write($"{Global.runpath}/Log/drive.txt", drvlog);
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
            ProcessStartInfo? cmdshell = null;
            cmdshell = new ProcessStartInfo(cmd)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process? f = Process.Start(cmdshell);
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
                    if (TWRPInstall.IsChecked == true)
                    {
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} push {MagiskFile.Text} /tmp/");
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp install /tmp/{Path.GetFileNameWithoutExtension(MagiskFile.Text)}.");
                    }
                    else if (ADBSideload.IsChecked == true)
                    {
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} sideload \"{MagiskFile.Text}\"");
                    }
                    SukiHost.ShowDialog(new PureDialog("执行完成！"), allowBackgroundClose: true);
                    BusyInstall.IsBusy = false;
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请在右侧选择Magisk文件！"), allowBackgroundClose: true);
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
                        SukiHost.ShowDialog(new PureDialog("已推送至根目录，请自行安装。"), allowBackgroundClose: true);
                    }
                    BusyInstall.IsBusy = false;
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