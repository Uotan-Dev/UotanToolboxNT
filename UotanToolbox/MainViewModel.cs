using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using UotanToolbox.Common;
using UotanToolbox.Features;
using UotanToolbox.Features.Settings;
using UotanToolbox.Services;
using UotanToolbox.Utilities;

namespace UotanToolbox;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private bool _windowLocked;

    public IAvaloniaReadOnlyList<MainPageBase> DemoPages { get; }

    public IAvaloniaReadOnlyList<SukiColorTheme> Themes { get; }

    private readonly ISukiDialogManager _dialogManager;

    public IAvaloniaReadOnlyList<SukiBackgroundStyle> BackgroundStyles { get; }

    public ISukiToastManager ToastManager { get; }
    public ISukiDialogManager DialogManager { get; }

    [ObservableProperty] private ThemeVariant _baseTheme;
    [ObservableProperty] private bool _animationsEnabled;
    [ObservableProperty] private MainPageBase _activePage;
    [ObservableProperty] private SukiBackgroundStyle _backgroundStyle = SukiBackgroundStyle.Gradient;
    [ObservableProperty] private string _customShaderFile;
    [ObservableProperty] private bool _transitionsEnabled;
    [ObservableProperty] private double _transitionTime;

    [ObservableProperty]
    private string _status, _codeName, _bLStatus, _vABStatus;
    private readonly SukiTheme _theme;
    private readonly SettingsViewModel _theming;

    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }
    public MainViewModel(IEnumerable<MainPageBase> demoPages, PageNavigationService nav, ISukiToastManager toastManager, ISukiDialogManager dialogManager)
    {
        ToastManager = toastManager;
        DialogManager = dialogManager;
        Status = "--"; CodeName = "--"; BLStatus = "--"; VABStatus = "--";
        DemoPages = new AvaloniaList<MainPageBase>(demoPages.OrderBy(x => x.Index).ThenBy(x => x.DisplayName));
        _theming = (SettingsViewModel)DemoPages.First(x => x is SettingsViewModel);
        _theming.BackgroundStyleChanged += style => BackgroundStyle = style;
        _theming.BackgroundAnimationsChanged += enabled => AnimationsEnabled = enabled;
        _theming.CustomBackgroundStyleChanged += shader => CustomShaderFile = shader;
        _theming.BackgroundTransitionsChanged += enabled => TransitionsEnabled = enabled;
        _dialogManager = dialogManager;
        BackgroundStyles = new AvaloniaList<SukiBackgroundStyle>(Enum.GetValues<SukiBackgroundStyle>());
        _theme = SukiTheme.GetInstance();
        nav.NavigationRequested += t =>
        {
            MainPageBase page = DemoPages.FirstOrDefault(x => x.GetType() == t);
            if (page is null || ActivePage?.GetType() == t)
            {
                return;
            }

            ActivePage = page;
        };
        Themes = _theme.ColorThemes;
        BaseTheme = _theme.ActiveBaseTheme;
        _theme.OnBaseThemeChanged += async variant =>
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
        _theme.OnColorThemeChanged += async theme =>
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
    private static void OpenURL(string url)
    {
        UrlUtilities.OpenURL(url);
    }
}