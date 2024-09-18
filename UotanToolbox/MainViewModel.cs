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
    [ObservableProperty] bool _windowLocked;

    public IAvaloniaReadOnlyList<MainPageBase> DemoPages { get; }

    public IAvaloniaReadOnlyList<SukiColorTheme> Themes { get; }

    public IAvaloniaReadOnlyList<SukiBackgroundStyle> BackgroundStyles { get; }

    [ObservableProperty] ThemeVariant _baseTheme;
    [ObservableProperty] bool _animationsEnabled;
    [ObservableProperty] MainPageBase _activePage;
    [ObservableProperty] SukiBackgroundStyle _backgroundStyle = SukiBackgroundStyle.Gradient;
    [ObservableProperty] string _customShaderFile;
    [ObservableProperty] bool _transitionsEnabled;
    [ObservableProperty] double _transitionTime;

    [ObservableProperty]
    string _status, _codeName, _bLStatus, _vABStatus;
    ISukiToastManager toastManager;
    readonly SukiTheme _theme;
    readonly SettingsViewModel _theming;

    static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);

    public MainViewModel(IEnumerable<MainPageBase> demoPages, PageNavigationService nav)
    {
        Status = "--"; CodeName = "--"; BLStatus = "--"; VABStatus = "--";
        DemoPages = new AvaloniaList<MainPageBase>(demoPages.OrderBy(x => x.Index).ThenBy(x => x.DisplayName));
        _theming = (SettingsViewModel)DemoPages.First(x => x is SettingsViewModel);
        _theming.BackgroundStyleChanged += style => BackgroundStyle = style;
        _theming.BackgroundAnimationsChanged += enabled => AnimationsEnabled = enabled;
        _theming.CustomBackgroundStyleChanged += shader => CustomShaderFile = shader;
        _theming.BackgroundTransitionsChanged += enabled => TransitionsEnabled = enabled;

        BackgroundStyles = new AvaloniaList<SukiBackgroundStyle>(Enum.GetValues<SukiBackgroundStyle>());
        _theme = SukiTheme.GetInstance();

        nav.NavigationRequested += t =>
        {
            var page = DemoPages.FirstOrDefault(x => x.GetType() == t);

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
    Task ToggleAnimations()
    {
        AnimationsEnabled = !AnimationsEnabled;
        var title = AnimationsEnabled ? $"{GetTranslation("MainView_AnimationEnabled")}" : $"{GetTranslation("MainView_AnimationDisabled")}";

        var content = AnimationsEnabled
            ? $"{GetTranslation("MainView_BackgroundAnimationsEnabled")}"
            : $"{GetTranslation("MainView_BackgroundAnimationsDisabled")}";

        return (Task)toastManager.CreateToast()
.WithTitle(title)
.WithContent(content)
.OfType(NotificationType.Success)
.Dismiss().ByClicking()
.Dismiss().After(TimeSpan.FromSeconds(3))
.Queue();
    }

    [RelayCommand]
    void ToggleBaseTheme() => _theme.SwitchBaseTheme();

    public void ChangeTheme(SukiColorTheme theme) => _theme.ChangeColorTheme(theme);

    [RelayCommand]
    static void OpenURL(string url) => UrlUtilities.OpenURL(url);
}