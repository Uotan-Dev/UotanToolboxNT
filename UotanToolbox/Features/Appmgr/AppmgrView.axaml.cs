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
}