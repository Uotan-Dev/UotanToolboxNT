using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SukiUI.Controls;
using SukiUI.Enums;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Home;

public partial class HomeView : UserControl
{
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public HomeView()
    {
        /* Sample of using i18n resources
        ResourceManager resourceManager = new ResourceManager("UotanToolbox.Assets.Resources", typeof(App).Assembly);
        string welcomeMessage = resourceManager.GetString("GreetingText", CultureInfo.CurrentCulture);
        */

        InitializeComponent();
    }

    private async void CopyButton_OnClick(object sender, RoutedEventArgs args)
    {
        if (sender is Button button)
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            var dataObject = new DataObject();
            if (button.Content != null)
            {
                var text = button.Content.ToString();
                if (text != null)
                    dataObject.Set(DataFormats.Text, text);
            }
            if (clipboard != null)
                await clipboard.SetDataObjectAsync(dataObject);
            await SukiHost.ShowToast(GetTranslation("Home_Copy"), "o(*≧▽≦)ツ", NotificationType.Success);
        }
    }
}