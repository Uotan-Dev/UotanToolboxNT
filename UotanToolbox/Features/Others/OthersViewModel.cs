using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using System.ComponentModel.DataAnnotations;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Others;

public partial class OthersViewModel : MainPageBase
{
    [ObservableProperty] private string _scrResolution = "1920x1280", _selectedSimpleContent;
    [ObservableProperty] private int _scrDPI = 520, _scrDP = 380;
    [ObservableProperty] private static AvaloniaList<string>? _simpleContent;

    [ObservableProperty][Range(1d, 4d)] private double _fontScale = 1, _windowScale = 1, _transitionScale = 1, _animationLast = 1;

    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public OthersViewModel() : base(GetTranslation("Sidebar_Others"), MaterialIconKind.WrenchCogOutline, -350)
    {
    }
}