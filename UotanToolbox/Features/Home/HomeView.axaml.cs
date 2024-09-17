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
    private ISukiToastManager toastManager;
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public HomeView()
    {
        InitializeComponent();
    }

    private async void CopyButton_OnClick(object sender, RoutedEventArgs args)
    {
        if (sender is Button button)
        {
            Avalonia.Input.Platform.IClipboard clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            DataObject dataObject = new DataObject();
            if (button.Content != null)
            {
                string text = button.Content.ToString();
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