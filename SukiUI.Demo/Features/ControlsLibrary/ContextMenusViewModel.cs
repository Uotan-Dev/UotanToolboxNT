using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Controls;

namespace SukiUI.Demo.Features.ControlsLibrary
{
    public partial class ContextMenusViewModel() : DemoPageBase("Context Menus", MaterialIconKind.Menu)
    {
        [RelayCommand]
        private void OptionClicked(bool withIcon)
        {
            SukiHost.ShowToast("Clicked Context Menu", withIcon ? "You clicked the option with the icon." : "You clicked the option without the icon.");
        }

        [RelayCommand]
        private void NestedOptionClicked()
        {
            SukiHost.ShowToast("Clicked Context Menu", "You clicked the nested option");
        }
    }
}