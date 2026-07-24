using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Wiredflash;

public partial class SetVbmetaDialogView : UserControl
{
    public AvaloniaList<string> Command = ["--disable-verity --disable-verification", "--disable-verity", "--disable-verification"];

    public SetVbmetaDialogView()
    {
        InitializeComponent();
        CommandList.ItemsSource = Command;
    }

    private async void Confirm(object sender, RoutedEventArgs args)
    {
        Global.VbmetaCommand = CommandList.SelectedItem!.ToString()!;
        Global.MainDialogManager.DismissDialog();
    }

    private async void Cancel(object sender, RoutedEventArgs args)
    {
        Global.MainDialogManager.DismissDialog();
    }
}