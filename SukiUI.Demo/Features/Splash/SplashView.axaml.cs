using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace SukiUI.Demo.Features.Splash;

public partial class SplashView : UserControl
{
    public SplashView()
    {
        InitializeComponent();
    }

    private async void CopyButton_OnClick(object? sender, RoutedEventArgs args)
    {
        Button button  = (Button)sender!;
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        var dataObject = new DataObject();
        dataObject.Set(DataFormats.Text, button.Content.ToString());
        await clipboard.SetDataObjectAsync(dataObject);
    }
}