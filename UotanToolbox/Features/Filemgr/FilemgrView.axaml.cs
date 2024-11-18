using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using SukiUI.Dialogs;
using UotanToolbox.Common;


namespace UotanToolbox.Features.Filemgr;

public partial class FilemgrView : UserControl
{
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }
    public AvaloniaList<Node> TreeViewContent { get; } = [];
    public FilemgrView()
    {
        InitializeComponent();
        FileView.ItemsSource = new Project[10];
    }

    private async void Open(object sender, RoutedEventArgs args)
    {
        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
    }
}