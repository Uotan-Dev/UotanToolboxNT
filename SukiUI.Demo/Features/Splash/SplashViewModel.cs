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
using Avalonia.Collections;
using SukiUI.Demo.Features.ControlsLibrary;
using System.Linq;

namespace SukiUI.Demo.Features.Splash;

public partial class SplashViewModel(PageNavigationService nav) : DemoPageBase("Home", MaterialIconKind.HomeOutline, int.MinValue)
{
    [ObservableProperty][Range(0d, 100d)] private double _progressValue = 50;
    [ObservableProperty] private string _status;
    [ObservableProperty] private string _bLStatus;
    [ObservableProperty] private string _vABStatus;
    [ObservableProperty] private string _codeName;
    [ObservableProperty] private string _vNDKVersion;
    [ObservableProperty] private string _cPUCode;
    [ObservableProperty] private string _powerOnTime;
    [ObservableProperty] private string _deviceBrand;
    [ObservableProperty] private string _deviceModel;
    [ObservableProperty] private string _androidSDK;
    [ObservableProperty] private string _cPUABI;
    [ObservableProperty] private string _displayHW;
    [ObservableProperty] private string _density;
    [ObservableProperty] private bool _isConnected;

    public AvaloniaList<string> SimpleContent { get; } = new();
    [ObservableProperty] private string _selectedSimpleContent;

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
                SimpleContent.AddRange(Enumerable.Range(1, 50).Select(x => $"Option {x}"));
                SelectedSimpleContent = SimpleContent.First();

                Dictionary<string, string> DevicesInfo = await GetDevicesInfo.DevicesInfo(devices[0]);
                Status = DevicesInfo["Status"];
                BLStatus = DevicesInfo["BLStatus"];
                VABStatus = DevicesInfo["VABStatus"];
                CodeName = DevicesInfo["CodeName"];
                VNDKVersion = DevicesInfo["VNDKVersion"];
                CPUCode = DevicesInfo["CPUCode"];
                PowerOnTime = DevicesInfo["PowerOnTime"];
                DeviceBrand = DevicesInfo["DeviceBrand"];
                DeviceModel = DevicesInfo["DeviceModel"];
                AndroidSDK = DevicesInfo["AndroidSDK"];
                CPUABI = DevicesInfo["CPUABI"];
                DisplayHW = DevicesInfo["DisplayHW"];
                Density = DevicesInfo["Density"];
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