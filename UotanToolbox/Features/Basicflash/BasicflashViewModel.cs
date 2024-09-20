using Material.Icons;
using SukiUI.Dialogs;
using UotanToolbox.Common;


namespace UotanToolbox.Features.Basicflash;

public partial class BasicflashViewModel : MainPageBase
{

    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public BasicflashViewModel(ISukiDialogManager dialogManager) : base(GetTranslation("Sidebar_Basicflash"), MaterialIconKind.CableData, -1000)
    {
    }
}