using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Scrcpy;

public partial class ScrcpyView : UserControl
{
    public ISukiDialogManager dialogManager;
    public ISukiToastManager toastManager;
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public ScrcpyView(ISukiDialogManager sukiDialogManager, ISukiToastManager sukiToastManager)
    {
        dialogManager = sukiDialogManager;
        toastManager = sukiToastManager;
        InitializeComponent();
    }

    private async void OpenFolderButton_Clicked(object sender, RoutedEventArgs args)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        System.Collections.Generic.IReadOnlyList<IStorageFolder> files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
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