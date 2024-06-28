using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SukiUI.Controls;

namespace UotanToolbox.Features.Components
{
    public partial class ConnectionDialog : UserControl
    {
        public bool Result { get; private set; }

        public ConnectionDialog(string DialogContent)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var dialogueTextBlock = this.FindControl<TextBlock>("DialogContent");
                if (dialogueTextBlock != null)
                {
                    dialogueTextBlock.Text = DialogContent;
                }
            });
            Result = false;
            InitializeComponent();
        }

        private void Cancel_OnClick(object? sender, RoutedEventArgs e)
        {
            Result = false;
            SukiHost.CloseDialog();
        }

        private void Confirm_OnClick(object? sender, RoutedEventArgs e)
        {
            Result = true;
            SukiHost.CloseDialog();
        }
    }
}
