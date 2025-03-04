using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.Threading.Tasks;
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
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Global.MainDialogManager.CreateDialog()
                         .OfType(NotificationType.Warning)
                         .WithTitle(GetTranslation("Common_Warn"))
                         .WithContent(GetTranslation("Appmgr_ConfirmDeleteApp"))
                         .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                         {
                             MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
                             if (sukiViewModel.Status == GetTranslation("Home_Android"))
                             {
                                 await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm uninstall -k --user 0 {packageName}");
                             }
                             else if (sukiViewModel.Status == GetTranslation("Home_OpenHOS"))
                             {
                                 await CallExternalProgram.HDC($"-t {Global.thisdevice} app uninstall {packageName}");
                             }
                             else
                             {
                                 Global.MainDialogManager.CreateDialog()
                                       .OfType(NotificationType.Error)
                                       .WithTitle(GetTranslation("Common_Error"))
                                       .WithContent(GetTranslation("Common_OpenADBOrHDC"))
                                       .Dismiss().ByClickingBackground()
                                       .TryShow();
                             }
                         }, true)
                         .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                         .TryShow();

            AppmgrViewModel newAppmgr = new AppmgrViewModel();
            _ = newAppmgr.Connect();
        });
    }

    public async void CopyButton_OnClick(object sender, RoutedEventArgs args)
    {
        if (sender is Button button)
        {
            Avalonia.Input.Platform.IClipboard clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            DataObject dataObject = new DataObject();
            if (button.Content != null)
            {
                string text = button.Content.ToString();
                if (text != null)
                {
                    dataObject.Set(DataFormats.Text, text);
                }
            }
            if (clipboard != null)
            {
                await clipboard.SetDataObjectAsync(dataObject);
            }

            Global.MainToastManager.CreateSimpleInfoToast()
                .WithTitle(GetTranslation("Home_Copy"))
                .WithContent("o(*≧▽≦)ツ")
                .OfType(NotificationType.Success)
                .Dismiss().ByClicking()
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Queue();
        }
    }

    private static FilePickerFileType ApkPicker { get; } = new("APP File")
    {
        Patterns = new[] { "*.apk", "*.hap" },
        AppleUniformTypeIdentifiers = new[] { "*.apk", "*.hap" }
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
                ApkFile.Text = ApkFile.Text + files[i].TryGetLocalPath() + "|||";
            }
        }
    }
}