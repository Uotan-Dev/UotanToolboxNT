using Material.Icons;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using UotanToolbox.Common;

namespace UotanToolbox.Features.FormatExtract;

public partial class FormatExtractViewModel : MainPageBase
{
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public FormatExtractViewModel(ISukiDialogManager dialogManager, ISukiToastManager toastManager) : base(GetTranslation("Sidebar_FormatExtract"), MaterialIconKind.AccountHardHatOutline, -400)
    {
        Global.formatExtractView = new FormatExtractView(dialogManager, toastManager);
    }
}