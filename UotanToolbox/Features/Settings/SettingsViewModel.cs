using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Newtonsoft.Json;
using SukiUI;
using SukiUI.Dialogs;
using SukiUI.Enums;
using SukiUI.Models;
using SukiUI.Toasts;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Utilities;

namespace UotanToolbox.Features.Settings;

public partial class SettingsViewModel : MainPageBase
{
    public Action<SukiBackgroundStyle> BackgroundStyleChanged { get; set; }
    public Action<bool> BackgroundAnimationsChanged { get; set; }
    public Action<bool> BackgroundTransitionsChanged { get; set; }
    public Action<string> CustomBackgroundStyleChanged { get; set; }

    public IAvaloniaReadOnlyList<SukiColorTheme> AvailableColors { get; }
    public IAvaloniaReadOnlyList<SukiBackgroundStyle> AvailableBackgroundStyles { get; }
    public IAvaloniaReadOnlyList<string> CustomShaders { get; } = new AvaloniaList<string> { "Space", "Weird", "Clouds" };

    public AvaloniaList<string> LanguageList { get; } = [GetTranslation("Settings_Default"), "English", "简体中文"];
    [ObservableProperty] private string _selectedLanguageList;

    private readonly SukiTheme _theme = SukiTheme.GetInstance();

    [ObservableProperty] private bool _isLightTheme;
    [ObservableProperty] private SukiBackgroundStyle _backgroundStyle;
    [ObservableProperty] private bool _backgroundAnimations;
    [ObservableProperty] private bool _backgroundTransitions;
    
    private string _customShader = null;

    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public SettingsViewModel() : base(GetTranslation("Sidebar_Settings"), MaterialIconKind.SettingsOutline, -200)
    {
        if (UotanToolbox.Settings.Default.Language is null or "")
        {
            SelectedLanguageList = GetTranslation("Settings_Default");
        }
        else if (UotanToolbox.Settings.Default.Language == "en-US")
        {
            SelectedLanguageList = "English";
        }
        else if (UotanToolbox.Settings.Default.Language == "zh-CN")
        {
            SelectedLanguageList = "简体中文";
        }

        AvailableBackgroundStyles = new AvaloniaList<SukiBackgroundStyle>(Enum.GetValues<SukiBackgroundStyle>());
        AvailableColors = _theme.ColorThemes;

        IsLightTheme = _theme.ActiveBaseTheme == ThemeVariant.Light;
        _theme.OnBaseThemeChanged += variant =>
            IsLightTheme = variant == ThemeVariant.Light;
        if (Global.isLightThemeChanged == false)
        {
            IsLightTheme = UotanToolbox.Settings.Default.IsLightTheme;
            Global.isLightThemeChanged = true;
        }
        _theme.OnColorThemeChanged += theme =>
        {
            // TODO: Implement a way to make this correct, might need to wrap the thing in a VM, this isn't ideal.
        };
    }

    partial void OnIsLightThemeChanged(bool value) =>
        _theme.ChangeBaseTheme(value ? ThemeVariant.Light : ThemeVariant.Dark);

    partial void OnSelectedLanguageListChanged(string value)
    {
        string OldLanguage = null;
        if (value == GetTranslation("Settings_Default")) OldLanguage = "";
        else if (value == "English") OldLanguage = "en-US";
        else if (value == "简体中文") OldLanguage = "zh-CN";
        if (OldLanguage != UotanToolbox.Settings.Default.Language)
        {
            if (value == GetTranslation("Settings_Default")) UotanToolbox.Settings.Default.Language = "";
            else if (value == "English") UotanToolbox.Settings.Default.Language = "en-US";
            else if (value == "简体中文") UotanToolbox.Settings.Default.Language = "zh-CN";
            UotanToolbox.Settings.Default.Save();
            Global.MainToastManager.CreateToast().WithTitle($"{GetTranslation("Settings_LanguageHasBeenSet")}").WithContent(GetTranslation("Settings_RestartTheApplication")).OfType(NotificationType.Success).Dismiss().ByClicking().Dismiss().After(TimeSpan.FromSeconds(3)).Queue();
        }
    }

    [RelayCommand]
    private void SwitchToColorTheme(SukiColorTheme colorTheme)
    {
        _theme.ChangeColorTheme(colorTheme);
    }

    partial void OnBackgroundStyleChanged(SukiBackgroundStyle value) =>
        BackgroundStyleChanged?.Invoke(value);

    partial void OnBackgroundAnimationsChanged(bool value) =>
        BackgroundAnimationsChanged?.Invoke(value);

    partial void OnBackgroundTransitionsChanged(bool value) =>
        BackgroundTransitionsChanged?.Invoke(value);

    [RelayCommand]
    private void TryCustomShader(string shaderType)
    {
        _customShader = _customShader == shaderType ? null : shaderType;
        CustomBackgroundStyleChanged?.Invoke(_customShader);
    }
}