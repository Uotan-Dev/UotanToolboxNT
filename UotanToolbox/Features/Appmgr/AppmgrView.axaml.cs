using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Appmgr;

public partial class AppmgrView : UserControl
{
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public AppmgrView()
    {
        InitializeComponent();
    }

    private async void UninstallButton_Click(object sender, RoutedEventArgs e)
    {
        Button button = (Button)sender;
        ApplicationInfo applicationInfo = (ApplicationInfo)button.DataContext;
        await UninstallApplication(applicationInfo.Name);
    }

    private async Task UninstallApplication(string packageName)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            bool result = false;
            _ = Global.MainDialogManager.CreateDialog()
                         .WithTitle("Warn")
                         .WithContent(GetTranslation("Appmgr_ConfirmDeleteApp"))
                         .WithActionButton("Yes", _ => result = true, true)
                         .WithActionButton("No", _ => result = false, true)
                         .TryShow();
            if (result == true)
            {
                _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm uninstall -k --user 0 {packageName}");
            }

            AppmgrViewModel newAppmgr = new AppmgrViewModel();
            _ = newAppmgr.Connect();
        });
    }


    private static FilePickerFileType ApkPicker { get; } = new("APK File")
    {
        Patterns = new[] { "*.apk" }
    };

    private async void OpenApkFile(object sender, RoutedEventArgs args)
    {
        ApkFile.Text = null;
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = true,
            FileTypeFilter = new[] { ApkPicker, FilePickerFileTypes.TextPlain }
        });
        if (files.Count >= 1)
        {
            for (int i = 0; i < files.Count; i++)
            {
                ApkFile.Text = ApkFile.Text + StringHelper.FilePath(files[i].Path.ToString()) + "|||";
            }
        }
    }
}