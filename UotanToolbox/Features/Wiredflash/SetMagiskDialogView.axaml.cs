using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SkiaSharp;
using SukiUI.Dialogs;
using System;
using UotanToolbox.Common;
using UotanToolbox.Common.PatchHelper;

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
        Patterns = new[] { "*.zip", "*.apk", "*.ko" },
        AppleUniformTypeIdentifiers = new[] { "*.zip", "*.apk", "*.ko" }
    };

    private async void OpenMagiskFile(object sender, RoutedEventArgs args)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this);
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
        Global.MagiskAPKPath = MagiskFile.Text;
        Global.SetBoot = BootImagesList.SelectedItem.ToString();
        Global.MainDialogManager.DismissDialog();
    }

    private async void Cancel(object sender, RoutedEventArgs args)
    {
        Global.MainDialogManager.DismissDialog();
    }
}