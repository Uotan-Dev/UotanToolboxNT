using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Controls;
using SukiUI.Demo.Utilities;
using System;
using System.Threading.Tasks;

namespace SukiUI.Demo.Features.ControlsLibrary
{
    public partial class ToastsViewModel() : DemoPageBase("Toasts", MaterialIconKind.BellRing)
    {
        [RelayCommand]
        public void OpenSourceURL() =>
            UrlUtilities.OpenURL(
                $"https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/{nameof(ToastsViewModel)}.cs");

        [RelayCommand]
        public Task ShowSingleStandardToast() =>
            SukiHost.ShowToast("A Simple Toast", "This is the content of a toast.");

        [RelayCommand]
        public async Task ShowThreeStandardToasts()
        {
            for (var i = 1; i <= 3; i++)
                await SukiHost.ShowToast("A Simple Toast", $"This is toast number {i} of 3.");
        }

        [RelayCommand]
        public Task ShowToastWithCallback()
        {
            return SukiHost.ShowToast("Click This Toast", "Click this toast to open a dialog.", TimeSpan.FromSeconds(15),
                () => SukiHost.ShowDialog(
                    new TextBlock { Text = "You clicked the toast! - Click anywhere outside of this dialog to close." },
                    allowBackgroundClose: true));
        }
    }
}