using Material.Icons;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Modifypartition;

public partial class ModifypartitionViewModel : MainPageBase
{
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public ModifypartitionViewModel() : base(GetTranslation("Sidebar_ModifyPartition"), MaterialIconKind.ChartPieOutline, -300)
    {
    }
}