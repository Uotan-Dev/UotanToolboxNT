using Material.Icons;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Customizedflash;

public partial class CustomizedflashViewModel : MainPageBase
{
    static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);

    public CustomizedflashViewModel() : base(GetTranslation("Sidebar_Customizedflash"), MaterialIconKind.PencilPlusOutline, -500)
    {
    }
}