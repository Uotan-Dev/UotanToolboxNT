using Avalonia.Collections;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Newtonsoft.Json;
using SukiUI;
using SukiUI.Controls;
using SukiUI.Enums;
using SukiUI.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Features.Components;
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

    private readonly SukiTheme _theme = SukiTheme.GetInstance();

    [ObservableProperty] private bool _isLightTheme;
    [ObservableProperty] private SukiBackgroundStyle _backgroundStyle;
    [ObservableProperty] private bool _backgroundAnimations;
    [ObservableProperty] private bool _backgroundTransitions;
    [ObservableProperty] private string _currentVersion = Global.currentVersion;
    [ObservableProperty] private string _binVersion = null;

    private string _customShader = null;

    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public SettingsViewModel() : base(GetTranslation("Sidebar_Settings"), MaterialIconKind.SettingsOutline, int.MaxValue)
    {
        _ = CheckBinVersion();
        AvailableBackgroundStyles = new AvaloniaList<SukiBackgroundStyle>(Enum.GetValues<SukiBackgroundStyle>());
        AvailableColors = _theme.ColorThemes;
        IsLightTheme = _theme.ActiveBaseTheme == ThemeVariant.Light;
        _theme.OnBaseThemeChanged += variant =>
            IsLightTheme = variant == ThemeVariant.Light;
        _theme.OnColorThemeChanged += theme =>
        {
            // TODO: Implement a way to make this correct, might need to wrap the thing in a VM, this isn't ideal.
        };
    }

    public async Task CheckBinVersion()
    {
        BinVersion = await StringHelper.GetBinVersion();
    }

    partial void OnIsLightThemeChanged(bool value) =>
        _theme.ChangeBaseTheme(value ? ThemeVariant.Light : ThemeVariant.Dark);

    [RelayCommand]
    private void SwitchToColorTheme(SukiColorTheme colorTheme) =>
        _theme.ChangeColorTheme(colorTheme);

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

    [RelayCommand]
    private void OpenURL(string url) => UrlUtilities.OpenURL(url);

    [RelayCommand]
    private static async Task GetUpdate()
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                string url = "https://toolbox.uotan.cn/api/list";
                var content = new StringContent("{}", System.Text.Encoding.UTF8);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                HttpResponseMessage response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                dynamic convertedBody = JsonConvert.DeserializeObject<dynamic>(responseBody);
                SettingsViewModel vm = new SettingsViewModel();
                if (convertedBody.release_version != vm.CurrentVersion)
                {
                    var dialog = new CustomizedDialog(GetTranslation("Settings_NewVersionAvailable"), convertedBody.release_content);
                    await SukiHost.ShowDialogAsync(dialog);
                    if (dialog.Result == true) UrlUtilities.OpenURL("https://toolbox.uotan.cn");
                }
                else if (convertedBody.beta_version != vm.CurrentVersion)
                {
                    var dialog = new CustomizedDialog(GetTranslation("Settings_NewVersionAvailable"), convertedBody.beta_content);
                    await SukiHost.ShowDialogAsync(dialog);
                    if (dialog.Result == true) UrlUtilities.OpenURL("https://toolbox.uotan.cn");
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Settings_UpToDate")), allowBackgroundClose: true);
                }
            }
        }
        catch (HttpRequestException e)
        {
            SukiHost.ShowDialog(new ErrorDialog(e.Message));
        }
    }
}