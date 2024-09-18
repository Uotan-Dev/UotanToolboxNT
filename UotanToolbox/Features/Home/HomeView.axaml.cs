using System;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using SukiUI.Toasts;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Home;

public partial class HomeView : UserControl
{
    ISukiToastManager toastManager;
    static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);

    public HomeView() => InitializeComponent();

    async void CopyButton_OnClick(object sender, RoutedEventArgs args)
    {
        if (sender is Button button)
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            var dataObject = new DataObject();

            if (button.Content != null)
            {
                var text = button.Content.ToString();

                if (text != null)
                {
                    dataObject.Set(DataFormats.Text, text);
                }
            }

            if (clipboard != null)
            {
                await clipboard.SetDataObjectAsync(dataObject);
            }

            _ = toastManager.CreateToast()
    .WithTitle(GetTranslation("Home_Copy"))
    .WithContent("o(*≧▽≦)ツ")
    .OfType(NotificationType.Success)
    .Dismiss().ByClicking()
    .Dismiss().After(TimeSpan.FromSeconds(3))
    .Queue();
        }
    }
}