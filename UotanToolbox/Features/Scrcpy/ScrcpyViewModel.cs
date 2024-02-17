using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Scrcpy;

public partial class ScrcpyViewModel : MainPageBase
{
    [ObservableProperty] private bool _windowFixed = false, _computerControl = true, _fullScreen = false, _showBorder = true,
                        _showTouch = true, _renderAllFrame = false, _closeScreen = false, _screenAwake = false, _screenAwakeStatus = true;
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private string _windowTitle;
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
        IsConnected = true;
        return Task.Run(async () =>
        {
            string arg = $"-s {Global.thisdevice} ";
            if (WindowTitle != "" && WindowTitle !=null)
            {
                arg += $"--window-title {WindowTitle} ";
            }
            if (WindowFixed) arg += "--always-on-top ";
            if (FullScreen) arg += "--fullscreen ";
            if (!ShowBorder) arg += "--window-borderless ";
            if (ShowTouch) arg += "--show-touches ";
            if (RenderAllFrame) arg += "--render-expired-frames ";
            if (!ComputerControl) arg += "--no-control ";
            if (CloseScreen) arg += "--turn-screen-off ";
            if (ScreenAwake) arg += "--stay-awake ";
            await CallExternalProgram.Scrcpy(arg);
            IsConnected = false;
        });
    }
}