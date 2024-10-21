using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using ReactiveUI;
using Splat;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Home;

public partial class HomeViewModel : MainPageBase
{
    [ObservableProperty]
    private string _progressDisk = "0", _memLevel = "0", _status = "--", _bLStatus = "--",
    _vABStatus = "--", _codeName = "--", _vNDKVersion = "--", _cPUCode = "--",
    _powerOnTime = "--", _deviceBrand = "--", _deviceModel = "--", _systemSDK = "--",
    _cPUABI = "--", _displayHW = "--", _density = "--", _boardID = "--", _platform = "--",
    _compile = "--", _kernel = "--", _selectedSimpleContent = null, _diskType = "--",
    _batteryLevel = "0", _batteryInfo = "--", _useMem = "--", _diskInfo = "--";
    [ObservableProperty] private bool _IsConnecting;
    [ObservableProperty] private bool _commonDevicesList;
    [ObservableProperty] private static AvaloniaList<string> _simpleContent;
    public IAvaloniaReadOnlyList<MainPageBase> DemoPages { get; }

    [ObservableProperty] private bool _animationsEnabled;
    [ObservableProperty] private MainPageBase _activePage;
    [ObservableProperty] private bool _windowLocked = false;

    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public HomeViewModel() : base(GetTranslation("Sidebar_HomePage"), MaterialIconKind.HomeOutline, int.MinValue)
    {
        _ = CheckDeviceList();
        _ = this.WhenAnyValue(x => x.SelectedSimpleContent)
            .Subscribe(option =>
            {
                if (option != null && option != Global.thisdevice && SimpleContent != null && SimpleContent.Count != 0)
                {
                    Global.thisdevice = option;
                    _ = ConnectCore();
                }
            });
    }

    public async Task CheckDeviceList()
    {
        while (true)
        {
            if (await ListChecker() == true)
            {
                CommonDevicesList = true;
                _ = await GetDevicesList();
                CommonDevicesList = false;
            }
            await Task.Delay(1000);
        }
    }

    public async Task<bool> ListChecker()
    {
        if (Global.checkdevice)
        {
            string[] devices = await GetDevicesInfo.DevicesList();
            if (devices.Length != 0)
            {
                AvaloniaList<string> tempDeviceslist = new AvaloniaList<string>(devices);
                if (Global.deviceslist != null)
                {
                    if (Global.deviceslist.SequenceEqual(tempDeviceslist) != true)
                    {
                        return true;
                    }
                }
                else if (Global.deviceslist == null)
                {
                    return true;
                }
            }
            else
            {
                if (Global.deviceslist != null && Global.deviceslist.Count != 0)
                {
                    Global.deviceslist.Clear();
                    Global.thisdevice = null;
                    SimpleContent = null;
                    IsConnecting = false;
                    Global.MainToastManager.CreateToast()
                        .WithTitle(GetTranslation("Home_Prompt"))
                        .WithContent(GetTranslation("Home_Disconnected"))
                        .OfType(NotificationType.Warning)
                        .Dismiss().ByClicking()
                        .Dismiss().After(TimeSpan.FromSeconds(3))
                        .Queue();
                    MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
                    Status = sukiViewModel.Status = BLStatus = sukiViewModel.BLStatus = VABStatus = sukiViewModel.VABStatus = CodeName = sukiViewModel.CodeName = "--";
                    VNDKVersion = CPUCode = PowerOnTime = DeviceBrand = DeviceModel = SystemSDK = CPUABI = DisplayHW = Density = DiskType = BoardID = Platform = Compile = Kernel = BatteryInfo = UseMem = DiskInfo = "--";
                    BatteryLevel = MemLevel = ProgressDisk = "0";
                }
            }
        }
        return false;
    }

    public async Task<bool> GetDevicesList()
    {
        string[] devices = await GetDevicesInfo.DevicesList();
        if (devices.Length != 0)
        {
            Global.deviceslist = new AvaloniaList<string>(devices);
            SimpleContent = Global.deviceslist;
            if (SelectedSimpleContent == null || !string.Join("", SimpleContent).Contains(SelectedSimpleContent))
            {
                SelectedSimpleContent = Global.thisdevice != null && Global.deviceslist.Contains(Global.thisdevice) ? Global.thisdevice : SimpleContent.First();
            }
            return true;
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
            return false;
        }
    }

    public async Task ConnectCore()
    {
        IsConnecting = true;
        MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
        Dictionary<string, string> DevicesInfo = await GetDevicesInfo.DevicesInfo(Global.thisdevice);
        Status = sukiViewModel.Status = DevicesInfo["Status"];
        BLStatus = sukiViewModel.BLStatus = DevicesInfo["BLStatus"];
        VABStatus = sukiViewModel.VABStatus = DevicesInfo["VABStatus"];
        CodeName = sukiViewModel.CodeName = DevicesInfo["CodeName"];
        VNDKVersion = DevicesInfo["VNDKVersion"];
        CPUCode = DevicesInfo["CPUCode"];
        PowerOnTime = DevicesInfo["PowerOnTime"];
        DeviceBrand = DevicesInfo["DeviceBrand"];
        DeviceModel = DevicesInfo["DeviceModel"];
        SystemSDK = DevicesInfo["SystemSDK"];
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
        IsConnecting = false;
    }

    [RelayCommand]
    public async Task FreshDeviceList()
    {
        AvaloniaList<string> OldDeviceList = Global.deviceslist;
        if (await GetDevicesList() && Global.thisdevice != null && string.Join("", Global.deviceslist).Contains(Global.thisdevice))
        {
            if (OldDeviceList != Global.deviceslist)
            {
                CommonDevicesList = true;
                await ConnectCore();
                CommonDevicesList = false;
            }
        }
    }

    [RelayCommand]
    public async Task RebootSys()
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System") || sukiViewModel.Status == GetTranslation("Home_Recovery") || sukiViewModel.Status == GetTranslation("Home_Sideload"))
            {
                await CallExternalProgram.ADB($"-s {Global.thisdevice} reboot");
            }
            else if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
            {
                await CallExternalProgram.Fastboot($"-s {Global.thisdevice} reboot");
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_ModeError")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    [RelayCommand]
    public async Task RebootRec()
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System") || sukiViewModel.Status == GetTranslation("Home_Recovery") || sukiViewModel.Status == GetTranslation("Home_Sideload"))
            {
                await CallExternalProgram.ADB($"-s {Global.thisdevice} reboot recovery");
            }
            else if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
            {
                string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem reboot-recovery");
                if (output.Contains("unknown command"))
                {
                    _ = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} flash misc {Global.runpath}/Image/misc.img");
                    _ = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} reboot");
                }
                else
                {
                    _ = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} reboot recovery");
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_ModeError")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    [RelayCommand]
    public async Task RebootBL()
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System") || sukiViewModel.Status == GetTranslation("Home_Recovery") || sukiViewModel.Status == GetTranslation("Home_Sideload"))
            {
                await CallExternalProgram.ADB($"-s {Global.thisdevice} reboot bootloader");
            }
            else if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
            {
                await CallExternalProgram.Fastboot($"-s {Global.thisdevice} reboot-bootloader");
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_ModeError")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    [RelayCommand]
    public async Task RebootFB()
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System") || sukiViewModel.Status == GetTranslation("Home_Recovery") || sukiViewModel.Status == GetTranslation("Home_Sideload"))
            {
                await CallExternalProgram.ADB($"-s {Global.thisdevice} reboot fastboot");
            }
            else if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
            {
                await CallExternalProgram.Fastboot($"-s {Global.thisdevice} reboot-fastboot");
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_ModeError")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    [RelayCommand]
    public async Task PowerOff()
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System") || sukiViewModel.Status == GetTranslation("Home_Recovery") || sukiViewModel.Status == GetTranslation("Home_Sideload"))
            {
                await CallExternalProgram.ADB($"-s {Global.thisdevice} reboot -p");
            }
            else if (sukiViewModel.Status == GetTranslation("Home_Fastboot"))
            {
                string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem poweroff");
                _ = output.Contains("unknown command")
                    ? Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Home_NotSupported")).Dismiss().ByClickingBackground().TryShow()
                    : Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Home_Successful")).Dismiss().ByClickingBackground().TryShow();
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_ModeError")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    [RelayCommand]
    public async Task RebootEDL()
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System") || sukiViewModel.Status == GetTranslation("Home_Recovery") || sukiViewModel.Status == GetTranslation("Home_Sideload"))
            {
                await CallExternalProgram.ADB($"-s {Global.thisdevice} reboot edl");
            }
            else if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
            {
                await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem edl");
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_ModeError")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }
}