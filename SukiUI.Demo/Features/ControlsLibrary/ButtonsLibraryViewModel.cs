using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using System.Threading.Tasks;

namespace SukiUI.Demo.Features.ControlsLibrary;

public partial class ButtonsLibraryViewModel() : DemoPageBase("Buttons", MaterialIconKind.CursorDefaultClick)
{
    [ObservableProperty] private bool _isBusy;

    [RelayCommand]
    public Task ButtonClicked()
    {
        if (IsBusy)
            return Task.CompletedTask;

        return Task.Run(async () =>
        {
            IsBusy = true;
            await Task.Delay(3000);
            IsBusy = false;
        });
    }
}