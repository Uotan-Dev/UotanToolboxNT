using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SukiUI.Controls;
using SukiUI.Enums;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Features.Others;

public partial class OthersView : UserControl
{
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public AvaloniaList<string> Unit = ["DPI", "DP"];
    public OthersView()
    {
        InitializeComponent();
        SetUnit.ItemsSource = Unit;
        _ = GetDisplayInfo();
    }

    public void SetNull()
    {
        ScrResolution.Text = "--";
        ScrDPI.Text = "--";
        ScrDP.Text = "--";
        LockTime.Text = "--";
        FontZoom.Value = 0;
        NowFontZoom.Text = "0";
        WindowZoom.Value = 0;
        TransitionZoom.Value = 0;
        AnimationDuration.Value = 0;
        NoCharge.IsChecked = false;
        WirelessCharge.IsChecked = false;
        USUCharge.IsChecked = false;
        ACCharge.IsChecked = false;
    }

    public async Task GetDisplayInfo()
    {
        while (true)
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                if (ScrResolution.Text == "--" && ScrDPI.Text == "--" && ScrDP.Text == "--")
                {
                    try
                    {

                        ScrResolution.Text = StringHelper.ColonSplit(StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {Global.thisdevice} shell wm size")));
                        ScrDPI.Text = StringHelper.Density(await CallExternalProgram.ADB($"-s {Global.thisdevice} shell wm density"));
                        ScrDP.Text = StringHelper.GetDP(ScrResolution.Text, ScrDPI.Text).ToString();
                        LockTime.Text = (StringHelper.Onlynum(await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings get system screen_off_timeout")) / 1000).ToString() + "s";
                        FontZoom.Value = StringHelper.OnlynumFloat(await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings get system font_scale"));
                        NowFontZoom.Text = FontZoom.Value.ToString();
                        WindowZoom.Value = StringHelper.OnlynumFloat(await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings get global window_animation_scale"));
                        TransitionZoom.Value = StringHelper.OnlynumFloat(await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings get global transition_animation_scale"));
                        AnimationDuration.Value = StringHelper.OnlynumFloat(await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings get global animator_duration_scale"));
                    }
                    catch
                    {
                        SetNull();
                    }
                }
            }
            else
            {
                SetNull();
            }
            await Task.Delay(1000);
        }
    }

    private async void SetInfo(object sender, RoutedEventArgs args)
    {
        BusyDisplay.IsBusy = true;
        Display.IsEnabled = false;
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                if (!string.IsNullOrEmpty(Transverse.Text) && !string.IsNullOrEmpty(Direction.Text))
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell wm size {Transverse.Text}x{Direction.Text}");
                    SukiHost.ShowDialog(new PureDialog("执行成功！"), allowBackgroundClose: true);
                }
                if (!string.IsNullOrEmpty(DPIorDP.Text))
                {
                    if (SetUnit.SelectedItem == null)
                    {
                        SukiHost.ShowDialog(new PureDialog("请选择单位！"), allowBackgroundClose: true);
                    }
                    else if (SetUnit.SelectedItem.ToString() == "DPI")
                    {
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} shell wm density {DPIorDP.Text}");
                        SukiHost.ShowDialog(new PureDialog("执行成功！"), allowBackgroundClose: true);
                    }
                    else if (SetUnit.SelectedItem.ToString() == "DP")
                    {
                        if (!string.IsNullOrEmpty(ScrResolution.Text) && ScrResolution.Text != "--")
                        {
                            await CallExternalProgram.ADB($"-s {Global.thisdevice} shell wm density {StringHelper.GetDPI(ScrResolution.Text, DPIorDP.Text)}");
                            SukiHost.ShowDialog(new PureDialog("执行成功！"), allowBackgroundClose: true);
                        }
                        else
                        {
                            SukiHost.ShowDialog(new PureDialog("未能获取屏幕分辨率无法设置！"), allowBackgroundClose: true);
                        }
                    }
                }
                if ((string.IsNullOrEmpty(Transverse.Text) || string.IsNullOrEmpty(Direction.Text)) && (SetUnit.SelectedItem == null || string.IsNullOrEmpty(DPIorDP.Text)))
                {
                    SukiHost.ShowDialog(new PureDialog("请输入要修改的参数！"), allowBackgroundClose: true);
                }

            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请进入系统模式并开启USB调试！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
        Display.IsEnabled = true;
        BusyDisplay.IsBusy = false;
    }

    private async void BackInfo(object sender, RoutedEventArgs args)
    {
        BusyDisplay.IsBusy = true;
        Display.IsEnabled = false;
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                await CallExternalProgram.ADB($"-s {Global.thisdevice} shell wm size reset");
                await CallExternalProgram.ADB($"-s {Global.thisdevice} shell wm density reset");
                SukiHost.ShowDialog(new PureDialog("已恢复默认！"), allowBackgroundClose: true);
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请进入系统模式并开启USB调试！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
        Display.IsEnabled = true;
        BusyDisplay.IsBusy = false;
    }

    public void SetEnable(bool is_busy)
    {
        if (is_busy)
        {
            BusyBattery.IsBusy = true;
            Battery.IsEnabled = false;
        }
        else
        {
            Battery.IsEnabled = true;
            BusyBattery.IsBusy = false;
        }
    }

    private async void SetTemp(object sender, RoutedEventArgs args)
    {
        SetEnable(true);
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                if (!string.IsNullOrEmpty(Temp.Text))
                {
                    float temp = StringHelper.OnlynumFloat(Temp.Text);
                    if (temp >= 100)
                    {
                        await SukiHost.ShowToast("温度太高了！", "手机要爆炸了(╯‵□′)╯", NotificationType.Error);
                    }
                    else if (temp < -30 && temp > -273.15)
                    {
                        await SukiHost.ShowToast("温度太低了！", "手机要受不了了{{{(>_<)}}}", NotificationType.Error);
                    }
                    else if (temp < -273.15)
                    {
                        await SukiHost.ShowToast("比绝对零度还低！", "快去申请诺贝奖吧！(￣y▽,￣)╭ ", NotificationType.Error);
                        return;
                    }
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dumpsys battery set temp {temp * 10}");
                    SukiHost.ShowDialog(new PureDialog("执行成功！"), allowBackgroundClose: true);
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请输入温度！"), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请进入系统模式并开启USB调试！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
        SetEnable(false);
    }
    private async void SetBLevel(object sender, RoutedEventArgs args)
    {
        SetEnable(true);
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                if (!string.IsNullOrEmpty(BLevel.Text))
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dumpsys battery set level {BLevel.Text}");
                    SukiHost.ShowDialog(new PureDialog("执行成功！"), allowBackgroundClose: true);
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请输入电量！"), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请进入系统模式并开启USB调试！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
        SetEnable(false);
    }

    private async void BuckBattery(object sender, RoutedEventArgs args)
    {
        SetEnable(true);
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dumpsys battery reset");
                SukiHost.ShowDialog(new PureDialog("已恢复默认！"), allowBackgroundClose: true);
                NoCharge.IsChecked = false;
                WirelessCharge.IsChecked = false;
                USUCharge.IsChecked = false;
                ACCharge.IsChecked = false;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请进入系统模式并开启USB调试！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
        SetEnable(false);
    }

    private async void SetNoCharge(object sender, RoutedEventArgs args)
    {
        if (NoCharge.IsChecked != null && (bool)NoCharge.IsChecked)
        {
            SetEnable(true);
            if (await GetDevicesInfo.SetDevicesInfoLittle())
            {
                MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
                if (sukiViewModel.Status == GetTranslation("Home_System"))
                {
                    WirelessCharge.IsChecked = false;
                    USUCharge.IsChecked = false;
                    ACCharge.IsChecked = false;
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dumpsys battery set status 1");
                    SukiHost.ShowDialog(new PureDialog("执行成功！"), allowBackgroundClose: true);
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请进入系统模式并开启USB调试！"), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
            }
            SetEnable(false);
        }
    }

    private async void SetWirelessCharge(object sender, RoutedEventArgs args)
    {
        if (WirelessCharge.IsChecked != null && (bool)WirelessCharge.IsChecked)
        {
            SetEnable(true);
            if (await GetDevicesInfo.SetDevicesInfoLittle())
            {
                MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
                if (sukiViewModel.Status == GetTranslation("Home_System"))
                {
                    NoCharge.IsChecked = false;
                    USUCharge.IsChecked = false;
                    ACCharge.IsChecked = false;
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dumpsys battery set status 1");
                    SukiHost.ShowDialog(new PureDialog("执行成功！"), allowBackgroundClose: true);
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请进入系统模式并开启USB调试！"), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
            }
            SetEnable(false);
        }
    }

    private async void SetUSUCharge(object sender, RoutedEventArgs args)
    {
        if (USUCharge.IsChecked != null && (bool)USUCharge.IsChecked)
        {
            SetEnable(true);
            if (await GetDevicesInfo.SetDevicesInfoLittle())
            {
                MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
                if (sukiViewModel.Status == GetTranslation("Home_System"))
                {
                    WirelessCharge.IsChecked = false;
                    NoCharge.IsChecked = false;
                    ACCharge.IsChecked = false;
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dumpsys battery set status 1");
                    SukiHost.ShowDialog(new PureDialog("执行成功！"), allowBackgroundClose: true);
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请进入系统模式并开启USB调试！"), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
            }
            SetEnable(false);
        }
    }

    private async void SetACCharge(object sender, RoutedEventArgs args)
    {
        if (ACCharge.IsChecked != null && (bool)ACCharge.IsChecked)
        {
            SetEnable(true);
            if (await GetDevicesInfo.SetDevicesInfoLittle())
            {
                MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
                if (sukiViewModel.Status == GetTranslation("Home_System"))
                {

                    WirelessCharge.IsChecked = false;
                    USUCharge.IsChecked = false;
                    NoCharge.IsChecked = false;
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dumpsys battery set status 1");
                    SukiHost.ShowDialog(new PureDialog("执行成功！"), allowBackgroundClose: true);
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请进入系统模式并开启USB调试！"), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
            }
            SetEnable(false);
        }
    }

    private async void SetLockTime(object sender, RoutedEventArgs args)
    {
        BusyLock.IsBusy = true;
        Lock.IsEnabled = false;
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                if (!string.IsNullOrEmpty(NewLockTime.Text))
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put system screen_off_timeout {StringHelper.Onlynum(NewLockTime.Text) * 1000}");
                    SukiHost.ShowDialog(new PureDialog("执行成功！"), allowBackgroundClose: true);
                    LockTime.Text = (StringHelper.Onlynum(await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings get system screen_off_timeout")) / 1000).ToString() + "s";
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog("请输入锁屏时间！"), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请进入系统模式并开启USB调试！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
        BusyLock.IsBusy = false;
        Lock.IsEnabled = true;
    }

    private async void ShowOrHide(object sender, RoutedEventArgs args)
    {
        ShowAndHide.IsEnabled = false;
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                string addshell = "";
                if (Time.IsChecked == true)
                    addshell += "clock,";
                if (GPS.IsChecked == true)
                    addshell += "location,";
                if (Headset.IsChecked == true)
                    addshell += "headset,";
                if (Clock.IsChecked == true)
                    addshell += "alarm_clock,";
                if (Voice.IsChecked == true)
                    addshell += "volume,";
                if (LTE.IsChecked == true)
                    addshell += "mobile,";
                if (Bluetooth.IsChecked == true)
                    addshell += "bluetooth,";
                if (BatteryICO.IsChecked == true)
                    addshell += "battery,";
                if (WIFI.IsChecked == true)
                    addshell += "wifi,";
                if (NFC.IsChecked == true)
                    addshell += "nfc,";
                if (Fly.IsChecked == true)
                    addshell += "airplane,";
                await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put secure icon_blacklist rotate,ime,{addshell}");
                if (Second.IsChecked == true)
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put secure clock_seconds 1");
                }
                else
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put secure clock_seconds 0");
                }
                if (Rotate.IsChecked == true)
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put secure show_rotation_suggestions 0");
                }
                else
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put secure show_rotation_suggestions 1");
                }
                if (RemoveX.IsChecked == true)
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put global captive_portal_http_url \"http://connect.rom.miui.com/generate_204\"");
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put global captive_portal_https_url \"https://connect.rom.miui.com/generate_204\"");
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put global time_zone Asia/Shanghai");
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put global ntp_server ntp1.aliyun.com");
                }
                await SukiHost.ShowToast("执行成功！", "但不一定生效哦！", NotificationType.Success);
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请进入系统模式并开启USB调试！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
        ShowAndHide.IsEnabled = true;
    }

    public void SetFalse(bool totf)
    {
        if (totf)
        {
            SetFontZoomBut.IsEnabled = false;
            SetWindowZoomBut.IsEnabled = false;
            SetTransitionZoomBut.IsEnabled = false;
            SetAnimationDurationBut.IsEnabled = false;
        }
        else
        {
            SetFontZoomBut.IsEnabled = true;
            SetWindowZoomBut.IsEnabled = true;
            SetTransitionZoomBut.IsEnabled = true;
            SetAnimationDurationBut.IsEnabled = true;
        }
    }

    private void SetFontZoom(object sender, RoutedEventArgs args)
    {
        if (sender is Button button)
        {
            if (button.Content != null)
            {
                string text = button.Content.ToString();
                if (text != null)
                {
                    int zoom = StringHelper.Onlynum(text);
                    FontZoom.Value = zoom;
                    FontZoomBut(sender, args);
                }
            }
        }
    }

    private async void FontZoomBut(object sender, RoutedEventArgs args)
    {
        SetFalse(true);
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put system font_scale {FontZoom.Value}");
                await SukiHost.ShowToast("执行成功！", "但不一定生效哦！", NotificationType.Success);
                NowFontZoom.Text = StringHelper.OnlynumFloat(await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings get system font_scale")).ToString();
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请进入系统模式并开启USB调试！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
        SetFalse(false);
    }

    private void SetWindowZoom(object sender, RoutedEventArgs args)
    {
        if (sender is Button button)
        {
            if (button.Content != null)
            {
                string text = button.Content.ToString();
                if (text != null)
                {
                    int zoom = StringHelper.Onlynum(text);
                    WindowZoom.Value = zoom;
                    WindowZoomBut(sender, args);
                }
            }
        }
    }

    private async void WindowZoomBut(object sender, RoutedEventArgs args)
    {
        SetFalse(true);
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put global window_animation_scale {WindowZoom.Value}");
                await SukiHost.ShowToast("执行成功！", "但不一定生效哦！", NotificationType.Success);
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请进入系统模式并开启USB调试！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
        SetFalse(false);
    }

    private void SetTransitionZoom(object sender, RoutedEventArgs args)
    {
        if (sender is Button button)
        {
            if (button.Content != null)
            {
                string text = button.Content.ToString();
                if (text != null)
                {
                    int zoom = StringHelper.Onlynum(text);
                    TransitionZoom.Value = zoom;
                    TransitionZoomBut(sender, args);
                }
            }
        }
    }

    private async void TransitionZoomBut(object sender, RoutedEventArgs args)
    {
        SetFalse(true);
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put global transition_animation_scale {TransitionZoom.Value}");
                await SukiHost.ShowToast("执行成功！", "但不一定生效哦！", NotificationType.Success);
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请进入系统模式并开启USB调试！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
        SetFalse(false);
    }

    private void SetAnimationDuration(object sender, RoutedEventArgs args)
    {
        if (sender is Button button)
        {
            if (button.Content != null)
            {
                string text = button.Content.ToString();
                if (text != null)
                {
                    int zoom = StringHelper.Onlynum(text);
                    AnimationDuration.Value = zoom;
                    AnimationDurationBut(sender, args);
                }
            }
        }
    }

    private async void AnimationDurationBut(object sender, RoutedEventArgs args)
    {
        SetFalse(true);
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put global animator_duration_scale {AnimationDuration.Value}");
                await SukiHost.ShowToast("执行成功！", "但不一定生效哦！", NotificationType.Success);
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog("请进入系统模式并开启USB调试！"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("设备未连接！"), allowBackgroundClose: true);
        }
        SetFalse(false);
    }
}