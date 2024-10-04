using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Microsoft.VisualBasic;
using ReactiveUI;
using SukiUI.Dialogs;
using SukiUI.Controls;
using SukiUI.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Scrcpy;

public partial class ScrcpyViewModel : MainPageBase
{
    [ObservableProperty]
    private bool _recordScreen = false, _windowFixed = false, _computerControl = true, _fullScreen = false, _showBorder = true,
                        _showTouch = true, _closeScreen = false, _screenAwake = false, _screenAwakeStatus = true, _clipboardSync = true, _cameraMirror = false;
    [ObservableProperty] private bool _IsConnecting;
    [ObservableProperty] private string _windowTitle, _recordFolder;

    [ObservableProperty][Range(0d, 50d)] private double _bitRate = 8;
    [ObservableProperty][Range(0d, 144d)] private double _frameRate = 60;
    [ObservableProperty][Range(0d, 2048d)] private double _sizeResolution = 0;
    private ISukiDialogManager dialogManager;
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public ScrcpyViewModel() : base("Scrcpy", MaterialIconKind.CellphoneLink, -500)
    {
        _ = this.WhenAnyValue(x => x.ComputerControl)
            .Subscribe(jug =>
            {
                if (!jug)
                {
                    ScreenAwakeStatus = false;
                    ScreenAwake = false;
                }
                else
                {
                    ScreenAwakeStatus = true;
                }
            });
    }

    [RelayCommand]
    public async Task Connect()
    {
        if (Global.System == "Windows" | Global.System == "macOS")
        {
            if (await GetDevicesInfo.SetDevicesInfoLittle())
            {
                MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
                if (sukiViewModel.Status == GetTranslation("Home_System"))
                {
                    IsConnecting = true;
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        string arg = $"-s \"{Global.thisdevice}\" ";
                        if (RecordScreen)
                        {
                            if (string.IsNullOrEmpty(RecordFolder))
                            {
                                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Scrcpy_RecordFileNotChosen")).Dismiss().ByClickingBackground().TryShow();
                                IsConnecting = false;
                                return;
                            }
                            DateTime now = DateTime.Now;
                            string formattedDateTime = now.ToString("yyyy-MM-dd-HH-mm-ss");
                            arg += $"--record {RecordFolder}/{Global.thisdevice}-{formattedDateTime}.mp4 ";
                        }
                        arg += $"--video-bit-rate {BitRate}M --max-fps {FrameRate} ";
                        if (SizeResolution != 0)
                        {
                            arg += $"--max-size {SizeResolution} ";
                        }

                        if (WindowTitle is not "" and not null)
                        {
                            arg += $"--window-title \"{WindowTitle.Replace("\"", "\\\"")}\" ";
                        }
                        if (WindowFixed)
                        {
                            arg += "--always-on-top ";
                        }

                        if (FullScreen)
                        {
                            arg += "--fullscreen ";
                        }

                        if (!ShowBorder)
                        {
                            arg += "--window-borderless ";
                        }

                        if (ShowTouch)
                        {
                            arg += "--show-touches ";
                        }

                        if (!ComputerControl)
                        {
                            arg += "--no-control ";
                        }

                        if (CloseScreen)
                        {
                            arg += "--turn-screen-off ";
                        }

                        if (ScreenAwake)
                        {
                            arg += "--stay-awake ";
                        }

                        if (!ClipboardSync)
                        {
                            arg += "--no-clipboard-autosync ";
                        }

                        if (CameraMirror)
                        {
                            arg += "--video-source=camera ";
                        }

                        _ = await CallExternalProgram.Scrcpy(arg);
                        IsConnecting = false;
                    });
                }
                else
                {
                    _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_OpenADB")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotSupportSystem")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async Task SystemControl(string shell)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                await CallExternalProgram.ADB($"-s {Global.thisdevice} {shell}");
            }
            else
            {
                //SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_OpenADB")), allowBackgroundClose: true);
            }
        }
        else
        {
            //SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
        }
    }

    [RelayCommand]
    public async Task Back() => await SystemControl("shell input keyevent 4");

    [RelayCommand]
    public async Task Home() => await SystemControl("shell input keyevent 3");

    [RelayCommand]
    public async Task Mul() => await SystemControl("shell input keyevent 187");

    [RelayCommand]
    public async Task Lock() => await SystemControl("shell input keyevent 26");

    [RelayCommand]
    public async Task VolU() => await SystemControl("shell input keyevent 24");

    [RelayCommand]
    public async Task VolD() => await SystemControl("shell input keyevent 25");

    [RelayCommand]
    public async Task Mute() => await SystemControl("shell input keyevent 164");

    [RelayCommand]
    public async Task SC()
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                string pngname = String.Format($"{DateAndTime.Now:yyyy-MM-dd_HH-mm-ss}");
                await CallExternalProgram.ADB($"-s {Global.thisdevice} shell /system/bin/screencap -p /sdcard/{pngname}.png");
                //await SukiHost.ShowToast(GetTranslation("Home_Succeeded"), $"{GetTranslation("Home_Saved")} {pngname}.png {GetTranslation("Home_ToStorage")}", NotificationType.Success);
            }
            else
            {
                //SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_OpenADB")), allowBackgroundClose: true);
            }
        }
        else
        {
            //SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
        }
    }
}