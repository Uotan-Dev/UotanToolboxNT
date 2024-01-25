using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Controls;

namespace SukiUI.Demo.Features.ControlsLibrary
{
    public partial class ContextMenusViewModel() : DemoPageBase("Context Menus", MaterialIconKind.Menu)
    {
        [RelayCommand]
        public void TestClicked(bool withIcon)
        {
            SukiHost.ShowToast("Clicked Context Menu", withIcon ? "You clicked the option with the icon." : "You clicked the option without the icon.");
        }
    }
}