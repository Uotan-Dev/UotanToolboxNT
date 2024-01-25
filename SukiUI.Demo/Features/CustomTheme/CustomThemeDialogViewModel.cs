using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
using SukiUI.Models;

namespace SukiUI.Demo.Features.CustomTheme;

public partial class CustomThemeDialogViewModel(SukiTheme theme) : ObservableObject
{
    [ObservableProperty] private string _displayName = "Pink";
    [ObservableProperty] private Color _primaryColor = Colors.DeepPink;
    [ObservableProperty] private Color _accentColor = Colors.Pink;

    private readonly SukiTheme _theme = theme;

    [RelayCommand]
    public void TryCreateTheme()
    {
        if (string.IsNullOrEmpty(DisplayName)) return;
        var theme = new SukiColorTheme(DisplayName, PrimaryColor, AccentColor);
        _theme.AddColorTheme(theme);
        _theme.ChangeColorTheme(theme);
        SukiHost.CloseDialog();
    }
}