using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Others;

public partial class OthersViewModel : MainPageBase
{
    [ObservableProperty][Range(1d, 4d)] float _fontScale = 0, _windowScale = 0, _transitionScale = 0, _animationLast = 0;

    static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);

    public OthersViewModel() : base(GetTranslation("Sidebar_Others"), MaterialIconKind.WrenchCogOutline, -350)
    {
    }
}