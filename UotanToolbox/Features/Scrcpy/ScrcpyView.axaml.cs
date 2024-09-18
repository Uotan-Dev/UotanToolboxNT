using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SukiUI.Dialogs;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Scrcpy;

public partial class ScrcpyView : UserControl
{
    ISukiDialogManager dialogManager;
    static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);

    public ScrcpyView() => InitializeComponent();

    async void OpenFolderButton_Clicked(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Open Folder",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            if (FileHelper.TestPermission(StringHelper.FilePath(files[0].Path.ToString())))
            {
                RecordFolder.Text = StringHelper.FilePath(files[0].Path.ToString());
            }
            else
            {
                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_FolderNoPermission")).Dismiss().ByClickingBackground().TryShow();
            }
        }
    }
}