using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace SukiUI.Demo.Features.ControlsLibrary
{
    public partial class CardsViewModel() : DemoPageBase("Cards", MaterialIconKind.Cards)
    {
        [ObservableProperty] private bool _isOpaque;
        [ObservableProperty] private bool _isInteractive;
    }
}