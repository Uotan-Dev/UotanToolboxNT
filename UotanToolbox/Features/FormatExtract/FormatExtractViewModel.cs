using Material.Icons;
using UotanToolbox.Common;

namespace UotanToolbox.Features.FormatExtract;

public partial class FormatExtractViewModel : MainPageBase
{
    static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);

    public FormatExtractViewModel() : base(GetTranslation("Sidebar_FormatExtract"), MaterialIconKind.AccountHardHatOutline, -400)
    {
    }
}