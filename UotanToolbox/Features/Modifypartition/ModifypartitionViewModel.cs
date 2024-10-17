using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System.Threading.Tasks;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Modifypartition;

public partial class ModifypartitionViewModel : MainPageBase
{
    [ObservableProperty] private bool _IsEnabled = true;
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public ModifypartitionViewModel(ISukiDialogManager dialogManager, ISukiToastManager toastManager) : base(GetTranslation("Sidebar_ModifyPartition"), MaterialIconKind.ChartPieOutline, -300)
    {
        Global.modifypartitionView = new ModifypartitionView(dialogManager, toastManager);
        async Task LoadMassage()
        {
            bool result = false;
            _ = dialogManager.CreateDialog()
                .WithTitle("Warn")
                .WithContent(GetTranslation("Modifypartition_Warn"))
                .WithActionButton("Yes", _ => result = true, true)
                .WithActionButton("No", _ => result = false, true)
                .TryShow();
            IsEnabled = result;
        }
    }

}