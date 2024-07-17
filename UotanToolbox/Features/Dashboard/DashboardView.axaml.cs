﻿using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
        SetDefaultMagisk();
    }

    public void SetDefaultMagisk()
    {
        string filepath = Path.Combine(Global.runpath, "APK", "Magisk-v27.0.apk");
        if (File.Exists(filepath))
        {
            MagiskFile.Text = filepath;
        }
        else
        {
            MagiskFile.Text = null;
        }
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
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_NoBoth")), allowBackgroundClose: true);
                }
                else if (!string.IsNullOrEmpty(UnlockFile.Text) && string.IsNullOrEmpty(UnlockCode.Text))
                {
                    await CallExternalProgram.Fastboot($"-s {Global.thisdevice} flash unlock \"{UnlockFile.Text}\"");
                    string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem unlock-go");
                    if (output.Contains("OKAY"))
                    {
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_UnlockSucc")), allowBackgroundClose: true);
                    }
                    else
                    {
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_UnlockFailed")), allowBackgroundClose: true);
                    }
                }
                else if (string.IsNullOrEmpty(UnlockFile.Text) && !string.IsNullOrEmpty(UnlockCode.Text))
                {
                    string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem unlock {UnlockCode.Text}");
                    if (output.Contains("OKAY"))
                    {
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_UnlockSucc")), allowBackgroundClose: true);
                    }
                    else
                    {
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_UnlockFailed")), allowBackgroundClose: true);
                    }
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_SelectUnlock")), allowBackgroundClose: true);
                }
                BusyUnlock.IsBusy = false;
                UnlockPanel.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterFastboot")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
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
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_RelockSucc")), allowBackgroundClose: true);
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_RelockFailed")), allowBackgroundClose: true);
                }
                BusyUnlock.IsBusy = false;
                UnlockPanel.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterFastboot")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
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
                    var newDialog = new ConnectionDialog(GetTranslation("Basicflash_BasicUnlock"));
                    await SukiHost.ShowDialogAsync(newDialog);
                    if (newDialog.Result == true)
                    {
                        await CallExternalProgram.Fastboot($"-s {Global.thisdevice} {SimpleContent.SelectedItem}");
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_CheckUnlock")), allowBackgroundClose: true);
                    }
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_SelectCommand")), allowBackgroundClose: true);
                }
                BusyBaseUnlock.IsBusy = false;
                BaseUnlockPanel.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterFastboot")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
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
                        var newDialog = new ConnectionDialog(GetTranslation("Basicflash_RecoverySucc"));
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
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_RecoveryFailed")), allowBackgroundClose: true);
                    }
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_SelectRecovery")), allowBackgroundClose: true);
                }
                BusyFlash.IsBusy = false;
                FlashRecovery.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterFastboot")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
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
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_BootSucc")), allowBackgroundClose: true);
                    }
                    else
                    {
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_BootFailed")), allowBackgroundClose: true);
                    }
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_SelectRecovery")), allowBackgroundClose: true);
                }
                BusyFlash.IsBusy = false;
                FlashRecovery.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterFastboot")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
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

    private async Task MagiskPretreatment()
    {
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
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_MagsikNotSupport")), allowBackgroundClose: true);
                patch_busy(false);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_ErrorClean")), allowBackgroundClose: true);
            patch_busy(false);
        }
    }

    private async void OpenMagiskFile(object sender, RoutedEventArgs args)
    {
        patch_busy(true);
        try
        {
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
            await MagiskPretreatment();
        }
        catch (Exception ex)
        {
            SukiHost.ShowDialog(new PureDialog(ex.Message), allowBackgroundClose: true);
        }
        patch_busy(false);
    }

    private async void OpenBootFile(object sender, RoutedEventArgs args)
    {
        try
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
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_SelectBoot")), allowBackgroundClose: true);
                patch_busy(false);
                return;
            }
            BootInfo.os_version = StringHelper.StringRegex(mb_output, osVersionPattern, 1);
            BootInfo.patch_level = StringHelper.StringRegex(mb_output, osPatchLevelPattern, 1);
            BootPatchHelper.dtb_detect();
            await BootPatchHelper.kernel_detect();
            await BootPatchHelper.ramdisk_detect();
            SukiHost.ShowDialog(new PureDialog($"{GetTranslation("Basicflash_DetectdBoot")}\nArch:{BootInfo.arch}\nOS:{BootInfo.os_version}\nPatch_level:{BootInfo.patch_level}\nRamdisk:{BootInfo.have_ramdisk}\nKMI:{BootInfo.kmi}"), allowBackgroundClose: true);
            ArchList.SelectedItem = BootInfo.arch;
            patch_busy(false);
        }
        catch (Exception ex)
        {
            SukiHost.ShowDialog(new PureDialog(ex.Message), allowBackgroundClose: true);
        }
    }
    private async void StartPatch(object sender, RoutedEventArgs args)
    {
        if (!BootInfo.userful | !BootInfo.have_ramdisk)
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_SelectBootMagisk")), allowBackgroundClose: true);
            return;
        }
        if (!ZipInfo.userful)
        {
            if (!string.IsNullOrEmpty(MagiskFile.Text))
            {
                patch_busy(true);
                await MagiskPretreatment();
                patch_busy(false);
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请选择有效的面具文件"), allowBackgroundClose: true);
                return;
            }
        }
        if (!BootPatchHelper.CheckComponentFiles(ZipInfo.tmp_path, ArchList.SelectedItem.ToString()))
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_FileError")), allowBackgroundClose: true);
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
            _ => throw new ArgumentException($"{GetTranslation("Basicflash_UnknowArch")}{ArchList.SelectedItem}")
        };
        string compPath = Path.Combine(Path.Combine(ZipInfo.tmp_path, "lib"), archSubfolder);
        try
        {
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
            SukiHost.ShowDialog(new ErrorDialog(GetTranslation("Basicflash_PreMagisk") + ex));
            patch_busy(false);
            return;
        }
        (string mb_output, int exitcode) = await CallExternalProgram.MagiskBoot($"compress=xz stub.apk stub.xz", Path.Combine(ZipInfo.tmp_path, "assets"));
        if (mb_output.Contains("error"))
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_ErrorComp")), allowBackgroundClose: true);
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
                    SukiHost.ShowDialog(new ErrorDialog(GetTranslation("Basicflash_FileError") + e));
                    return;
                }
            case 2:
                SukiHost.ShowDialog(new ErrorDialog(GetTranslation("Basicflash_UnsupportImage")));
                return;
            default:
                SukiHost.ShowDialog(new ErrorDialog(GetTranslation("Basicflash_CheckError")));
                return;
            }
            //以上完成ramdisk.cpio的修补
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
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_PatchDone")), allowBackgroundClose: true);
                patch_busy(false);
                FileHelper.OpenFolder(Path.GetDirectoryName(BootFile.Text));
                ZipInfo.userful = false;
                BootInfo.userful = false;
                BootFile.Text = null;
                SetDefaultMagisk();
                ArchList.SelectedItem = null;
                return;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_CleanDirError")), allowBackgroundClose: true);
                patch_busy(false);
                return;
            }
        }
        catch (Exception ex)
        {
            SukiHost.ShowDialog(new PureDialog(ex.Message), allowBackgroundClose: true);
        }
    }

    private async void FlashBoot(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Fastboot"))
            {
                if (!string.IsNullOrEmpty(BootFile.Text))
                {
                    string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} flash boot \"{BootFile.Text}\"");
                    if (!output.Contains("FAILED") && !output.Contains("error"))
                    {
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_FlashSucc")), allowBackgroundClose: true);
                    }
                    else
                    {
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_RecoveryFailed")), allowBackgroundClose: true);
                    }
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_SelectBoot")), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterFastboot")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
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
                if (drvlog.Contains(GetTranslation("Basicflash_Success")))
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_InstallSuccess")), allowBackgroundClose: true);
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_InstallFailed")), allowBackgroundClose: true);
                }
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_NotUsed")), allowBackgroundClose: true);
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
                if (drvlog.Contains(GetTranslation("Basicflash_Success")))
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_InstallSuccess")), allowBackgroundClose: true);
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_InstallFailed")), allowBackgroundClose: true);
                }
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_NotUsed")), allowBackgroundClose: true);
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
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_Execution")), allowBackgroundClose: true);
            });
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_NotUsed")), allowBackgroundClose: true);
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
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_Execution")), allowBackgroundClose: true);
                    BusyInstall.IsBusy = false;
                    InstallZIP.IsEnabled = true;
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_SelectMagiskRight")), allowBackgroundClose: true);
                }
            }
            else if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                if (MagiskFile.Text != null)
                {
                    BusyInstall.IsBusy = true;
                    InstallZIP.IsEnabled = false;
                    var newDialog = new ConnectionDialog(GetTranslation("Basicflash_PushMagisk"));
                    await SukiHost.ShowDialogAsync(newDialog);
                    if (newDialog.Result == true)
                    {
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} push \"{MagiskFile.Text}\" /sdcard/magisk.apk");
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_InstallMagisk")), allowBackgroundClose: true);
                    }
                    BusyInstall.IsBusy = false;
                    InstallZIP.IsEnabled = true;
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_SelectMagiskRight")), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterRecovery")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
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
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_Execution")), allowBackgroundClose: true);
                BusyInstall.IsBusy = false;
                InstallZIP.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterRecovery")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
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
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_Execution")), allowBackgroundClose: true);
                BusyInstall.IsBusy = false;
                InstallZIP.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterRecovery")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
        }
    }
}