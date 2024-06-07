using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using ReactiveUI;
using SukiUI.Controls;
using SukiUI.MessageBox;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Features.Advanced;

public partial class AdvancedViewModel : MainPageBase
{
    [ObservableProperty] private string _qcnFile, _superEmptyFile, _formatName, _extractName, _advancedLog;
    [ObservableProperty] private bool _flashing;

    public AdvancedViewModel() : base("高级", MaterialIconKind.WrenchCogOutline, -300)
    {
    }

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
            string cmd;
            if (Global.System == "Windows")
            {
                cmd = "bin\\Windows\\adb\\fastboot.exe";
            }
            else
            {
                cmd = $"bin/{Global.System}/adb/fastboot";
            }
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

    private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (!String.IsNullOrEmpty(outLine.Data))
        {
            StringBuilder sb = new StringBuilder(AdvancedLog);
            AdvancedLog = sb.AppendLine(outLine.Data).ToString();
        }
    }

    [RelayCommand]
    private async Task WriteQcn()
    {
        // Write QCN File
        if (QcnFile != null)
        {
            string qcnfilepatch = QcnFile;
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "901D" || sukiViewModel.Status == "9091")
            {
                AdvancedLog = "正在写入...";
                int com = StringHelper.Onlynum(Global.thisdevice);
                string shell = string.Format("-w -p {0} -f \"{1}\"", com, qcnfilepatch);
                await QCNTool(shell);
                if (AdvancedLog.Contains("error"))
                {
                    SukiHost.ShowDialog(new ConnectionDialog("写入失败"), allowBackgroundClose: true);
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog("写入成功"), allowBackgroundClose: true);
                }
             }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("请先开启901D/9091端口！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new ConnectionDialog("请先选择QCN文件"), allowBackgroundClose: true);
        }
    }
    [RelayCommand]
    private async Task BackupQcn()
    {
        // Backup QCN file
        MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
        if (sukiViewModel.Status == "901D" || sukiViewModel.Status == "9091")
        {
            AdvancedLog = "正在备份...";
            int com = StringHelper.Onlynum(Global.thisdevice);
            string shell = string.Format("-r -p {0} -f {1}\\backup -n 00000.qcn", com, Global.runpath);
            await QCNTool(shell);
            if (AdvancedLog.Contains("error"))
            {
                SukiHost.ShowDialog(new ConnectionDialog("备份失败"), allowBackgroundClose: true);
            }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("备份成功"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new ConnectionDialog("请先开启901D/9091端口！"), allowBackgroundClose: true);
        }
    }
    [RelayCommand]
    private void OpenBackup()
    {
        // Open QCN backup file directory
        string filepath = string.Format(@"{0}\backup", System.IO.Directory.GetCurrentDirectory());
        Process.Start("Explorer.exe", filepath);
    }

    [RelayCommand]
    private async Task Enable901d()
    {
        MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
        if (sukiViewModel.Status == "系统")
        {
            var newDialog = new ConnectionDialog("该操作需要ROOT权限，请确保手机已ROOT，\n\r并在接下来的弹窗中授予 Shell ROOT权限！");
            await SukiHost.ShowDialogAsync(newDialog);
            if (newDialog.Result == true)
            {
                await CallExternalProgram.ADB($"-s {Global.thisdevice} shell su -c \"setprop sys.usb.config diag,adb\"");
                SukiHost.ShowDialog(new ConnectionDialog("执行完成，请查看您的设备！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new ConnectionDialog("请将设备进入系统后执行！"), allowBackgroundClose: true);
        }
    }

    [RelayCommand]
    private async Task Enable9091()
    {
        MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
        if (sukiViewModel.Status == "系统")
        {
            var newDialog = new ConnectionDialog("该操作仅限小米设备！其它设备将无法使用！");
            await SukiHost.ShowDialogAsync(newDialog);
            if (newDialog.Result == true)
            {
                await CallExternalProgram.ADB($"-s {Global.thisdevice} push bin/apk/mi_diag.apk /sdcard");
                await CallExternalProgram.ADB($"-s {Global.thisdevice} shell \"am start -a miui.intent.action.OPEN\"");
                SukiHost.ShowDialog(new ConnectionDialog("已将名为\"mi_diag.apk\"的文件推送至设备根目录，请安装完成后点击确定！"), allowBackgroundClose: true);
                await CallExternalProgram.ADB($"-s {Global.thisdevice} shell \"am start -n com.longcheertel.midtest/\"");
                await CallExternalProgram.ADB($"-s {Global.thisdevice} shell \"am start -n com.longcheertel.midtest/com.longcheertel.midtest.Diag\"");
            }
        }
        else
        {
            SukiHost.ShowDialog(new ConnectionDialog("请将设备进入系统后执行！"), allowBackgroundClose: true);
        }
    }

    [RelayCommand]
    private async Task FlashSuperEmpty()
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Fastboot")
            {
                if (SuperEmptyFile != null)
                {
                    Flashing = true;
                    AdvancedLog = "正在刷入...";
                    await Fastboot($"-s {Global.thisdevice} wipe-super \"{SuperEmptyFile}\"");
                    if (!AdvancedLog.Contains("FAILED") && !AdvancedLog.Contains("error"))
                    {
                        SukiHost.ShowDialog(new ConnectionDialog("刷入成功！"), allowBackgroundClose: true);
                    }
                    else
                    {
                        SukiHost.ShowDialog(new ConnectionDialog("刷入失败！"), allowBackgroundClose: true);
                    }
                    Flashing = false;
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog("请选择SuperEmpty文件！"), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("请进入Fastboot模式！"), allowBackgroundClose: true);
            }
        }
    }
}