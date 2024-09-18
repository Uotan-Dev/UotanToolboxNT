using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Dialogs;
using UotanToolbox.Common;

namespace UotanToolbox.Features.FormatExtract;

public partial class FormatExtractView : UserControl
{
    ISukiDialogManager dialogManager;
    static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);

    public FormatExtractView() => InitializeComponent();
    static readonly string adb_log_path = Path.Combine(Global.log_path, "adb.txt");
    string output = "";
    public async Task QCNTool(string shell)
    {
        await Task.Run(() =>
        {
            var cmd = "Bin\\QSML\\QCNTool.exe";

            var qcntool = new ProcessStartInfo(cmd, shell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = Global.backup_path,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var qcn = new Process();
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
            var cmd = Path.Combine(Global.bin_path, "platform-tools", "fastboot");

            var fastboot = new ProcessStartInfo(cmd, fbshell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var fb = new Process();
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
            var cmd = Path.Combine(Global.bin_path, "platform-tools", "adb");

            var adbexe = new ProcessStartInfo(cmd, adbshell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var adb = new Process();
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

    async void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (!string.IsNullOrEmpty(outLine.Data))
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var sb = new StringBuilder(FormatExtractLog.Text);
                FormatExtractLog.Text = sb.AppendLine(outLine.Data).ToString();
                FormatExtractLog.ScrollToLine(StringHelper.TextBoxLine(FormatExtractLog.Text));
                var op = new StringBuilder(output);
                output = op.AppendLine(outLine.Data).ToString();
            });
        }
    }

    async void OpenQcnFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open QCN File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            QcnFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    async void WriteQcn(object sender, RoutedEventArgs args)
    {
        // Write QCN File
        if (Global.System == "Windows")
        {
            if (await GetDevicesInfo.SetDevicesInfoLittle())
            {
                if (!string.IsNullOrEmpty(QcnFile.Text))
                {
                    var qcnfilepatch = QcnFile.Text;
                    var sukiViewModel = GlobalData.MainViewModelInstance;

                    if (sukiViewModel.Status is "901D" or "9091")
                    {
                        BusyQCN.IsBusy = true;
                        QCN.IsEnabled = false;
                        output = "";
                        FormatExtractLog.Text = GetTranslation("FormatExtract_Writing") + "\n";
                        var com = StringHelper.Onlynum(Global.thisdevice);
                        var shell = string.Format($"-w -p {com} -f \"{qcnfilepatch}\"");
                        await QCNTool(shell);

                        _ = FormatExtractLog.Text.Contains("error")
                            ? dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_WriteFailed")).Dismiss().ByClickingBackground().TryShow()
                            : dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_WriteSucc")).Dismiss().ByClickingBackground().TryShow();

                        BusyQCN.IsBusy = false;
                        QCN.IsEnabled = true;
                    }
                    else
                    {
                        _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_Open901D")).Dismiss().ByClickingBackground().TryShow();
                    }
                }
                else
                {
                    _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_SelectQCN")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotSupportSystem")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    async void BackupQcn(object sender, RoutedEventArgs args)
    {
        // Backup QCN file
        if (Global.System == "Windows")
        {
            if (await GetDevicesInfo.SetDevicesInfoLittle())
            {
                var sukiViewModel = GlobalData.MainViewModelInstance;

                if (sukiViewModel.Status is "901D" or "9091")
                {
                    BusyQCN.IsBusy = true;
                    QCN.IsEnabled = false;
                    output = "";
                    FormatExtractLog.Text = GetTranslation("FormatExtract_BackingUp") + "\n";
                    var com = StringHelper.Onlynum(Global.thisdevice);
                    var shell = string.Format($"-r -p {com}");
                    await QCNTool(shell);

                    _ = FormatExtractLog.Text.Contains("error")
                        ? dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_BackupFailed")).Dismiss().ByClickingBackground().TryShow()
                        : dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_BackupSucc")).Dismiss().ByClickingBackground().TryShow();

                    BusyQCN.IsBusy = false;
                    QCN.IsEnabled = true;
                }
                else
                {
                    _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_Open901D")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotSupportSystem")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    async void OpenBackup(object sender, RoutedEventArgs args)
    {
        if (OperatingSystem.IsLinux() && Global.backup_path == null)
        {
            var result = false;

            _ = dialogManager.CreateDialog()
.WithTitle("Warn")
.WithContent(GetTranslation("FormatExtract_ExtractFolder"))
.WithActionButton("Yes", _ => result = true, true)
.WithActionButton("No", _ => result = false, true)
.TryShow();

            if (result == true)
            {
                var topLevel = TopLevel.GetTopLevel(this);

                var files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
                {
                    Title = "Select Buckup Folder",
                    AllowMultiple = false
                });

                if (files.Count >= 1)
                {
                    if (FileHelper.TestPermission(StringHelper.FilePath(files[0].Path.ToString())))
                    {
                        Global.backup_path = StringHelper.FilePath(files[0].Path.ToString());
                    }
                    else
                    {
                        _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_FolderNoPermission")).Dismiss().ByClickingBackground().TryShow();
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }

        if (Global.backup_path != null)
        {
            FileHelper.OpenFolder(Path.Combine(Global.backup_path));
        }
    }

    async void Enable901d(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                BusyQCN.IsBusy = true;
                QCN.IsEnabled = false;
                var result = false;

                _ = dialogManager.CreateDialog()
.WithTitle("Warn")
.WithContent(GetTranslation("Common_NeedRoot"))
.WithActionButton("Yes", _ => result = true, true)
.WithActionButton("No", _ => result = false, true)
.TryShow();

                if (result == true)
                {
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell su -c \"setprop sys.usb.config diag,adb\"");
                    _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                }

                BusyQCN.IsBusy = false;
                QCN.IsEnabled = true;
            }
            else
            {
                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    async void Enable9091(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                BusyQCN.IsBusy = true;
                QCN.IsEnabled = false;
                var result = false;

                _ = dialogManager.CreateDialog()
.WithTitle("Warn")
.WithContent(GetTranslation("FormatExtract_OnlyXiaomi"))
.WithActionButton("Yes", _ => result = true, true)
.WithActionButton("No", _ => result = false, true)
.TryShow();

                if (result == true)
                {
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} push APK/mi_diag.apk /sdcard");
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell \"am start -a miui.intent.action.OPEN\"");
                    _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_DiagApk")).Dismiss().ByClickingBackground().TryShow();
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell \"am start -n com.longcheertel.midtest/\"");
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell \"am start -n com.longcheertel.midtest/com.longcheertel.midtest.Diag\"");
                }

                BusyQCN.IsBusy = false;
                QCN.IsEnabled = true;
            }
            else
            {
                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    async void OpenEmptyFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open SuperEmpty File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            SuperEmptyFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    async void FlashSuperEmpty(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

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
                        ? dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Basicflash_FlashSucc")).Dismiss().ByClickingBackground().TryShow()
                        : dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Basicflash_RecoveryFailed")).Dismiss().ByClickingBackground().TryShow();

                    BusyFlash.IsBusy = false;
                    SuperEmpty.IsEnabled = true;
                    Global.checkdevice = true;
                }
                else
                {
                    _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_SelectSuperEmpty")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    async void ADBFormat(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

            if (sukiViewModel.Status == GetTranslation("Home_Recovery"))
            {
                if (!string.IsNullOrEmpty(FormatName.Text))
                {
                    BusyFormat.IsBusy = true;
                    Format.IsEnabled = false;
                    output = "";
                    FormatExtractLog.Text = GetTranslation("FormatExtract_Formatting") + "\n";
                    var formatsystem = "";

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

                    var partname = FormatName.Text;
                    await FeaturesHelper.GetPartTable(Global.thisdevice);
                    FeaturesHelper.PushMakefs(Global.thisdevice);
                    var sdxx = FeaturesHelper.FindDisk(partname);

                    if (sdxx != "")
                    {
                        await Task.Run(() =>
                        {
                            Thread.Sleep(1000);
                        });

                        var partnum = StringHelper.Partno(FeaturesHelper.FindPart(partname), partname);
                        var shell = string.Format($"-s {Global.thisdevice} shell {formatsystem} /dev/block/{sdxx}{partnum}");
                        await ADB(shell);
                    }
                    else
                    {
                        _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_NotFound")).Dismiss().ByClickingBackground().TryShow();
                    }

                    BusyFormat.IsBusy = false;
                    Format.IsEnabled = true;
                }
                else
                {
                    _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_EnterFormatPart")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterRecovery")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    async void FastbootFormat(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

            if (sukiViewModel.Status == GetTranslation("Home_Fastboot"))
            {
                if (!string.IsNullOrEmpty(FormatName.Text))
                {
                    BusyFormat.IsBusy = true;
                    Format.IsEnabled = false;
                    output = "";
                    FormatExtractLog.Text = GetTranslation("FormatExtract_Formatting") + "\n";
                    var partname = FormatName.Text;
                    var shell = string.Format($"-s {Global.thisdevice} erase {partname}");
                    await Fastboot(shell);
                    BusyFormat.IsBusy = false;
                    Format.IsEnabled = true;
                }
                else
                {
                    _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_EnterFormatPart")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    async void FormatData(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

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
                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterRecovery")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    async void TWRPFormatData(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

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
                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterRecovery")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    async void ExtractPart(object sender, RoutedEventArgs args)
    {
        if (OperatingSystem.IsLinux() && Global.backup_path == null)
        {
            var result = false;

            _ = dialogManager.CreateDialog()
.WithTitle("Warn")
.WithContent(GetTranslation("FormatExtract_ExtractFolder"))
.WithActionButton("Yes", _ => result = true, true)
.WithActionButton("No", _ => result = false, true)
.TryShow();

            if (result == true)
            {
                var topLevel = TopLevel.GetTopLevel(this);

                var files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
                {
                    Title = "Select Buckup Folder",
                    AllowMultiple = false
                });

                if (files.Count >= 1)
                {
                    if (FileHelper.TestPermission(StringHelper.FilePath(files[0].Path.ToString())))
                    {
                        Global.backup_path = StringHelper.FilePath(files[0].Path.ToString());
                    }
                    else
                    {
                        _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_FolderNoPermission")).Dismiss().ByClickingBackground().TryShow();
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }

        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

            if (sukiViewModel.Status == GetTranslation("Home_Recovery"))
            {
                if (!string.IsNullOrEmpty(ExtractName.Text))
                {
                    BusyExtract.IsBusy = true;
                    Extract.IsEnabled = false;
                    output = "";
                    FormatExtractLog.Text = GetTranslation("FormatExtract_Extracting") + "\n";
                    var partname = ExtractName.Text;
                    await FeaturesHelper.GetPartTable(Global.thisdevice);
                    var sdxx = FeaturesHelper.FindDisk(partname);

                    if (sdxx != "")
                    {
                        var partnum = StringHelper.Partno(FeaturesHelper.FindPart(partname), partname);
                        var shell = string.Format($"-s {Global.thisdevice} shell dd if=/dev/block/{sdxx}{partnum} of={partname}.img");
                        await ADB(shell);
                        FileHelper.Write(adb_log_path, output);

                        if (output.Contains("No space left on device"))
                        {
                            FormatExtractLog.Text = GetTranslation("FormatExtract_TryUseData");
                            shell = string.Format($"-s {Global.thisdevice} shell rm /{partname}.img");
                            await ADB(shell);
                            shell = string.Format($"-s {Global.thisdevice} shell dd if=/dev/block/{sdxx}{partnum} of=/sdcard/{partname}.img");
                            await ADB(shell);
                            shell = string.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img {Global.backup_path}/");
                            await ADB(shell);
                            shell = string.Format($"-s {Global.thisdevice} shell rm /sdcard/{partname}.img");
                            await ADB(shell);
                        }
                        else
                        {
                            shell = string.Format($"-s {Global.thisdevice} pull /{partname}.img {Global.backup_path}/");
                            await ADB(shell);
                            shell = string.Format($"-s {Global.thisdevice} shell rm /{partname}.img");
                            await ADB(shell);
                        }
                    }
                    else
                    {
                        FormatExtractLog.Text = GetTranslation("FormatExtract_NotFound");
                    }

                    BusyExtract.IsBusy = false;
                    Extract.IsEnabled = true;
                }
                else
                {
                    _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_EnterExtractPart")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                if (!string.IsNullOrEmpty(ExtractName.Text))
                {
                    var result = false;

                    _ = dialogManager.CreateDialog()
    .WithTitle("Warn")
    .WithContent(GetTranslation("Common_NeedRoot"))
    .WithActionButton("Yes", _ => result = true, true)
    .WithActionButton("No", _ => result = false, true)
    .TryShow();

                    if (result == true)
                    {
                        BusyExtract.IsBusy = true;
                        Extract.IsEnabled = false;
                        output = "";
                        FormatExtractLog.Text = GetTranslation("FormatExtract_Extracting") + "\n";
                        var partname = ExtractName.Text;
                        await FeaturesHelper.GetPartTableSystem(Global.thisdevice);
                        var sdxx = FeaturesHelper.FindDisk(partname);

                        if (sdxx != "")
                        {
                            var partnum = StringHelper.Partno(FeaturesHelper.FindPart(partname), partname);
                            var shell = string.Format($"-s {Global.thisdevice} shell su -c \"dd if=/dev/block/{sdxx}{partnum} of=/sdcard/{partname}.img\"");
                            await ADB(shell);
                            shell = string.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img {Global.backup_path}/");
                            await ADB(shell);
                            shell = string.Format($"-s {Global.thisdevice} shell su -c \"rm /sdcard/{partname}.img\"");
                            await ADB(shell);
                        }
                        else
                        {
                            FormatExtractLog.Text = GetTranslation("FormatExtract_NotFound");
                        }

                        BusyExtract.IsBusy = false;
                        Extract.IsEnabled = true;
                    }
                }
                else
                {
                    _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_EnterExtractPart")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterRecOrOpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    async void ExtractVPart(object sender, RoutedEventArgs args)
    {
        if (OperatingSystem.IsLinux() && Global.backup_path == null)
        {
            var result = false;

            _ = dialogManager.CreateDialog()
.WithTitle("Warn")
.WithContent(GetTranslation("FormatExtract_ExtractFolder"))
.WithActionButton("Yes", _ => result = true, true)
.WithActionButton("No", _ => result = false, true)
.TryShow();

            if (result == true)
            {
                var topLevel = TopLevel.GetTopLevel(this);

                var files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
                {
                    Title = "Select Buckup Folder",
                    AllowMultiple = false
                });

                if (files.Count >= 1)
                {
                    if (FileHelper.TestPermission(StringHelper.FilePath(files[0].Path.ToString())))
                    {
                        Global.backup_path = StringHelper.FilePath(files[0].Path.ToString());
                    }
                    else
                    {
                        _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_FolderNoPermission")).Dismiss().ByClickingBackground().TryShow();
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }

        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

            if (sukiViewModel.Status == GetTranslation("Home_Recovery"))
            {
                if (!string.IsNullOrEmpty(ExtractName.Text))
                {
                    BusyExtract.IsBusy = true;
                    Extract.IsEnabled = false;
                    output = "";
                    FormatExtractLog.Text = GetTranslation("FormatExtract_Extracting") + "\n";
                    var partname = ExtractName.Text;
                    var shell = string.Format($"-s {Global.thisdevice} shell ls -l /dev/block/mapper/{partname}");
                    var vmpart = await CallExternalProgram.ADB(shell);

                    if (!vmpart.Contains("No such file or directory"))
                    {
                        char[] charSeparators = { ' ', '\r', '\n' };
                        var line = vmpart.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                        var devicepoint = line[^1];
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
                            shell = string.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img {Global.backup_path}/");
                            await ADB(shell);
                            shell = string.Format($"-s {Global.thisdevice} shell rm /sdcard/{partname}.img");
                            await ADB(shell);
                        }
                        else
                        {
                            shell = string.Format($"-s {Global.thisdevice} pull /{partname}.img {Global.backup_path}/");
                            await ADB(shell);
                            shell = string.Format($"-s {Global.thisdevice} shell rm /{partname}.img");
                            await ADB(shell);
                        }
                    }
                    else
                    {
                        FormatExtractLog.Text = GetTranslation("FormatExtract_NotFound");
                    }

                    BusyExtract.IsBusy = false;
                    Extract.IsEnabled = true;
                }
                else
                {
                    _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_EnterExtractPart")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                if (!string.IsNullOrEmpty(ExtractName.Text))
                {
                    var result = false;

                    _ = dialogManager.CreateDialog()
    .WithTitle("Warn")
    .WithContent(GetTranslation("Common_NeedRoot"))
    .WithActionButton("Yes", _ => result = true, true)
    .WithActionButton("No", _ => result = false, true)
    .TryShow();

                    if (result == true)
                    {
                        BusyExtract.IsBusy = true;
                        Extract.IsEnabled = false;
                        output = "";
                        FormatExtractLog.Text = GetTranslation("FormatExtract_Extracting") + "\n";
                        var partname = ExtractName.Text;
                        var shell = string.Format($"-s {Global.thisdevice} shell su -c \"ls -l /dev/block/mapper/{partname}\"");
                        var vmpart = await CallExternalProgram.ADB(shell);

                        if (!vmpart.Contains("No such file or directory"))
                        {
                            char[] charSeparators = { ' ', '\r', '\n' };
                            var line = vmpart.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                            var devicepoint = line[^1];
                            shell = string.Format($"-s {Global.thisdevice} shell su -c \"dd if={devicepoint} of=/sdcard/{partname}.img\"");
                            await ADB(shell);
                            shell = string.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img {Global.backup_path}/");
                            await ADB(shell);
                            shell = string.Format($"-s {Global.thisdevice} shell su -c \"rm /sdcard/{partname}.img\"");
                            await ADB(shell);
                        }
                        else
                        {
                            FormatExtractLog.Text = GetTranslation("FormatExtract_NotFound");
                        }

                        BusyExtract.IsBusy = false;
                        Extract.IsEnabled = true;
                    }
                }
                else
                {
                    _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("FormatExtract_EnterExtractPart")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterRecOrOpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    async void OpenExtractFile(object sender, RoutedEventArgs args)
    {
        if (OperatingSystem.IsLinux() && Global.backup_path == null)
        {
            var result = false;

            _ = dialogManager.CreateDialog()
.WithTitle("Warn")
.WithContent(GetTranslation("FormatExtract_ExtractFolder"))
.WithActionButton("Yes", _ => result = true, true)
.WithActionButton("No", _ => result = false, true)
.TryShow();

            if (result == true)
            {
                var topLevel = TopLevel.GetTopLevel(this);

                var files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
                {
                    Title = "Select Buckup Folder",
                    AllowMultiple = false
                });

                if (files.Count >= 1)
                {
                    if (FileHelper.TestPermission(StringHelper.FilePath(files[0].Path.ToString())))
                    {
                        Global.backup_path = StringHelper.FilePath(files[0].Path.ToString());
                    }
                    else
                    {
                        _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_FolderNoPermission")).Dismiss().ByClickingBackground().TryShow();
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }

        if (Global.backup_path != null)
        {
            FileHelper.OpenFolder(Path.Combine(Global.backup_path));
        }
    }
}