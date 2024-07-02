using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SukiUI.Controls;

namespace UotanToolbox.Features.Components
{
    public partial class ErrorDialog : UserControl
    {
        public ErrorDialog(string DialogContent)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var dialogueTextBlock = this.FindControl<TextBlock>("DialogContent");
                if (dialogueTextBlock != null)
                {
                    dialogueTextBlock.Text = DialogContent;
                }
            });
            InitializeComponent();
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            SukiHost.CloseDialog();
        }
    }
}