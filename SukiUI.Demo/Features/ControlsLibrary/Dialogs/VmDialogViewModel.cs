using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
using SukiUI.Demo.Utilities;

namespace SukiUI.Demo.Features.ControlsLibrary.Dialogs
{
    public partial class VmDialogViewModel : ObservableObject
    {
        [RelayCommand]
        public void OpenViewLocatorSource() =>
            UrlUtilities.OpenURL("https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Common/ViewLocator.cs");

        [RelayCommand]
        public void CloseDialog() => SukiHost.CloseDialog();
    }
}