using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.IO;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Customizedflash;

public partial class FlashVbmetaDialogView : UserControl
{
    private CustomizedflashView? _owner;
    public AvaloniaList<string> Command = ["--disable-verity --disable-verification", "--disable-verity", "--disable-verification"];

    public FlashVbmetaDialogView()
    {
        InitializeComponent();
        CommandList.ItemsSource = Command;
    }

    public FlashVbmetaDialogView(CustomizedflashView owner) : this()
    {
        _owner = owner;
    }

    private static FilePickerFileType VbmetaPicker { get; } = new("Vbmeta File")
    {
        Patterns = new[] { "*vbmeta*.img", "*VBMETA*.img" },
        AppleUniformTypeIdentifiers = new[] { "*vbmeta*.img", "*VBMETA*.img" }
    };

    private async void SelectVbmeta(object sender, RoutedEventArgs args)
    {
        string command = CommandList.SelectedItem.ToString();
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = true,
            FileTypeFilter = new[] { VbmetaPicker, FilePickerFileTypes.TextPlain }
        });
        if (files.Count >= 1)
        {
            Global.checkdevice = false;
            try
            {
                for (int i = 0; i < files.Count; i++)
                {
                    await _owner.Fastboot($"-s {Global.thisdevice} {command} flash {Path.GetFileNameWithoutExtension(files[i].Name)} \"{files[i].TryGetLocalPath()}\"");
                }
            }
            finally
            {
                Global.checkdevice = true;
            }
        }
        Global.MainDialogManager.DismissDialog();
    }

    private async void Continue(object sender, RoutedEventArgs args)
    {
        _owner.CustomizedflashLog.Text = "";
        Global.checkdevice = false;
        try
        {
            await _owner.Fastboot($"-s {Global.thisdevice} --disable-verity --disable-verification flash vbmeta \"{Path.Combine(Global.runpath, "Image", "vbmeta.img")}\"");
            await _owner.Fastboot($"-s {Global.thisdevice} --disable-verity --disable-verification flash vbmeta_system \"{Path.Combine(Global.runpath, "Image", "vbmeta.img")}\"");
            await _owner.Fastboot($"-s {Global.thisdevice} --disable-verity --disable-verification flash vbmeta_vendor \"{Path.Combine(Global.runpath, "Image", "vbmeta.img")}\"");
        }
        finally
        {
            Global.checkdevice = true;
        }
        Global.MainDialogManager.DismissDialog();
    }

    private async void Cancel(object sender, RoutedEventArgs args)
    {
        Global.MainDialogManager.DismissDialog();
    }
}