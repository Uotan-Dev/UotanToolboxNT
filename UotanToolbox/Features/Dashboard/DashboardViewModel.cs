using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Controls;
using UotanToolbox.Common;
using UotanToolbox.Features.Splash;
using System.Threading.Tasks;

namespace UotanToolbox.Features.Dashboard;

public partial class DashboardViewModel : DemoPageBase
{
    public AvaloniaList<string> SimpleContent { get; } = new();
    [ObservableProperty] private string _name;
    [ObservableProperty] private int _stepperIndex;
    [ObservableProperty] private string _selectedSimpleContent;
    [ObservableProperty] private bool _unlocking;
    [ObservableProperty] private bool _baseUnlocking;
    [ObservableProperty] private bool _flashing;
    [ObservableProperty] private string _unlockFile;
    [ObservableProperty] private string _unlockCode;
    [ObservableProperty] private string _recFile;

    public DashboardViewModel() : base("刷入", MaterialIconKind.CableData, -1000)
    {
        StepperIndex = 1;
        SimpleContent.AddRange(["oem unlock", "oem unlock-go", "flashing unlock", "flashing unlock_critical"]);
    }

    [RelayCommand]
    public async Task Unlock()
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            SukiUIDemoViewModel sukiViewModel = GlobalData.SukiUIDemoViewModelInstance;
            if (sukiViewModel.Status == "Fastboot")
            {
                Unlocking = true;
                if (UnlockFile != null && UnlockCode != null)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        SukiHost.ShowDialog(new ConnectionDialog("请勿同时填写两种方式！"), allowBackgroundClose: true);
                    });
                }
                else if (UnlockFile != null && UnlockCode == null)
                {
                    await CallExternalProgram.Fastboot($"-s {Global.thisdevice} flash unlock \"{UnlockFile}\"");
                    string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem unlock-go");
                    if (output.IndexOf("OKAY") != -1)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            SukiHost.ShowDialog(new ConnectionDialog("解锁成功!"), allowBackgroundClose: true);
                        });
                    }
                    else
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            SukiHost.ShowDialog(new ConnectionDialog("解锁失败！"), allowBackgroundClose: true);
                        });
                    }
                }
                else if (UnlockFile == null && UnlockCode != null)
                {
                    string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem unlock {UnlockCode}");
                    if (output.IndexOf("OKAY") != -1)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            SukiHost.ShowDialog(new ConnectionDialog("解锁成功！"), allowBackgroundClose: true);
                        });
                    }
                    else
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            SukiHost.ShowDialog(new ConnectionDialog("解锁失败！"), allowBackgroundClose: true);
                        });
                    }
                }
                else
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        SukiHost.ShowDialog(new ConnectionDialog("请选择解锁文件,或输入解锁码！"), allowBackgroundClose: true);
                    });
                }
                Unlocking = false;
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SukiHost.ShowDialog(new ConnectionDialog("请进入Fastboot模式！"), allowBackgroundClose: true);
                });
            }
        }
    }

    [RelayCommand]
    public async Task Lock()
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            SukiUIDemoViewModel sukiViewModel = GlobalData.SukiUIDemoViewModelInstance;
            if (sukiViewModel.Status == "Fastboot")
            {
                Unlocking = true;
                await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem lock-go");
                string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} flashing lock");
                if (output.IndexOf("OKAY") != -1)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        SukiHost.ShowDialog(new ConnectionDialog("回锁成功！"), allowBackgroundClose: true);
                    });
                }
                else
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        SukiHost.ShowDialog(new ConnectionDialog("回锁失败！"), allowBackgroundClose: true);
                    });
                }
                Unlocking = false;
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SukiHost.ShowDialog(new ConnectionDialog("请进入Fastboot模式！"), allowBackgroundClose: true);
                });
            }
        }
    }

    [RelayCommand]
    public async Task BaseUnlock()
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            SukiUIDemoViewModel sukiViewModel = GlobalData.SukiUIDemoViewModelInstance;
            if (sukiViewModel.Status == "Fastboot")
            {
                BaseUnlocking = true;
                if (SelectedSimpleContent != null)
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        var newDialog = new ConnectionDialog("该功能仅支持部分品牌设备！\n\r执行后您的设备应当出现确认解锁提示，\n\r若未出现则为您的设备不支持该操作。");
                        await SukiHost.ShowDialogAsync(newDialog);
                        if (newDialog.Result == true)
                        {
                            await CallExternalProgram.Fastboot($"-s {Global.thisdevice} {SelectedSimpleContent}");
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                SukiHost.ShowDialog(new ConnectionDialog("执行完成，请查看您的设备！"), allowBackgroundClose: true);
                            });
                        }
                    });
                }
                else
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        SukiHost.ShowDialog(new ConnectionDialog("请选择解锁命令！"), allowBackgroundClose: true);
                    });
                }
                BaseUnlocking = false;
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SukiHost.ShowDialog(new ConnectionDialog("请进入Fastboot模式！"), allowBackgroundClose: true);
                });
            }
        }
    }

    private async Task FlashRec(string shell)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            SukiUIDemoViewModel sukiViewModel = GlobalData.SukiUIDemoViewModelInstance;
            if (sukiViewModel.Status == "Fastboot")
            {
                Flashing = true;
                if (RecFile != null)
                {
                    string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} {shell} \"{RecFile}\"");
                    if (output.IndexOf("FAILED") == -1 && output.IndexOf("error") == -1)
                    {
                        var newDialog = new ConnectionDialog("刷入成功！是否重启到Recovery？");
                        await SukiHost.ShowDialogAsync(newDialog);
                        if (newDialog.Result == true)
                        {
                            output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} oem reboot-recovery");
                            if (output.Contains("unknown command"))
                            {
                                await CallExternalProgram.Fastboot($"-s {Global.thisdevice} flash misc bin/img/misc.img");
                                await CallExternalProgram.Fastboot($"-s {Global.thisdevice} reboot");
                            }
                        }
                    }
                    else
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            SukiHost.ShowDialog(new ConnectionDialog("刷入失败！"), allowBackgroundClose: true);
                        });
                    }
                }
                else
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        SukiHost.ShowDialog(new ConnectionDialog("请选择Recovery文件！"), allowBackgroundClose: true);
                    });
                }
                Flashing = false;
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SukiHost.ShowDialog(new ConnectionDialog("请进入Fastboot模式！"), allowBackgroundClose: true);
                });
            }
        }
    }

    [RelayCommand]
    public async Task FlashToRec()
    {
        await FlashRec("flash recovery");
    }

    [RelayCommand]
    public async Task FlashToRecA()
    {
        await FlashRec("flash recovery_a");
    }

    [RelayCommand]
    public async Task FlashToRecB()
    {
        await FlashRec("flash recovery_b");
    }

    [RelayCommand]
    public async Task BootRec()
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            SukiUIDemoViewModel sukiViewModel = GlobalData.SukiUIDemoViewModelInstance;
            if (sukiViewModel.Status == "Fastboot")
            {
                Flashing = true;
                if (RecFile != null)
                {
                    string output = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} boot \"{RecFile}\"");
                    if (output.IndexOf("Finished") != -1)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            SukiHost.ShowDialog(new ConnectionDialog("启动成功！"), allowBackgroundClose: true);
                        });
                    }
                    else
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            SukiHost.ShowDialog(new ConnectionDialog("启动失败！"), allowBackgroundClose: true);
                        });
                    }
                }
                else
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        SukiHost.ShowDialog(new ConnectionDialog("请选择Recovery文件！"), allowBackgroundClose: true);
                    });
                }
                Flashing = false;
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SukiHost.ShowDialog(new ConnectionDialog("请进入Fastboot模式！"), allowBackgroundClose: true);
                });
            }
        }
    }

    [RelayCommand]
    public async Task FlashToBootA()
    {
        await FlashRec("flash boot_a");
    }

    [RelayCommand]
    public async Task FlashToBootB()
    {
        await FlashRec("flash boot_b");
    }

    [RelayCommand]
    public void ShowDialog()
    {
        SukiHost.ShowDialog(new DialogViewModel(), allowBackgroundClose: true);
    }
}