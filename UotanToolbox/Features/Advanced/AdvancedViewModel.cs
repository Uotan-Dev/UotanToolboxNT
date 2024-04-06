using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using ReactiveUI;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Advanced;

public partial class AdvancedViewModel : MainPageBase
{
    [ObservableProperty] private string _qcnFile;

    public AdvancedViewModel() : base("高级", MaterialIconKind.WrenchCogOutline, -300)
    {
    }

    [RelayCommand]
    private async Task WriteQcn()
    {
        // 写入QCN文件
    }
    [RelayCommand]
    private async Task BackupQcn()
    {
        // 备份QCN文件
    }
    [RelayCommand]
    private async Task OpenBackup()
    {
        // 备份QCN文件
    }
    [RelayCommand]
    private async Task Enable901d()
    {
        // 开启901D
    }
    [RelayCommand]
    private async Task Enable9091()
    {
        // 开启9091
    }
}