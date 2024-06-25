using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Controls;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using UotanToolbox.Common;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Features.Modifypartition;

public partial class ModifypartitionView : UserControl
{
    public ModifypartitionView()
    {
        InitializeComponent();
    }

    private async void ReadPart(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Recovery" || sukiViewModel.Status == "系统")
            {
                BusyPart.IsBusy = true;
                ReadPartBut.IsEnabled = false;
                PartList.ItemsSource = null;
                if (Global.sdatable == "" && sukiViewModel.Status == "Recovery")
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp unmount data");
                }
                if (sukiViewModel.Status == "系统")
                {
                    var newDialog = new ConnectionDialog("当前为系统模式，在系统下提取分区需要ROOT权限，\n\r请确保手机已ROOT，并在接下来的弹窗中授予 Shell ROOT权限！");
                    await SukiHost.ShowDialogAsync(newDialog);
                    if (newDialog.Result == false)
                    {
                        BusyPart.IsBusy = false;
                        ReadPartBut.IsEnabled = true;
                        return;
                    }
                    await FeaturesHelper.GetPartTableSystem(Global.thisdevice);
                }
                else
                {
                    await FeaturesHelper.GetPartTable(Global.thisdevice);
                }
                string choice = "";
                if (sda.IsChecked != null && (bool)sda.IsChecked)
                    choice = Global.sdatable;
                if (sdb.IsChecked != null && (bool)sdb.IsChecked)
                    choice = Global.sdbtable;
                if (sdc.IsChecked != null && (bool)sdc.IsChecked)
                    choice = Global.sdctable;
                if (sdd.IsChecked != null && (bool)sdd.IsChecked)
                    choice = Global.sddtable;
                if (sde.IsChecked != null && (bool)sde.IsChecked)
                    choice = Global.sdetable;
                if (sdf.IsChecked != null && (bool)sdf.IsChecked)
                    choice = Global.sdftable;
                if (emmc.IsChecked != null && (bool)emmc.IsChecked)
                    choice = Global.emmcrom;
                if (choice != "")
                {
                    string[] parts = choice.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 6)
                    {
                        string size = String.Format("{0}", StringHelper.DiskSize(choice));
                        PartSize.Text = size;
                        PartModel[] part = new PartModel[parts.Length - 6];
                        for (int i = 6; i < parts.Length; i++)
                        {
                            string[] items = parts[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (items.Length == 5)
                            {
                                part[i - 6] = new PartModel(items[0], items[1], items[2], items[3], "", items[4], "");
                            }
                            else if (items.Length == 6)
                            {
                                part[i - 6] = new PartModel(items[0], items[1], items[2], items[3], items[4], items[5], "");
                            }
                            else if (items.Length >= 7)
                            {
                                part[i - 6] = new PartModel(items[0], items[1], items[2], items[3], items[4], items[5], items[6]);
                            }
                        }
                        PartList.ItemsSource = part;
                    }
                    else
                    {
                        SukiHost.ShowDialog(new ConnectionDialog("分区获取失败！"));
                    }
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog("请先选择需要读取的磁盘！"));
                }
                BusyPart.IsBusy = false;
                ReadPartBut.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("请将设备进入Recovery模式或系统后执行！"));
            }
        }
        else
        {
            SukiHost.ShowDialog(new ConnectionDialog("设备未连接！"));
        }
    }

    private async void RMPart(object sender, RoutedEventArgs args)
    {

    }

    private async void ESPON(object sender, RoutedEventArgs args)
    {

    }

    private async void MKPart(object sender, RoutedEventArgs args)
    {

    }

    private async void RemoveLimit(object sender, RoutedEventArgs args)
    {

    }
}