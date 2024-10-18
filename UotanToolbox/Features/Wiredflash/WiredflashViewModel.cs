using Material.Icons;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Wiredflash;

public partial class WiredflashViewModel : MainPageBase
{
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public WiredflashViewModel() : base(GetTranslation("Sidebar_WiredFlash"), MaterialIconKind.JumpRope, -600)
    {
    }
}