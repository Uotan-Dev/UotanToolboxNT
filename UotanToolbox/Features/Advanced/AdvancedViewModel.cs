using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using ReactiveUI;
using SukiUI.Controls;
using SukiUI.MessageBox;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Features.Components;

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
        if (QcnFile != null)
        {
            string qcnfilepatch = QcnFile;
            string usbdevices = await CallExternalProgram.Devcon("find usb*");
            if (usbdevices.IndexOf("901D (") != -1 || usbdevices.IndexOf("9091 (") != -1)
            {
                int com = StringHelper.FindDIAGCom(usbdevices);
                string shell = string.Format("-w -p {0} -f \"{1}\"", com, qcnfilepatch);
                string output = await CallExternalProgram.QCNTool(shell);
                if (output.IndexOf("error") != -1)
                {
                    SukiHost.ShowDialog(new ConnectionDialog("写入失败"), allowBackgroundClose: true);
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog("写入成功"), allowBackgroundClose: true);
                }
             }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("无可用设备"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new ConnectionDialog("请先选择QCN文件"), allowBackgroundClose: true);
        }
    }
    [RelayCommand]
    private async Task BackupQcn()
    {
        // 备份QCN文件
        string qcnfilepatch = QcnFile;
        string usbdevices = await CallExternalProgram.Devcon("find usb*");
        if (usbdevices.IndexOf("901D (") != -1 || usbdevices.IndexOf("9091 (") != -1)
        {
            int com = StringHelper.FindDIAGCom(usbdevices);
            string shell = string.Format("-r -p {0} -f {1}\\backup", com, System.IO.Directory.GetCurrentDirectory());
            string output = await CallExternalProgram.QCNTool(shell);
            if (output.IndexOf("error") != -1)
            {
                SukiHost.ShowDialog(new ConnectionDialog("备份失败"), allowBackgroundClose: true);
            }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("备份成功"), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new ConnectionDialog("无可用设备"), allowBackgroundClose: true);
        }
    }
    [RelayCommand]
    private async Task OpenBackup()
    {
        // 打开QCN文件备份目录
        string filepath = String.Format(@"{0}\backup", System.IO.Directory.GetCurrentDirectory());
        Process.Start("Explorer.exe", filepath);
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