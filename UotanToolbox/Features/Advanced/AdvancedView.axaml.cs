using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Advanced;

public partial class AdvancedView : UserControl
{
    public AdvancedView()
    {
        InitializeComponent();
    }

    private async void OpenQcnFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open QCN File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            QcnFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    private async void OpenEmptyFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open SuperEmpty File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            SuperEmptyFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }
}