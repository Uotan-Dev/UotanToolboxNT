using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Demo.Common;
using SukiUI.Demo.Features.Dashboard;
using SukiUI.Demo.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using SukiUI.Controls;
using Avalonia.Threading;
using Avalonia.Collections;
using System.Linq;
using Microsoft.VisualBasic;
namespace SukiUI.Demo.Features.Splash;

public partial class SplashViewModel(PageNavigationService nav) : DemoPageBase("Home", MaterialIconKind.HomeOutline, int.MinValue)
{
    [ObservableProperty]
    private string _progressDisk, _memLevel, _status, _bLStatus, _vABStatus, _codeName, _vNDKVersion, _cPUCode, _powerOnTime,
        _deviceBrand, _deviceModel, _androidSDK, _cPUABI, _displayHW, _density, _boardID, _platform, _compile, _kernel, _selectedSimpleContent,
        _diskType, _batteryLevel, _batteryInfo, _useMem, _diskInfo;
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private AvaloniaList<string> _simpleContent;

    [RelayCommand]
    public void OpenDashboard()
    {
        nav.RequestNavigation<DashboardViewModel>();
    }

    private async Task GetDevicesList()
    {
        IsConnected = true;
        string[] devices = await GetDevicesInfo.DevicesList();
        if (devices.Length !=  0)
        {
            SimpleContent = new AvaloniaList<string>(devices);
            if (SelectedSimpleContent == null || string.Concat(SimpleContent).IndexOf(SelectedSimpleContent) == -1)
            {
                SelectedSimpleContent = SimpleContent.First();
            }
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var newDialog = new ConnectionDialog("设备未连接!");
                await SukiHost.ShowDialogAsync(new ConnectionDialog("设备未连接!"));
                if (newDialog.Result == true)
                {
                    /// Code here...
                }
            });
        }
        IsConnected = false;
    }

    [RelayCommand]
    public async Task Connect()
    {
        await GetDevicesList();
        IsConnected = true;
        if (SelectedSimpleContent != null && string.Concat(SimpleContent).IndexOf(SelectedSimpleContent) != -1)
        {
            Dictionary<string, string> DevicesInfo = await GetDevicesInfo.DevicesInfo(SelectedSimpleContent);
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
            DiskType = DevicesInfo["DiskType"];
            BoardID = DevicesInfo["BoardID"];
            Platform = DevicesInfo["Platform"];
            Compile = DevicesInfo["Compile"];
            Kernel = DevicesInfo["Kernel"];
            BatteryLevel = DevicesInfo["BatteryLevel"];
            BatteryInfo = DevicesInfo["BatteryInfo"];
            MemLevel = DevicesInfo["MemLevel"];
            UseMem = DevicesInfo["UseMem"];
            DiskInfo = DevicesInfo["DiskInfo"];
            ProgressDisk = DevicesInfo["ProgressDisk"];
        }
        IsConnected = false;
    }

    private async Task ADBControl(string shell)
    {
        await GetDevicesList();
        if (SelectedSimpleContent != null && string.Concat(SimpleContent).IndexOf(SelectedSimpleContent) != -1)
        {
            Dictionary<string, string> DevicesInfoLittle = await GetDevicesInfo.DevicesInfoLittle(SelectedSimpleContent);
            Status = DevicesInfoLittle["Status"];
            BLStatus = DevicesInfoLittle["BLStatus"];
            VABStatus = DevicesInfoLittle["VABStatus"];
            CodeName = DevicesInfoLittle["CodeName"];
            if (Status == "系统" || Status == "Recovery" || Status == "Sideload")
            {
                await CallExternalProgram.ADB($"-s {SelectedSimpleContent} {shell}");
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SukiHost.ShowDialog(new ConnectionDialog("设备连接状态错误!"), allowBackgroundClose: true);
                });
            }
        }
    }

    private async Task FastbootControl(string shell)
    {
        await GetDevicesList();
        if (SelectedSimpleContent != null && string.Concat(SimpleContent).IndexOf(SelectedSimpleContent) != -1)
        {
            Dictionary<string, string> DevicesInfoLittle = await GetDevicesInfo.DevicesInfoLittle(SelectedSimpleContent);
            Status = DevicesInfoLittle["Status"];
            BLStatus = DevicesInfoLittle["BLStatus"];
            VABStatus = DevicesInfoLittle["VABStatus"];
            CodeName = DevicesInfoLittle["CodeName"];
            if (Status == "Fastboot" || Status == "Fastbootd")
            {
                await CallExternalProgram.Fastboot($"-s {SelectedSimpleContent} {shell}");
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SukiHost.ShowDialog(new ConnectionDialog("设备连接状态错误!"), allowBackgroundClose: true);
                });
            }
        }
    }


    [RelayCommand]
    public async Task Back()
    {
        await ADBControl("shell input keyevent 4");
    }

    [RelayCommand]
    public async Task Home()
    {
        await ADBControl("shell input keyevent 3");
    }

    [RelayCommand]
    public async Task Mul()
    {
        await ADBControl("shell input keyevent 187");
    }

    [RelayCommand]
    public async Task Lock()
    {
        await ADBControl("shell input keyevent 26");
    }

    [RelayCommand]

    public async Task VolU()
    {
        await ADBControl("shell input keyevent 24");
    }

    [RelayCommand]
    public async Task VolD()
    {
        await ADBControl("shell input keyevent 25");
    }

    [RelayCommand]
    public async Task Mute()
    {
        await ADBControl("shell input keyevent 164");
    }

    [RelayCommand]
    public async Task SC()
    {
        await ADBControl($"shell /system/bin/screencap -p /sdcard/{DateAndTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.png");
    }

    [RelayCommand]
    public async Task AReboot()
    {
        await ADBControl("reboot");
    }

    [RelayCommand]
    public async Task ARRec()
    {
        await ADBControl("reboot recovery");
    }

    [RelayCommand]
    public async Task ARSide()
    {
        await ADBControl("reboot sideload");
    }

    [RelayCommand]
    public async Task ARBoot()
    {
        await ADBControl("reboot bootloader");
    }

    [RelayCommand]
    public async Task ARFast()
    {
        await ADBControl("reboot fastboot");
    }

    [RelayCommand]
    public async Task ARTSide()
    {
        await ADBControl("shell twrp sideload");
    }

    [RelayCommand]
    public async Task FReboot()
    {
        await FastbootControl("reboot");
    }

    [RelayCommand]
    public async Task FRRec()
    {
        await GetDevicesList();
        if (SelectedSimpleContent != null && string.Concat(SimpleContent).IndexOf(SelectedSimpleContent) != -1)
        {
            Dictionary<string, string> DevicesInfoLittle = await GetDevicesInfo.DevicesInfoLittle(SelectedSimpleContent);
            Status = DevicesInfoLittle["Status"];
            BLStatus = DevicesInfoLittle["BLStatus"];
            VABStatus = DevicesInfoLittle["VABStatus"];
            CodeName = DevicesInfoLittle["CodeName"];
            if (Status == "Fastboot")
            {
                string output = await CallExternalProgram.Fastboot($"-s {SelectedSimpleContent} oem reboot-recovery");
                if (output.IndexOf("unknown command") != -1)
                {
                    await CallExternalProgram.Fastboot("flash misc bin/img/misc.img");
                    await CallExternalProgram.Fastboot("reboot");
                }
                else
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        SukiHost.ShowDialog(new ConnectionDialog("设备连接状态错误!"), allowBackgroundClose: true);
                    });
                }
            }
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SukiHost.ShowDialog(new ConnectionDialog("设备未连接!"), allowBackgroundClose: true);
            });
        }
    }

    [RelayCommand]
    public async Task FRShut()
    {
        await GetDevicesList();
        if (SelectedSimpleContent != null && string.Concat(SimpleContent).IndexOf(SelectedSimpleContent) != -1)
        {
            Dictionary<string, string> DevicesInfoLittle = await GetDevicesInfo.DevicesInfoLittle(SelectedSimpleContent);
            Status = DevicesInfoLittle["Status"];
            BLStatus = DevicesInfoLittle["BLStatus"];
            VABStatus = DevicesInfoLittle["VABStatus"];
            CodeName = DevicesInfoLittle["CodeName"];
            if (Status == "Fastboot")
            {
                string output = await CallExternalProgram.Fastboot($"-s {SelectedSimpleContent} oem poweroff");
                if (output.IndexOf("unknown command") != -1)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        SukiHost.ShowDialog(new ConnectionDialog("当前设备不支持此命令！"), allowBackgroundClose: true);
                    });
                }
                else
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        SukiHost.ShowDialog(new ConnectionDialog("执行成功，拔出设备连接线即可关机！"), allowBackgroundClose: true);
                    });
                }
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SukiHost.ShowDialog(new ConnectionDialog("设备连接状态错误!"), allowBackgroundClose: true);
                });
            }
        }
    }

    [RelayCommand]
    public async Task FRBoot()
    {
        await FastbootControl("reboot-bootloader");
    }

    [RelayCommand]
    public async Task FRFast()
    {
        await FastbootControl("reboot-fastboot");
    }

    [RelayCommand]
    public async Task FREDL()
    {
        await FastbootControl("oem edl");
    }
}