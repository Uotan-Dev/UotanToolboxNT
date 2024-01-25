using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Controls;
using SukiUI.Demo.Features.ControlsLibrary.Dialogs;
using SukiUI.Demo.Utilities;

namespace SukiUI.Demo.Features.ControlsLibrary
{
    public partial class DialogsViewModel() : DemoPageBase("Dialogs", MaterialIconKind.Forum)
    {
        [RelayCommand]
        public void OpenStandardDialog() =>
            SukiHost.ShowDialog(new StandardDialog());

        [RelayCommand]
        public void OpenBackgroundCloseDialog() =>
            SukiHost.ShowDialog(new BackgroundCloseDialog(), allowBackgroundClose: true);
        
        [RelayCommand]
        public void OpenViewModelDialog() =>
            SukiHost.ShowDialog(new VmDialogViewModel(), allowBackgroundClose: true);

        [RelayCommand]
        public void OpenSourceURL() =>
            UrlUtilities.OpenURL(
                $"https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/{nameof(DialogsViewModel)}.cs");
    }
}