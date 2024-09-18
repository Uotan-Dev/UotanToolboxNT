using Material.Icons;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Wiredflash;

public partial class WiredflashViewModel : MainPageBase
{
    static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);

    public WiredflashViewModel() : base(GetTranslation("Sidebar_WiredFlash"), MaterialIconKind.JumpRope, -600)
    {
    }
}