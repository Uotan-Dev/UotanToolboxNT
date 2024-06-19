using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SukiUI.Controls;

namespace UotanToolbox.Features.Components
{
    public partial class PureDialog : UserControl
    {
        public bool Result { get; private set; }

        public PureDialog(string DialogContent)
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
    }
}
