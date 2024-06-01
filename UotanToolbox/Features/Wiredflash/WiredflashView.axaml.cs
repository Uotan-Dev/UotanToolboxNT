using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Wiredflash;

public partial class WiredflashView : UserControl
{
    public WiredflashView()
    {
        InitializeComponent();
    }

    private async void OpenFastbootFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Fastboot File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            FastbootFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }
}