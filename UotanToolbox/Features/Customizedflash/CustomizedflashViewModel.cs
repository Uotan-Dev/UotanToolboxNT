using Material.Icons;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Customizedflash;

public partial class CustomizedflashViewModel : MainPageBase
{
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public CustomizedflashViewModel() : base(GetTranslation("Sidebar_Customizedflash"), MaterialIconKind.PencilPlusOutline, -500, "自定义刷入预制分区刷入System_extsystem_extProductproductOdmodmVendor_bootvendorBootInit_boot自定义分区刷入禁用vbmeta切换槽位")
    {
    }
}