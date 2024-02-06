using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Diagnostics;
using System.Globalization;
using System.Resources;

namespace SukiUI.Demo.Features.Splash;

public partial class SplashView : UserControl
{
    public SplashView()
    {
        /* Sample of using i18n resources
        ResourceManager resourceManager = new ResourceManager("SukiUI.Demo.Assets.Resources", typeof(App).Assembly);
        string welcomeMessage = resourceManager.GetString("GreetingText", CultureInfo.CurrentCulture);
        */

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