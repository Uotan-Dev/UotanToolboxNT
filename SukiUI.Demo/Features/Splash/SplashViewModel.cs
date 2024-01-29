using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Demo.Common;
using SukiUI.Demo.Features.Dashboard;
using SukiUI.Demo.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using SukiUI.Controls;
using SukiUI.Demo.Utilities;
using SukiUI.Demo.Features.ControlsLibrary.Dialogs;
using Avalonia.Threading;

namespace SukiUI.Demo.Features.Splash;

public partial class SplashViewModel(PageNavigationService nav) : DemoPageBase("ึ๗าณ", MaterialIconKind.HomeOutline, int.MinValue)
{
    [ObservableProperty][Range(0d, 100d)] private double _progressValue = 50;
    [ObservableProperty] private string _status;
    [ObservableProperty] private string _bLStatus;
    [ObservableProperty] private string _vABStatus;
    [ObservableProperty] private string _codeName;
    [ObservableProperty] private bool _isConnected;

    [RelayCommand]
    public void OpenDashboard()
    {
        nav.RequestNavigation<DashboardViewModel>();
    }

    [RelayCommand]
    public void OpenConnectionDialog() =>
        SukiHost.ShowDialog(new ConnectionDialog(), allowBackgroundClose: true);

    [RelayCommand]
    public Task Connect()
    {
        IsConnected = true;
        return Task.Run(async () =>
        {
            string[] devices = await GetDevicesInfo.DevicesList();
            if(devices.Length != 0)
            {
                Dictionary<string, string> DevicesInfo = await GetDevicesInfo.DevicesInfo(devices[0]);
                Status = DevicesInfo["Status"];
                BLStatus = DevicesInfo["BLStatus"];
                VABStatus = DevicesInfo["VABStatus"];
                CodeName = DevicesInfo["CodeName"];
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SukiHost.ShowDialog(new ConnectionDialog(), allowBackgroundClose: true);
                });
            }
            IsConnected = false;
        });
    }
}