using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Controls;
using System;
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

    private async void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (!String.IsNullOrEmpty(outLine.Data))
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StringBuilder sb = new StringBuilder(AdvancedLog.Text);
                AdvancedLog.Text = sb.AppendLine(outLine.Data).ToString();
                AdvancedLog.ScrollToLine(StringHelper.TextBoxLine(AdvancedLog.Text));
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
                    AdvancedLog.Text = "正在刷入...";
                    await Fastboot($"-s {Global.thisdevice} wipe-super \"{SuperEmptyFile.Text}\"");
                    if (!AdvancedLog.Text.Contains("FAILED") && !AdvancedLog.Text.Contains("error"))
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            SukiHost.ShowDialog(new ConnectionDialog("刷入成功！"), allowBackgroundClose: true);
                        });
                    }
                    else
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            SukiHost.ShowDialog(new ConnectionDialog("刷入失败！"), allowBackgroundClose: true);
                        });
                    }
                    BusyFlash.IsBusy = false;
                }
                else
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        SukiHost.ShowDialog(new ConnectionDialog("请选择SuperEmpty文件！"), allowBackgroundClose: true);
                    });
                }
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SukiHost.ShowDialog(new ConnectionDialog("请进入Fastboot模式！"), allowBackgroundClose: true);
                });
            }
        }
    }
}