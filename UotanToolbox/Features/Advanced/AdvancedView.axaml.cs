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

namespace UotanToolbox.Features.Advanced;

public partial class AdvancedView : UserControl
{
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public AdvancedView()
    {
        InitializeComponent();
    }
    private readonly static string adb_log_path = Path.Combine(Global.runpath, "Log", "adb.txt");
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
                StringBuilder sb = new StringBuilder(AdvancedLog.Text);
                AdvancedLog.Text = sb.AppendLine(outLine.Data).ToString();
                AdvancedLog.ScrollToLine(StringHelper.TextBoxLine(AdvancedLog.Text));
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
                    AdvancedLog.Text = "正在写入...\n";
                    int com = StringHelper.Onlynum(Global.thisdevice);
                    string shell = string.Format("-w -p {0} -f \"{1}\"", com, qcnfilepatch);
                    await QCNTool(shell);
                    if (AdvancedLog.Text.Contains("error"))
                    {
                        SukiHost.ShowDialog(new PureDialog("写入失败"), allowBackgroundClose: true);
                    }
                    else
                    {
                        SukiHost.ShowDialog(new PureDialog("写入成功"), allowBackgroundClose: true);
                    }
                    BusyQCN.IsEnabled = false;
                    QCN.IsEnabled = true;
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请先开启901D/9091端口！"), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请先选择QCN文件"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
    }

    private async void BackupQcn(object sender, RoutedEventArgs args)
    {
        // Backup QCN file
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "901D" || sukiViewModel.Status == "9091")
            {
                BusyQCN.IsBusy = true;
                QCN.IsEnabled = false;
                AdvancedLog.Text = "正在备份...\n";
                int com = StringHelper.Onlynum(Global.thisdevice);
                string shell = string.Format("-r -p {0} -f {1}/Backup -n 00000.qcn", com, Global.runpath);
                await QCNTool(shell);
                if (AdvancedLog.Text.Contains("error"))
                {
                    SukiHost.ShowDialog(new PureDialog("备份失败"), allowBackgroundClose: true);
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("备份成功"), allowBackgroundClose: true);
                }
                BusyQCN.IsEnabled = false;
                QCN.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请先开启901D/9091端口！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
    }

    private async void OpenBackup(object sender, RoutedEventArgs args)
    {
        FileHelper.OpenFolder(Path.Combine(Global.runpath, "Backup"));
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
                var newDialog = new ConnectionDialog("该操作需要ROOT权限，请确保手机已ROOT，\n\r并在接下来的弹窗中授予 Shell ROOT权限！");
                await SukiHost.ShowDialogAsync(newDialog);
                if (newDialog.Result == true)
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell su -c \"setprop sys.usb.config diag,adb\"");
                    SukiHost.ShowDialog(new PureDialog("执行完成，请查看您的设备！"), allowBackgroundClose: true);
                }
                BusyQCN.IsBusy = false;
                QCN.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请将设备进入系统后执行！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
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
                var newDialog = new ConnectionDialog("该操作仅限小米设备！其它设备将无法使用！");
                await SukiHost.ShowDialogAsync(newDialog);
                if (newDialog.Result == true)
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} push APK/mi_diag.apk /sdcard");
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell \"am start -a miui.intent.action.OPEN\"");
                    SukiHost.ShowDialog(new PureDialog("已将名为\"mi_diag.apk\"的文件推送至设备根目录，请安装完成后点击确定！"), allowBackgroundClose: true);
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell \"am start -n com.longcheertel.midtest/\"");
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell \"am start -n com.longcheertel.midtest/com.longcheertel.midtest.Diag\"");
                }
                BusyQCN.IsBusy = false;
                QCN.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请将设备进入系统后执行！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
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
                    AdvancedLog.Text = "正在刷入...\n";
                    await Fastboot($"-s {Global.thisdevice} flash cust \"{SuperEmptyFile.Text}\"");
                    if (!output.Contains("FAILED") && !output.Contains("error"))
                    {
                        SukiHost.ShowDialog(new PureDialog("刷入成功！"), allowBackgroundClose: true);
                    }
                    else
                    {
                        SukiHost.ShowDialog(new PureDialog("刷入失败！"), allowBackgroundClose: true);
                    }
                    BusyFlash.IsBusy = false;
                    SuperEmpty.IsEnabled = true;
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请选择SuperEmpty文件！"), allowBackgroundClose: true);
                }
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
                    AdvancedLog.Text = "正在格式化...\n";
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
                        SukiHost.ShowDialog(new PureDialog("未找到该分区！"), allowBackgroundClose: true);
                    }
                    BusyFormat.IsBusy = false;
                    Format.IsEnabled = true;
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请输入需要格式化的分区名称！"), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请将设备进入Recovery模式后执行！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
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
                    AdvancedLog.Text = "正在格式化...\n";
                    string partname = FormatName.Text;
                    string shell = String.Format($"-s {Global.thisdevice} erase {partname}");
                    await Fastboot(shell);
                    BusyFormat.IsBusy = false;
                    Format.IsEnabled = true;
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请输入需要格式化的分区名称！"), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请将设备进入Fastboot模式后执行！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
    }

    private async void FormatData(object sender, RoutedEventArgs args)
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
                    AdvancedLog.Text = "正在格式化...\n";
                    await ADB($"-s {Global.thisdevice} shell recovery --wipe_data");
                    BusyFormat.IsBusy = false;
                    Format.IsEnabled = true;
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请输入需要格式化的分区名称！"), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请将设备进入Recovery模式后执行！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
    }

    private async void TWRPFormatData(object sender, RoutedEventArgs args)
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
                    AdvancedLog.Text = "正在格式化...\n";
                    await ADB($"-s {Global.thisdevice} shell twrp format data");
                    BusyFormat.IsBusy = false;
                    Format.IsEnabled = true;
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请输入需要格式化的分区名称！"), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请将设备进入Recovery模式后执行！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
    }

    private async void ExtractPart(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Recovery"))
            {
                if (!string.IsNullOrEmpty(ExtractName.Text))
                {
                    BusyExtract.IsBusy = true;
                    Extract.IsEnabled = false;
                    AdvancedLog.Text = "正在提取...\n";
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
                            AdvancedLog.Text = "根目录空间不足，正在尝试使用Data分区...";
                            shell = String.Format($"-s {Global.thisdevice} shell rm /{partname}.img");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} shell dd if=/dev/block/{sdxx}{partnum} of=/sdcard/{partname}.img");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img {Global.runpath}/Backup/");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} shell rm /sdcard/{partname}.img");
                            await ADB(shell);
                        }
                        else
                        {
                            shell = String.Format($"-s {Global.thisdevice} pull /{partname}.img {Global.runpath}/Backup/");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} shell rm /{partname}.img");
                            await ADB(shell);
                        }
                    }
                    else
                    {
                        AdvancedLog.Text = "未找到该分区!";
                    }
                    BusyExtract.IsBusy = false;
                    Extract.IsEnabled = true;
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请输入需要提取的分区名称！"), allowBackgroundClose: true);
                }
            }
            else if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                if (!string.IsNullOrEmpty(ExtractName.Text))
                {
                    var newDialog = new ConnectionDialog("当前为系统模式，在系统下提取分区需要ROOT权限，\n\r请确保手机已ROOT，并在接下来的弹窗中授予 Shell ROOT权限！");
                    await SukiHost.ShowDialogAsync(newDialog);
                    if (newDialog.Result == true)
                    {
                        BusyExtract.IsBusy = true;
                        Extract.IsEnabled = false;
                        AdvancedLog.Text = "正在提取...\n";
                        string partname = ExtractName.Text;
                        await FeaturesHelper.GetPartTableSystem(Global.thisdevice);
                        string sdxx = FeaturesHelper.FindDisk(partname);
                        if (sdxx != "")
                        {
                            string partnum = StringHelper.Partno(FeaturesHelper.FindPart(partname), partname);
                            string shell = String.Format($"-s {Global.thisdevice} shell su -c \"dd if=/dev/block/{sdxx}{partnum} of=/sdcard/{partname}.img\"");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img {Global.runpath}/Backup/");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} shell su -c \"rm /sdcard/{partname}.img\"");
                            await ADB(shell);
                        }
                        else
                        {
                            AdvancedLog.Text = "未找到该分区!";
                        }
                        BusyExtract.IsBusy = false;
                        Extract.IsEnabled = true;
                    }
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请输入需要提取的分区名称！"), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请将设备进入Recovery模式或系统后执行！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
    }

    private async void ExtractVPart(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Recovery"))
            {
                if (!string.IsNullOrEmpty(ExtractName.Text))
                {
                    BusyExtract.IsBusy = true;
                    Extract.IsEnabled = false;
                    AdvancedLog.Text = "正在提取...\n";
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
                            AdvancedLog.Text = "根目录空间不足，正在尝试使用Data分区...";
                            shell = String.Format($"-s {Global.thisdevice} shell rm /{partname}.img");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} shell dd if={devicepoint} of=/sdcard/{partname}.img");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img {Global.runpath}/Backup/");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} shell rm /sdcard/{partname}.img");
                            await ADB(shell);
                        }
                        else
                        {
                            shell = String.Format($"-s {Global.thisdevice} pull /{partname}.img {Global.runpath}/Backup/");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} shell rm /{partname}.img");
                            await ADB(shell);
                        }
                    }
                    else
                    {
                        AdvancedLog.Text = "未找到该分区!";
                    }
                    BusyExtract.IsBusy = false;
                    Extract.IsEnabled = true;
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请输入需要提取的分区名称！"), allowBackgroundClose: true);
                }
            }
            else if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                if (!string.IsNullOrEmpty(ExtractName.Text))
                {
                    var newDialog = new ConnectionDialog("当前为系统模式，在系统下提取分区需要ROOT权限，\n\r请确保手机已ROOT，并在接下来的弹窗中授予 Shell ROOT权限！");
                    await SukiHost.ShowDialogAsync(newDialog);
                    if (newDialog.Result == true)
                    {
                        BusyExtract.IsBusy = true;
                        Extract.IsEnabled = false;
                        AdvancedLog.Text = "正在提取...\n";
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
                            shell = String.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img {Global.runpath}/Backup/");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} shell su -c \"rm /sdcard/{partname}.img\"");
                            await ADB(shell);
                        }
                        else
                        {
                            AdvancedLog.Text = "未找到该分区!";
                        }
                        BusyExtract.IsBusy = false;
                        Extract.IsEnabled = true;
                    }
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请输入需要提取的分区名称！"), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请将设备进入Recovery模式或系统后执行！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose : true);
        }
    }

    private async void OpenExtractFile(object sender, RoutedEventArgs args)
    {
        FileHelper.OpenFolder(Path.Combine(Global.runpath, "Backup"));
    }
}