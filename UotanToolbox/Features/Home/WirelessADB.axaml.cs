using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System.IO;
using System.Text.RegularExpressions;
using UotanToolbox.Common;


namespace UotanToolbox.Features.Home;

public partial class WirelessADB : SukiWindow
{
    private readonly ISukiDialogManager _thisDialogManager = new SukiDialogManager();
    private readonly ISukiToastManager _thisToastManager = new SukiToastManager();
    private static IImage image;
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public static Bitmap ConvertToBitmap(byte[] imageData)
    {
        using (var stream = new MemoryStream(imageData))
        {
            return new Bitmap(stream);
        }
    }
    public WirelessADB()
    {
        InitializeComponent();
        image = QRCode.Source;
        DialogHost.Manager = _thisDialogManager;
        ToastHost.Manager = _thisToastManager;
        StartScanm();
        QRCode.Source = ConvertToBitmap(ADBPairHelper.QRCodeInit(Global.serviceID, Global.password));
    }

    private async void SetOH(object sender, RoutedEventArgs args)
    {
        if ((bool)OHCheck.IsChecked)
        {
            WTW.IsEnabled = false;
            PairingCode.IsEnabled = false;
            QRCode.Source = image;
        }
        else
        {
            WTW.IsEnabled = true;
            PairingCode.IsEnabled = true;
            QRCode.Source = ConvertToBitmap(ADBPairHelper.QRCodeInit(Global.serviceID, Global.password));
        }
    }

    private async void StartScanm()
    {
        await ADBPairHelper.ScanmDNS(Global.serviceID, Global.password, _thisDialogManager);
    }

    private async void WConnect(object sender, RoutedEventArgs args)
    {
        string input = IPAndPort.Text;
        string password = PairingCode.Text;
        Connect.IsBusy = true;
        ConnectPanel.IsEnabled = false;
        if (!string.IsNullOrEmpty(input))
        {
            if (!(bool)OHCheck.IsChecked)
            {
                string result = await CallExternalProgram.ADB($"pair {input} {password}");
                if (result.Contains("Successfully paired to "))
                {
                    _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Succ")).OfType(NotificationType.Success).WithContent(GetTranslation("WirelessADB_Connect")).Dismiss().ByClickingBackground().TryShow();
                }
                else
                {
                    _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(result).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                string result = await CallExternalProgram.HDC($"tconn {input}");
                if (result.Contains("Connect OK"))
                {
                    _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Succ")).OfType(NotificationType.Success).WithContent(GetTranslation("WirelessADB_Connect")).Dismiss().ByClickingBackground().TryShow();
                }
                else
                {
                    _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(result).Dismiss().ByClickingBackground().TryShow();
                }
            }
        }
        else
        {
            _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterAll")).Dismiss().ByClickingBackground().TryShow();
        }
        Connect.IsBusy = false;
        ConnectPanel.IsEnabled = true;
    }

    private async void TWConnect(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                Connect.IsBusy = true;
                ConnectPanel.IsEnabled = false;
                string output = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell ip addr show to 0.0.0.0/0 scope global");
                string pattern = @"inet\s+(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})/";

                Match match = Regex.Match(output, pattern);

                if (!match.Success)
                {
                    _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(output).Dismiss().ByClickingBackground().TryShow();
                    return;
                }
                output = await CallExternalProgram.ADB($"-s {Global.thisdevice} tcpip 5555");
                if (output.Contains("restarting"))
                {
                    string output2 = await CallExternalProgram.ADB($"connect {match.Groups[1].Value}");
                    if (output2.Contains("connected"))
                    {
                        _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Succ")).OfType(NotificationType.Success).WithContent(GetTranslation("WirelessADB_Connect")).Dismiss().ByClickingBackground().TryShow();
                    }
                    else
                    {
                        _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(output + "\n" + output2).Dismiss().ByClickingBackground().TryShow();
                    }
                }
                Connect.IsBusy = false;
                ConnectPanel.IsEnabled = true;
            }
            else
            {
                _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }
}