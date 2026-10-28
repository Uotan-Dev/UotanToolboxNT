using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UotanToolbox.Common;


namespace UotanToolbox.Features.Home;

public partial class WirelessADB : SukiWindow
{
    private const double AspectRatio = 850.0 / 455.0;
    private Size _lastSize = new Size(850, 455);
    private readonly ISukiDialogManager _thisDialogManager = new SukiDialogManager();
    private readonly ISukiToastManager _thisToastManager = new SukiToastManager();
    private static IImage? image;
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    private CancellationTokenSource? _scanCts;

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
        this.GetObservable(ClientSizeProperty).Subscribe(OnClientSizeChanged);
        this.CanResize = Global.SetResize;
        Closed += OnClosed;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _scanCts?.Cancel();
        _scanCts?.Dispose();
        _scanCts = null;
    }

    private void OnClientSizeChanged(Size newSize)
    {
        double deltaWidth = Math.Abs(newSize.Width - _lastSize.Width);
        double deltaHeight = Math.Abs(newSize.Height - _lastSize.Height);

        if (deltaWidth > deltaHeight)
        {
            double expectedHeight = newSize.Width / AspectRatio;
            if (Math.Abs(newSize.Height - expectedHeight) > 1)
                this.Height = expectedHeight;
        }
        else
        {
            double expectedWidth = newSize.Height * AspectRatio;
            if (Math.Abs(newSize.Width - expectedWidth) > 1)
                this.Width = expectedWidth;
        }

        _lastSize = new Size(this.Width, this.Height);
    }

    private async void SetOH(object sender, RoutedEventArgs args)
    {
        if (OHCheck.IsChecked == true)
        {
            WTW.IsEnabled = false;
            PairingCode.IsEnabled = false;
            QRCode.Source = image;
        }
        else
        {
            WTW.IsEnabled = true;
            PairingCode.IsEnabled = true;
            // 重新生成二维码，确保 serviceID/password 与 StartScanm 一致
            QRCode.Source = ConvertToBitmap(ADBPairHelper.QRCodeInit(Global.serviceID, Global.password));
        }
    }

    private async void StartScanm()
    {
        _scanCts = new CancellationTokenSource();
        await ADBPairHelper.ScanmDNS(Global.serviceID, Global.password, _thisDialogManager, _scanCts.Token);
    }

    private async void WConnect(object sender, RoutedEventArgs args)
    {
        string input = IPAndPort.Text ?? string.Empty;
        string password = PairingCode.Text ?? string.Empty;
        Connect.IsBusy = true;
        ConnectPanel.IsEnabled = false;
        try
        {
            if (string.IsNullOrEmpty(input))
            {
                _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterAll")).Dismiss().ByClickingBackground().TryShow();
                return;
            }

            if (OHCheck.IsChecked == true)
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
                return;
            }

            // 场景 1：纯 IP（不含端口）+ 无配对码 → 直接 adb connect <IP>（兼容默认端口或 adbd 已在监听）
            // 场景 2：IP:port 形式 + 有配对码 → adb pair <IP:port> <code>，再 adb connect <IP:5555>
            // 场景 3：IP:port 形式 + 无配对码 → 直接 adb connect <IP:port>
            bool hasPort = input.Contains(':');
            bool hasPairCode = !string.IsNullOrEmpty(password);

            if (!hasPort && !hasPairCode)
            {
                string connectResult = await CallExternalProgram.ADB($"connect {input}");
                if (connectResult.Contains("connected to") || connectResult.Contains("already connected"))
                {
                    _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Succ")).OfType(NotificationType.Success).WithContent(GetTranslation("WirelessADB_Connect")).Dismiss().ByClickingBackground().TryShow();
                }
                else
                {
                    _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(connectResult).Dismiss().ByClickingBackground().TryShow();
                }
                return;
            }

            if (hasPort && !hasPairCode)
            {
                string connectResult = await CallExternalProgram.ADB($"connect {input}");
                if (connectResult.Contains("connected to") || connectResult.Contains("already connected"))
                {
                    _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Succ")).OfType(NotificationType.Success).WithContent(GetTranslation("WirelessADB_Connect")).Dismiss().ByClickingBackground().TryShow();
                }
                else
                {
                    _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(connectResult).Dismiss().ByClickingBackground().TryShow();
                }
                return;
            }

            // 有配对码：先 pair，再用 host + 默认端口 connect
            string pairResult = await CallExternalProgram.ADB($"pair {input} {password}");
            if (!pairResult.Contains("Successfully paired to "))
            {
                _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(pairResult).Dismiss().ByClickingBackground().TryShow();
                return;
            }

            // 从 input 中提取 host，使用默认 5555 端口连接
            string host = input;
            int colonIdx = input.IndexOf(':');
            if (colonIdx >= 0)
            {
                host = input.Substring(0, colonIdx);
            }
            string connectAddr = $"{host}:5555";
            string connectResult2 = await CallExternalProgram.ADB($"connect {connectAddr}");
            if (connectResult2.Contains("connected to") || connectResult2.Contains("already connected"))
            {
                _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Succ")).OfType(NotificationType.Success).WithContent(GetTranslation("WirelessADB_Connect")).Dismiss().ByClickingBackground().TryShow();
            }
            else
            {
                _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(pairResult + "\n" + connectResult2).Dismiss().ByClickingBackground().TryShow();
            }
        }
        finally
        {
            Connect.IsBusy = false;
            ConnectPanel.IsEnabled = true;
        }
    }

    private async void TWConnect(object sender, RoutedEventArgs args)
    {
        if (!await GetDevicesInfo.SetDevicesInfoLittle())
        {
            _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
            return;
        }

        MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
        if (sukiViewModel.Status != GetTranslation("Home_Android"))
        {
            _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
            return;
        }

        Connect.IsBusy = true;
        ConnectPanel.IsEnabled = false;
        try
        {
            string output = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell ip addr show to 0.0.0.0/0 scope global");
            string pattern = @"inet\s+(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})/";

            Match match = Regex.Match(output, pattern);

            if (!match.Success)
            {
                _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(output).Dismiss().ByClickingBackground().TryShow();
                return;
            }

            output = await CallExternalProgram.ADB($"-s {Global.thisdevice} tcpip 5555");
            if (!output.Contains("restarting"))
            {
                _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(output).Dismiss().ByClickingBackground().TryShow();
                return;
            }

            // 等待 adbd 在 TCP 端口重启，避免竞态条件导致 connect 失败
            await Task.Delay(2000);

            string ip = match.Groups[1].Value;
            string output2 = await CallExternalProgram.ADB($"connect {ip}:5555");
            if (output2.Contains("connected to") || output2.Contains("already connected"))
            {
                _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Succ")).OfType(NotificationType.Success).WithContent(GetTranslation("WirelessADB_Connect")).Dismiss().ByClickingBackground().TryShow();
            }
            else
            {
                _thisDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(output + "\n" + output2).Dismiss().ByClickingBackground().TryShow();
            }
        }
        finally
        {
            Connect.IsBusy = false;
            ConnectPanel.IsEnabled = true;
        }
    }
}
