using Avalonia.Collections;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
using UotanToolbox.Common;
using UotanToolbox.Features;
using UotanToolbox.Services;
using UotanToolbox.Utilities;
using SukiUI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SukiUI;

namespace UotanToolbox;

public partial class MainViewModel : ObservableObject
{
    public IAvaloniaReadOnlyList<MainPageBase> DemoPages { get; }

    public IAvaloniaReadOnlyList<SukiColorTheme> Themes { get; }

    [ObservableProperty] private ThemeVariant _baseTheme;
    [ObservableProperty] private bool _animationsEnabled;
    [ObservableProperty] private MainPageBase? _activePage;

    [ObservableProperty]
    private string _status, _codeName, _bLStatus, _vABStatus;

    private readonly SukiTheme _theme;

    public MainViewModel(IEnumerable<MainPageBase> demoPages, PageNavigationService nav)
    {
        Status = "--"; CodeName = "--"; BLStatus = "--"; VABStatus = "--";
        DemoPages = new AvaloniaList<MainPageBase>(demoPages.OrderBy(x => x.Index).ThenBy(x => x.DisplayName));
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
            await SukiHost.ShowToast("Successfully Changed Theme", $"Changed Theme To {variant}");
        };
        _theme.OnColorThemeChanged += async theme =>
            await SukiHost.ShowToast("Successfully Changed Color", $"Changed Color To {theme.DisplayName}.");
        _theme.OnBackgroundAnimationChanged +=
            value => AnimationsEnabled = value;
        GlobalData.MainViewModelInstance = this;
    }

    [RelayCommand]
    private Task ToggleAnimations()
    {
        AnimationsEnabled = !AnimationsEnabled;
        var title = AnimationsEnabled ? "Animation Enabled" : "Animation Disabled";
        var content = AnimationsEnabled
            ? "Background animations are now enabled."
            : "Background animations are now disabled.";
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