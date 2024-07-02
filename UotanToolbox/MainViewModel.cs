using Avalonia.Collections;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI;
using SukiUI.Controls;
using SukiUI.Enums;
using SukiUI.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Features;
using UotanToolbox.Features.Settings;
using UotanToolbox.Services;
using UotanToolbox.Utilities;

namespace UotanToolbox;

public partial class MainViewModel : ObservableObject
{
    public IAvaloniaReadOnlyList<MainPageBase> DemoPages { get; }

    public IAvaloniaReadOnlyList<SukiColorTheme> Themes { get; }

    public IAvaloniaReadOnlyList<SukiBackgroundStyle> BackgroundStyles { get; }

    [ObservableProperty] private ThemeVariant _baseTheme;
    [ObservableProperty] private bool _animationsEnabled;
    [ObservableProperty] private bool _windowLocked = true;
    [ObservableProperty] private MainPageBase _activePage;
    [ObservableProperty] private SukiBackgroundStyle _backgroundStyle = SukiBackgroundStyle.Gradient;
    [ObservableProperty] private string _customShaderFile;
    [ObservableProperty] private bool _transitionsEnabled;
    [ObservableProperty] private double _transitionTime;

    [ObservableProperty]
    private string _status, _codeName, _bLStatus, _vABStatus;

    private readonly SukiTheme _theme;
    private readonly SettingsViewModel _theming;

    private static readonly ResourceManager resMgr = new ResourceManager("UotanToolbox.Assets.Resources", typeof(App).Assembly);
    private static string GetTranslation(string key) => resMgr.GetString(key, CultureInfo.CurrentCulture) ?? "?????";

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
            if (page is null || ActivePage?.GetType() == t) return;
            ActivePage = page;
        };
        Themes = _theme.ColorThemes;
        BaseTheme = _theme.ActiveBaseTheme;
        _theme.OnBaseThemeChanged += async variant =>
        {
            BaseTheme = variant;
            await SukiHost.ShowToast($"{GetTranslation("MainView_SuccessfullyChangedTheme")}", $"{GetTranslation("MainView_ChangedThemeTo")} {variant}");
        };
        _theme.OnColorThemeChanged += async theme =>
            await SukiHost.ShowToast($"{GetTranslation("MainView_SuccessfullyChangedColor")}", $"{GetTranslation("MainView_ChangedColorTo")} {theme.DisplayName}.");
        GlobalData.MainViewModelInstance = this;
    }

    [RelayCommand]
    private Task ToggleAnimations()
    {
        AnimationsEnabled = !AnimationsEnabled;
        var title = AnimationsEnabled ? $"{GetTranslation("MainView_AnimationEnabled")}" : $"{GetTranslation("MainView_AnimationDisabled")}";
        var content = AnimationsEnabled
            ? $"{GetTranslation("MainView_BackgroundAnimationsEnabled")}"
            : $"{GetTranslation("MainView_BackgroundAnimationsDisabled")}";
        return SukiHost.ShowToast(title, content);
    }

    [RelayCommand]
    private void ToggleBaseTheme() =>
        _theme.SwitchBaseTheme();

    public void ChangeTheme(SukiColorTheme theme) =>
        _theme.ChangeColorTheme(theme);

    [RelayCommand]
    private void OpenURL(string url) => UrlUtilities.OpenURL(url);
}