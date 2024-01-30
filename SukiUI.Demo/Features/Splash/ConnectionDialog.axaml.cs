using Avalonia.Controls;
using Avalonia.Threading;

namespace SukiUI.Demo.Features.Splash
{
    public partial class ConnectionDialog : UserControl
    {
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
            InitializeComponent();
        }
    }
}
