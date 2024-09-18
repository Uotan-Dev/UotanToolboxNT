using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Dialogs;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Appmgr;

public partial class AppmgrView : UserControl
{
    ISukiDialogManager dialogManager;
    static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);

    public AppmgrView() => InitializeComponent();

    async void UninstallButton_Click(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var applicationInfo = (ApplicationInfo)button.DataContext;
        await UninstallApplication(applicationInfo.Name);
    }

    async Task UninstallApplication(string packageName)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var result = false;

            _ = dialogManager.CreateDialog()
                         .WithTitle("Warn")
                         .WithContent(GetTranslation("Appmgr_ConfirmDeleteApp"))
                         .WithActionButton("Yes", _ => result = true, true)
                         .WithActionButton("No", _ => result = false, true)
                         .TryShow();

            if (result == true)
            {
                _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm uninstall -k --user 0 {packageName}");
            }

            var newAppmgr = new AppmgrViewModel();
            _ = newAppmgr.Connect();
        });
    }

    static FilePickerFileType ApkPicker { get; } = new("APK File")
    {
        Patterns = new[] { "*.apk" }
    };

    async void OpenApkFile(object sender, RoutedEventArgs args)
    {
        ApkFile.Text = null;
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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