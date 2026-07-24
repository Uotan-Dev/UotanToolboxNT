using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI;
using SukiUI.Dialogs;
using SukiUI.Enums;
using SukiUI.Models;
using SukiUI.Toasts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Common.Devices;
using UotanToolbox.Features;
using UotanToolbox.Features.Settings;
using UotanToolbox.Services;
using UotanToolbox.Utilities;

namespace UotanToolbox;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private bool _windowLocked = false;

    public IAvaloniaReadOnlyList<MainPageBase> DemoPages { get; }

    public IAvaloniaReadOnlyList<SukiColorTheme> Themes { get; }

    public IAvaloniaReadOnlyList<SukiBackgroundStyle> BackgroundStyles { get; }

    public ISukiToastManager ToastManager { get; }
    public ISukiDialogManager DialogManager { get; }

    [ObservableProperty] private ThemeVariant _baseTheme;
    [ObservableProperty] private bool _animationsEnabled;
    [ObservableProperty] private MainPageBase _activePage = null!;
    [ObservableProperty] private SukiBackgroundStyle _backgroundStyle = SukiBackgroundStyle.Gradient;
    [ObservableProperty] private string _customShaderFile = string.Empty;
    [ObservableProperty] private bool _transitionsEnabled;
    [ObservableProperty] private double _transitionTime;

    [ObservableProperty]
    private string _status = string.Empty, _codeName = string.Empty, _bLStatus = string.Empty, _vABStatus = string.Empty;
    private readonly SukiTheme _theme;
    private readonly SettingsViewModel _theming;

    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }
    public MainViewModel(IEnumerable<MainPageBase> demoPages, PageNavigationService nav, ISukiToastManager toastManager, ISukiDialogManager dialogManager)
    {
        Global.MainToastManager = ToastManager = toastManager;
        Global.MainDialogManager = DialogManager = dialogManager;
        Status = "--"; CodeName = "--"; BLStatus = "--"; VABStatus = "--";
        DemoPages = new AvaloniaList<MainPageBase>(demoPages.OrderBy(x => x.Index).ThenBy(x => x.DisplayName));
        _theming = (SettingsViewModel)DemoPages.First(x => x is SettingsViewModel);
        _theming.BackgroundStyleChanged += style => BackgroundStyle = style;
        _theming.BackgroundAnimationsChanged += enabled => AnimationsEnabled = enabled;
        _theming.CustomBackgroundStyleChanged += shader => CustomShaderFile = shader!;
        _theming.BackgroundTransitionsChanged += enabled => TransitionsEnabled = enabled;
        BackgroundStyles = new AvaloniaList<SukiBackgroundStyle>(Enum.GetValues<SukiBackgroundStyle>());
        _theme = SukiTheme.GetInstance();
        nav.NavigationRequested += t =>
        {
            MainPageBase? page = DemoPages.FirstOrDefault(x => x.GetType() == t);
            if (page is null || ActivePage?.GetType() == t)
            {
                return;
            }

            ActivePage = page;
        };
        Themes = _theme.ColorThemes;
        BaseTheme = _theme.ActiveBaseTheme;
        _theme.OnBaseThemeChanged += variant =>
        {
            BaseTheme = variant;
            _ = toastManager.CreateToast()
                            .WithTitle($"{GetTranslation("MainView_SuccessfullyChangedTheme")}")
                            .WithContent($"{GetTranslation("MainView_ChangedThemeTo")} {variant}")
                            .OfType(NotificationType.Success)
                            .Dismiss().ByClicking()
                            .Dismiss().After(TimeSpan.FromSeconds(3))
                            .Queue();
        };
        _theme.OnColorThemeChanged += theme =>
                    toastManager.CreateToast()
                                .WithTitle($"{GetTranslation("MainView_SuccessfullyChangedColor")}")
                                .WithContent($"{GetTranslation("MainView_ChangedColorTo")} {theme.DisplayName}")
                                .OfType(NotificationType.Success)
                                .Dismiss().ByClicking()
                                .Dismiss().After(TimeSpan.FromSeconds(3))
                                .Queue();
        GlobalData.MainViewModelInstance = this;
    }

    [RelayCommand]
    private void ToggleAnimations()
    {
        AnimationsEnabled = !AnimationsEnabled;
        ToastManager.CreateSimpleInfoToast()
            .WithTitle(AnimationsEnabled ? "Animation Enabled" : "Animation Disabled")
            .WithContent(AnimationsEnabled ? "Background animations are now enabled." : "Background animations are now disabled.")
            .Queue();

    }

    [RelayCommand]
    private void ToggleBaseTheme()
    {
        _theme.SwitchBaseTheme();
    }

    public void ChangeTheme(SukiColorTheme theme)
    {
        _theme.ChangeColorTheme(theme);
    }

    [RelayCommand]
    public async Task RebootSys()
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var deviceManager = Global.DeviceManager;
            var device = deviceManager?.Devices.FirstOrDefault(d => d.Id == Global.thisdevice);
            if (device != null)
            {
                switch (device.Transport)
                {
                    case TransportType.Adb:
                        await deviceManager!.ExecuteAsync(device, "reboot");
                        break;
                    case TransportType.Fastboot:
                        await deviceManager!.ExecuteAsync(device, "reboot");
                        break;
                    case TransportType.Hdc:
                        await deviceManager!.ExecuteAsync(device, "target boot");
                        break;
                    default:
                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_ModeError")).Dismiss().ByClickingBackground().TryShow();
                        break;
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    [RelayCommand]
    public async Task RebootRec()
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var deviceManager = Global.DeviceManager;
            var device = deviceManager?.Devices.FirstOrDefault(d => d.Id == Global.thisdevice);
            if (device != null)
            {
                switch (device.Transport)
                {
                    case TransportType.Adb:
                        await deviceManager!.ExecuteAsync(device, "reboot recovery");
                        break;
                    case TransportType.Fastboot:
                        string output = await deviceManager!.ExecuteAsync(device, "oem reboot-recovery");
                        if (output.Contains("unknown command"))
                        {
                            await deviceManager!.ExecuteAsync(device, $"flash misc \"{Path.Combine(Global.runpath, "Image", "misc.img")}\"");
                            await deviceManager!.ExecuteAsync(device, "reboot");
                        }
                        break;
                    case TransportType.Hdc:
                        await deviceManager!.ExecuteAsync(device, "target boot -recovery");
                        break;
                    default:
                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_ModeError")).Dismiss().ByClickingBackground().TryShow();
                        break;
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    [RelayCommand]
    public async Task RebootBL()
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var deviceManager = Global.DeviceManager;
            var device = deviceManager?.Devices.FirstOrDefault(d => d.Id == Global.thisdevice);
            if (device != null)
            {
                switch (device.Transport)
                {
                    case TransportType.Adb:
                        await deviceManager!.ExecuteAsync(device, "reboot bootloader");
                        break;
                    case TransportType.Fastboot:
                        await deviceManager!.ExecuteAsync(device, "reboot-bootloader");
                        break;
                    case TransportType.Hdc:
                        await deviceManager!.ExecuteAsync(device, "target boot -bootloader");
                        break;
                    default:
                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_ModeError")).Dismiss().ByClickingBackground().TryShow();
                        break;
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    [RelayCommand]
    public async Task RebootFB()
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var deviceManager = Global.DeviceManager;
            var device = deviceManager?.Devices.FirstOrDefault(d => d.Id == Global.thisdevice);
            if (device != null)
            {
                switch (device.Transport)
                {
                    case TransportType.Adb:
                        await deviceManager!.ExecuteAsync(device, "reboot fastboot");
                        break;
                    case TransportType.Fastboot:
                        await deviceManager!.ExecuteAsync(device, "reboot-fastboot");
                        break;
                    case TransportType.Hdc:
                        await deviceManager!.ExecuteAsync(device, "target boot -fastboot");
                        break;
                    default:
                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_ModeError")).Dismiss().ByClickingBackground().TryShow();
                        break;
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    [RelayCommand]
    public async Task Disconnect()
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var deviceManager = Global.DeviceManager;
            var device = deviceManager?.Devices.FirstOrDefault(d => d.Id == Global.thisdevice);
            if (device != null)
            {
                switch (device.Transport)
                {
                    case TransportType.Adb:
                        await deviceManager!.ExecuteAsync(device, "disconnect");
                        break;
                    case TransportType.Hdc:
                        await deviceManager!.ExecuteAsync(device, $"tconn {device.Id} -remove");
                        break;
                    default:
                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_ModeError")).Dismiss().ByClickingBackground().TryShow();
                        break;
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    [RelayCommand]
    public async Task RestartADB()
    {
        // kill adb/hdc processes via DeviceManager when appropriate
        if (Global.DeviceManager != null)
        {
            // send generic kill commands to each transport if needed
            foreach (var dev in Global.DeviceManager.Devices)
            {
                switch (dev.Transport)
                {
                    case TransportType.Adb:
                        await Global.DeviceManager.ExecuteAsync(dev, "kill-server");
                        break;
                    case TransportType.Hdc:
                        await Global.DeviceManager.ExecuteAsync(dev, "kill -r");
                        break;
                }
            }
        }
        else
        {
            await FeaturesHelper.AdbCmd("", "kill-server");
            await FeaturesHelper.HdcCmd("", "kill -r");
        }
    }

    [RelayCommand]
    private static void OpenURL(string url) => UrlUtilities.OpenURL(url);
}