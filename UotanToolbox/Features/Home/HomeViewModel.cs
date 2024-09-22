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
using Microsoft.VisualBasic;
using ReactiveUI;
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
    _powerOnTime = "--", _deviceBrand = "--", _deviceModel = "--", _androidSDK = "--",
    _cPUABI = "--", _displayHW = "--", _density = "--", _boardID = "--", _platform = "--",
    _compile = "--", _kernel = "--", _selectedSimpleContent = null, _diskType = "--",
    _batteryLevel = "0", _batteryInfo = "--", _useMem = "--", _diskInfo = "--";
    [ObservableProperty] private bool _IsConnecting;
    [ObservableProperty] private bool _commonDevicesList;
    [ObservableProperty] private static AvaloniaList<string> _simpleContent;
    public ISukiDialogManager DialogManager { get; }
    public ISukiToastManager ToastManager { get; }
    public IAvaloniaReadOnlyList<MainPageBase> DemoPages { get; }

    [ObservableProperty] private bool _animationsEnabled;
    [ObservableProperty] private MainPageBase _activePage;
    [ObservableProperty] private bool _windowLocked = false;

    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public HomeViewModel(ISukiToastManager toastManager, ISukiDialogManager dialogManager) : base(GetTranslation("Sidebar_HomePage"), MaterialIconKind.HomeOutline, int.MinValue)
    {
        _ = CheckEnvironment();
        _ = CheckDeviceList();
        ToastManager = toastManager;
        DialogManager = dialogManager;
        Global.homeView = new HomeView(toastManager);
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

    public async Task CheckEnvironment()
    {
        string filepath1 = "";
        string filepath2 = "";
        if (Global.System == "Windows")
        {
            filepath1 = Path.Combine(Global.bin_path, "platform-tools", "adb.exe");
            filepath2 = Path.Combine(Global.bin_path, "platform-tools", "fastboot.exe");
        }
        else
        {
            filepath1 = Path.Combine(Global.bin_path, "platform-tools", "adb");
            filepath2 = Path.Combine(Global.bin_path, "platform-tools", "fastboot");
        }
        if (!File.Exists(filepath1) || !File.Exists(filepath2))
        {
            _ = DialogManager.CreateDialog()
                .WithTitle("Warn")
                .WithContent(GetTranslation("Home_Missing"))
                .OfType(Avalonia.Controls.Notifications.NotificationType.Warning)
                .WithActionButton("OK", _ => Process.GetCurrentProcess().Kill(), true)
                .TryShow();

        }
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
            _ = DialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
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
        IsConnecting = false;
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
                    _ = ToastManager.CreateToast()
    .WithTitle(GetTranslation("Home_Prompt"))
    .WithContent(GetTranslation("Home_Disconnected"))
    .OfType(NotificationType.Warning)
    .Dismiss().ByClicking()
    .Dismiss().After(TimeSpan.FromSeconds(3))
    .Queue();
                    MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
                    Status = sukiViewModel.Status = BLStatus = sukiViewModel.BLStatus = VABStatus = sukiViewModel.VABStatus = CodeName = sukiViewModel.CodeName = "--";
                    VNDKVersion = CPUCode = PowerOnTime = DeviceBrand = DeviceModel = AndroidSDK = CPUABI = DisplayHW = Density = DiskType = BoardID = Platform = Compile = Kernel = BatteryInfo = UseMem = DiskInfo = "--";
                    BatteryLevel = MemLevel = ProgressDisk = "0";
                }
            }
        }
        return false;
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

    private async Task SystemControl(string shell)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} {shell}");
            }
            else
            {
                _ = DialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = DialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async Task ADBControl(string shell)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System") || sukiViewModel.Status == GetTranslation("Home_Recovery") || sukiViewModel.Status == GetTranslation("Home_Sideload"))
            {
                _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} {shell}");
            }
            else
            {
                _ = DialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterRecOrOpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = DialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async Task FastbootControl(string shell)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
            {
                _ = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} {shell}");
            }
            else
            {
                _ = DialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = DialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }


    [RelayCommand]
    public async Task Back()
    {
        await SystemControl("shell input keyevent 4");
    }

    [RelayCommand]
    public async Task Home()
    {
        await SystemControl("shell input keyevent 3");
    }

    [RelayCommand]
    public async Task Mul()
    {
        await SystemControl("shell input keyevent 187");
    }

    [RelayCommand]
    public async Task Lock()
    {
        await SystemControl("shell input keyevent 26");
    }

    [RelayCommand]
    public async Task VolU()
    {
        await SystemControl("shell input keyevent 24");
    }

    [RelayCommand]
    public async Task VolD()
    {
        await SystemControl("shell input keyevent 25");
    }

    [RelayCommand]
    public async Task Mute()
    {
        await SystemControl("shell input keyevent 164");
    }

    [RelayCommand]
    public async Task SC()
    {
        string pngname = string.Format($"{DateAndTime.Now:yyyy-MM-dd_HH-mm-ss}");
        await SystemControl($"shell /system/bin/screencap -p /sdcard/{pngname}.png");
        _ = ToastManager.CreateToast()
    .WithTitle(GetTranslation("Home_Succeeded"))
    .WithContent($"{GetTranslation("Home_Saved")} {pngname}.png {GetTranslation("Home_ToStorage")}")
    .OfType(NotificationType.Success)
    .Dismiss().ByClicking()
    .Dismiss().After(TimeSpan.FromSeconds(3))
    .Queue();
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
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System") || sukiViewModel.Status == GetTranslation("Home_Recovery") || sukiViewModel.Status == GetTranslation("Home_Sideload"))
            {
                string output = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp sideload");
                if (output.Contains("not found"))
                {
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} reboot sideload");
                }
            }
            else
            {
                _ = DialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterRecOrOpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = DialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
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
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
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
                _ = DialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = DialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    [RelayCommand]
    public async Task FRShut()
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Fastboot"))
            {
                string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem poweroff");
                _ = output.Contains("unknown command")
                    ? DialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Home_NotSupported")).Dismiss().ByClickingBackground().TryShow()
                    : DialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Home_Successful")).Dismiss().ByClickingBackground().TryShow();
            }
            else
            {
                _ = DialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = DialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
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