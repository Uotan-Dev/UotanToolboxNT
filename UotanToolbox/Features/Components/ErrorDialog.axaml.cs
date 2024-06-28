using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SukiUI.Controls;

namespace SukiUI.Demo.Features.ControlsLibrary.Dialogs;

public partial class StandardDialog : UserControl
{
    public StandardDialog(string DialogContent)
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

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        SukiHost.CloseDialog();
    }
}