using Material.Icons;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Advanced;

public partial class AdvancedViewModel : MainPageBase
{
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public AdvancedViewModel() : base(GetTranslation("Sidebar_Advanced"), MaterialIconKind.WrenchCogOutline, -300)
    {
    }
}