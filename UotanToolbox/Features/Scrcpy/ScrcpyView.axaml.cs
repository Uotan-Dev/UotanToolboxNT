using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SukiUI.Controls;
using UotanToolbox.Common;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Features.Scrcpy;

public partial class ScrcpyView : UserControl
{
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public ScrcpyView()
    {
        InitializeComponent();
    }

    private async void OpenFolderButton_Clicked(object sender, RoutedEventArgs args)
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
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_FolderNoPermission")), allowBackgroundClose: true);
            }
        }
    }
}