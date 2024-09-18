using System;
using System.Net.Http;
using System.Threading.Tasks;
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
    [ObservableProperty] string _selectedLanguageList;

    readonly SukiTheme _theme = SukiTheme.GetInstance();

    [ObservableProperty] bool _isLightTheme;
    [ObservableProperty] SukiBackgroundStyle _backgroundStyle;
    [ObservableProperty] bool _backgroundAnimations;
    [ObservableProperty] bool _backgroundTransitions;
    [ObservableProperty] string _currentVersion = Global.currentVersion;
    [ObservableProperty] string _binVersion;
    ISukiDialogManager dialogManager;
    ISukiToastManager toastManager;
    string _customShader;

    static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);

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

        _ = CheckBinVersion();
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

    public async Task CheckBinVersion() => BinVersion = await StringHelper.GetBinVersion();

    partial void OnIsLightThemeChanged(bool value) =>
        _theme.ChangeBaseTheme(value ? ThemeVariant.Light : ThemeVariant.Dark);

    partial void OnSelectedLanguageListChanged(string value)
    {
        if (value == GetTranslation("Settings_Default")) UotanToolbox.Settings.Default.Language = "";
        else if (value == "English") UotanToolbox.Settings.Default.Language = "en-US";
        else if (value == "简体中文") UotanToolbox.Settings.Default.Language = "zh-CN";

        UotanToolbox.Settings.Default.Save();
        // _ = toastManager.CreateToast().WithTitle($"{GetTranslation("Settings_LanguageHasBeenSet")}").WithContent(GetTranslation("Settings_RestartTheApplication")).OfType(NotificationType.Success).Dismiss().ByClicking().Dismiss().After(TimeSpan.FromSeconds(3)).Queue();
    }

    [RelayCommand]
    void SwitchToColorTheme(SukiColorTheme colorTheme)
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
    void TryCustomShader(string shaderType)
    {
        _customShader = _customShader == shaderType ? null : shaderType;
        CustomBackgroundStyleChanged?.Invoke(_customShader);
    }

    [RelayCommand]
    void OpenURL(string url) => UrlUtilities.OpenURL(url);

    [RelayCommand]
    async Task GetUpdate()
    {
        try
        {
            using var client = new HttpClient();
            var url = "https://toolbox.uotan.cn/api/list";
            var content = new StringContent("{}", System.Text.Encoding.UTF8);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var response = await client.PostAsync(url, content);
            _ = response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            var convertedBody = JsonConvert.DeserializeObject<dynamic>(responseBody);
            var vm = new SettingsViewModel();

            if (convertedBody.release_version != vm.CurrentVersion)
            {
                var result = false;

                dialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Settings_NewVersionAvailable"))
                    .WithContent((string)JsonConvert.SerializeObject(convertedBody.release_content))
                    .WithActionButton("Yes", _ => result = true, true)
                    .WithActionButton("No", _ => result = false, true)
                    .TryShow();

                if (result == true)
                {
                    UrlUtilities.OpenURL("https://toolbox.uotan.cn");
                }
            }
            else if (convertedBody.beta_version != vm.CurrentVersion)
            {
                var result = false;

                dialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Settings_NewVersionAvailable"))
                    .WithContent((string)JsonConvert.SerializeObject(convertedBody.beta_content))
                    .WithActionButton("Yes", _ => result = true, true)
                    .WithActionButton("No", _ => result = false, true)
                    .TryShow();

                if (result == true)
                {
                    UrlUtilities.OpenURL("https://toolbox.uotan.cn");
                }
            }
            else
            {
                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Settings_UpToDate")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        catch (HttpRequestException e)
        {
            _ = dialogManager.CreateDialog().WithTitle("Error").WithActionButton("知道了", _ => { }, true).WithContent(e.Message).TryShow();
        }
    }
}