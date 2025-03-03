using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.IO;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Common.PatchHelper;

namespace UotanToolbox.Features.Basicflash;

public partial class BasicflashView : UserControl
{
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    private readonly string unlock_log_path = Path.Combine(Global.log_path, "unlock.txt");
    public AvaloniaList<string> SimpleUnlock = ["oem unlock", "oem unlock-go", "flashing unlock", "flashing unlock_critical"];
    public AvaloniaList<string> Command = ["shell twrp sideload", "reboot sideload", "reboot safe-mode", "reboot muc", "reboot factory", "reboot admin"];
    public AvaloniaList<string> Arch = ["aarch64", "armeabi", "X86-64", "X86"];
    public AvaloniaList<string> Band = [GetTranslation("Band_Common"), GetTranslation("Band_Huawei"), GetTranslation("Band_Sony")];

    public BasicflashView()
    {
        InitializeComponent();
        SimpleContent.ItemsSource = SimpleUnlock;
        ArchList.ItemsSource = Arch;
        RebootComm.ItemsSource = Command;
        UnlockBand.ItemsSource = Band;
        UnlockBand.SelectedIndex = 0;
        SetDefaultMagisk();
    }

    public void SetDefaultMagisk()
    {
        string filepath = Path.Combine(Global.runpath, "APK", "Magisk-v27.0.apk");
        MagiskFile.Text = File.Exists(filepath) ? filepath : null;
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
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            UnlockFile.Text = files[0].TryGetLocalPath();
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
                    Global.MainDialogManager.CreateDialog()
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .OfType(NotificationType.Error)
                                                .WithContent(GetTranslation("Basicflash_NoBoth"))
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
                }
                else if (!string.IsNullOrEmpty(UnlockFile.Text) && string.IsNullOrEmpty(UnlockCode.Text))
                {
                    _ = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} flash unlock \"{UnlockFile.Text}\"");
                    string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem unlock-go");
                    _ = output.Contains("OKAY")
                        ? Global.MainDialogManager.CreateDialog()
                                                  .WithTitle(GetTranslation("Common_Succ"))
                                                  .OfType(NotificationType.Success)
                                                  .WithContent(GetTranslation("Basicflash_UnlockSucc"))
                                                  .Dismiss().ByClickingBackground()
                                                  .TryShow()
                        : Global.MainDialogManager.CreateDialog()
                                                  .WithTitle(GetTranslation("Common_Error"))
                                                  .OfType(NotificationType.Error)
                                                  .WithContent(GetTranslation("Basicflash_UnlockFailed"))
                                                  .Dismiss().ByClickingBackground()
                                                  .TryShow();
                }
                else if (string.IsNullOrEmpty(UnlockFile.Text) && !string.IsNullOrEmpty(UnlockCode.Text))
                {
                    if (UnlockBand.SelectedItem.ToString() == GetTranslation("Band_Common"))
                    {
                        string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem unlock {UnlockCode.Text}");
                        FileHelper.Write(unlock_log_path, output);
                        _ = output.Contains("OKAY")
                            ? Global.MainDialogManager.CreateDialog()
                                                      .WithTitle(GetTranslation("Common_Succ"))
                                                      .OfType(NotificationType.Success)
                                                      .WithContent(GetTranslation("Basicflash_UnlockSucc"))
                                                      .Dismiss().ByClickingBackground()
                                                      .TryShow()
                            : Global.MainDialogManager.CreateDialog()
                                                      .WithTitle(GetTranslation("Common_Error"))
                                                      .OfType(NotificationType.Error)
                                                      .WithContent(GetTranslation("Basicflash_UnlockFailed"))
                                                      .Dismiss().ByClickingBackground()
                                                      .TryShow();
                    }
                    else if (UnlockBand.SelectedItem.ToString() == GetTranslation("Band_Huawei"))
                    {
                        if (UnlockCode.Text.Length == 16)
                        {
                            string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem unlock {UnlockCode.Text}");
                            FileHelper.Write(unlock_log_path, output);
                            if (output.Contains("OKAY"))
                            {
                                Global.MainDialogManager.CreateDialog()
                                                        .WithTitle(GetTranslation("Common_Succ"))
                                                        .OfType(NotificationType.Success)
                                                        .WithContent(GetTranslation("Basicflash_UnlockSucc"))
                                                        .Dismiss().ByClickingBackground()
                                                        .TryShow();
                            }
                            else if (output.Contains("Necessary to disable phone finder", StringComparison.OrdinalIgnoreCase) || output.Contains("Command not allowed", StringComparison.OrdinalIgnoreCase))
                            {
                                Global.MainDialogManager.CreateDialog()
                                                    .WithTitle(GetTranslation("Common_Error"))
                                                    .OfType(NotificationType.Error)
                                                    .WithContent(GetTranslation("Basicflash_FindPhone"))
                                                    .Dismiss().ByClickingBackground()
                                                    .TryShow();
                            }
                            else if (output.Contains("password wrong", StringComparison.OrdinalIgnoreCase))
                            {
                                Global.MainDialogManager.CreateDialog()
                                                    .WithTitle(GetTranslation("Common_Error"))
                                                    .OfType(NotificationType.Error)
                                                    .WithContent(GetTranslation("Basicflash_CdoeError"))
                                                    .Dismiss().ByClickingBackground()
                                                    .TryShow();
                            }
                            else
                            {
                                Global.MainDialogManager.CreateDialog()
                                                    .WithTitle(GetTranslation("Common_Error"))
                                                    .OfType(NotificationType.Error)
                                                    .WithContent(GetTranslation("Basicflash_CheckLog"))
                                                    .Dismiss().ByClickingBackground()
                                                    .TryShow();
                            }
                        }
                        else
                        {
                            Global.MainDialogManager.CreateDialog()
                                                    .WithTitle(GetTranslation("Common_Error"))
                                                    .OfType(NotificationType.Error)
                                                    .WithContent(GetTranslation("Basicflash_NotMatch"))
                                                    .Dismiss().ByClickingBackground()
                                                    .TryShow();
                        }
                    }
                    else if (UnlockBand.SelectedItem.ToString() == GetTranslation("Band_Sony"))
                    {
                        string unlockcode = UnlockCode.Text;
                        if (!UnlockCode.Text.Substring(0, 2).Equals("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            unlockcode = "0x" + UnlockCode.Text;
                        }
                        string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem unlock {unlockcode}");
                        FileHelper.Write(unlock_log_path, output);
                        _ = output.Contains("OKAY")
                            ? Global.MainDialogManager.CreateDialog()
                                                      .WithTitle(GetTranslation("Common_Succ"))
                                                      .OfType(NotificationType.Success)
                                                      .WithContent(GetTranslation("Basicflash_UnlockSucc"))
                                                      .Dismiss().ByClickingBackground()
                                                      .TryShow()
                            : Global.MainDialogManager.CreateDialog()
                                                      .WithTitle(GetTranslation("Common_Error"))
                                                      .OfType(NotificationType.Error)
                                                      .WithContent(GetTranslation("Basicflash_UnlockFailed"))
                                                      .Dismiss().ByClickingBackground()
                                                      .TryShow();
                    }                    
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .OfType(NotificationType.Error)
                                                .WithContent(GetTranslation("Basicflash_SelectUnlock"))
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
                }
                BusyUnlock.IsBusy = false;
                UnlockPanel.IsEnabled = true;
                Global.checkdevice = true;
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                                            .WithTitle(GetTranslation("Common_Error"))
                                            .OfType(NotificationType.Error)
                                            .WithContent(GetTranslation("Common_EnterFastboot"))
                                            .Dismiss().ByClickingBackground()
                                            .TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .OfType(NotificationType.Error)
                                        .WithContent(GetTranslation("Common_NotConnected"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
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
                Global.MainDialogManager.CreateDialog()
                      .WithTitle(GetTranslation("Common_Warn"))
                      .WithContent(GetTranslation("Basicflash_RelockTip"))
                      .OfType(NotificationType.Warning)
                      .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ => {
                          await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem lock-go");
                          string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} flashing lock");
                          if (output.Contains("OKAY"))
                          {
                              Global.MainDialogManager.CreateDialog()
                                                      .WithTitle(GetTranslation("Common_Succ"))
                                                      .OfType(NotificationType.Success)
                                                      .WithContent(GetTranslation("Basicflash_LockSucc"))
                                                      .Dismiss().ByClickingBackground()
                                                      .TryShow();
                          }
                          else
                          {
                              Global.MainDialogManager.CreateDialog()
                                                      .WithTitle(GetTranslation("Common_Error"))
                                                      .OfType(NotificationType.Error)
                                                      .WithContent(GetTranslation("Basicflash_LockFailed"))
                                                      .Dismiss().ByClickingBackground()
                                                      .TryShow();
                          }
                          BusyUnlock.IsBusy = false;
                          UnlockPanel.IsEnabled = true;
                          Global.checkdevice = true;
                      }, true)
                      .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                      .TryShow();
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                                            .WithTitle(GetTranslation("Common_Error"))
                                            .OfType(NotificationType.Error)
                                            .WithContent(GetTranslation("Common_EnterFastboot"))
                                            .Dismiss().ByClickingBackground()
                                            .TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .OfType(NotificationType.Error)
                                        .WithContent(GetTranslation("Common_NotConnected"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
        }
    }

    private async void BaseUnlock(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Fastboot"))
            {
                if (SimpleContent.SelectedItem != null)
                {
                    Global.MainDialogManager.CreateDialog()
                                                .WithTitle(GetTranslation("Common_Warn"))
                                                .WithContent(GetTranslation("Basicflash_BasicUnlock"))
                                                .OfType(NotificationType.Warning)
                                                .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                                                {
                                                    BusyBaseUnlock.IsBusy = true;
                                                    BaseUnlockPanel.IsEnabled = false;
                                                    await CallExternalProgram.Fastboot($"-s {Global.thisdevice} {SimpleContent.SelectedItem}");
                                                    Global.MainDialogManager.CreateDialog()
                                                    .WithTitle(GetTranslation("Common_Execution"))
                                                    .OfType(NotificationType.Information)
                                                    .WithContent(GetTranslation("Basicflash_CheckUnlock"))
                                                    .Dismiss().ByClickingBackground()
                                                    .TryShow();
                                                    BusyBaseUnlock.IsBusy = false;
                                                    BaseUnlockPanel.IsEnabled = true;
                                                }, true)
                                                .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                                                .TryShow();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .OfType(NotificationType.Error)
                                                .WithContent(GetTranslation("Basicflash_SelectCommand"))
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                                            .WithTitle(GetTranslation("Common_Error"))
                                            .OfType(NotificationType.Error)
                                            .WithContent(GetTranslation("Common_EnterFastboot"))
                                            .Dismiss().ByClickingBackground()
                                            .TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .OfType(NotificationType.Error)
                                        .WithContent(GetTranslation("Common_NotConnected"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
        }
    }

    private async void OpenRecFile(object sender, RoutedEventArgs args)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            RecFile.Text = files[0].TryGetLocalPath();
        }
    }

    private async Task FlashRec(string shell)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
            {
                Global.checkdevice = false;
                BusyFlash.IsBusy = true;
                FlashRecovery.IsEnabled = false;
                if (!string.IsNullOrEmpty(RecFile.Text))
                {
                    string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} {shell} \"{RecFile.Text}\"");
                    if (!output.Contains("FAILED") && !output.Contains("error"))
                    {
                        Global.MainDialogManager.CreateDialog()
                                                    .WithTitle(GetTranslation("Common_Succ"))
                                                    .WithContent(GetTranslation("Basicflash_RecoverySucc"))
                                                    .OfType(NotificationType.Success)
                                                    .WithActionButton(GetTranslation("Basicflash_RebootToFastbootd"), async _ =>
                                                    {
                                                        await CallExternalProgram.Fastboot($"-s {Global.thisdevice} reboot-fastboot");
                                                    }, true)
                                                    .WithActionButton(GetTranslation("Basicflash_RebootToRecovery"), async _ =>
                                                    {
                                                        output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem reboot-recovery");
                                                        if (output.Contains("unknown command"))
                                                        {
                                                            await CallExternalProgram.Fastboot($"-s {Global.thisdevice} flash misc \"{Path.Combine(Global.runpath, "Image", "misc.img")}\"");
                                                            await CallExternalProgram.Fastboot($"-s {Global.thisdevice} reboot");
                                                        }
                                                    }, true)
                                                    .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                                                    .TryShow();
                    }
                    else
                    {
                        Global.MainDialogManager.CreateDialog()
                                                    .WithTitle(GetTranslation("Common_Error"))
                                                    .OfType(NotificationType.Error)
                                                    .WithContent(GetTranslation("Basicflash_RecoveryFailed"))
                                                    .Dismiss().ByClickingBackground()
                                                    .TryShow();
                    }
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .OfType(NotificationType.Error)
                                                .WithContent(GetTranslation("Basicflash_SelectRecovery"))
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
                }
                BusyFlash.IsBusy = false;
                FlashRecovery.IsEnabled = true;
                Global.checkdevice = true;
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                                            .WithTitle(GetTranslation("Common_Error"))
                                            .OfType(NotificationType.Error)
                                            .WithContent(GetTranslation("Common_EnterFastboot"))
                                            .Dismiss().ByClickingBackground()
                                            .TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .OfType(NotificationType.Error)
                                        .WithContent(GetTranslation("Common_NotConnected"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
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
                    _ = output.Contains("Finished")
                        ? Global.MainDialogManager.CreateDialog()
                                                  .WithTitle(GetTranslation("Common_Succ"))
                                                  .OfType(NotificationType.Success)
                                                  .WithContent(GetTranslation("Basicflash_BootSucc"))
                                                  .Dismiss().ByClickingBackground()
                                                  .TryShow()
                        : Global.MainDialogManager.CreateDialog()
                                                  .WithTitle(GetTranslation("Common_Error"))
                                                  .OfType(NotificationType.Error)
                                                  .WithContent(GetTranslation("Basicflash_BootFailed"))
                                                  .Dismiss().ByClickingBackground()
                                                  .TryShow();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .OfType(NotificationType.Error)
                                                .WithContent(GetTranslation("Basicflash_SelectRecovery"))
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
                }
                BusyFlash.IsBusy = false;
                FlashRecovery.IsEnabled = true;
                Global.checkdevice = true;
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                                            .WithTitle(GetTranslation("Common_Error"))
                                            .OfType(NotificationType.Error)
                                            .WithContent(GetTranslation("Common_EnterFastboot"))
                                            .Dismiss().ByClickingBackground()
                                            .TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .OfType(NotificationType.Error)
                                        .WithContent(GetTranslation("Common_NotConnected"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
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
            if (sukiViewModel.Status == GetTranslation("Home_Android") || sukiViewModel.Status == GetTranslation("Home_Recovery") || sukiViewModel.Status == GetTranslation("Home_Sideload"))
            {
                BusyReboot.IsBusy = true;
                RebootPanel.IsEnabled = false;
                if (RebootComm.SelectedItem != null)
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} {RebootComm.SelectedItem}");
                    Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Execution"))
                                        .OfType(NotificationType.Information)
                                        .WithContent(GetTranslation("Common_Execution"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
                }
                BusyReboot.IsBusy = false;
                RebootPanel.IsEnabled = true;
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .OfType(NotificationType.Error)
                                        .WithContent(GetTranslation("Common_EnterRecOrOpenADB"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .OfType(NotificationType.Error)
                                        .WithContent(GetTranslation("Common_NotConnected"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
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
            TopLevel topLevel = TopLevel.GetTopLevel(this);
            System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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
            MagiskFile.Text = files[0].TryGetLocalPath();
            Global.Zipinfo = await ZipDetect.Zip_Detect(MagiskFile.Text);
            Global.MainDialogManager.CreateDialog()
                                        .OfType(NotificationType.Information)
                                        .WithContent($"{GetTranslation("Basicflash_DetectZIP")}\nUseful:{Global.Zipinfo.IsUseful}\nMode:{Global.Zipinfo.Mode}\nVersion:{Global.Zipinfo.Version}")
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
        }
        catch (Exception ex)
        {
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .OfType(NotificationType.Error)
                                        .WithContent(ex.Message)
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
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
            TopLevel topLevel = TopLevel.GetTopLevel(this);
            System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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
            BootFile.Text = files[0].TryGetLocalPath();
            Global.Bootinfo = await BootDetect.Boot_Detect(BootFile.Text);
            ArchList.SelectedItem = Global.Bootinfo.Arch;
            Global.MainDialogManager.CreateDialog()
                                        .OfType(NotificationType.Information)
                                        .WithContent($"{GetTranslation("Basicflash_DetectdBoot")}\nArch:{Global.Bootinfo.Arch}\nOS:{Global.Bootinfo.OSVersion}\nPatch_level:{Global.Bootinfo.PatchLevel}\nRamdisk:{Global.Bootinfo.HaveRamdisk}\nKMI:{Global.Bootinfo.KMI}\nKERNEL_FMT:{Global.Bootinfo.Compress}")
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
        }
        catch (Exception ex)
        {
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .OfType(NotificationType.Error)
                                        .WithContent(ex.Message)
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
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
            if (Global.Bootinfo.IsUseful != true | string.IsNullOrEmpty(MagiskFile.Text))
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
                    throw new Exception(GetTranslation("Basicflash_CantKSU"));
            }
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Succ"))
                                        .WithContent(GetTranslation("Basicflash_PatchDone"))
                                        .OfType(NotificationType.Success)
                                        .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ => await FlashBoot(newboot), true)
                                        .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => FileHelper.OpenFolder(Path.GetDirectoryName(Global.Bootinfo.Path)), true)
                                        .TryShow();
            Global.Zipinfo = new ZipInfo("", "", "", "", "", false, PatchMode.None, "");
            Global.Bootinfo = new BootInfo("", "", "", false, false, "", "", "", "", false, false, false, "", "", "");
            SetDefaultMagisk();
            BootFile.Text = null;
            ArchList.SelectedItem = null;
        }
        catch (Exception ex)
        {
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .OfType(NotificationType.Error)
                                        .WithContent(ex.Message)
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
        }
        patch_busy(false);
    }

    private async Task FlashBoot(string boot)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
            {
                Global.checkdevice = false;
                string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} flash boot \"{boot}\"");
                if (!output.Contains("FAILED") && !output.Contains("error"))
                {
                    Global.MainDialogManager.CreateDialog()
                                                .WithTitle(GetTranslation("Common_Succ"))
                                                .WithContent(GetTranslation("Basicflash_BootFlashSucc"))
                                                .OfType(NotificationType.Success)
                                                .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ => await CallExternalProgram.Fastboot($"-s {Global.thisdevice} reboot"), true)
                                                .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                                                .TryShow();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .OfType(NotificationType.Error)
                                                .WithContent(GetTranslation("Basicflash_RecoveryFailed"))
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
                }
                Global.checkdevice = true;
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                                            .WithTitle(GetTranslation("Common_Error"))
                                            .OfType(NotificationType.Error)
                                            .WithContent(GetTranslation("Common_EnterFastboot"))
                                            .Dismiss().ByClickingBackground()
                                            .TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .OfType(NotificationType.Error)
                                        .WithContent(GetTranslation("Common_NotConnected"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
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
                        _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} push \"{MagiskFile.Text}\" /tmp/magisk.apk");
                        _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp install /tmp/magisk.apk");
                    }
                    else
                    {
                        Global.MainDialogManager.CreateDialog()
                                                    .WithTitle(GetTranslation("Common_Error"))
                                                    .OfType(NotificationType.Error)
                                                    .WithContent(GetTranslation("Common_EnterRecovery"))
                                                    .Dismiss().ByClickingBackground()
                                                    .TryShow();
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
                        _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} sideload \"{MagiskFile.Text}\"");
                    }
                    else
                    {
                        Global.MainDialogManager.CreateDialog()
                                                    .WithTitle(GetTranslation("Common_Error"))
                                                    .OfType(NotificationType.Error)
                                                    .WithContent(GetTranslation("Common_EnterSideload"))
                                                    .Dismiss().ByClickingBackground()
                                                    .TryShow();
                    }
                }
                Global.MainDialogManager.CreateDialog()
                                            .WithTitle(GetTranslation("Common_Execution"))
                                            .OfType(NotificationType.Information)
                                            .WithContent(GetTranslation("Common_Execution"))
                                            .Dismiss().ByClickingBackground()
                                            .TryShow();
                BusyInstall.IsBusy = false;
                InstallZIP.IsEnabled = true;
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                                            .WithTitle(GetTranslation("Common_Error"))
                                            .OfType(NotificationType.Error)
                                            .WithContent(GetTranslation("Basicflash_SelectMagiskRight"))
                                            .Dismiss().ByClickingBackground()
                                            .TryShow();
            }
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                if (MagiskFile.Text != null)
                {
                    Global.MainDialogManager.CreateDialog()
                                                .WithTitle(GetTranslation("Common_Warn"))
                                                .WithContent(GetTranslation("Basicflash_PushMagisk"))
                                                .OfType(NotificationType.Warning)
                                                .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                                                {
                                                    BusyInstall.IsBusy = true;
                                                    InstallZIP.IsEnabled = false;
                                                    await CallExternalProgram.ADB($"-s {Global.thisdevice} push \"{MagiskFile.Text}\" /sdcard/magisk.apk");
                                                    Global.MainDialogManager.CreateDialog()
                                                                                .WithTitle(GetTranslation("Common_Error"))
                                                                                .OfType(NotificationType.Error)
                                                                                .WithContent(GetTranslation("Basicflash_InstallMagisk"))
                                                                                .Dismiss().ByClickingBackground()
                                                                                .TryShow();
                                                    BusyInstall.IsBusy = false;
                                                    InstallZIP.IsEnabled = true;
                                                }, true)
                                                .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                                                .TryShow();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .OfType(NotificationType.Error)
                                                .WithContent(GetTranslation("Basicflash_SelectMagiskRight"))
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
                }
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .OfType(NotificationType.Error)
                                        .WithContent(GetTranslation("Common_NotConnected"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
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
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} push \"{Path.Combine(Global.runpath, "ZIP", "DisableAutoRecovery.zip")}\" /tmp/");
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp install /tmp/DisableAutoRecovery.zip");
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .OfType(NotificationType.Error)
                                                .WithContent(GetTranslation("Common_EnterRecovery"))
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
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
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} sideload ZIP/DisableAutoRecovery.zip");
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .OfType(NotificationType.Error)
                                                .WithContent(GetTranslation("Common_EnterSideload"))
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
                }
            }
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Execution"))
                                        .OfType(NotificationType.Information)
                                        .WithContent(GetTranslation("Common_Execution"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
            BusyInstall.IsBusy = false;
            InstallZIP.IsEnabled = true;
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .OfType(NotificationType.Error)
                                        .WithContent(GetTranslation("Common_NotConnected"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
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
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} push \"{Path.Combine(Global.runpath, "ZIP", "copy-partitions.zip")}\" /tmp/");
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp install /tmp/copy-partitions.zip");
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .OfType(NotificationType.Error)
                                                .WithContent(GetTranslation("Common_EnterRecovery"))
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
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
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} sideload ZIP/copy-partitions.zip");
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .OfType(NotificationType.Error)
                                                .WithContent(GetTranslation("Common_EnterSideload"))
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
                }
            }
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Execution"))
                                        .OfType(NotificationType.Information)
                                        .WithContent(GetTranslation("Common_Execution"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
            BusyInstall.IsBusy = false;
            InstallZIP.IsEnabled = true;
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .OfType(NotificationType.Error)
                                        .WithContent(GetTranslation("Common_NotConnected"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
        }
    }
}