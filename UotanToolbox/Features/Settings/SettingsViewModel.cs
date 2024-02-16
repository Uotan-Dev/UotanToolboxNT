using Avalonia.Collections;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI;
using SukiUI.Models;
using System.Globalization;

namespace UotanToolbox.Features.Settings;

public partial class SettingsViewModel : MainPageBase
{
    public IAvaloniaReadOnlyList<SukiColorTheme> AvailableColors { get; }

    public AvaloniaList<string> SimpleContent { get; } = new();

    private readonly SukiTheme _theme = SukiTheme.GetInstance();

    [ObservableProperty] private bool _isBackgroundAnimated;
    [ObservableProperty] private bool _isLightTheme;
    [ObservableProperty] private string _selectedSimpleContent = "";

    public SettingsViewModel() : base("设置", MaterialIconKind.SettingsOutline, -200)
    {
        AvailableColors = _theme.ColorThemes;
        IsLightTheme = _theme.ActiveBaseTheme == ThemeVariant.Light;
        IsBackgroundAnimated = _theme.IsBackgroundAnimated;
        _theme.OnBaseThemeChanged += variant =>
            IsLightTheme = variant == ThemeVariant.Light;
        _theme.OnColorThemeChanged += theme =>
        {
            // TODO: Implement a way to make the correct, might need to wrap the thing in a VM, this isn't ideal.
        };
        _theme.OnBackgroundAnimationChanged += value =>
            IsBackgroundAnimated = value;
        SimpleContent.AddRange(["Option1", "Option2"]);
        /*string cultureName = CultureInfo.CurrentCulture.Name;
        if (cultureName == "")
        {
            SelectedSimpleContent = "English";
        }

        this.WhenAnyValue(x => x.SelectedSimpleContent)
            .Subscribe((Action<string>)(option =>
            {
                if (option == "English")
                {
                    Assets.Resources.Culture = new CultureInfo("");
                    cultureSetter("en-US");
                    Debug.WriteLine("English");
                }
                else if (option == "简体中文")
                {
                    Assets.Resources.Culture = new CultureInfo("zh-CN");
                    cultureSetter("zh-CN");
                    Debug.WriteLine("简体中文");
                }
            }));
        */
    }

    private void cultureSetter(string language)
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(language);
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(language);
    }

    partial void OnIsLightThemeChanged(bool value) =>
        _theme.ChangeBaseTheme(value ? ThemeVariant.Light : ThemeVariant.Dark);

    partial void OnIsBackgroundAnimatedChanged(bool value) =>
        _theme.SetBackgroundAnimationsEnabled(value);

    [RelayCommand]
    public void SwitchToColorTheme(SukiColorTheme colorTheme) =>
        _theme.ChangeColorTheme(colorTheme);
}