using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using UotanToolbox.Common;


namespace UotanToolbox.Features.EDL;

public partial class EDLViewModel : MainPageBase
{
    [ObservableProperty]
    private string firehoseFile, currentDevice = "当前连接：COM5", xMLFile, partNamr, eDLLog;
    [ObservableProperty]
    private bool auto = true, uFS = false, eMMC = false, spinor = false;
    [ObservableProperty]
    private int selectFlie = 0, selectUFSLun = 0, selectBand = 1;
    [ObservableProperty]
    private AvaloniaList<string> builtInFile = ["common", "mi_auth", "mi_noauth_625", "mi_noauth_778g"], uFSLun = ["6", "7", "8"], bandList = ["commonl", "mi", "oppo", "oneplus", "meizu", "zte", "lg"];

    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public EDLViewModel() : base("9008刷机", MaterialIconKind.CableData, -350)
    {
    }

    [RelayCommand]
    public async Task SendFirehose()
    {
        
    }

    [RelayCommand]
    public async Task BatchWrite()
    {
        
    }

    [RelayCommand]
    public async Task BatchRead()
    {

    }

    [RelayCommand]
    public async Task WritePartTable()
    {

    }

    [RelayCommand]
    public async Task ReadPartTable()
    {

    }

    [RelayCommand]
    public async Task WritePart()
    {

    }

    [RelayCommand]
    public async Task ReadPart()
    {

    }

    [RelayCommand]
    public async Task ErasePart()
    {

    }

    [RelayCommand]
    public async Task RebootEDL()
    {

    }

    [RelayCommand]
    public async Task RebootSys()
    {

    }

    [RelayCommand]
    public async Task Restore()
    {

    }

    [RelayCommand]
    public async Task OpenOEMLock()
    {

    }

    [RelayCommand]
    public async Task ReconfigureUFS()
    {

    }
}