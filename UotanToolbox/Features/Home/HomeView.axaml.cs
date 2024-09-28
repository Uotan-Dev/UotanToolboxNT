using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SukiUI.Controls;
using SukiUI.Enums;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UotanToolbox.Common;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Features.Home;

public partial class HomeView : UserControl
{
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public HomeView()
    {
        InitializeComponent();
    }

    private async void CopyButton_OnClick(object sender, RoutedEventArgs args)
    {
        if (sender is Button button)
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            var dataObject = new DataObject();
            if (button.Content != null)
            {
                var text = button.Content.ToString();
                if (text != null)
                    dataObject.Set(DataFormats.Text, text);
            }
            if (clipboard != null)
                await clipboard.SetDataObjectAsync(dataObject);
            await SukiHost.ShowToast(GetTranslation("Home_Copy"), "o(*≧▽≦)ツ", NotificationType.Success);
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
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_Execution")), allowBackgroundClose: true);
            });
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_NotUsed")), allowBackgroundClose: true);
        }
    }

    private async void OpenReUSBP(object sender, RoutedEventArgs args)
    {
        if (Global.System == "Windows")
        {
            string cmd = @"drive\ReUSB3.bat";
            ProcessStartInfo cmdshell = null;
            cmdshell = new ProcessStartInfo(cmd)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process f = Process.Start(cmdshell);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_Execution")), allowBackgroundClose: true);
            });
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Basicflash_NotUsed")), allowBackgroundClose: true);
        }
    }

    private void OpenWirelessADB(object sender, RoutedEventArgs args) => new WirelessADB().Show();
}