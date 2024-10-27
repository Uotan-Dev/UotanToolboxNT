using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Home;

public partial class HomeView : UserControl
{
    public static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public HomeView()
    {
        _ = CheckEnvironment();
        InitializeComponent();
    }

    public async Task CheckEnvironment()
    {
        string filepath1 = "";
        string filepath2 = "";
        if (Global.System == "Windows")
        {
            filepath1 = Path.Combine(Global.bin_path, "platform-tools", "adb.exe");
            filepath2 = Path.Combine(Global.bin_path, "platform-tools", "fastboot.exe");
        }
        else
        {
            filepath1 = Path.Combine(Global.bin_path, "platform-tools", "adb");
            filepath2 = Path.Combine(Global.bin_path, "platform-tools", "fastboot");
        }
        if (!File.Exists(filepath1) || !File.Exists(filepath2))
        {
            Global.MainDialogManager.CreateDialog()
                .WithTitle(GetTranslation("Common_Warn"))
                .WithContent(GetTranslation("Home_Missing"))
                .OfType(NotificationType.Warning)
                .WithActionButton("OK", _ => Process.GetCurrentProcess().Kill(), true)
                .TryShow();

        }
    }

    public async void CopyButton_OnClick(object sender, RoutedEventArgs args)
    {
        if (sender is Button button)
        {
            Avalonia.Input.Platform.IClipboard clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            DataObject dataObject = new DataObject();
            if (button.Content != null)
            {
                string text = button.Content.ToString();
                if (text != null)
                {
                    dataObject.Set(DataFormats.Text, text);
                }
            }
            if (clipboard != null)
            {
                await clipboard.SetDataObjectAsync(dataObject);
            }

            Global.MainToastManager.CreateSimpleInfoToast()
                .WithTitle(GetTranslation("Home_Copy"))
                .WithContent("o(*≧▽≦)ツ")
                .OfType(NotificationType.Success)
                .Dismiss().ByClicking()
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Queue();
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
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Succ")).OfType(NotificationType.Success).WithContent(GetTranslation("Common_InstallSuccess")).Dismiss().ByClickingBackground().TryShow();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_InstallFailed")).Dismiss().ByClickingBackground().TryShow();
                }
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Basicflash_NotUsed")).Dismiss().ByClickingBackground().TryShow();
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
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Succ")).OfType(NotificationType.Success).WithContent(GetTranslation("Common_InstallSuccess")).Dismiss().ByClickingBackground().TryShow();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_InstallFailed")).Dismiss().ByClickingBackground().TryShow();
                }
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Basicflash_NotUsed")).Dismiss().ByClickingBackground().TryShow();
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
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
            });
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Basicflash_NotUsed")).Dismiss().ByClickingBackground().TryShow();
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
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
            });
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Basicflash_NotUsed")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private void OpenWirelessADB(object sender, RoutedEventArgs args) => new WirelessADB().Show();
}