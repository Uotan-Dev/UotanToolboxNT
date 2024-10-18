using Material.Icons;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Customizedflash;

public partial class CustomizedflashViewModel : MainPageBase
{
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public CustomizedflashViewModel() : base(GetTranslation("Sidebar_Customizedflash"), MaterialIconKind.PencilPlusOutline, -500)
    {
    }
}