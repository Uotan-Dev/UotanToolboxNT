using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Demo.Common;
using SukiUI.Demo.Features.Dashboard;
using SukiUI.Demo.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SukiUI.Demo.Features.Splash;

public partial class SplashViewModel(PageNavigationService nav) : DemoPageBase("Welcome", MaterialIconKind.Hand, int.MinValue)
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
    public Task Connect()
    {
        IsConnected = true;
        return Task.Run(async () =>
        {
            string[] devices = await GetDevicesInfo.DevicesList();
            Dictionary<string, string> DevicesInfo = await GetDevicesInfo.DevicesInfo(devices[0]);
            Status = DevicesInfo["Status"];
            BLStatus = DevicesInfo["BLStatus"];
            VABStatus = DevicesInfo["VABStatus"];
            CodeName = DevicesInfo["CodeName"];
            IsConnected = false;
        });
    }
}