using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Controls;
using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Features.Advanced;

public partial class AdvancedView : UserControl
{
    public AdvancedView()
    {
        InitializeComponent();
    }
    private readonly static string adb_log_path = Path.Combine(Global.runpath, "log", "adb.txt");
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
            if (sukiViewModel.Status == "Fastboot")
            {
                if (SuperEmptyFile.Text != null)
                {
                    BusyFlash.IsBusy = true;
                    AdvancedLog.Text = "正在刷入...\n";
                    await Fastboot($"-s {Global.thisdevice} flash cust \"{SuperEmptyFile.Text}\"");
                    if (!output.Contains("FAILED") && !output.Contains("error"))
                    {
                        SukiHost.ShowDialog(new ConnectionDialog("刷入成功！"));
                    }
                    else
                    {
                        SukiHost.ShowDialog(new ConnectionDialog("刷入失败！"));
                    }
                    BusyFlash.IsBusy = false;
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog("请选择SuperEmpty文件！"));
                }
            }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("请进入Fastboot模式！"));
            }
        }
    }

    private async void ADBFormat(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Recovery")
            {
                if (FormatName.Text != null)
                {
                    BusyFormat.IsBusy = true;
                    AdvancedLog.Text = "正在格式化...\n";
                    string formatsystem = "";
                    if (EXT4.IsChecked != null && (bool)EXT4.IsChecked)
                        formatsystem = "mke2fs -t ext4";
                    if (F2FS.IsChecked != null && (bool)F2FS.IsChecked)
                        formatsystem = "/tmp/mkfs.f2fs";
                    if (FAT32.IsChecked != null && (bool)FAT32.IsChecked)
                        formatsystem = "mkfs.fat -F32 -s1";
                    if (exFAT.IsChecked != null && (bool)exFAT.IsChecked)
                        formatsystem = "/tmp/mkntfs -f";
                    if (NTFS.IsChecked != null && (bool)NTFS.IsChecked)
                        formatsystem = "mkexfatfs -n exfat";
                    string partname = FormatName.Text;
                    await FeaturesHelper.GetPartTable(Global.thisdevice);
                    FeaturesHelper.PushMakefs(Global.thisdevice);
                    string sdxx = FeaturesHelper.FindDisk(partname);
                    if (sdxx != "")
                    {
                        string partnum = StringHelper.Partno(FeaturesHelper.FindPart(partname), partname);
                        string shell = String.Format($"-s {Global.thisdevice} shell {formatsystem} /dev/block/{sdxx}{partnum}");
                        await ADB(shell);
                    }
                    else
                    {
                        SukiHost.ShowDialog(new ConnectionDialog("未找到该分区！"));
                    }
                    BusyFormat.IsBusy = false;
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog("请输入需要格式化的分区名称！"));
                }
            }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("请将设备进入Recovery模式后执行！"));
            }
        }
    }

    private async void FastbootFormat(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Fastboot")
            {
                if (FormatName.Text != null)
                {
                    BusyFormat.IsBusy = true;
                    AdvancedLog.Text = "正在格式化...\n";
                    string partname = FormatName.Text;
                    string shell = String.Format($"-s {Global.thisdevice} erase {partname}");
                    await Fastboot(shell);
                    BusyFormat.IsBusy = false;
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog("请输入需要格式化的分区名称！"));
                }
            }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("请将设备进入Fastboot模式后执行！"));
            }
        }
    }

    private async void FormatData(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Recovery")
            {
                if (FormatName.Text != null)
                {
                    BusyFormat.IsBusy = true;
                    AdvancedLog.Text = "正在格式化...\n";
                    await ADB($"-s {Global.thisdevice} shell recovery --wipe_data");
                    BusyFormat.IsBusy = false;
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog("请输入需要格式化的分区名称！"));
                }
            }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("请将设备进入Recovery模式后执行！"));
            }
        }
    }

    private async void TWRPFormatData(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Recovery")
            {
                if (FormatName.Text != null)
                {
                    BusyFormat.IsBusy = true;
                    AdvancedLog.Text = "正在格式化...\n";
                    await ADB($"-s {Global.thisdevice} shell twrp format data");
                    BusyFormat.IsBusy = false;
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog("请输入需要格式化的分区名称！"));
                }
            }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("请将设备进入Recovery模式后执行！"));
            }
        }
    }

    private async void ExtractPart(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Recovery")
            {
                if (ExtractName.Text != null)
                {
                    BusyExtract.IsBusy = true;
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
                            shell = String.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img {Global.runpath}/backup/");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} shell rm /sdcard/{partname}.img");
                            await ADB(shell);
                        }
                        else
                        {
                            shell = String.Format($"-s {Global.thisdevice} pull /{partname}.img {Global.runpath}/backup/");
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
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog("请输入需要提取的分区名称！"));
                }
            }
            else if (sukiViewModel.Status == "系统")
            {
                if (ExtractName.Text != null)
                {
                    var newDialog = new ConnectionDialog("当前为系统模式，在系统下提取分区需要ROOT权限，\n\r请确保手机已ROOT，并在接下来的弹窗中授予 Shell ROOT权限！");
                    await SukiHost.ShowDialogAsync(newDialog);
                    if (newDialog.Result == true)
                    {
                        BusyExtract.IsBusy = true;
                        AdvancedLog.Text = "正在提取...\n";
                        string partname = ExtractName.Text;
                        await FeaturesHelper.GetPartTableSystem(Global.thisdevice);
                        string sdxx = FeaturesHelper.FindDisk(partname);
                        if (sdxx != "")
                        {
                            string partnum = StringHelper.Partno(FeaturesHelper.FindPart(partname), partname);
                            string shell = String.Format($"-s {Global.thisdevice} shell su -c \"dd if=/dev/block/{sdxx}{partnum} of=/sdcard/{partname}.img\"");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img {Global.runpath}/backup/");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} shell su -c \"rm /sdcard/{partname}.img\"");
                            await ADB(shell);
                        }
                        else
                        {
                            AdvancedLog.Text = "未找到该分区!";
                        }
                        BusyExtract.IsBusy = false;
                    }
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog("请输入需要提取的分区名称！"));
                }
            }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("请将设备进入Recovery模式或系统后执行！"));
            }
        }
    }

    private async void ExtractVPart(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Recovery")
            {
                if (ExtractName.Text != null)
                {
                    BusyExtract.IsBusy = true;
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
                            shell = String.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img {Global.runpath}/backup/");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} shell rm /sdcard/{partname}.img");
                            await ADB(shell);
                        }
                        else
                        {
                            shell = String.Format($"-s {Global.thisdevice} pull /{partname}.img {Global.runpath}/backup/");
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
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog("请输入需要提取的分区名称！"));
                }
            }
            else if (sukiViewModel.Status == "系统")
            {
                if (ExtractName.Text != null)
                {
                    var newDialog = new ConnectionDialog("当前为系统模式，在系统下提取分区需要ROOT权限，\n\r请确保手机已ROOT，并在接下来的弹窗中授予 Shell ROOT权限！");
                    await SukiHost.ShowDialogAsync(newDialog);
                    if (newDialog.Result == true)
                    {
                        BusyExtract.IsBusy = true;
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
                            shell = String.Format($"-s {Global.thisdevice} pull /sdcard/{partname}.img {Global.runpath}/backup/");
                            await ADB(shell);
                            shell = String.Format($"-s {Global.thisdevice} shell su -c \"rm /sdcard/{partname}.img\"");
                            await ADB(shell);
                        }
                        else
                        {
                            AdvancedLog.Text = "未找到该分区!";
                        }
                        BusyExtract.IsBusy = false;
                    }
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog("请输入需要提取的分区名称！"));
                }
            }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("请将设备进入Recovery模式或系统后执行！"));
            }
        }
    }

    private async void  OpenExtractFile(object sender, RoutedEventArgs args)
    {
        string filepath = string.Format(@"{0}\backup", System.IO.Directory.GetCurrentDirectory());
        FileHelper.OpenFolder(filepath);
    }
}