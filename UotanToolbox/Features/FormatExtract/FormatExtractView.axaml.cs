using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Features.FormatExtract;

public partial class FormatExtractView : UserControl
{
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public FormatExtractView()
    {
        InitializeComponent();
    }
    private readonly static string adb_log_path = Path.Combine(Global.log_path, "adb.txt");
    private string output = "";
    public async Task QCNTool(string shell)
    {
        await Task.Run(() =>
        {
            string cmd = "bin\\Windows\\QCNTool.exe";
            ProcessStartInfo qcntool = new ProcessStartInfo(cmd, shell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using Process qcn = new Process();
            qcn.StartInfo = qcntool;
            qcn.Start();
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
            fb.Start();
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
            adb.Start();
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
        if (!String.IsNullOrEmpty(outLine.Data))
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StringBuilder sb = new StringBuilder(FormatExtractLog.Text);
                FormatExtractLog.Text = sb.AppendLine(outLine.Data).ToString();
                FormatExtractLog.ScrollToLine(StringHelper.TextBoxLine(FormatExtractLog.Text));
                StringBuilder op = new StringBuilder(output);
                output = op.AppendLine(outLine.Data).ToString();
            });
        }
    }

    private async void OpenQcnFile(object sender, RoutedEventArgs args)
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

    private async void WriteQcn(object sender, RoutedEventArgs args)
    {
        // Write QCN File
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            if (!string.IsNullOrEmpty(QcnFile.Text))
            {
                string qcnfilepatch = QcnFile.Text;
                MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
                if (sukiViewModel.Status == "901D" || sukiViewModel.Status == "9091")
                {
                    BusyQCN.IsBusy = true;
                    QCN.IsEnabled = false;
                    output = "";
                    FormatExtractLog.Text = GetTranslation("FormatExtract_Writing") + "\n";
                    int com = StringHelper.Onlynum(Global.thisdevice);
                    string shell = string.Format("-w -p {0} -f \"{1}\"", com, qcnfilepatch);
                    await QCNTool(shell);
                    if (FormatExtractLog.Text.Contains("error"))
                    {
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("FormatExtract_WriteFailed")), allowBackgroundClose: true);
                    }
                    else
                    {
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("FormatExtract_WriteSucc")), allowBackgroundClose: true);
                    }
                    BusyQCN.IsEnabled = false;
                    QCN.IsEnabled = true;
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("FormatExtract_Open901D")), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("FormatExtract_SelectQCN")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
        }
    }

    private async void BackupQcn(object sender, RoutedEventArgs args)
    {
        // Backup QCN file
        if (OperatingSystem.IsLinux() && Global.backup_path == null)
        {
            var newDialog = new ConnectionDialog(GetTranslation("FormatExtract_ExtractFolder"));
            await SukiHost.ShowDialogAsync(newDialog);
            if (newDialog.Result == true)
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
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_FolderNoPermission")), allowBackgroundClose: true);
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
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "901D" || sukiViewModel.Status == "9091")
            {
                BusyQCN.IsBusy = true;
                QCN.IsEnabled = false;
                output = "";
                FormatExtractLog.Text = GetTranslation("FormatExtract_BackingUp") + "\n";
                int com = StringHelper.Onlynum(Global.thisdevice);
                string shell = string.Format("-r -p {0} -f \"{1}\" -n 00000.qcn", com, Global.backup_path);
                await QCNTool(shell);
                if (FormatExtractLog.Text.Contains("error"))
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("FormatExtract_BackupFailed")), allowBackgroundClose: true);
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("FormatExtract_BackupSucc")), allowBackgroundClose: true);
                }
                BusyQCN.IsEnabled = false;
                QCN.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("FormatExtract_Open901D")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
        }
    }

    private async void OpenBackup(object sender, RoutedEventArgs args)
    {
        if (OperatingSystem.IsLinux() && Global.backup_path == null)
        {
            var newDialog = new ConnectionDialog(GetTranslation("FormatExtract_ExtractFolder"));
            await SukiHost.ShowDialogAsync(newDialog);
            if (newDialog.Result == true)
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
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_FolderNoPermission")), allowBackgroundClose: true);
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

    private async void Enable901d(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                BusyQCN.IsBusy = true;
                QCN.IsEnabled = false;
                var newDialog = new ConnectionDialog(GetTranslation("Common_NeedRoot"));
                await SukiHost.ShowDialogAsync(newDialog);
                if (newDialog.Result == true)
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell su -c \"setprop sys.usb.config diag,adb\"");
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_Execution")), allowBackgroundClose: true);
                }
                BusyQCN.IsBusy = false;
                QCN.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_OpenADB")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
        }
    }

    private async void Enable9091(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                BusyQCN.IsBusy = true;
                QCN.IsEnabled = false;
                var newDialog = new ConnectionDialog(GetTranslation("FormatExtract_OnlyXiaomi"));
                await SukiHost.ShowDialogAsync(newDialog);
                if (newDialog.Result == true)
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} push APK/mi_diag.apk /sdcard");
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell \"am start -a miui.intent.action.OPEN\"");
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("FormatExtract_DiagApk")), allowBackgroundClose: true);
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell \"am start -n com.longcheertel.midtest/\"");
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell \"am start -n com.longcheertel.midtest/com.longcheertel.midtest.Diag\"");
                }
                BusyQCN.IsBusy = false;
                QCN.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_OpenADB")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
        }
    }

    private async void OpenEmptyFile(object sender, RoutedEventArgs args)
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

    private async void FlashSuperEmpty(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
            {
                if (!string.IsNullOrEmpty(SuperEmptyFile.Text))
                {
                    BusyFlash.IsBusy = true;
                    SuperEmpty.IsEnabled = false;
                    output = "";
                    FormatExtractLog.Text = GetTranslation("Customizedflash_Flashing") + "\n";
                    await Fastboot($"-s {Global.thisdevice} wipe-super \"{SuperEmptyFile.Text}\"");
                    if (!output.Contains("FAILED") && !output.Contains("error"))
                    {
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_FlashSucc")), allowBackgroundClose: true);
                    }
                    else
                    {
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_RecoveryFailed")), allowBackgroundClose: true);
                    }
                    BusyFlash.IsBusy = false;
                    SuperEmpty.IsEnabled = true;
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("FormatExtract_SelectSuperEmpty")), allowBackgroundClose: true);
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
                        formatsystem = "mke2fs -t ext4";
                    if (F2FS.IsChecked != null && (bool)F2FS.IsChecked)
                        formatsystem = "/tmp/mkfs.f2fs";
                    if (FAT32.IsChecked != null && (bool)FAT32.IsChecked)
                        formatsystem = "mkfs.fat -F32 -s1";
                    if (exFAT.IsChecked != null && (bool)exFAT.IsChecked)
                        formatsystem = "mkexfatfs -n exfat";
                    if (NTFS.IsChecked != null && (bool)NTFS.IsChecked)
                        formatsystem = "/tmp/mkntfs -f";
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
                        string shell = String.Format($"-s {Global.thisdevice} shell {formatsystem} /dev/block/{sdxx}{partnum}");
                        await ADB(shell);
                    }
                    else
                    {
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("FormatExtract_NotFound")), allowBackgroundClose: true);
                    }
                    BusyFormat.IsBusy = false;
                    Format.IsEnabled = true;
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("FormatExtract_EnterFormatPart")), allowBackgroundClose: true);
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

    private async void FastbootFormat(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Fastboot"))
            {
                if (!string.IsNullOrEmpty(FormatName.Text))
                {
                    BusyFormat.IsBusy = true;
                    Format.IsEnabled = false;
                    output = "";
                    FormatExtractLog.Text = GetTranslation("FormatExtract_Formatting") + "\n";
                    string partname = FormatName.Text;
                    string shell = String.Format($"-s {Global.thisdevice} erase {partname}");
                    await Fastboot(shell);
                    BusyFormat.IsBusy = false;
                    Format.IsEnabled = true;
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("FormatExtract_EnterFormatPart")), allowBackgroundClose: true);
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
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterRecovery")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
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
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterRecovery")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
        }
    }

    private async void ExtractPart(object sender, RoutedEventArgs args)
    {
        if (OperatingSystem.IsLinux() && Global.backup_path == null)
        {
            var newDialog = new ConnectionDialog(GetTranslation("FormatExtract_ExtractFolder"));
            await SukiHost.ShowDialogAsync(newDialog);
            if (newDialog.Result == true)
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
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_FolderNoPermission")), allowBackgroundClose: true);
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
                        string shell = String.Format($"-s {Global.thisdevice} shell dd if=/dev/block/{sdxx}{partnum} of={partname}.img");
                        await ADB(shell);
                        FileHelper.Write(adb_log_path, output);
                        if (output.Contains("No space left on device"))
                        {
                            FormatExtractLog.Text = GetTranslation("FormatExtract_TryUseData");
                            shell = String.Format($"-s {Global.thisdevice} shell rm /{partname}.img");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} shell dd if=/dev/block/{sdxx}{partnum} of=/sdcard/{partname}.img");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img {Global.backup_path}/");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} shell rm /sdcard/{partname}.img");
                            await ADB(shell);
                        }
                        else
                        {
                            shell = String.Format($"-s {Global.thisdevice} pull /{partname}.img {Global.backup_path}/");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} shell rm /{partname}.img");
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
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("FormatExtract_EnterExtractPart")), allowBackgroundClose: true);
                }
            }
            else if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                if (!string.IsNullOrEmpty(ExtractName.Text))
                {
                    var newDialog = new ConnectionDialog(GetTranslation("Common_NeedRoot"));
                    await SukiHost.ShowDialogAsync(newDialog);
                    if (newDialog.Result == true)
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
                            string shell = String.Format($"-s {Global.thisdevice} shell su -c \"dd if=/dev/block/{sdxx}{partnum} of=/sdcard/{partname}.img\"");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img {Global.backup_path}/");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} shell su -c \"rm /sdcard/{partname}.img\"");
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
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("FormatExtract_EnterExtractPart")), allowBackgroundClose: true);
                }
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

    private async void ExtractVPart(object sender, RoutedEventArgs args)
    {
        if (OperatingSystem.IsLinux() && Global.backup_path == null)
        {
            var newDialog = new ConnectionDialog(GetTranslation("FormatExtract_ExtractFolder"));
            await SukiHost.ShowDialogAsync(newDialog);
            if (newDialog.Result == true)
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
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_FolderNoPermission")), allowBackgroundClose: true);
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
                    string shell = String.Format($"-s {Global.thisdevice} shell ls -l /dev/block/mapper/{partname}");
                    string vmpart = await CallExternalProgram.ADB(shell);
                    if (!vmpart.Contains("No such file or directory"))
                    {
                        char[] charSeparators = { ' ', '\r', '\n' };
                        string[] line = vmpart.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                        string devicepoint = line[line.Length - 1];
                        shell = String.Format($"-s {Global.thisdevice} shell dd if={devicepoint} of={partname}.img");
                        await ADB(shell);
                        FileHelper.Write(adb_log_path, output);
                        if (output.Contains("No space left on device"))
                        {
                            FormatExtractLog.Text = GetTranslation("FormatExtract_TryUseData");
                            shell = String.Format($"-s {Global.thisdevice} shell rm /{partname}.img");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} shell dd if={devicepoint} of=/sdcard/{partname}.img");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img {Global.backup_path}/");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} shell rm /sdcard/{partname}.img");
                            await ADB(shell);
                        }
                        else
                        {
                            shell = String.Format($"-s {Global.thisdevice} pull /{partname}.img {Global.backup_path}/");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} shell rm /{partname}.img");
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
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("FormatExtract_EnterExtractPart")), allowBackgroundClose: true);
                }
            }
            else if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                if (!string.IsNullOrEmpty(ExtractName.Text))
                {
                    var newDialog = new ConnectionDialog(GetTranslation("Common_NeedRoot"));
                    await SukiHost.ShowDialogAsync(newDialog);
                    if (newDialog.Result == true)
                    {
                        BusyExtract.IsBusy = true;
                        Extract.IsEnabled = false;
                        output = "";
                        FormatExtractLog.Text = GetTranslation("FormatExtract_Extracting") + "\n";
                        string partname = ExtractName.Text;
                        string shell = String.Format($"-s {Global.thisdevice} shell su -c \"ls -l /dev/block/mapper/{partname}\"");
                        string vmpart = await CallExternalProgram.ADB(shell);
                        if (!vmpart.Contains("No such file or directory"))
                        {
                            char[] charSeparators = { ' ', '\r', '\n' };
                            string[] line = vmpart.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                            string devicepoint = line[line.Length - 1];
                            shell = String.Format($"-s {Global.thisdevice} shell su -c \"dd if={devicepoint} of=/sdcard/{partname}.img\"");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img {Global.backup_path}/");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} shell su -c \"rm /sdcard/{partname}.img\"");
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
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("FormatExtract_EnterExtractPart")), allowBackgroundClose: true);
                }
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

    private async void OpenExtractFile(object sender, RoutedEventArgs args)
    {
        if (OperatingSystem.IsLinux() && Global.backup_path == null)
        {
            var newDialog = new ConnectionDialog(GetTranslation("FormatExtract_ExtractFolder"));
            await SukiHost.ShowDialogAsync(newDialog);
            if (newDialog.Result == true)
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
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_FolderNoPermission")), allowBackgroundClose: true);
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