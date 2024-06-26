﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using ReactiveUI;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Scrcpy;

public partial class ScrcpyViewModel : MainPageBase
{
    [ObservableProperty]
    private bool _recordScreen = false, _windowFixed = false, _computerControl = true, _fullScreen = false, _showBorder = true,
                        _showTouch = true, _closeScreen = false, _screenAwake = false, _screenAwakeStatus = true;
    [ObservableProperty] private bool _IsConnecting;
    [ObservableProperty] private string _windowTitle, _recordFolder;

    [ObservableProperty][Range(0d, 10d)] private double _bitRate = 8;
    [ObservableProperty][Range(0d, 180d)] private double _frameRate = 60;
    [ObservableProperty][Range(0d, 2048d)] private double _sizeResolution = 0;

    public ScrcpyViewModel() : base("Scrcpy", MaterialIconKind.CellphoneLink, -500)
    {
        this.WhenAnyValue(x => x.ComputerControl)
            .Subscribe(jug =>
            {
                if (!jug)
                {
                    ScreenAwakeStatus = false;
                    ScreenAwake = false;
                }
                else ScreenAwakeStatus = true;
            });
    }

    [RelayCommand]
    public Task Connect()
    {
        IsConnecting = true;
        return Task.Run(async () =>
        {
            string arg = $"-s {Global.thisdevice} ";
            if (RecordScreen)
            {
                if (RecordFolder != null)
                {
                    DateTime now = DateTime.Now;
                    string formattedDateTime = now.ToString("yyyy-MM-dd-HH-mm-ss");
                    arg += $"--record {RecordFolder}/{Global.thisdevice}-{formattedDateTime}.mp4 ";
                }
            }
            arg += $"--video-bit-rate {BitRate}M --max-fps {FrameRate} ";
            if (SizeResolution != 0) arg += $"--max-size {SizeResolution} ";

            if (WindowTitle != "" && WindowTitle != null)
            {
                arg += $"--window-title {WindowTitle} ";
            }
            if (WindowFixed) arg += "--always-on-top ";
            if (FullScreen) arg += "--fullscreen ";
            if (!ShowBorder) arg += "--window-borderless ";
            if (ShowTouch) arg += "--show-touches ";
            if (!ComputerControl) arg += "--no-control ";
            if (CloseScreen) arg += "--turn-screen-off ";
            if (ScreenAwake) arg += "--stay-awake ";
            await CallExternalProgram.Scrcpy(arg);
            IsConnecting = false;
        });
    }
}