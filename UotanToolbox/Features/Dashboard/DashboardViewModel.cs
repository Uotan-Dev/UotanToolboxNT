using Material.Icons;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Dashboard;

public partial class DashboardViewModel : MainPageBase
{
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public DashboardViewModel() : base(GetTranslation("Sidebar_Basicflash"), MaterialIconKind.CableData, -1000)
    {
    }
}