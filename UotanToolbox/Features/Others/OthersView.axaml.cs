using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.Threading.Tasks;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Others;

public partial class OthersView : UserControl
{
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public AvaloniaList<string> Unit = ["DPI", "DP"];
    public OthersView()
    {
        InitializeComponent();
        SetUnit.ItemsSource = Unit;
        GetDisplayInfo();
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
            if (await GetDevicesInfo.SetDevicesInfoLittle())
            {
                MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
                if (sukiViewModel.Status == GetTranslation("Home_Android"))
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
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                if (!string.IsNullOrEmpty(Transverse.Text) && !string.IsNullOrEmpty(Direction.Text))
                {
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell wm size {Transverse.Text}x{Direction.Text}");
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                }
                if (!string.IsNullOrEmpty(DPIorDP.Text))
                {
                    if (SetUnit.SelectedItem == null)
                    {
                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Others_SetUnit")).Dismiss().ByClickingBackground().TryShow();
                    }
                    else if (SetUnit.SelectedItem.ToString() == "DPI")
                    {
                        _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell wm density {DPIorDP.Text}");
                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                    }
                    else if (SetUnit.SelectedItem.ToString() == "DP")
                    {
                        if (!string.IsNullOrEmpty(ScrResolution.Text) && ScrResolution.Text != "--")
                        {
                            _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell wm density {StringHelper.GetDPI(ScrResolution.Text, DPIorDP.Text)}");
                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                        }
                        else
                        {
                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Others_NoScreen")).Dismiss().ByClickingBackground().TryShow();
                        }
                    }
                }
                if ((string.IsNullOrEmpty(Transverse.Text) || string.IsNullOrEmpty(Direction.Text)) && (SetUnit.SelectedItem == null || string.IsNullOrEmpty(DPIorDP.Text)))
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Others_EnterPara")).Dismiss().ByClickingBackground().TryShow();
                }

            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
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
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell wm size reset");
                _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell wm density reset");
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Others_Restored")).Dismiss().ByClickingBackground().TryShow();
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
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
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                if (!string.IsNullOrEmpty(Temp.Text))
                {
                    float temp = StringHelper.OnlynumFloat(Temp.Text);
                    if (temp >= 100)
                    {
                        Global.MainToastManager.CreateToast()
                            .WithTitle(GetTranslation("Others_TempHigh"))
                            .WithContent(GetTranslation("Others_PhoneBoom") + "(╯‵□′)╯")
                            .OfType(NotificationType.Error)
                            .Dismiss().ByClicking()
                            .Dismiss().After(TimeSpan.FromSeconds(3))
                            .Queue();
                    }
                    else if (temp is < (-30) and > (float)-273.15)
                    {
                        Global.MainToastManager.CreateToast()
                            .WithTitle(GetTranslation("Others_TempLow"))
                            .WithContent(GetTranslation("Others_PhoneTake") + "{{{(>_<)}}}")
                            .OfType(NotificationType.Error)
                            .Dismiss().ByClicking()
                            .Dismiss().After(TimeSpan.FromSeconds(3))
                            .Queue();
                    }
                    else if (temp < -273.15)
                    {
                        Global.MainToastManager.CreateToast()
                            .WithTitle(GetTranslation("Others_TempLower"))
                            .WithContent(GetTranslation("Others_Nobel") + "(￣y▽,￣)╭ ")
                            .OfType(NotificationType.Error)
                            .Dismiss().ByClicking()
                            .Dismiss().After(TimeSpan.FromSeconds(3))
                            .Queue();
                        return;
                    }
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dumpsys battery set temp {temp * 10}");
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Others_EnterTemp")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
        SetEnable(false);
    }
    private async void SetBLevel(object sender, RoutedEventArgs args)
    {
        SetEnable(true);
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                if (!string.IsNullOrEmpty(BLevel.Text))
                {
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dumpsys battery set level {BLevel.Text}");
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Others_EnterBattery")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
        SetEnable(false);
    }

    private async void BuckBattery(object sender, RoutedEventArgs args)
    {
        SetEnable(true);
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dumpsys battery reset");
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Others_Restored")).Dismiss().ByClickingBackground().TryShow();
                NoCharge.IsChecked = false;
                WirelessCharge.IsChecked = false;
                USUCharge.IsChecked = false;
                ACCharge.IsChecked = false;
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
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
                if (sukiViewModel.Status == GetTranslation("Home_Android"))
                {
                    WirelessCharge.IsChecked = false;
                    USUCharge.IsChecked = false;
                    ACCharge.IsChecked = false;
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dumpsys battery set status 1");
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
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
                if (sukiViewModel.Status == GetTranslation("Home_Android"))
                {
                    NoCharge.IsChecked = false;
                    USUCharge.IsChecked = false;
                    ACCharge.IsChecked = false;
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dumpsys battery set status 1");
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
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
                if (sukiViewModel.Status == GetTranslation("Home_Android"))
                {
                    WirelessCharge.IsChecked = false;
                    NoCharge.IsChecked = false;
                    ACCharge.IsChecked = false;
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dumpsys battery set status 1");
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
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
                if (sukiViewModel.Status == GetTranslation("Home_Android"))
                {

                    WirelessCharge.IsChecked = false;
                    USUCharge.IsChecked = false;
                    NoCharge.IsChecked = false;
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dumpsys battery set status 1");
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
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
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                if (!string.IsNullOrEmpty(NewLockTime.Text))
                {
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put system screen_off_timeout {StringHelper.Onlynum(NewLockTime.Text) * 1000}");
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                    LockTime.Text = (StringHelper.Onlynum(await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings get system screen_off_timeout")) / 1000).ToString() + "s";
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Others_EnterLock")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
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
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                string addshell = "";
                if (Time.IsChecked == true)
                {
                    addshell += "clock,";
                }

                if (GPS.IsChecked == true)
                {
                    addshell += "location,";
                }

                if (Headset.IsChecked == true)
                {
                    addshell += "headset,";
                }

                if (Clock.IsChecked == true)
                {
                    addshell += "alarm_clock,";
                }

                if (Voice.IsChecked == true)
                {
                    addshell += "volume,";
                }

                if (LTE.IsChecked == true)
                {
                    addshell += "mobile,";
                }

                if (Bluetooth.IsChecked == true)
                {
                    addshell += "bluetooth,";
                }

                if (BatteryICO.IsChecked == true)
                {
                    addshell += "battery,";
                }

                if (WIFI.IsChecked == true)
                {
                    addshell += "wifi,";
                }

                if (NFC.IsChecked == true)
                {
                    addshell += "nfc,";
                }

                if (Fly.IsChecked == true)
                {
                    addshell += "airplane,";
                }

                _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put secure icon_blacklist rotate,ime,{addshell}");
                _ = Second.IsChecked == true
                    ? await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put secure clock_seconds 1")
                    : await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put secure clock_seconds 0");
                _ = Rotate.IsChecked == true
                    ? await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put secure show_rotation_suggestions 0")
                    : await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put secure show_rotation_suggestions 1");
                if (RemoveX.IsChecked == true)
                {
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put global captive_portal_http_url \"http://connect.rom.miui.com/generate_204\"");
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put global captive_portal_https_url \"https://connect.rom.miui.com/generate_204\"");
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put global time_zone Asia/Shanghai");
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put global ntp_server ntp1.aliyun.com");
                }
                Global.MainToastManager.CreateToast()
                    .WithTitle(GetTranslation("Common_Execution"))
                    .WithContent(GetTranslation("Others_NotEffect"))
                    .OfType(NotificationType.Success)
                    .Dismiss().ByClicking()
                    .Dismiss().After(TimeSpan.FromSeconds(3))
                    .Queue();
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
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
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put system font_scale {FontZoom.Value}");
                Global.MainToastManager.CreateToast()
                    .WithTitle(GetTranslation("Common_Execution"))
                    .WithContent(GetTranslation("Others_NotEffect"))
                    .OfType(NotificationType.Success)
                    .Dismiss().ByClicking()
                    .Dismiss().After(TimeSpan.FromSeconds(3))
                    .Queue();
                NowFontZoom.Text = StringHelper.OnlynumFloat(await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings get system font_scale")).ToString();
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
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
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put global window_animation_scale {WindowZoom.Value}");
                Global.MainToastManager.CreateToast()
                    .WithTitle(GetTranslation("Common_Execution"))
                    .WithContent(GetTranslation("Others_NotEffect"))
                    .OfType(NotificationType.Success)
                    .Dismiss().ByClicking()
                    .Dismiss().After(TimeSpan.FromSeconds(3))
                    .Queue();
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
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
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put global transition_animation_scale {TransitionZoom.Value}");
                Global.MainToastManager.CreateToast()
                    .WithTitle(GetTranslation("Common_Execution"))
                    .WithContent(GetTranslation("Others_NotEffect"))
                    .OfType(NotificationType.Success)
                    .Dismiss().ByClicking()
                    .Dismiss().After(TimeSpan.FromSeconds(3))
                    .Queue();
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
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
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell settings put global animator_duration_scale {AnimationDuration.Value}");
                Global.MainToastManager.CreateToast()
                    .WithTitle(GetTranslation("Common_Execution"))
                    .WithContent(GetTranslation("Others_NotEffect"))
                    .OfType(NotificationType.Success)
                    .Dismiss().ByClicking()
                    .Dismiss().After(TimeSpan.FromSeconds(3))
                    .Queue();
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
        SetFalse(false);
    }
}