using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Common.PatchHelper;
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
            Global.Zipinfo = await ZipDetect.Zip_Detect(MagiskFile.Text);
            SukiHost.ShowDialog(new PureDialog($"Zip内检测到：\nUseful:{Global.Zipinfo.IsUseful}\nMode:{Global.Zipinfo.Mode}\nVersion:{Global.Zipinfo.Version}"), allowBackgroundClose: true);
        }
        catch (Exception ex)
        {
            SukiHost.ShowDialog(new PureDialog(ex.Message), allowBackgroundClose: true);
        }
        patch_busy(false);
    }

    private async void OpenBootFile(object sender, RoutedEventArgs args)
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
            BootFile.Text = Uri.UnescapeDataString(StringHelper.FilePath(files[0].Path.ToString()));
            Global.Bootinfo = await BootDetect.Boot_Detect(BootFile.Text);
            ArchList.SelectedItem = Global.Bootinfo.Arch;
            SukiHost.ShowDialog(new PureDialog($"Boot内检测到\nArch:{Global.Bootinfo.Arch}\nOS:{Global.Bootinfo.OSVersion}\nPatch_level:{Global.Bootinfo.PatchLevel}\nRamdisk:{Global.Bootinfo.HaveRamdisk}\nKMI:{Global.Bootinfo.KMI}"), allowBackgroundClose: true);
        }
        catch (Exception ex)
        {
            SukiHost.ShowDialog(new PureDialog(ex.Message), allowBackgroundClose: true);
        }
        patch_busy(false);
    }

    private async void StartPatch(object sender, RoutedEventArgs args)
    {
        patch_busy(true);
        try
        {
            EnvironmentVariable.KEEPVERITY = (bool)KEEPVERITY.IsChecked;
            EnvironmentVariable.KEEPFORCEENCRYPT = (bool)KEEPFORCEENCRYPT.IsChecked;
            EnvironmentVariable.PATCHVBMETAFLAG = (bool)PATCHVBMETAFLAG.IsChecked;
            EnvironmentVariable.RECOVERYMODE = (bool)RECOVERYMODE.IsChecked;
            EnvironmentVariable.LEGACYSAR = (bool)LEGACYSAR.IsChecked;
            if (Global.Zipinfo == new ZipInfo("", "", "", "", "", false, PatchMode.None, ""))
            {
                Global.Zipinfo = await ZipDetect.Zip_Detect(MagiskFile.Text);
            }
            if ((Global.Zipinfo.Mode == PatchMode.None) | (Global.Zipinfo.IsUseful != true) | (Global.Bootinfo.IsUseful != true))
            {
                throw new Exception("请选择合适的Zip与Boot文件");
            }
            switch (Global.Zipinfo.Mode)
            {
                case PatchMode.Magisk:
                    await MagiskPatch.Magisk_Patch(Global.Zipinfo, Global.Bootinfo);
                    break;
                case PatchMode.KernelSU:
                    //await KernelSU_Patch(Global.Zipinfo, Global.Bootinfo);
                    //break;
                    throw new Exception("暂不支持KernelSU修补");
            }
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_PatchDone")), allowBackgroundClose: true);
            FileHelper.OpenFolder(Path.GetDirectoryName(Global.Bootinfo.Path));
            Global.Zipinfo = new ZipInfo("", "", "", "", "", false, PatchMode.None, "");
            Global.Bootinfo = new BootInfo("", "", "", false, false, "", "", "", "", false, false, false, "", "");
            MagiskFile.Text = null;
            BootFile.Text = null;
            ArchList.SelectedItem = null;
        }
        catch (Exception ex)
        {
            SukiHost.ShowDialog(new PureDialog(ex.Message), allowBackgroundClose: true);
        }
        patch_busy(false);
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
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_InstallSuccess")), allowBackgroundClose: true);
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_InstallFailed")), allowBackgroundClose: true);
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
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_InstallSuccess")), allowBackgroundClose: true);
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_InstallFailed")), allowBackgroundClose: true);
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