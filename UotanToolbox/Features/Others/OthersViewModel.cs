using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Others;

public partial class OthersViewModel : MainPageBase
{
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public OthersViewModel() : base(GetTranslation("Sidebar_Others"), MaterialIconKind.WrenchCogOutline, -350)
    {
    }
}