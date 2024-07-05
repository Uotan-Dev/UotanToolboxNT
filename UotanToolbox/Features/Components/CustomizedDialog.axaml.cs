using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SukiUI.Controls;

namespace UotanToolbox.Features.Components
{
    public partial class CustomizedDialog : UserControl
    {
        public bool Result { get; private set; }

        public CustomizedDialog() { }
        public CustomizedDialog(string DialogTitle, string DialogContent)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var titleTextBlock = this.FindControl<TextBlock>("DialogTitle");
                if (titleTextBlock != null)
                {
                    titleTextBlock.Text = DialogTitle;
                }

                var dialogueTextBlock = this.FindControl<TextBlock>("DialogContent");
                if (dialogueTextBlock != null)
                {
                    dialogueTextBlock.Text = DialogContent;
                }
            });
            Result = false;
            InitializeComponent();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            Result = false;
            SukiHost.CloseDialog();
        }

        private void Confirm_OnClick(object sender, RoutedEventArgs e)
        {
            Result = true;
            SukiHost.CloseDialog();
        }
    }
}
