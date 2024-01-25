using Avalonia.Controls;
using Avalonia.Interactivity;
using SukiUI.Controls;

namespace SukiUI.Demo.Features.ControlsLibrary.Dialogs
{
    public partial class StandardDialog : UserControl
    {
        public StandardDialog()
        {
            InitializeComponent();
        }

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            SukiHost.CloseDialog();
        }
    }
}