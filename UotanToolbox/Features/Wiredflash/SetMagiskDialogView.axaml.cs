using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Wiredflash;

public partial class SetMagiskDialogView : UserControl
{
    public AvaloniaList<string> BootImages = ["boot", "init_boot", "vendor_boot"];
    public SetMagiskDialogView()
    {
        InitializeComponent();
        MagiskFile.Text = Global.MagiskAPKPath;
        BootImagesList.ItemsSource = BootImages;
    }

    public static FilePickerFileType Zip { get; } = new("Zip")
    {
        Patterns = new[] { "*.zip", "*.apk" },
        AppleUniformTypeIdentifiers = new[] { "*.zip", "*.apk" }
    };

    private async void OpenMagiskFile(object sender, RoutedEventArgs args)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            FileTypeFilter = new[] { Zip },
            Title = "Open File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            MagiskFile.Text = files[0].TryGetLocalPath();
        }
    }

    private async void Confirm(object sender, RoutedEventArgs args)
    {
        Global.MagiskAPKPath = MagiskFile.Text!;
        Global.SetBoot = BootImagesList.SelectedItem!.ToString()!;
        Global.MainDialogManager.DismissDialog();
    }

    private async void Cancel(object sender, RoutedEventArgs args)
    {
        Global.MainDialogManager.DismissDialog();
    }
}