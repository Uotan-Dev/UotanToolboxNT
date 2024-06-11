using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Controls;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Features.Customizedflash;

public partial class CustomizedflashView : UserControl
{
    public CustomizedflashView()
    {
        InitializeComponent();
    }

    private async void OpenFastbootdFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            FileTypeFilter = new[] { FastbootdTXT },
            Title = "Open TXT File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            FastbootdFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    public static FilePickerFileType FastbootdTXT { get; } = new("FastbootdTXT")
    {
        Patterns = new[] { "*fastbootd.txt" },
        AppleUniformTypeIdentifiers = new[] { "*fastbootd.txt" }
    };
}