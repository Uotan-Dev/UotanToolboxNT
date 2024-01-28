using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;

namespace SukiUI.Demo.Features.ControlsLibrary
{
    public partial class MiscViewModel() : DemoPageBase("Miscellaneous", MaterialIconKind.DotsHorizontalCircle)
    {
        [ObservableProperty] private bool _isBusy;

        [RelayCommand]
        private async Task ToggleBusy()
        {
            IsBusy = true;
            await Task.Delay(3000);
            IsBusy = false;
        }
    }
}