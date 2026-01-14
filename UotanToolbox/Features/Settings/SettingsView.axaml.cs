using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using SkiaSharp;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Settings;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private static FilePickerFileType CsvPicker { get; } = new("CSV File")
    {
        Patterns = new[] { "*.csv" },
        AppleUniformTypeIdentifiers = new[] { "*.csv" }
    };

    private async void OpenCSVFile(object sender, RoutedEventArgs args)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false,
            FileTypeFilter = new[] { CsvPicker }
        });
        if (files.Count >= 1)
        {
            CSVPath.Text = files[0].TryGetLocalPath();
            Global.BootPatchPath = CSVPath.Text;
        }
    }
}