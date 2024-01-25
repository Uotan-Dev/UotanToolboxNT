using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SukiUI.Controls;

namespace SukiUI.Demo.Features.ControlsLibrary
{
    public partial class ColorsView : UserControl
    {
        public ColorsView()
        {
            InitializeComponent();
        }

        private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (VisualRoot is not Window win) return;
            if (sender is not Control control) return;
            if (control.DataContext is not ColorViewModel vm) return;
            win.Clipboard!.SetTextAsync(vm.Name);
            SukiHost.ShowToast("Copied To Clipboard", $"Copied the name of {vm.Name} to clipboard.");
        }
    }
}