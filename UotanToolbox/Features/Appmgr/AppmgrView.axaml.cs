using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using SukiUI.Controls;
using UotanToolbox.Features.Home;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using UotanToolbox.Common;
using Avalonia.Threading;
using UotanToolbox.Features.Components;
using Avalonia.Platform.Storage;
using System.Collections.Generic;

namespace UotanToolbox.Features.Appmgr;

public partial class AppmgrView : UserControl
{
    public AppmgrView()
    {
        InitializeComponent();
    }

    private async void UninstallButton_Click(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var applicationInfo = (ApplicationInfo)button.DataContext;
        await UninstallApplication(applicationInfo.Name);
    }

    private async Task UninstallApplication(string packageName)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var newDialog = new ConnectionDialog("确认删除应用？");
            await SukiHost.ShowDialogAsync(newDialog);
            if(newDialog.Result == true) await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm uninstall -k --user 0 {packageName}");
            var newAppmgr = new AppmgrViewModel();
            _ = newAppmgr.Connect();
        });
    }


    private static FilePickerFileType ApkPicker { get; } = new("APK File")
    {
        Patterns = new[] { "*.apk" }
    };

    private async void OpenApkFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false,
            FileTypeFilter = new[] { ApkPicker, FilePickerFileTypes.TextPlain }
        });
        if (files.Count >= 1)
        {
            ApkFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }
}