using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SukiUI.Controls;
using System;
using System.IO;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Common.PatchHelper;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Features.Basicflash;

public partial class BasicflashView : UserControl
{
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public AvaloniaList<string> SimpleUnlock = ["oem unlock", "oem unlock-go", "flashing unlock", "flashing unlock_critical"];
    public AvaloniaList<string> Command = ["shell twrp sideload", "reboot sideload", "reboot safe-mode", "reboot muc", "reboot factory", "reboot admin"];
    public AvaloniaList<string> Arch = ["aarch64", "armeabi", "X86-64", "X86"];

    public BasicflashView()
    {
        InitializeComponent();
        SimpleContent.ItemsSource = SimpleUnlock;
        ArchList.ItemsSource = Arch;
        RebootComm.ItemsSource = Command;
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
                Global.checkdevice = false;
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
                Global.checkdevice = true;
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
                Global.checkdevice = false;
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
                Global.checkdevice = true;
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
                Global.checkdevice = false;
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
                                await CallExternalProgram.Fastboot($"-s {Global.thisdevice} flash misc {Global.runpath}/Image/misc.img");
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
                Global.checkdevice = true;
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
                Global.checkdevice = false;
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
                Global.checkdevice = true;
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

    public async void MoreReboot(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System") || sukiViewModel.Status == GetTranslation("Home_Recovery") || sukiViewModel.Status == GetTranslation("Home_Sideload"))
            {
                BusyReboot.IsBusy = true;
                RebootPanel.IsEnabled = false;
                if (RebootComm.SelectedItem != null)
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} {RebootComm.SelectedItem}");
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_Execution")), allowBackgroundClose: true);
                }
                BusyReboot.IsBusy = false;
                RebootPanel.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterRecOrOpenADB")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
        }
    }

    public static FilePickerFileType Zip { get; } = new("Zip")
    {
        Patterns = new[] { "*.zip", "*.apk", "*.ko" },
        AppleUniformTypeIdentifiers = new[] { "*.zip", "*.apk", "*.ko" }
    };

    private async void OpenMagiskFile(object sender, RoutedEventArgs args)
    {
        patch_busy(true);
        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                FileTypeFilter = new[] { Zip },
                Title = "Open File",
                AllowMultiple = false
            });
            if (files.Count == 0)
            {
                patch_busy(false);
                return;
            }
            MagiskFile.Text = StringHelper.FilePath(files[0].Path.ToString());
            Global.Zipinfo = await ZipDetect.Zip_Detect(MagiskFile.Text);
            SukiHost.ShowDialog(new PureDialog($"{GetTranslation("Basicflash_DetectZIP")}\nUseful:{Global.Zipinfo.IsUseful}\nMode:{Global.Zipinfo.Mode}\nVersion:{Global.Zipinfo.Version}"), allowBackgroundClose: true);
        }
        catch (Exception ex)
        {
            SukiHost.ShowDialog(new PureDialog(ex.Message), allowBackgroundClose: true);
        }
        patch_busy(false);
    }

    public static FilePickerFileType Image { get; } = new("Image")
    {
        Patterns = new[] { "*.img" },
        AppleUniformTypeIdentifiers = new[] { "*.img" }
    };

    private async void OpenBootFile(object sender, RoutedEventArgs args)
    {
        patch_busy(true);
        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                FileTypeFilter = new[] { Image },
                Title = "Open File",
                AllowMultiple = false
            });
            if (files.Count == 0)
            {
                patch_busy(false);
                return;
            }
            BootFile.Text = StringHelper.FilePath(files[0].Path.ToString());
            Global.Bootinfo = await BootDetect.Boot_Detect(BootFile.Text);
            ArchList.SelectedItem = Global.Bootinfo.Arch;
            SukiHost.ShowDialog(new PureDialog($"{GetTranslation("Basicflash_DetectdBoot")}\nArch:{Global.Bootinfo.Arch}\nOS:{Global.Bootinfo.OSVersion}\nPatch_level:{Global.Bootinfo.PatchLevel}\nRamdisk:{Global.Bootinfo.HaveRamdisk}\nKMI:{Global.Bootinfo.KMI}"), allowBackgroundClose: true);
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
            if (Global.Bootinfo.IsUseful != true | String.IsNullOrEmpty(MagiskFile.Text))
            {
                throw new Exception(GetTranslation("Basicflash_SelectBootMagisk"));
            }
            if ((Global.Zipinfo.Mode == PatchMode.None) | (Global.Zipinfo.IsUseful != true))
            {
                Global.Zipinfo = await ZipDetect.Zip_Detect(MagiskFile.Text);
            }
            string newboot = null;
            switch (Global.Zipinfo.Mode)
            {
                case PatchMode.Magisk:
                    newboot = await MagiskPatch.Magisk_Patch(Global.Zipinfo, Global.Bootinfo);
                    break;
                case PatchMode.GKI:
                    newboot = await KernelSUPatch.GKI_Patch(Global.Zipinfo, Global.Bootinfo);
                    break;
                case PatchMode.LKM:
                    newboot = await KernelSUPatch.LKM_Patch(Global.Zipinfo, Global.Bootinfo);
                    break;
                    //throw new Exception(GetTranslation("Basicflash_CantKSU"));
            }
            var newDialog = new ConnectionDialog(GetTranslation("Basicflash_PatchDone"));
            await SukiHost.ShowDialogAsync(newDialog);
            if (newDialog.Result == true)
            {
                await FlashBoot(newboot);
            }
            else
            {
                FileHelper.OpenFolder(Path.GetDirectoryName(Global.Bootinfo.Path));
            }
            Global.Zipinfo = new ZipInfo("", "", "", "", "", false, PatchMode.None, "");
            Global.Bootinfo = new BootInfo("", "", "", false, false, "", "", "", "", false, false, false, "", "");
            SetDefaultMagisk();
            BootFile.Text = null;
            ArchList.SelectedItem = null;
        }
        catch (Exception ex)
        {
            SukiHost.ShowDialog(new PureDialog(ex.Message), allowBackgroundClose: true);
        }
        patch_busy(false);
    }

    private static async Task FlashBoot(string boot)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Fastboot"))
            {
                Global.checkdevice = false;
                string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} flash boot \"{boot}\"");
                if (!output.Contains("FAILED") && !output.Contains("error"))
                {
                    var newDialog = new ConnectionDialog(GetTranslation("Basicflash_BootFlashSucc"));
                    await SukiHost.ShowDialogAsync(newDialog);
                    if (newDialog.Result == true)
                    {
                        await CallExternalProgram.Fastboot($"-s {Global.thisdevice} reboot");
                    }
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_RecoveryFailed")), allowBackgroundClose: true);
                }
                Global.checkdevice = true;
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

    private async void FlashMagisk(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (MagiskFile.Text != null)
            {
                BusyInstall.IsBusy = true;
                InstallZIP.IsEnabled = false;
                if (TWRPInstall.IsChecked == true)
                {
                    if (sukiViewModel.Status == "Recovery")
                    {
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} push {MagiskFile.Text} /tmp/magisk.apk");
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp install /tmp/magisk.apk");
                    }
                    else
                    {
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterRecovery")), allowBackgroundClose: true);
                    }
                }
                else if (ADBSideload.IsChecked == true)
                {
                    if (sukiViewModel.Status == GetTranslation("Home_Recovery"))
                    {
                        string output = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp sideload");
                        if (output.Contains("not found"))
                        {
                            await CallExternalProgram.ADB($"-s {Global.thisdevice} reboot sideload");
                        }
                        await Task.Delay(2000);
                        await GetDevicesInfo.SetDevicesInfoLittle();
                    }
                    if (sukiViewModel.Status == "Sideload")
                    {
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} sideload \"{MagiskFile.Text}\"");
                    }
                    else
                    {
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterSideload")), allowBackgroundClose: true);
                    }
                }
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_Execution")), allowBackgroundClose: true);
                BusyInstall.IsBusy = false;
                InstallZIP.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_SelectMagiskRight")), allowBackgroundClose: true);
            }
            if (sukiViewModel.Status == GetTranslation("Home_System"))
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
            BusyInstall.IsBusy = true;
            InstallZIP.IsEnabled = false;
            if (TWRPInstall.IsChecked == true)
            {
                if (sukiViewModel.Status == "Recovery")
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} push {Global.runpath}/ZIP/DisableAutoRecovery.zip /tmp/");
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp install /tmp/DisableAutoRecovery.zip");
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterRecovery")), allowBackgroundClose: true);
                }
            }
            else if (ADBSideload.IsChecked == true)
            {
                if (sukiViewModel.Status == GetTranslation("Home_Recovery"))
                {
                    string output = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp sideload");
                    if (output.Contains("not found"))
                    {
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} reboot sideload");
                    }
                    await Task.Delay(2000);
                    await GetDevicesInfo.SetDevicesInfoLittle();
                }
                if (sukiViewModel.Status == "Sideload")
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} sideload ZIP/DisableAutoRecovery.zip");
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterSideload")), allowBackgroundClose: true);
                }
            }
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_Execution")), allowBackgroundClose: true);
            BusyInstall.IsBusy = false;
            InstallZIP.IsEnabled = true;
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
            BusyInstall.IsBusy = true;
            InstallZIP.IsEnabled = false;
            if (TWRPInstall.IsChecked == true)
            {
                if (sukiViewModel.Status == "Recovery")
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} push {Global.runpath}/ZIP/copy-partitions.zip /tmp/");
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp install /tmp/copy-partitions.zip");
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterRecovery")), allowBackgroundClose: true);
                }
            }
            else if (ADBSideload.IsChecked == true)
            {
                if (sukiViewModel.Status == GetTranslation("Home_Recovery"))
                {
                    string output = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp sideload");
                    if (output.Contains("not found"))
                    {
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} reboot sideload");
                    }
                    await Task.Delay(2000);
                    await GetDevicesInfo.SetDevicesInfoLittle();
                }
                if (sukiViewModel.Status == "Sideload")
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} sideload ZIP/copy-partitions.zip");
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterSideload")), allowBackgroundClose: true);
                }
            }
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_Execution")), allowBackgroundClose: true);
            BusyInstall.IsBusy = false;
            InstallZIP.IsEnabled = true;
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
        }
    }
}