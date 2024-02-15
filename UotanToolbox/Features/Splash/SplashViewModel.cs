using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using UotanToolbox.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using SukiUI.Controls;
using Avalonia.Threading;
using Avalonia.Collections;
using System.Linq;
using Microsoft.VisualBasic;
namespace UotanToolbox.Features.Splash;
using ReactiveUI;

using System;
using System.Diagnostics;
using UotanToolbox;

public partial class SplashViewModel : DemoPageBase
{
    [ObservableProperty]
    private string _progressDisk = "--", _memLevel = "--", _status = "--", _bLStatus = "--",
    _vABStatus = "--", _codeName = "--", _vNDKVersion = "--", _cPUCode = "--",
    _powerOnTime = "--", _deviceBrand = "--", _deviceModel = "--", _androidSDK = "--",
    _cPUABI = "--", _displayHW = "--", _density = "--", _boardID = "--", _platform = "--",
    _compile = "--", _kernel = "--", _selectedSimpleContent = "--", _diskType = "--",
    _batteryLevel = "--", _batteryInfo = "--", _useMem = "--", _diskInfo = "--";
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private bool _devicesList;
    [ObservableProperty] private static AvaloniaList<string>? _simpleContent;

    public IAvaloniaReadOnlyList<DemoPageBase>? DemoPages { get; }

    [ObservableProperty] private bool _animationsEnabled;
    [ObservableProperty] private DemoPageBase? _activePage;
    [ObservableProperty] private bool _windowLocked = false;

    public SplashViewModel() : base("主页", MaterialIconKind.HomeOutline, int.MinValue)
    {
        _ = Connect();
        this.WhenAnyValue(x => x.SelectedSimpleContent)
            .Subscribe((Action<string>)(option =>
            {
            if (option != "--" && SimpleContent != null)
                    _ = ConnectMinimum(option);
            }));
    }

    public async Task ConnectMinimum(string option)
    {
        Global.thisdevice = option;
        await ConnectCore();
    }

    public async Task<bool> GetDevicesList()
    {
        string[] devices = await GetDevicesInfo.DevicesList();
        if (devices.Length !=  0)
        {
            DevicesList = true;
            Global.deviceslist = new AvaloniaList<string>(devices);
            SimpleContent = Global.deviceslist;
            if (SelectedSimpleContent == null || !string.Join("", SimpleContent).Contains(SelectedSimpleContent))
            {
                if (Global.thisdevice != null && Global.deviceslist.Contains(Global.thisdevice))
                {
                    SelectedSimpleContent = Global.thisdevice;
                }
                else
                {
                    Global.thisdevice = SimpleContent.First();
                    SelectedSimpleContent = SimpleContent.First();
                }
            }
            DevicesList = false;
            return true;
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SukiHost.ShowDialog(new ConnectionDialog("设备未连接!"), allowBackgroundClose: true);
            });
            SukiUIDemoViewModel sukiViewModel = GlobalData.SukiUIDemoViewModelInstance;
            SimpleContent = null;
            Status = "--";
            sukiViewModel.Status = "--";
            BLStatus = "--";
            sukiViewModel.BLStatus = "--";
            VABStatus = "--";
            sukiViewModel.VABStatus = "--";
            CodeName = "--";
            sukiViewModel.CodeName = "--";
            VNDKVersion = "--";
            CPUCode = "--";
            PowerOnTime = "--";
            DeviceBrand = "--";
            DeviceModel = "--";
            AndroidSDK = "--";
            CPUABI = "--";
            DisplayHW = "--";
            Density = "--";
            DiskType = "--";
            BoardID = "--";
            Platform = "--";
            Compile = "--";
            Kernel = "--";
            BatteryLevel = "--";
            BatteryInfo = "--";
            MemLevel = "--";
            UseMem = "--";
            DiskInfo = "--";
            ProgressDisk = "--";
            return false;
        }
    }

    public async Task ConnectLittle()
    {
        if (await GetDevicesList() && Global.thisdevice != null && Global.deviceslist.Contains(Global.thisdevice))
        {
            SukiUIDemoViewModel sukiViewModel = GlobalData.SukiUIDemoViewModelInstance;
            Dictionary<string, string> DevicesInfoLittle = await GetDevicesInfo.DevicesInfoLittle(Global.thisdevice);
            Status = DevicesInfoLittle["Status"];
            sukiViewModel.Status = DevicesInfoLittle["Status"];
            BLStatus = DevicesInfoLittle["BLStatus"];
            sukiViewModel.BLStatus = DevicesInfoLittle["BLStatus"];
            VABStatus = DevicesInfoLittle["VABStatus"];
            sukiViewModel.VABStatus = DevicesInfoLittle["VABStatus"];
            CodeName = DevicesInfoLittle["CodeName"];
            sukiViewModel.CodeName = DevicesInfoLittle["CodeName"];
        }
    }

    [RelayCommand]
    public async Task Connect()
    {
        if (await GetDevicesList() && Global.thisdevice != null && string.Join("", Global.deviceslist).Contains(Global.thisdevice))
            await ConnectCore();
    }

    public async Task ConnectCore()
    {
        IsConnected = true;
        SukiUIDemoViewModel sukiViewModel = GlobalData.SukiUIDemoViewModelInstance;
        Dictionary<string, string> DevicesInfo = await GetDevicesInfo.DevicesInfo(Global.thisdevice);
        Status = DevicesInfo["Status"];
        sukiViewModel.Status = DevicesInfo["Status"];
        BLStatus = DevicesInfo["BLStatus"];
        sukiViewModel.BLStatus = DevicesInfo["BLStatus"];
        VABStatus = DevicesInfo["VABStatus"];
        sukiViewModel.VABStatus = DevicesInfo["VABStatus"];
        CodeName = DevicesInfo["CodeName"];
        sukiViewModel.CodeName = DevicesInfo["CodeName"];
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
        IsConnected = false;
    }


    private async Task ADBControl(string shell)
    {
        await ConnectLittle();
        SukiUIDemoViewModel sukiViewModel = GlobalData.SukiUIDemoViewModelInstance;
        if (sukiViewModel.Status == "系统" || sukiViewModel.Status == "Recovery" || sukiViewModel.Status == "Sideload")
        {
            await CallExternalProgram.ADB($"-s {Global.thisdevice} {shell}");
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SukiHost.ShowDialog(new ConnectionDialog("设备连接状态错误!"), allowBackgroundClose: true);
            });
        }
    }

    private async Task FastbootControl(string shell)
    {
        await ConnectLittle();
        SukiUIDemoViewModel sukiViewModel = GlobalData.SukiUIDemoViewModelInstance;
        if (sukiViewModel.Status == "Fastboot" || sukiViewModel.Status == "Fastbootd")
        {
            await CallExternalProgram.Fastboot($"-s {Global.thisdevice} {shell}");
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SukiHost.ShowDialog(new ConnectionDialog("设备连接状态错误!"), allowBackgroundClose: true);
            });
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
        await ADBControl($"shell /system/bin/screencap -p /sdcard/{DateAndTime.Now:yyyy-MM-dd_HH-mm-ss}.png");
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
        await ConnectLittle();
        SukiUIDemoViewModel sukiViewModel = GlobalData.SukiUIDemoViewModelInstance;
        if (sukiViewModel.Status == "Recovery")
        {
            string output = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp sideload");
            if (output.Contains("not found"))
            {
                await CallExternalProgram.ADB($"-s {Global.thisdevice} reboot sideload");
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
    public async Task AREDL()
    {
        await ADBControl("reboot edl");
    }

    [RelayCommand]
    public async Task FReboot()
    {
        await FastbootControl("reboot");
    }

    [RelayCommand]
    public async Task FRRec()
    {
        await ConnectLittle();
        SukiUIDemoViewModel sukiViewModel = GlobalData.SukiUIDemoViewModelInstance;
        if (sukiViewModel.Status == "Fastboot")
        {
            string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem reboot-recovery");
            if (output.Contains("unknown command"))
            {
                await CallExternalProgram.Fastboot($"-s {Global.thisdevice} flash misc bin/img/misc.img");
                await CallExternalProgram.Fastboot($"-s {Global.thisdevice} reboot");
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

    [RelayCommand]
    public async Task FRShut()
    {
        await ConnectLittle();
        SukiUIDemoViewModel sukiViewModel = GlobalData.SukiUIDemoViewModelInstance;
        if (sukiViewModel.Status == "Fastboot")
        {
            string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem poweroff");
            if (output.Contains("unknown command"))
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