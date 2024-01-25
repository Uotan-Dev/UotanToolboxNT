using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace SukiUI.Demo.Features.ControlsLibrary
{
    public partial class PropertyGridViewModel() : DemoPageBase("PropertyGrid", MaterialIconKind.Grid)
    {
        [ObservableProperty] private FormViewModel? _form = new FormViewModel();
    }
}