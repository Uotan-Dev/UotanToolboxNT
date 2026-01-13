using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UotanToolbox.Common;


namespace UotanToolbox.Features.FormatExtract;

public partial class FormatExtractView : UserControl
{
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public FormatExtractView()
    {
        InitializeComponent();
    }
    private static readonly string adb_log_path = Path.Combine(Global.log_path, "adb.txt");
    private string output = "";
    public async Task QCNTool(string shell)
    {
        await Task.Run(() =>
        {
            string cmd = "Bin\\QSML\\QCNTool.exe";
            ProcessStartInfo qcntool = new ProcessStartInfo(cmd, shell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = Global.backup_path,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using Process qcn = new Process();
            qcn.StartInfo = qcntool;
            _ = qcn.Start();
            qcn.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            qcn.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            qcn.BeginOutputReadLine();
            qcn.BeginErrorReadLine();
            qcn.WaitForExit();
            qcn.Close();
        });
    }

    public async Task Fastboot(string fbshell)//Fastboot实时输出
    {
        await Task.Run(() =>
        {
            string cmd = Path.Combine(Global.bin_path, "platform-tools", "fastboot");
            ProcessStartInfo fastboot = new ProcessStartInfo(cmd, fbshell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using Process fb = new Process();
            fb.StartInfo = fastboot;
            _ = fb.Start();
            fb.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            fb.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            fb.BeginOutputReadLine();
            fb.BeginErrorReadLine();
            fb.WaitForExit();
            fb.Close();
        });
    }

    public async Task ADB(string adbshell)
    {
        await Task.Run(() =>
        {
            string cmd = Path.Combine(Global.bin_path, "platform-tools", "adb");
            ProcessStartInfo adbexe = new ProcessStartInfo(cmd, adbshell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using Process adb = new Process();
            adb.StartInfo = adbexe;
            _ = adb.Start();
            adb.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            adb.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            adb.BeginOutputReadLine();
            adb.BeginErrorReadLine();
            adb.WaitForExit();
            adb.Close();
        });
    }

    private async void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (!string.IsNullOrEmpty(outLine.Data))
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StringBuilder sb = new StringBuilder(FormatExtractLog.Text);
                FormatExtractLog.Text = sb.AppendLine(outLine.Data).ToString();
                FormatExtractLog.CaretIndex = FormatExtractLog.Text.Length;
                StringBuilder op = new StringBuilder(output);
                output = op.AppendLine(outLine.Data).ToString();
            });
        }
    }

    private async void OpenQcnFile(object sender, RoutedEventArgs args)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open QCN File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            QcnFile.Text = files[0].TryGetLocalPath();
        }
    }

    private async void WriteQcn(object sender, RoutedEventArgs args)
    {
        // Write QCN File
        if (Global.System == "Windows")
        {
            if (await GetDevicesInfo.SetDevicesInfoLittle())
            {
                if (!string.IsNullOrEmpty(QcnFile.Text))
                {
                    string qcnfilepatch = QcnFile.Text;
                    MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
                    if (sukiViewModel.Status is "901D" or "9091")
                    {
                        BusyQCN.IsBusy = true;
                        QCN.IsEnabled = false;
                        output = "";
                        FormatExtractLog.Text = GetTranslation("FormatExtract_Writing") + "\n";
                        int com = StringHelper.Onlynum(Global.thisdevice);
                        string shell = string.Format($"-w -p {com} -f \"{qcnfilepatch}\"");
                        await QCNTool(shell);
                        _ = FormatExtractLog.Text.Contains("error")
                            ? Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_WriteFailed")).Dismiss().ByClickingBackground().TryShow()
                            : Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Succ")).OfType(NotificationType.Success).WithContent(GetTranslation("FormatExtract_WriteSucc")).Dismiss().ByClickingBackground().TryShow();
                        BusyQCN.IsBusy = false;
                        QCN.IsEnabled = true;
                    }
                    else
                    {
                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_Open901D")).Dismiss().ByClickingBackground().TryShow();
                    }
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_SelectQCN")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotSupportSystem")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async void BackupQcn(object sender, RoutedEventArgs args)
    {
        // Backup QCN file
        if (Global.System == "Windows")
        {
            if (await GetDevicesInfo.SetDevicesInfoLittle())
            {
                MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
                if (sukiViewModel.Status is "901D" or "9091")
                {
                    BusyQCN.IsBusy = true;
                    QCN.IsEnabled = false;
                    output = "";
                    FormatExtractLog.Text = GetTranslation("FormatExtract_BackingUp") + "\n";
                    int com = StringHelper.Onlynum(Global.thisdevice);
                    string shell = string.Format($"-r -p {com}");
                    await QCNTool(shell);
                    _ = FormatExtractLog.Text.Contains("error")
                        ? Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_BackupFailed")).Dismiss().ByClickingBackground().TryShow()
                        : Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Succ")).OfType(NotificationType.Success).WithContent(GetTranslation("FormatExtract_BackupSucc")).Dismiss().ByClickingBackground().TryShow();
                    BusyQCN.IsBusy = false;
                    QCN.IsEnabled = true;
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_Open901D")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotSupportSystem")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async void OpenBackup(object sender, RoutedEventArgs args)
    {
        if (OperatingSystem.IsLinux() && Global.backup_path == null)
        {
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Warn"))
                                        .WithContent(GetTranslation("FormatExtract_ExtractFolder"))
                                        .OfType(NotificationType.Warning)
                                        .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                                        {
                                            TopLevel topLevel = TopLevel.GetTopLevel(this);
                                            System.Collections.Generic.IReadOnlyList<IStorageFolder> files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
                                            {
                                                Title = "Select Buckup Folder",
                                                AllowMultiple = false
                                            });
                                            if (files.Count >= 1)
                                            {
                                                if (FileHelper.TestPermission(files[0].TryGetLocalPath()))
                                                {
                                                    Global.backup_path = files[0].TryGetLocalPath();
                                                }
                                                else
                                                {
                                                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_FolderNoPermission")).Dismiss().ByClickingBackground().TryShow();
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }, true)
                                        .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                                        .TryShow();
        }
        if (Global.backup_path != null)
        {
            FileHelper.OpenFolder(Path.Combine(Global.backup_path));
        }
    }

    private async void Enable901d(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Warn"))
                                        .WithContent(GetTranslation("Common_NeedRoot"))
                                        .OfType(NotificationType.Warning)
                                        .WithActionButton(GetTranslation("Common_DebugMode"), async _ =>
                                        {
                                            BusyQCN.IsBusy = true;
                                            QCN.IsEnabled = false;
                                            await CallExternalProgram.ADB($"-s {Global.thisdevice} root");
                                            await CallExternalProgram.ADB($"-s {Global.thisdevice} shell setprop sys.usb.config diag,adb");
                                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                                            BusyQCN.IsBusy = false;
                                            QCN.IsEnabled = true;
                                        }, true)
                                        .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                                        {
                                            BusyQCN.IsBusy = true;
                                            QCN.IsEnabled = false;
                                            await CallExternalProgram.ADB($"-s {Global.thisdevice} shell su -c \"setprop sys.usb.config diag,adb\"");
                                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                                            BusyQCN.IsBusy = false;
                                            QCN.IsEnabled = true;
                                        }, true)
                                        .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                                        .TryShow();
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async void Enable9091(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Warn"))
                                        .WithContent(GetTranslation("FormatExtract_OnlyXiaomi"))
                                        .OfType(NotificationType.Warning)
                                        .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                                        {
                                            BusyQCN.IsBusy = true;
                                            QCN.IsEnabled = false;
                                            await CallExternalProgram.ADB($"-s {Global.thisdevice} push \"{Path.Combine(Global.runpath, "APK", "mi_diag.apk")}\" /sdcard");
                                            await CallExternalProgram.ADB($"-s {Global.thisdevice} shell \"am start -a miui.intent.action.OPEN\"");
                                            Global.MainDialogManager.CreateDialog()
                                                                    .WithTitle(GetTranslation("Common_Error"))
                                                                    .OfType(NotificationType.Information)
                                                                    .WithContent(GetTranslation("FormatExtract_DiagApk"))
                                                                    .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                                                                    {
                                                                        await CallExternalProgram.ADB($"-s {Global.thisdevice} shell \"am start -n com.longcheertel.midtest/\"");
                                                                        await CallExternalProgram.ADB($"-s {Global.thisdevice} shell \"am start -n com.longcheertel.midtest/com.longcheertel.midtest.Diag\"");
                                                                    }, true)
                                                                    .TryShow();
                                            BusyQCN.IsBusy = false;
                                            QCN.IsEnabled = true;
                                        }, true)
                                        .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                                        .TryShow();
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async void OpenEmptyFile(object sender, RoutedEventArgs args)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open SuperEmpty File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            SuperEmptyFile.Text = files[0].TryGetLocalPath();
        }
    }

    private async void FlashSuperEmpty(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
            {
                if (!string.IsNullOrEmpty(SuperEmptyFile.Text))
                {
                    Global.checkdevice = false;
                    BusyFlash.IsBusy = true;
                    SuperEmpty.IsEnabled = false;
                    output = "";
                    FormatExtractLog.Text = GetTranslation("Customizedflash_Flashing") + "\n";
                    await Fastboot($"-s {Global.thisdevice} wipe-super \"{SuperEmptyFile.Text}\"");
                    _ = !output.Contains("FAILED") && !output.Contains("error")
                        ? Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Succ")).OfType(NotificationType.Success).WithContent(GetTranslation("Basicflash_FlashSucc")).Dismiss().ByClickingBackground().TryShow()
                        : Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Basicflash_RecoveryFailed")).Dismiss().ByClickingBackground().TryShow();
                    BusyFlash.IsBusy = false;
                    SuperEmpty.IsEnabled = true;
                    Global.checkdevice = true;
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_SelectSuperEmpty")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async void ADBFormat(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Recovery"))
            {
                if (!string.IsNullOrEmpty(FormatName.Text))
                {
                    BusyFormat.IsBusy = true;
                    Format.IsEnabled = false;
                    output = "";
                    FormatExtractLog.Text = GetTranslation("FormatExtract_Formatting") + "\n";
                    string formatsystem = "";
                    if (EXT4.IsChecked != null && (bool)EXT4.IsChecked)
                    {
                        formatsystem = "mke2fs -t ext4";
                    }

                    if (F2FS.IsChecked != null && (bool)F2FS.IsChecked)
                    {
                        formatsystem = "/tmp/mkfs.f2fs";
                    }

                    if (FAT32.IsChecked != null && (bool)FAT32.IsChecked)
                    {
                        formatsystem = "mkfs.fat -F32 -s1";
                    }

                    if (exFAT.IsChecked != null && (bool)exFAT.IsChecked)
                    {
                        formatsystem = "mkexfatfs -n exfat";
                    }

                    if (NTFS.IsChecked != null && (bool)NTFS.IsChecked)
                    {
                        formatsystem = "/tmp/mkntfs -f";
                    }

                    string partname = FormatName.Text;
                    await FeaturesHelper.GetPartTable(Global.thisdevice);
                    FeaturesHelper.PushMakefs(Global.thisdevice);
                    string sdxx = FeaturesHelper.FindDisk(partname);
                    if (sdxx != "")
                    {
                        await Task.Run(() =>
                        {
                            Thread.Sleep(1000);
                        });
                        string partnum = StringHelper.Partno(FeaturesHelper.FindPart(partname), partname);
                        string shell = string.Format($"-s {Global.thisdevice} shell {formatsystem} /dev/block/{sdxx}{partnum}");
                        await ADB(shell);
                    }
                    else
                    {
                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NotFound")).Dismiss().ByClickingBackground().TryShow();
                    }
                    BusyFormat.IsBusy = false;
                    Format.IsEnabled = true;
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_EnterFormatPart")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterRecovery")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async void FastbootFormat(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
            {
                if (!string.IsNullOrEmpty(FormatName.Text))
                {
                    BusyFormat.IsBusy = true;
                    Format.IsEnabled = false;
                    output = "";
                    FormatExtractLog.Text = GetTranslation("FormatExtract_Formatting") + "\n";
                    string partname = FormatName.Text;
                    string shell = string.Format($"-s {Global.thisdevice} erase {partname}");
                    await Fastboot(shell);
                    BusyFormat.IsBusy = false;
                    Format.IsEnabled = true;
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_EnterFormatPart")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async void FormatData(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Recovery"))
            {
                BusyFormat.IsBusy = true;
                Format.IsEnabled = false;
                output = "";
                FormatExtractLog.Text = GetTranslation("FormatExtract_Formatting") + "\n";
                await ADB($"-s {Global.thisdevice} shell recovery --wipe_data");
                BusyFormat.IsBusy = false;
                Format.IsEnabled = true;
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterRecovery")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async void TWRPFormatData(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Recovery"))
            {
                BusyFormat.IsBusy = true;
                Format.IsEnabled = false;
                output = "";
                FormatExtractLog.Text = GetTranslation("FormatExtract_Formatting") + "\n";
                await ADB($"-s {Global.thisdevice} shell twrp format data");
                BusyFormat.IsBusy = false;
                Format.IsEnabled = true;
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterRecovery")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async void ExtractPart(object sender, RoutedEventArgs args)
    {
        if (OperatingSystem.IsLinux() && Global.backup_path == null)
        {
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Warn"))
                                        .WithContent(GetTranslation("FormatExtract_ExtractFolder"))
                                        .OfType(NotificationType.Warning)
                                        .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                                        {
                                            TopLevel topLevel = TopLevel.GetTopLevel(this);
                                            System.Collections.Generic.IReadOnlyList<IStorageFolder> files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
                                            {
                                                Title = "Select Buckup Folder",
                                                AllowMultiple = false
                                            });
                                            if (files.Count >= 1)
                                            {
                                                if (FileHelper.TestPermission(files[0].TryGetLocalPath()))
                                                {
                                                    Global.backup_path = files[0].TryGetLocalPath();
                                                }
                                                else
                                                {
                                                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_FolderNoPermission")).Dismiss().ByClickingBackground().TryShow();
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }, true)
                                        .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                                        .TryShow();
        }
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Recovery"))
            {
                if (!string.IsNullOrEmpty(ExtractName.Text))
                {
                    BusyExtract.IsBusy = true;
                    Extract.IsEnabled = false;
                    output = "";
                    FormatExtractLog.Text = GetTranslation("FormatExtract_Extracting") + "\n";
                    string partname = ExtractName.Text;
                    await FeaturesHelper.GetPartTable(Global.thisdevice);
                    string sdxx = FeaturesHelper.FindDisk(partname);
                    if (sdxx != "")
                    {
                        string partnum = StringHelper.Partno(FeaturesHelper.FindPart(partname), partname);
                        string shell = string.Format($"-s {Global.thisdevice} shell dd if=/dev/block/{sdxx}{partnum} of={partname}.img");
                        await ADB(shell);
                        FileHelper.Write(adb_log_path, output);
                        if (output.Contains("No space left on device"))
                        {
                            FormatExtractLog.Text = GetTranslation("FormatExtract_TryUseData");
                            shell = string.Format($"-s {Global.thisdevice} shell rm /{partname}.img");
                            await ADB(shell);
                            shell = string.Format($"-s {Global.thisdevice} shell dd if=/dev/block/{sdxx}{partnum} of=/sdcard/{partname}.img");
                            await ADB(shell);
                            shell = string.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img \"{Global.backup_path}\"");
                            await ADB(shell);
                            shell = string.Format($"-s {Global.thisdevice} shell rm /sdcard/{partname}.img");
                            await ADB(shell);
                        }
                        else
                        {
                            shell = string.Format($"-s {Global.thisdevice} pull /{partname}.img \"{Global.backup_path}\"");
                            await ADB(shell);
                            shell = string.Format($"-s {Global.thisdevice} shell rm /{partname}.img");
                            await ADB(shell);
                        }
                    }
                    else
                    {
                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NotFound")).Dismiss().ByClickingBackground().TryShow();
                    }
                    BusyExtract.IsBusy = false;
                    Extract.IsEnabled = true;
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_EnterExtractPart")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                if (!string.IsNullOrEmpty(ExtractName.Text))
                {
                    Global.MainDialogManager.CreateDialog()
                                                .WithTitle(GetTranslation("Common_Warn"))
                                                .WithContent(GetTranslation("Common_NeedRoot"))
                                                .OfType(NotificationType.Warning)
                                                .WithActionButton(GetTranslation("Common_DebugMode"), async _ =>
                                                {
                                                    BusyExtract.IsBusy = true;
                                                    Extract.IsEnabled = false;
                                                    output = "";
                                                    FormatExtractLog.Text = GetTranslation("FormatExtract_Extracting") + "\n";
                                                    string partname = ExtractName.Text;
                                                    await FeaturesHelper.GetPartTableSystemDebug(Global.thisdevice);
                                                    string sdxx = FeaturesHelper.FindDisk(partname);
                                                    if (sdxx != "")
                                                    {
                                                        string partnum = StringHelper.Partno(FeaturesHelper.FindPart(partname), partname);
                                                        string shell = string.Format($"-s {Global.thisdevice} shell dd if=/dev/block/{sdxx}{partnum} of=/tmp/{partname}.img");
                                                        await ADB(shell);
                                                        FileHelper.Write(adb_log_path, output);
                                                        if (!output.Contains("No space left on device"))
                                                        {
                                                            shell = string.Format($"-s {Global.thisdevice} pull /tmp/{partname}.img \"{Global.backup_path}\"");
                                                            await ADB(shell);
                                                            shell = string.Format($"-s {Global.thisdevice} shell rm /tmp/{partname}.img");
                                                            await ADB(shell);
                                                        }
                                                        else
                                                        {
                                                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NoSpace")).Dismiss().ByClickingBackground().TryShow();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NotFound")).Dismiss().ByClickingBackground().TryShow();
                                                    }
                                                    BusyExtract.IsBusy = false;
                                                    Extract.IsEnabled = true;
                                                }, true)
                                                .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                                                {
                                                    BusyExtract.IsBusy = true;
                                                    Extract.IsEnabled = false;
                                                    output = "";
                                                    FormatExtractLog.Text = GetTranslation("FormatExtract_Extracting") + "\n";
                                                    string partname = ExtractName.Text;
                                                    await FeaturesHelper.GetPartTableSystem(Global.thisdevice);
                                                    string sdxx = FeaturesHelper.FindDisk(partname);
                                                    if (sdxx != "")
                                                    {
                                                        string partnum = StringHelper.Partno(FeaturesHelper.FindPart(partname), partname);
                                                        string shell = string.Format($"-s {Global.thisdevice} shell su -c \"dd if=/dev/block/{sdxx}{partnum} of=/sdcard/{partname}.img\"");
                                                        await ADB(shell);
                                                        FileHelper.Write(adb_log_path, output);
                                                        if (!output.Contains("No space left on device"))
                                                        {
                                                            shell = string.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img \"{Global.backup_path}\"");
                                                            await ADB(shell);
                                                            shell = string.Format($"-s {Global.thisdevice} shell su -c \"rm /sdcard/{partname}.img\"");
                                                            await ADB(shell);
                                                        }
                                                        else
                                                        {
                                                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NoSpace")).Dismiss().ByClickingBackground().TryShow();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NotFound")).Dismiss().ByClickingBackground().TryShow();
                                                    }
                                                    BusyExtract.IsBusy = false;
                                                    Extract.IsEnabled = true;
                                                }, true)
                                                .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                                                .TryShow();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_EnterExtractPart")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterRecOrOpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async void ExtractVPart(object sender, RoutedEventArgs args)
    {
        if (OperatingSystem.IsLinux() && Global.backup_path == null)
        {
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Warn"))
                                        .WithContent(GetTranslation("FormatExtract_ExtractFolder"))
                                        .OfType(NotificationType.Warning)
                                        .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                                        {
                                            TopLevel topLevel = TopLevel.GetTopLevel(this);
                                            System.Collections.Generic.IReadOnlyList<IStorageFolder> files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
                                            {
                                                Title = "Select Buckup Folder",
                                                AllowMultiple = false
                                            });
                                            if (files.Count >= 1)
                                            {
                                                if (FileHelper.TestPermission(files[0].TryGetLocalPath()))
                                                {
                                                    Global.backup_path = files[0].TryGetLocalPath();
                                                }
                                                else
                                                {
                                                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_FolderNoPermission")).Dismiss().ByClickingBackground().TryShow();
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }, true)
                                        .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                                        .TryShow();
        }
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Recovery"))
            {
                if (!string.IsNullOrEmpty(ExtractName.Text))
                {
                    BusyExtract.IsBusy = true;
                    Extract.IsEnabled = false;
                    output = "";
                    FormatExtractLog.Text = GetTranslation("FormatExtract_Extracting") + "\n";
                    string partname = ExtractName.Text;
                    string shell = string.Format($"-s {Global.thisdevice} shell ls -l /dev/block/mapper/{partname}");
                    string vmpart = await CallExternalProgram.ADB(shell);
                    if (!vmpart.Contains("No such file or directory"))
                    {
                        char[] charSeparators = [' ', '\r', '\n'];
                        string[] line = vmpart.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                        string devicepoint = line[^1];
                        shell = string.Format($"-s {Global.thisdevice} shell dd if={devicepoint} of={partname}.img");
                        await ADB(shell);
                        FileHelper.Write(adb_log_path, output);
                        if (output.Contains("No space left on device"))
                        {
                            FormatExtractLog.Text = GetTranslation("FormatExtract_TryUseData");
                            shell = string.Format($"-s {Global.thisdevice} shell rm /{partname}.img");
                            await ADB(shell);
                            shell = string.Format($"-s {Global.thisdevice} shell dd if={devicepoint} of=/sdcard/{partname}.img");
                            await ADB(shell);
                            shell = string.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img \"{Global.backup_path}\"");
                            await ADB(shell);
                            shell = string.Format($"-s {Global.thisdevice} shell rm /sdcard/{partname}.img");
                            await ADB(shell);
                        }
                        else
                        {
                            shell = string.Format($"-s {Global.thisdevice} pull /{partname}.img \"{Global.backup_path}\"");
                            await ADB(shell);
                            shell = string.Format($"-s {Global.thisdevice} shell rm /{partname}.img");
                            await ADB(shell);
                        }
                    }
                    else
                    {
                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NotFound")).Dismiss().ByClickingBackground().TryShow();
                    }
                    BusyExtract.IsBusy = false;
                    Extract.IsEnabled = true;
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_EnterExtractPart")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                if (!string.IsNullOrEmpty(ExtractName.Text))
                {
                    Global.MainDialogManager.CreateDialog()
                                                .WithTitle(GetTranslation("Common_Warn"))
                                                .WithContent(GetTranslation("Common_NeedRoot"))
                                                .OfType(NotificationType.Warning)
                                                .WithActionButton(GetTranslation("Common_DebugMode"), async _ =>
                                                {
                                                    BusyExtract.IsBusy = true;
                                                    Extract.IsEnabled = false;
                                                    output = "";
                                                    FormatExtractLog.Text = GetTranslation("FormatExtract_Extracting") + "\n";
                                                    string partname = ExtractName.Text;
                                                    await CallExternalProgram.ADB($"-s {Global.thisdevice} root");
                                                    string shell = string.Format($"-s {Global.thisdevice} shell ls -l /dev/block/mapper/{partname}");
                                                    string vmpart = await CallExternalProgram.ADB(shell);
                                                    if (!vmpart.Contains("No such file or directory"))
                                                    {
                                                        char[] charSeparators = [' ', '\r', '\n'];
                                                        string[] line = vmpart.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                                                        string devicepoint = line[^1];
                                                        shell = string.Format($"-s {Global.thisdevice} shell dd if={devicepoint} of=/sdcard/{partname}.img");
                                                        await ADB(shell);
                                                        FileHelper.Write(adb_log_path, output);
                                                        if (!output.Contains("No space left on device"))
                                                        {
                                                            shell = string.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img \"{Global.backup_path}\"");
                                                            await ADB(shell);
                                                            shell = string.Format($"-s {Global.thisdevice} shell rm /sdcard/{partname}.img");
                                                            await ADB(shell);
                                                        }
                                                        else
                                                        {
                                                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NoSpace")).Dismiss().ByClickingBackground().TryShow();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NotFound")).Dismiss().ByClickingBackground().TryShow();
                                                    }
                                                    BusyExtract.IsBusy = false;
                                                    Extract.IsEnabled = true;
                                                }, true)
                                                .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                                                {
                                                    BusyExtract.IsBusy = true;
                                                    Extract.IsEnabled = false;
                                                    output = "";
                                                    FormatExtractLog.Text = GetTranslation("FormatExtract_Extracting") + "\n";
                                                    string partname = ExtractName.Text;
                                                    string shell = string.Format($"-s {Global.thisdevice} shell su -c \"ls -l /dev/block/mapper/{partname}\"");
                                                    string vmpart = await CallExternalProgram.ADB(shell);
                                                    if (!vmpart.Contains("No such file or directory"))
                                                    {
                                                        char[] charSeparators = [' ', '\r', '\n'];
                                                        string[] line = vmpart.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                                                        string devicepoint = line[^1];
                                                        shell = string.Format($"-s {Global.thisdevice} shell su -c \"dd if={devicepoint} of=/sdcard/{partname}.img\"");
                                                        await ADB(shell);
                                                        FileHelper.Write(adb_log_path, output);
                                                        if (!output.Contains("No space left on device"))
                                                        {
                                                            shell = string.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img \"{Global.backup_path}\"");
                                                            await ADB(shell);
                                                            shell = string.Format($"-s {Global.thisdevice} shell su -c \"rm /sdcard/{partname}.img\"");
                                                            await ADB(shell);
                                                        }
                                                        else
                                                        {
                                                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NoSpace")).Dismiss().ByClickingBackground().TryShow();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NotFound")).Dismiss().ByClickingBackground().TryShow();
                                                    }
                                                    BusyExtract.IsBusy = false;
                                                    Extract.IsEnabled = true;
                                                }, true)
                                                .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                                                .TryShow();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_EnterExtractPart")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterRecOrOpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async void FullBackup(object sender, RoutedEventArgs args)
    {
        if (OperatingSystem.IsLinux() && Global.backup_path == null)
        {
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Warn"))
                                        .WithContent(GetTranslation("FormatExtract_ExtractFolder"))
                                        .OfType(NotificationType.Warning)
                                        .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                                        {
                                            TopLevel topLevel = TopLevel.GetTopLevel(this);
                                            System.Collections.Generic.IReadOnlyList<IStorageFolder> files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
                                            {
                                                Title = "Select Buckup Folder",
                                                AllowMultiple = false
                                            });
                                            if (files.Count >= 1)
                                            {
                                                if (FileHelper.TestPermission(files[0].TryGetLocalPath()))
                                                {
                                                    Global.backup_path = files[0].TryGetLocalPath();
                                                }
                                                else
                                                {
                                                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_FolderNoPermission")).Dismiss().ByClickingBackground().TryShow();
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }, true)
                                        .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                                        .TryShow();
        }
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Recovery"))
            {
                BusyExtract.IsBusy = true;
                Extract.IsEnabled = false;
                output = "";
                string backup_images_folder = Path.Combine(Global.backup_path, $"{sukiViewModel.CodeName}_UotanToolbox_FullBackup_{sukiViewModel.VABStatus}_{DateTime.Now:yyyyMMddHHmmss}", "images");
                Directory.CreateDirectory(backup_images_folder);
                string backup_txt_path = Path.Combine(Global.backup_path, $"{sukiViewModel.CodeName}_UotanToolbox_FullBackup_{sukiViewModel.VABStatus}_{DateTime.Now:yyyyMMddHHmmss}", "flashall_fastboot.txt");
                string partlist = "";
                FormatExtractLog.Text = GetTranslation("FormatExtract_Extracting") + "\n";
                await FeaturesHelper.GetPartTable(Global.thisdevice);
                string[] diskTables = [Global.sdatable, Global.sdetable, Global.sdbtable, Global.sdctable, Global.sddtable, Global.sdftable, Global.sdgtable, Global.sdhtable, Global.emmcrom];
                string[] diskNames = ["sda", "sde", "sdb", "sdc", "sdd", "sdf", "sdg", "sdh", "mmcblk0p"];
                int desk = 0;
                for (int i = 0; i < diskTables.Length; i++)
                {
                    if (!string.IsNullOrEmpty(diskTables[i]))
                    {
                        string[] parts = [.. diskTables[i].Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Where(line => !string.IsNullOrWhiteSpace(line))];
                        if (parts.Length > 6)
                        {
                            PartModel[] part = new PartModel[parts.Length - 6];
                            for (int j = 6; j < parts.Length; j++)
                            {
                                string[] items = StringHelper.Items(parts[j].ToCharArray());
                                if (items[5] == "userdata")
                                    continue;
                                string partname = "";
                                if (partlist.Contains($"\r\n{items[5]}\r\n"))
                                {
                                    partname = $"{items[5]}_{diskNames[i]}";
                                }
                                else
                                {
                                    partname = items[5];
                                }
                                await ADB($"-s {Global.thisdevice} shell dd if=/dev/block/{diskNames[i]}{items[0]} of=/sdcard/{partname}.img");
                                FileHelper.Write(adb_log_path, output);
                                if (output.Contains("No space left on device"))
                                {
                                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NoSpace")).Dismiss().ByClickingBackground().TryShow();
                                    BusyExtract.IsBusy = false;
                                    Extract.IsEnabled = true;
                                    return;
                                }
                                else if (output.Contains("not found") || output.Contains("unknown command"))
                                {
                                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_DeviceDiscon")).Dismiss().ByClickingBackground().TryShow();
                                    BusyExtract.IsBusy = false;
                                    Extract.IsEnabled = true;
                                    return;
                                }
                                else
                                {
                                    await ADB($"-s {Global.thisdevice} pull /sdcard/{partname}.img \"{backup_images_folder}\"");
                                    await ADB($"-s {Global.thisdevice} shell rm /sdcard/{partname}.img");
                                    if (partlist.Contains($"\r\n{items[5]}\r\n"))
                                    {
                                        partlist += $"{items[5]}       /images/{partname}.img\r\n";
                                    }
                                    else
                                    {
                                        partlist += $"{items[5]}\r\n";
                                    }
                                }
                            }
                        }
                        else
                        {
                            desk++;
                            if (desk == 9)
                            {
                                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NoPart")).Dismiss().ByClickingBackground().TryShow();
                                BusyExtract.IsBusy = false;
                                Extract.IsEnabled = true;
                                return;
                            }
                        }
                    }
                }
                string[] otherpart = ["spl", "spl_a", "spl_b", "preloader_raw", "perloader_raw_a", "preloader_raw_b"];
                for (int i = 0; i < otherpart.Length; i++)
                {
                    string shell = string.Format($"-s {Global.thisdevice} shell ls -l /dev/block/by-name/{otherpart[i]}");
                    string path = await CallExternalProgram.ADB(shell);
                    string shell2 = string.Format($"-s {Global.thisdevice} shell ls -l /dev/block/bootdevice/by-name/{otherpart[i]}");
                    string path2 = await CallExternalProgram.ADB(shell);
                    if (!path.Contains("No such file or directory"))
                    {
                        shell = string.Format($"-s {Global.thisdevice} shell dd if=/dev/block/by-name/{otherpart[i]} of=/sdcard/{otherpart[i]}.img");
                        await ADB(shell);
                        FileHelper.Write(adb_log_path, output);
                        if (output.Contains("No space left on device"))
                        {
                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NoSpace")).Dismiss().ByClickingBackground().TryShow();
                            BusyExtract.IsBusy = false;
                            Extract.IsEnabled = true;
                            return;
                        }
                        else if (output.Contains("not found") || output.Contains("unknown command"))
                        {
                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_DeviceDiscon")).Dismiss().ByClickingBackground().TryShow();
                            BusyExtract.IsBusy = false;
                            Extract.IsEnabled = true;
                            return;
                        }
                        else
                        {
                            await ADB($"-s {Global.thisdevice} pull /sdcard/{otherpart[i]}.img \"{backup_images_folder}\"");
                            await ADB($"-s {Global.thisdevice} shell rm /sdcard/{otherpart[i]}.img");
                            partlist += $"{otherpart[i]}\r\n";
                        }
                    }
                    else if (!path2.Contains("No such file or directory"))
                    {
                        shell = string.Format($"-s {Global.thisdevice} shell dd if=/dev/block/bootdevice/by-name/{otherpart[i]} of=/sdcard/{otherpart[i]}.img");
                        await ADB(shell);
                        FileHelper.Write(adb_log_path, output);
                        if (output.Contains("No space left on device"))
                        {
                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NoSpace")).Dismiss().ByClickingBackground().TryShow();
                            BusyExtract.IsBusy = false;
                            Extract.IsEnabled = true;
                            return;
                        }
                        else if (output.Contains("not found") || output.Contains("unknown command"))
                        {
                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_DeviceDiscon")).Dismiss().ByClickingBackground().TryShow();
                            BusyExtract.IsBusy = false;
                            Extract.IsEnabled = true;
                            return;
                        }
                        else
                        {
                            await ADB($"-s {Global.thisdevice} pull /sdcard/{otherpart[i]}.img \"{backup_images_folder}\"");
                            await ADB($"-s {Global.thisdevice} shell rm /sdcard/{otherpart[i]}.img");
                            partlist += $"{otherpart[i]}\r\n";
                        }
                    }
                }
                var sortedParts = partlist
                    .Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                partlist = string.Join("\r\n", sortedParts);
                FileHelper.Write(backup_txt_path, partlist);
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Succ")).OfType(NotificationType.Success).WithContent(GetTranslation("FormatExtract_FullBackupDone")).Dismiss().ByClickingBackground().TryShow();
                BusyExtract.IsBusy = false;
                Extract.IsEnabled = true;
            }
            else if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                Global.MainDialogManager.CreateDialog()
                                            .WithTitle(GetTranslation("Common_Warn"))
                                            .WithContent(GetTranslation("Common_NeedRoot"))
                                            .OfType(NotificationType.Warning)
                                            .WithActionButton(GetTranslation("Common_DebugMode"), async _ =>
                                            {
                                                BusyExtract.IsBusy = true;
                                                Extract.IsEnabled = false;
                                                output = "";
                                                string backup_images_folder = Path.Combine(Global.backup_path, $"{sukiViewModel.CodeName}_UotanToolbox_FullBackup_{sukiViewModel.VABStatus}_{DateTime.Now:yyyyMMddHHmmss}", "images");
                                                Directory.CreateDirectory(backup_images_folder);
                                                string backup_txt_path = Path.Combine(Global.backup_path, $"{sukiViewModel.CodeName}_UotanToolbox_FullBackup_{sukiViewModel.VABStatus}_{DateTime.Now:yyyyMMddHHmmss}", "flashall_fastboot.txt");
                                                string partlist = "";
                                                FormatExtractLog.Text = GetTranslation("FormatExtract_Extracting") + "\n";
                                                await FeaturesHelper.GetPartTableSystemDebug(Global.thisdevice);
                                                string[] diskTables = [Global.sdatable, Global.sdetable, Global.sdbtable, Global.sdctable, Global.sddtable, Global.sdftable, Global.sdgtable, Global.sdhtable, Global.emmcrom];
                                                string[] diskNames = ["sda", "sde", "sdb", "sdc", "sdd", "sdf", "sdg", "sdh", "mmcblk0p"];
                                                int desk = 0;
                                                for (int i = 0; i < diskTables.Length; i++)
                                                {
                                                    if (!string.IsNullOrEmpty(diskTables[i]))
                                                    {
                                                        string[] parts = [.. diskTables[i].Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Where(line => !string.IsNullOrWhiteSpace(line))];
                                                        if (parts.Length > 6)
                                                        {
                                                            PartModel[] part = new PartModel[parts.Length - 6];
                                                            for (int j = 6; j < parts.Length; j++)
                                                            {
                                                                string[] items = StringHelper.Items(parts[j].ToCharArray());
                                                                if (items[5] == "userdata")
                                                                    continue;
                                                                string partname = "";
                                                                if (partlist.Contains($"\r\n{items[5]}\r\n"))
                                                                {
                                                                    partname = $"{items[5]}_{diskNames[i]}";
                                                                }
                                                                else
                                                                {
                                                                    partname = items[5];
                                                                }
                                                                await ADB($"-s {Global.thisdevice} shell dd if=/dev/block/{diskNames[i]}{items[0]} of=/sdcard/{partname}.img");
                                                                FileHelper.Write(adb_log_path, output);
                                                                if (output.Contains("No space left on device"))
                                                                {
                                                                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NoSpace")).Dismiss().ByClickingBackground().TryShow();
                                                                    BusyExtract.IsBusy = false;
                                                                    Extract.IsEnabled = true;
                                                                    return;
                                                                }
                                                                else if (output.Contains("not found") || output.Contains("unknown command"))
                                                                {
                                                                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_DeviceDiscon")).Dismiss().ByClickingBackground().TryShow();
                                                                    BusyExtract.IsBusy = false;
                                                                    Extract.IsEnabled = true;
                                                                    return;
                                                                }
                                                                else
                                                                {
                                                                    await ADB($"-s {Global.thisdevice} pull /sdcard/{partname}.img \"{backup_images_folder}\"");
                                                                    await ADB($"-s {Global.thisdevice} shell rm /sdcard/{partname}.img");
                                                                    if (partlist.Contains($"\r\n{items[5]}\r\n"))
                                                                    {
                                                                        partlist += $"{items[5]}       /images/{partname}.img\r\n";
                                                                    }
                                                                    else
                                                                    {
                                                                        partlist += $"{items[5]}\r\n";
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            desk++;
                                                            if (desk == 9)
                                                            {
                                                                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NoPart")).Dismiss().ByClickingBackground().TryShow();
                                                                BusyExtract.IsBusy = false;
                                                                Extract.IsEnabled = true;
                                                                return;
                                                            }
                                                        }
                                                    }
                                                }
                                                string[] otherpart = ["spl", "spl_a", "spl_b", "preloader_raw", "perloader_raw_a", "preloader_raw_b"];
                                                for (int i = 0; i < otherpart.Length; i++)
                                                {
                                                    string shell = string.Format($"-s {Global.thisdevice} shell ls -l /dev/block/by-name/{otherpart[i]}");
                                                    string path = await CallExternalProgram.ADB(shell);
                                                    string shell2 = string.Format($"-s {Global.thisdevice} shell ls -l /dev/block/bootdevice/by-name/{otherpart[i]}");
                                                    string path2 = await CallExternalProgram.ADB(shell);
                                                    if (!path.Contains("No such file or directory"))
                                                    {
                                                        shell = string.Format($"-s {Global.thisdevice} shell dd if=/dev/block/by-name/{otherpart[i]} of=/sdcard/{otherpart[i]}.img");
                                                        await ADB(shell);
                                                        FileHelper.Write(adb_log_path, output);
                                                        if (output.Contains("No space left on device"))
                                                        {
                                                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NoSpace")).Dismiss().ByClickingBackground().TryShow();
                                                            BusyExtract.IsBusy = false;
                                                            Extract.IsEnabled = true;
                                                            return;
                                                        }
                                                        else if (output.Contains("not found") || output.Contains("unknown command"))
                                                        {
                                                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_DeviceDiscon")).Dismiss().ByClickingBackground().TryShow();
                                                            BusyExtract.IsBusy = false;
                                                            Extract.IsEnabled = true;
                                                            return;
                                                        }
                                                        else
                                                        {
                                                            await ADB($"-s {Global.thisdevice} pull /sdcard/{otherpart[i]}.img \"{backup_images_folder}\"");
                                                            await ADB($"-s {Global.thisdevice} shell rm /sdcard/{otherpart[i]}.img");
                                                            partlist += $"{otherpart[i]}\r\n";
                                                        }
                                                    }
                                                    else if (!path2.Contains("No such file or directory"))
                                                    {
                                                        shell = string.Format($"-s {Global.thisdevice} shell dd if=/dev/block/bootdevice/by-name/{otherpart[i]} of=/sdcard/{otherpart[i]}.img");
                                                        await ADB(shell);
                                                        FileHelper.Write(adb_log_path, output);
                                                        if (output.Contains("No space left on device"))
                                                        {
                                                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NoSpace")).Dismiss().ByClickingBackground().TryShow();
                                                            BusyExtract.IsBusy = false;
                                                            Extract.IsEnabled = true;
                                                            return;
                                                        }
                                                        else if (output.Contains("not found") || output.Contains("unknown command"))
                                                        {
                                                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_DeviceDiscon")).Dismiss().ByClickingBackground().TryShow();
                                                            BusyExtract.IsBusy = false;
                                                            Extract.IsEnabled = true;
                                                            return;
                                                        }
                                                        else
                                                        {
                                                            await ADB($"-s {Global.thisdevice} pull /sdcard/{otherpart[i]}.img \"{backup_images_folder}\"");
                                                            await ADB($"-s {Global.thisdevice} shell rm /sdcard/{otherpart[i]}.img");
                                                            partlist += $"{otherpart[i]}\r\n";
                                                        }
                                                    }
                                                }
                                                var sortedParts = partlist
                                                    .Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries)
                                                    .Select(s => s.Trim())
                                                    .Where(s => !string.IsNullOrEmpty(s))
                                                    .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                                                    .ToArray();
                                                partlist = string.Join("\r\n", sortedParts);
                                                FileHelper.Write(backup_txt_path, partlist);
                                                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Succ")).OfType(NotificationType.Success).WithContent(GetTranslation("FormatExtract_FullBackupDone")).Dismiss().ByClickingBackground().TryShow();
                                                BusyExtract.IsBusy = false;
                                                Extract.IsEnabled = true;
                                            }, true)
                                            .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                                            {
                                                BusyExtract.IsBusy = true;
                                                Extract.IsEnabled = false;
                                                output = "";
                                                string backup_images_folder = Path.Combine(Global.backup_path, $"{sukiViewModel.CodeName}_UotanToolbox_FullBackup_{sukiViewModel.VABStatus}_{DateTime.Now:yyyyMMddHHmmss}", "images");
                                                Directory.CreateDirectory(backup_images_folder);
                                                string backup_txt_path = Path.Combine(Global.backup_path, $"{sukiViewModel.CodeName}_UotanToolbox_FullBackup_{sukiViewModel.VABStatus}_{DateTime.Now:yyyyMMddHHmmss}", "flashall_fastboot.txt");
                                                string partlist = "";
                                                FormatExtractLog.Text = GetTranslation("FormatExtract_Extracting") + "\n";
                                                await FeaturesHelper.GetPartTableSystem(Global.thisdevice);
                                                string[] diskTables = [Global.sdatable, Global.sdetable, Global.sdbtable, Global.sdctable, Global.sddtable, Global.sdftable, Global.sdgtable, Global.sdhtable, Global.emmcrom];
                                                string[] diskNames = ["sda", "sde", "sdb", "sdc", "sdd", "sdf", "sdg", "sdh", "mmcblk0p"];
                                                int desk = 0;
                                                for (int i = 0; i < diskTables.Length; i++)
                                                {
                                                    if (!string.IsNullOrEmpty(diskTables[i]))
                                                    {
                                                        string[] parts = [.. diskTables[i].Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Where(line => !string.IsNullOrWhiteSpace(line))];
                                                        if (parts.Length > 6)
                                                        {
                                                            PartModel[] part = new PartModel[parts.Length - 6];
                                                            for (int j = 6; j < parts.Length; j++)
                                                            {
                                                                string[] items = StringHelper.Items(parts[j].ToCharArray());
                                                                if (items[5] == "userdata")
                                                                    continue;
                                                                string partname = "";
                                                                if (partlist.Contains($"\r\n{items[5]}\r\n"))
                                                                {
                                                                    partname = $"{items[5]}_{diskNames[i]}";
                                                                }
                                                                else
                                                                {
                                                                    partname = items[5];
                                                                }
                                                                await ADB($"-s {Global.thisdevice} shell su -c \"dd if=/dev/block/{diskNames[i]}{items[0]} of=/sdcard/{partname}.img\"");
                                                                FileHelper.Write(adb_log_path, output);
                                                                if (output.Contains("No space left on device"))
                                                                {
                                                                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NoSpace")).Dismiss().ByClickingBackground().TryShow();
                                                                    BusyExtract.IsBusy = false;
                                                                    Extract.IsEnabled = true;
                                                                    return;
                                                                }
                                                                else if (output.Contains("not found") || output.Contains("unknown command"))
                                                                {
                                                                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_DeviceDiscon")).Dismiss().ByClickingBackground().TryShow();
                                                                    BusyExtract.IsBusy = false;
                                                                    Extract.IsEnabled = true;
                                                                    return;
                                                                }
                                                                else
                                                                {
                                                                    await ADB($"-s {Global.thisdevice} pull /sdcard/{partname}.img \"{backup_images_folder}\"");
                                                                    await ADB($"-s {Global.thisdevice} shell su -c \"rm /sdcard/{partname}.img\"");
                                                                    if (partlist.Contains($"\r\n{items[5]}\r\n"))
                                                                    {
                                                                        partlist += $"{items[5]}       /images/{partname}.img\r\n";
                                                                    }
                                                                    else
                                                                    {
                                                                        partlist += $"{items[5]}\r\n";
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            desk++;
                                                            if (desk == 9)
                                                            {
                                                                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NoPart")).Dismiss().ByClickingBackground().TryShow();
                                                                BusyExtract.IsBusy = false;
                                                                Extract.IsEnabled = true;
                                                                return;
                                                            }
                                                        }
                                                    }
                                                }
                                                string[] otherpart = ["spl", "spl_a", "spl_b", "preloader_raw", "preloader_raw_a", "preloader_raw_b"];
                                                for (int i = 0; i < otherpart.Length; i++)
                                                {
                                                    string shell = string.Format($"-s {Global.thisdevice} shell su -c \"ls -l /dev/block/by-name/{otherpart[i]}\"");
                                                    string path = await CallExternalProgram.ADB(shell);
                                                    string shell2 = string.Format($"-s {Global.thisdevice} shell su -c \"ls -l /dev/block/bootdevice/by-name/{otherpart[i]}\"");
                                                    string path2 = await CallExternalProgram.ADB(shell);
                                                    if (!path.Contains("No such file or directory"))
                                                    {
                                                        shell = string.Format($"-s {Global.thisdevice} shell su -c \"dd if=/dev/block/by-name/{otherpart[i]} of=/sdcard/{otherpart[i]}.img\"");
                                                        await ADB(shell);
                                                        FileHelper.Write(adb_log_path, output);
                                                        if (output.Contains("No space left on device"))
                                                        {
                                                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NoSpace")).Dismiss().ByClickingBackground().TryShow();
                                                            BusyExtract.IsBusy = false;
                                                            Extract.IsEnabled = true;
                                                            return;
                                                        }
                                                        else if (output.Contains("not found") || output.Contains("unknown command"))
                                                        {
                                                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_DeviceDiscon")).Dismiss().ByClickingBackground().TryShow();
                                                            BusyExtract.IsBusy = false;
                                                            Extract.IsEnabled = true;
                                                            return;
                                                        }
                                                        else
                                                        {
                                                            await ADB($"-s {Global.thisdevice} pull /sdcard/{otherpart[i]}.img \"{backup_images_folder}\"");
                                                            await ADB($"-s {Global.thisdevice} shell su -c \"rm /sdcard/{otherpart[i]}.img\"");
                                                            partlist += $"{otherpart[i]}\r\n";
                                                        }
                                                    }
                                                    else if (!path2.Contains("No such file or directory"))
                                                    {
                                                        shell = string.Format($"-s {Global.thisdevice} shell su -c \"dd if=/dev/block/bootdevice/by-name/{otherpart[i]} of=/sdcard/{otherpart[i]}.img\"");
                                                        await ADB(shell);
                                                        FileHelper.Write(adb_log_path, output);
                                                        if (output.Contains("No space left on device"))
                                                        {
                                                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NoSpace")).Dismiss().ByClickingBackground().TryShow();
                                                            BusyExtract.IsBusy = false;
                                                            Extract.IsEnabled = true;
                                                            return;
                                                        }
                                                        else if (output.Contains("not found") || output.Contains("unknown command"))
                                                        {
                                                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_DeviceDiscon")).Dismiss().ByClickingBackground().TryShow();
                                                            BusyExtract.IsBusy = false;
                                                            Extract.IsEnabled = true;
                                                            return;
                                                        }
                                                        else
                                                        {
                                                            await ADB($"-s {Global.thisdevice} pull /sdcard/{otherpart[i]}.img \"{backup_images_folder}\"");
                                                            await ADB($"-s {Global.thisdevice} shell su -c \"rm /sdcard/{otherpart[i]}.img\"");
                                                            partlist += $"{otherpart[i]}\r\n";
                                                        }
                                                    }
                                                }
                                                var sortedParts = partlist
                                                    .Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries)
                                                    .Select(s => s.Trim())
                                                    .Where(s => !string.IsNullOrEmpty(s))
                                                    .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                                                    .ToArray();
                                                partlist = string.Join("\r\n", sortedParts);
                                                FileHelper.Write(backup_txt_path, partlist);
                                                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Succ")).OfType(NotificationType.Success).WithContent(GetTranslation("FormatExtract_FullBackupDone")).Dismiss().ByClickingBackground().TryShow();
                                                BusyExtract.IsBusy = false;
                                                Extract.IsEnabled = true;
                                            }, true)
                                            .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                                            .TryShow();
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterRecOrOpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async void OpenExtractFile(object sender, RoutedEventArgs args)
    {
        if (OperatingSystem.IsLinux() && Global.backup_path == null)
        {
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Warn"))
                                        .WithContent(GetTranslation("FormatExtract_ExtractFolder"))
                                        .OfType(NotificationType.Warning)
                                        .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                                        {
                                            TopLevel topLevel = TopLevel.GetTopLevel(this);
                                            System.Collections.Generic.IReadOnlyList<IStorageFolder> files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
                                            {
                                                Title = "Select Buckup Folder",
                                                AllowMultiple = false
                                            });
                                            if (files.Count >= 1)
                                            {
                                                if (FileHelper.TestPermission(files[0].TryGetLocalPath()))
                                                {
                                                    Global.backup_path = files[0].TryGetLocalPath();
                                                }
                                                else
                                                {
                                                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_FolderNoPermission")).Dismiss().ByClickingBackground().TryShow();
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }, true)
                                        .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                                        .TryShow();
        }
        if (Global.backup_path != null)
        {
            FileHelper.OpenFolder(Path.Combine(Global.backup_path));
        }
    }
}