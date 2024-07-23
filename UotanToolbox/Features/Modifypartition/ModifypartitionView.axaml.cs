using Avalonia.Controls;
using Avalonia.Interactivity;
using SukiUI.Controls;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Features.Modifypartition;

public partial class ModifypartitionView : UserControl
{
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public ModifypartitionView()
    {
        InitializeComponent();
        _ = LoadMassage();
    }

    public async Task LoadMassage()
    {
        var newDialog = new ConnectionDialog(GetTranslation("Modifypartition_Warn"));
        await SukiHost.ShowDialogAsync(newDialog);
        if (newDialog.Result == true)
        {
            Modifypartition.IsEnabled = true;
        }
        else
        {
            Modifypartition.IsEnabled = false;
        }
    }

    private async void ReadPart(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Recovery" || sukiViewModel.Status == GetTranslation("Home_System"))
            {
                BusyPart.IsBusy = true;
                ReadPartBut.IsEnabled = false;
                PartList.ItemsSource = null;
                if (Global.sdatable == "" && sukiViewModel.Status == "Recovery")
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp unmount data");
                }
                if (sukiViewModel.Status == GetTranslation("Home_System"))
                {
                    var newDialog = new ConnectionDialog(GetTranslation("Common_NeedRoot"));
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
                        PartModel[] part = new PartModel[parts.Length - 5];
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
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Modifypartition_PartFailed")), allowBackgroundClose: true);
                    }
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Modifypartition_SelectDisk")), allowBackgroundClose: true);
                }
                BusyPart.IsBusy = false;
                ReadPartBut.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterRecOrOpenADB")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
        }
    }

    private async void RMPart(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Recovery")
            {
                if (IdNumber.Text != null && IdNumber.Text != "")
                {
                    RMPartBut.IsEnabled = false;
                    ESPONBut.IsEnabled = false;
                    string choice = "";
                    if (sda.IsChecked != null && (bool)sda.IsChecked)
                        choice = "sda";
                    if (sdb.IsChecked != null && (bool)sdb.IsChecked)
                        choice = "sdb";
                    if (sdc.IsChecked != null && (bool)sdc.IsChecked)
                        choice = "sdc";
                    if (sdd.IsChecked != null && (bool)sdd.IsChecked)
                        choice = "sdd";
                    if (sde.IsChecked != null && (bool)sde.IsChecked)
                        choice = "sde";
                    if (sdf.IsChecked != null && (bool)sdf.IsChecked)
                        choice = "sdf";
                    if (emmc.IsChecked != null && (bool)emmc.IsChecked)
                        choice = "mmcblk0";
                    if (choice != "")
                    {
                        Regex regex = new Regex("^(-?[0-9]*[.]*[0-9]{0,3})$");
                        if (regex.IsMatch(IdNumber.Text))
                        {
                            int partnum = StringHelper.Onlynum(IdNumber.Text);
                            string shell = String.Format($"-s {Global.thisdevice} shell /tmp/parted /dev/block/{choice} rm {partnum}");
                            await CallExternalProgram.ADB(shell);
                            ReadPart(sender, args);
                            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_Execution")), allowBackgroundClose: true);
                        }
                        else
                        {
                            SukiHost.ShowDialog(new PureDialog(GetTranslation("Modifypartition_EnterCorrNum")), allowBackgroundClose: true);
                        }
                    }
                    else
                    {
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Modifypartition_SelectAndRead")), allowBackgroundClose: true);
                    }
                    RMPartBut.IsEnabled = true;
                    ESPONBut.IsEnabled = true;
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Modifypartition_EnterNum")), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterRecovery")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
        }
    }

    private async void ESPON(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Recovery")
            {
                var newDialog = new ConnectionDialog(GetTranslation("Modifypartition_SetEFI"));
                await SukiHost.ShowDialogAsync(newDialog);
                if (newDialog.Result == true)
                {
                    if (IdNumber.Text != null && IdNumber.Text != "")
                    {
                        RMPartBut.IsEnabled = false;
                        ESPONBut.IsEnabled = false;
                        string choice = "";
                        if (sda.IsChecked != null && (bool)sda.IsChecked)
                            choice = "sda";
                        if (sdb.IsChecked != null && (bool)sdb.IsChecked)
                            choice = "sdb";
                        if (sdc.IsChecked != null && (bool)sdc.IsChecked)
                            choice = "sdc";
                        if (sdd.IsChecked != null && (bool)sdd.IsChecked)
                            choice = "sdd";
                        if (sde.IsChecked != null && (bool)sde.IsChecked)
                            choice = "sde";
                        if (sdf.IsChecked != null && (bool)sdf.IsChecked)
                            choice = "sdf";
                        if (emmc.IsChecked != null && (bool)emmc.IsChecked)
                            choice = "mmcblk0";
                        if (choice != "")
                        {
                            Regex regex = new Regex("^(-?[0-9]*[.]*[0-9]{0,3})$");
                            if (regex.IsMatch(IdNumber.Text))
                            {
                                int partnum = StringHelper.Onlynum(IdNumber.Text);
                                string shell = String.Format($"-s {Global.thisdevice} shell /tmp/parted /dev/block/{choice} set {partnum} esp on");
                                await CallExternalProgram.ADB(shell);
                                ReadPart(sender, args);
                                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_Execution")), allowBackgroundClose: true);
                            }
                            else
                            {
                                SukiHost.ShowDialog(new PureDialog(GetTranslation("Modifypartition_EnterNum")), allowBackgroundClose: true);
                            }
                        }
                        else
                        {
                            SukiHost.ShowDialog(new PureDialog(GetTranslation("Modifypartition_SelectAndRead")), allowBackgroundClose: true);
                        }
                        RMPartBut.IsEnabled = true;
                        ESPONBut.IsEnabled = true;
                    }
                    else
                    {
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Modifypartition_EnterNum")), allowBackgroundClose: true);
                    }
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterRecovery")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
        }
    }

    private async void MKPart(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Recovery")
            {
                if (NewPartitionName.Text != "" && NewPartitionFormat.Text != "" && NewPartitionStartpoint.Text != "" && NewPartitionEndpoint.Text != "")
                {
                    MKPartBut.IsEnabled = false;
                    string choice = "";
                    if (sda.IsChecked != null && (bool)sda.IsChecked)
                        choice = "sda";
                    if (sdb.IsChecked != null && (bool)sdb.IsChecked)
                        choice = "sdb";
                    if (sdc.IsChecked != null && (bool)sdc.IsChecked)
                        choice = "sdc";
                    if (sdd.IsChecked != null && (bool)sdd.IsChecked)
                        choice = "sdd";
                    if (sde.IsChecked != null && (bool)sde.IsChecked)
                        choice = "sde";
                    if (sdf.IsChecked != null && (bool)sdf.IsChecked)
                        choice = "sdf";
                    if (emmc.IsChecked != null && (bool)emmc.IsChecked)
                        choice = "mmcblk0";
                    if (choice != "")
                    {
                        string shell = String.Format($"-s {Global.thisdevice} shell /tmp/parted /dev/block/{choice} mkpart {NewPartitionName.Text} {NewPartitionFormat.Text} {NewPartitionStartpoint.Text} {NewPartitionEndpoint.Text}");
                        await CallExternalProgram.ADB(shell);
                        ReadPart(sender, args);
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_Execution")), allowBackgroundClose: true);
                    }
                    else
                    {
                        SukiHost.ShowDialog(new PureDialog(GetTranslation("Modifypartition_SelectAndRead")), allowBackgroundClose: true);
                    }
                    MKPartBut.IsEnabled = true;
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Modifypartition_EnterCreat")), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterRecovery")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
        }
    }

    private async void RemoveLimit(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Recovery")
            {
                RemoveLimitBut.IsEnabled = false;
                string choice = "";
                if (sda.IsChecked != null && (bool)sda.IsChecked)
                    choice = "sda";
                if (sdb.IsChecked != null && (bool)sdb.IsChecked)
                    choice = "";
                if (sdc.IsChecked != null && (bool)sdc.IsChecked)
                    choice = "sdc";
                if (sdd.IsChecked != null && (bool)sdd.IsChecked)
                    choice = "sdd";
                if (sde.IsChecked != null && (bool)sde.IsChecked)
                    choice = "sde";
                if (sdf.IsChecked != null && (bool)sdf.IsChecked)
                    choice = "sdf";
                if (emmc.IsChecked != null && (bool)emmc.IsChecked)
                    choice = "mmcblk0";
                if (choice != "")
                {
                    var newDialog = new ConnectionDialog(GetTranslation("Modifypartition_Set128"));
                    await SukiHost.ShowDialogAsync(newDialog);
                    if (newDialog.Result == true)
                    {
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} push {Global.runpath}/Push/sgdisk /tmp/");
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} shell chmod +x /tmp/sgdisk");
                        string shell = String.Format($"-s {Global.thisdevice} shell /tmp/sgdisk --resize-table=128 /dev/block/{choice}");
                        string limit = await CallExternalProgram.ADB(shell);
                        if (!limit.Contains("completed successfully"))
                        {
                            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_ExeFailed")), allowBackgroundClose: true);
                        }
                        else
                        {
                            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_Execution")), allowBackgroundClose: true);
                            await CallExternalProgram.ADB("reboot recovery");
                        }
                    }
                }
                else
                {
                    SukiHost.ShowDialog(new PureDialog(GetTranslation("Modifypartition_SelectCorrPart")), allowBackgroundClose: true);
                }
                RemoveLimitBut.IsEnabled = true;
            }
            else
            {
                SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_EnterRecovery")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
        }
    }
}