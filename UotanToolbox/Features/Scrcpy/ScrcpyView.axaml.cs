using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Scrcpy;

public partial class ScrcpyView : UserControl
{
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
            RecordFolder.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }
}