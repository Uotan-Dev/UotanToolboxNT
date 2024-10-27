using Material.Icons;
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