using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using ReactiveUI;
using SukiUI.Dialogs;
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
        if (Global.System == "Windows")
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
}