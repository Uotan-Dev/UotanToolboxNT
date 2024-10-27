using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UotanToolbox.Common;


namespace UotanToolbox.Features.Modifypartition;

public partial class ModifypartitionView : UserControl
{
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public ModifypartitionView()
    {
        InitializeComponent();
        _ = LoadMassage();
    }

    public async Task LoadMassage()
    {
        Global.MainDialogManager.CreateDialog()
            .WithTitle(GetTranslation("Common_Warn"))
            .WithContent(GetTranslation("Modifypartition_Warn"))
            .OfType(NotificationType.Warning)
            .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), _ => Modifypartition.IsEnabled = true, true)
            .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => Modifypartition.IsEnabled = false, true)
            .TryShow();
    }

    public void AddPartList()
    {
        string choice = "";
        if (sda.IsChecked != null && (bool)sda.IsChecked)
        {
            choice = Global.sdatable;
        }
        if (sdb.IsChecked != null && (bool)sdb.IsChecked)
        {
            choice = Global.sdbtable;
        }
        if (sdc.IsChecked != null && (bool)sdc.IsChecked)
        {
            choice = Global.sdctable;
        }
        if (sdd.IsChecked != null && (bool)sdd.IsChecked)
        {
            choice = Global.sddtable;
        }
        if (sde.IsChecked != null && (bool)sde.IsChecked)
        {
            choice = Global.sdetable;
        }
        if (sdf.IsChecked != null && (bool)sdf.IsChecked)
        {
            choice = Global.sdftable;
        }
        if (emmc.IsChecked != null && (bool)emmc.IsChecked)
        {
            choice = Global.emmcrom;
        }
        if (choice != "")
        {
            string[] parts = choice.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 6)
            {
                string size = string.Format("{0}", StringHelper.DiskSize(choice));
                PartSize.Text = size;
                PartModel[] part = new PartModel[parts.Length - 5];
                for (int i = 6; i < parts.Length; i++)
                {
                    string[] items = StringHelper.Items(parts[i].ToCharArray());
                    part[i - 6] = new PartModel(items[0], items[1], items[2], items[3], items[4], items[5], items[6]);
                }
                PartList.ItemsSource = part;
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Modifypartition_PartFailed")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Modifypartition_SelectDisk")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async void ReadPart(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Recovery" || sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                BusyPart.IsBusy = true;
                ReadPartBut.IsEnabled = false;
                PartList.ItemsSource = null;
                if (Global.sdatable == "" && sukiViewModel.Status == "Recovery")
                {
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp unmount data");
                }
                if (sukiViewModel.Status == GetTranslation("Home_Android"))
                {
                    Global.MainDialogManager.CreateDialog()
                        .WithTitle(GetTranslation("Common_Warn"))
                        .WithContent(GetTranslation("Common_NeedRoot"))
                        .OfType(NotificationType.Warning)
                        .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                        {
                            await FeaturesHelper.GetPartTableSystem(Global.thisdevice);
                            AddPartList();
                        }, true)
                        .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ =>
                        {
                            BusyPart.IsBusy = false;
                            ReadPartBut.IsEnabled = true;
                            return;
                        }, true)
                        .TryShow();
                }
                else
                {
                    await FeaturesHelper.GetPartTable(Global.thisdevice);
                    AddPartList();
                }
                BusyPart.IsBusy = false;
                ReadPartBut.IsEnabled = true;
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterRecOrOpenADB")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async void RMPart(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Recovery")
            {
                if (IdNumber.Text is not null and not "")
                {
                    RMPartBut.IsEnabled = false;
                    ESPONBut.IsEnabled = false;
                    string choice = "";
                    if (sda.IsChecked != null && (bool)sda.IsChecked)
                    {
                        choice = "sda";
                    }

                    if (sdb.IsChecked != null && (bool)sdb.IsChecked)
                    {
                        choice = "sdb";
                    }

                    if (sdc.IsChecked != null && (bool)sdc.IsChecked)
                    {
                        choice = "sdc";
                    }

                    if (sdd.IsChecked != null && (bool)sdd.IsChecked)
                    {
                        choice = "sdd";
                    }

                    if (sde.IsChecked != null && (bool)sde.IsChecked)
                    {
                        choice = "sde";
                    }

                    if (sdf.IsChecked != null && (bool)sdf.IsChecked)
                    {
                        choice = "sdf";
                    }

                    if (emmc.IsChecked != null && (bool)emmc.IsChecked)
                    {
                        choice = "mmcblk0";
                    }

                    if (choice != "")
                    {
                        Regex regex = new Regex("^(-?[0-9]*[.]*[0-9]{0,3})$");
                        if (regex.IsMatch(IdNumber.Text))
                        {
                            int partnum = StringHelper.Onlynum(IdNumber.Text);
                            string shell = string.Format($"-s {Global.thisdevice} shell /tmp/parted /dev/block/{choice} rm {partnum}");
                            _ = await CallExternalProgram.ADB(shell);
                            ReadPart(sender, args);
                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                        }
                        else
                        {
                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Modifypartition_EnterCorrNum")).Dismiss().ByClickingBackground().TryShow();
                        }
                    }
                    else
                    {
                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Modifypartition_SelectAndRead")).Dismiss().ByClickingBackground().TryShow();
                    }
                    RMPartBut.IsEnabled = true;
                    ESPONBut.IsEnabled = true;
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Modifypartition_EnterNum")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterRecovery")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async void ESPON(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Recovery")
            {
                Global.MainDialogManager.CreateDialog()
                                            .WithTitle(GetTranslation("Common_Warn"))
                                            .WithContent(GetTranslation("Modifypartition_SetEFI"))
                                            .OfType(NotificationType.Warning)
                                            .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                                            {
                                                if (IdNumber.Text is not null and not "")
                                                {
                                                    RMPartBut.IsEnabled = false;
                                                    ESPONBut.IsEnabled = false;
                                                    string choice = "";
                                                    if (sda.IsChecked != null && (bool)sda.IsChecked)
                                                    {
                                                        choice = "sda";
                                                    }

                                                    if (sdb.IsChecked != null && (bool)sdb.IsChecked)
                                                    {
                                                        choice = "sdb";
                                                    }

                                                    if (sdc.IsChecked != null && (bool)sdc.IsChecked)
                                                    {
                                                        choice = "sdc";
                                                    }

                                                    if (sdd.IsChecked != null && (bool)sdd.IsChecked)
                                                    {
                                                        choice = "sdd";
                                                    }

                                                    if (sde.IsChecked != null && (bool)sde.IsChecked)
                                                    {
                                                        choice = "sde";
                                                    }

                                                    if (sdf.IsChecked != null && (bool)sdf.IsChecked)
                                                    {
                                                        choice = "sdf";
                                                    }

                                                    if (emmc.IsChecked != null && (bool)emmc.IsChecked)
                                                    {
                                                        choice = "mmcblk0";
                                                    }

                                                    if (choice != "")
                                                    {
                                                        Regex regex = new Regex("^(-?[0-9]*[.]*[0-9]{0,3})$");
                                                        if (regex.IsMatch(IdNumber.Text))
                                                        {
                                                            int partnum = StringHelper.Onlynum(IdNumber.Text);
                                                            string shell = string.Format($"-s {Global.thisdevice} shell /tmp/parted /dev/block/{choice} set {partnum} esp on");
                                                            await CallExternalProgram.ADB(shell);
                                                            ReadPart(sender, args);
                                                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                                                        }
                                                        else
                                                        {
                                                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Modifypartition_EnterNum")).Dismiss().ByClickingBackground().TryShow();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Modifypartition_SelectAndRead")).Dismiss().ByClickingBackground().TryShow();
                                                    }
                                                    RMPartBut.IsEnabled = true;
                                                    ESPONBut.IsEnabled = true;
                                                }
                                                else
                                                {
                                                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Modifypartition_EnterNum")).Dismiss().ByClickingBackground().TryShow();
                                                }
                                            }, true)
                                            .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                                            .TryShow();
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterRecovery")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
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
                    {
                        choice = "sda";
                    }

                    if (sdb.IsChecked != null && (bool)sdb.IsChecked)
                    {
                        choice = "sdb";
                    }

                    if (sdc.IsChecked != null && (bool)sdc.IsChecked)
                    {
                        choice = "sdc";
                    }

                    if (sdd.IsChecked != null && (bool)sdd.IsChecked)
                    {
                        choice = "sdd";
                    }

                    if (sde.IsChecked != null && (bool)sde.IsChecked)
                    {
                        choice = "sde";
                    }

                    if (sdf.IsChecked != null && (bool)sdf.IsChecked)
                    {
                        choice = "sdf";
                    }

                    if (emmc.IsChecked != null && (bool)emmc.IsChecked)
                    {
                        choice = "mmcblk0";
                    }

                    if (choice != "")
                    {
                        string shell = string.Format($"-s {Global.thisdevice} shell /tmp/parted /dev/block/{choice} mkpart {NewPartitionName.Text} {NewPartitionFormat.Text} {NewPartitionStartpoint.Text} {NewPartitionEndpoint.Text}");
                        _ = await CallExternalProgram.ADB(shell);
                        ReadPart(sender, args);
                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                    }
                    else
                    {
                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Modifypartition_SelectAndRead")).Dismiss().ByClickingBackground().TryShow();
                    }
                    MKPartBut.IsEnabled = true;
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Modifypartition_EnterCreat")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterRecovery")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
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
                {
                    choice = "sda";
                }

                if (sdb.IsChecked != null && (bool)sdb.IsChecked)
                {
                    choice = "";
                }

                if (sdc.IsChecked != null && (bool)sdc.IsChecked)
                {
                    choice = "sdc";
                }

                if (sdd.IsChecked != null && (bool)sdd.IsChecked)
                {
                    choice = "sdd";
                }

                if (sde.IsChecked != null && (bool)sde.IsChecked)
                {
                    choice = "sde";
                }

                if (sdf.IsChecked != null && (bool)sdf.IsChecked)
                {
                    choice = "sdf";
                }

                if (emmc.IsChecked != null && (bool)emmc.IsChecked)
                {
                    choice = "mmcblk0";
                }

                if (choice != "")
                {
                    Global.MainDialogManager.CreateDialog()
                                                .WithTitle(GetTranslation("Common_Warn"))
                                                .WithContent(GetTranslation("Modifypartition_Set128"))
                                                .OfType(NotificationType.Warning)
                                                .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                                                {
                                                    await CallExternalProgram.ADB($"-s {Global.thisdevice} push {Global.runpath}/Push/sgdisk /tmp/");
                                                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell chmod +x /tmp/sgdisk");
                                                    string shell = string.Format($"-s {Global.thisdevice} shell /tmp/sgdisk --resize-table=128 /dev/block/{choice}");
                                                    string limit = await CallExternalProgram.ADB(shell);
                                                    if (!limit.Contains("completed successfully"))
                                                    {
                                                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_ExeFailed")).Dismiss().ByClickingBackground().TryShow();
                                                    }
                                                    else
                                                    {
                                                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                                                        await CallExternalProgram.ADB("reboot recovery");
                                                    }
                                                }, true)
                                                .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                                                .TryShow();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Modifypartition_SelectCorrPart")).Dismiss().ByClickingBackground().TryShow();
                }
                RemoveLimitBut.IsEnabled = true;
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterRecovery")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }
}