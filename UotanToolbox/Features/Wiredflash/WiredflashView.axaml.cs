using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Common.PatchHelper;


namespace UotanToolbox.Features.Wiredflash;

public partial class WiredflashView : UserControl
{
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    private readonly string adb_log_path = Path.Combine(Global.log_path, "adb.txt");
    private string output = "";

    public WiredflashView()
    {
        InitializeComponent();
    }

    public async Task Fastboot(string fbshell)//Fastboot实时输出
    {
        await Task.Run(() =>
        {
            string cmd = Path.Combine(Global.bin_path, "platform-tools", "fastboot");
            ProcessStartInfo fastboot = new ProcessStartInfo(cmd, fbshell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using Process fb = new Process();
            fb.StartInfo = fastboot;
            _ = fb.Start();
            fb.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            fb.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            fb.BeginOutputReadLine();
            fb.BeginErrorReadLine();
            fb.WaitForExit();
            fb.Close();
        });
    }

    public async Task ADB(string adbshell)
    {
        await Task.Run(() =>
        {
            string cmd = Path.Combine(Global.bin_path, "platform-tools", "adb");
            ProcessStartInfo adbexe = new ProcessStartInfo(cmd, adbshell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using Process adb = new Process();
            adb.StartInfo = adbexe;
            _ = adb.Start();
            adb.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            adb.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            adb.BeginOutputReadLine();
            adb.BeginErrorReadLine();
            adb.WaitForExit();
            adb.Close();
        });
    }

    public async Task RunBat(string batpath)//调用Bat
    {
        await Task.Run(() =>
        {
            string wkdir = Path.Combine(Global.bin_path, "platform-tools");
            Process process = null;
            process = new Process();
            process.StartInfo.FileName = batpath;
            process.StartInfo.WorkingDirectory = wkdir;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            _ = process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.Close();
        });
    }

    public async Task RunSH(string shpath)
    {
        await Task.Run(() =>
        {
            string wkdir = Path.Combine(Global.bin_path, "platform-tools");
            Process process = null;
            process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{shpath}\"";
            process.StartInfo.WorkingDirectory = wkdir;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            _ = process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.Close();
        });
    }

    private async void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (!string.IsNullOrEmpty(outLine.Data))
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StringBuilder sb = new StringBuilder(WiredflashLog.Text);
                WiredflashLog.Text = sb.AppendLine(outLine.Data).ToString();
                WiredflashLog.CaretIndex = WiredflashLog.Text.Length;
                StringBuilder op = new StringBuilder(output);
                output = op.AppendLine(outLine.Data).ToString();
            });
        }
    }

    public static FilePickerFileType FastbootTXT { get; } = new("FastbootTXT")
    {
        Patterns = new[] { "*fastboot.txt" },
        AppleUniformTypeIdentifiers = new[] { "*fastboot.txt" }
    };

    public static FilePickerFileType FastbootdTXT { get; } = new("FastbootdTXT")
    {
        Patterns = new[] { "*fastbootd.txt" },
        AppleUniformTypeIdentifiers = new[] { "*fastbootd.txt" }
    };

    private async void OpenFastbootFile(object sender, RoutedEventArgs args)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            FileTypeFilter = new[] { FastbootTXT },
            Title = "Open TXT File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            FastbootFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    private async void OpenFastbootdFile(object sender, RoutedEventArgs args)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            FileTypeFilter = new[] { FastbootdTXT },
            Title = "Open TXT File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            FastbootdFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    private async void CheckRoot(object sender, RoutedEventArgs args)
    {
        Global.MainDialogManager.CreateDialog()
                            .WithTitle(GetTranslation("Home_Prompt"))
                            .WithContent(GetTranslation("Wiredflash_AddRootTip"))
                            .OfType(NotificationType.Information)
                            .WithActionButton(GetTranslation("Wiredflash_OpenAPK"), _ => FileHelper.OpenFolder(Path.Combine(Global.runpath, "APK")), true)
                            .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), _ => { }, true)
                            .TryShow();
    }

    public void TXTFlashBusy(bool is_busy)
    {
        if (is_busy)
        {
            Global.checkdevice = false;
            BusyTXTFlash.IsBusy = true;
            TXTFlash.IsEnabled = false;
        }
        else
        {
            Global.checkdevice = true;
            BusyTXTFlash.IsBusy = false;
            TXTFlash.IsEnabled = true;
        }
    }

    private async void StartTXTFlash(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status is "Fastboot" or "Fastbootd")
            {
                if (FastbootFile.Text != "" || FastbootdFile.Text != "")
                {
                    TXTFlashBusy(true);
                    bool succ = true;
                    string fbtxt = FastbootFile.Text;
                    string fbdtxt = FastbootdFile.Text;
                    WiredflashLog.Text = "";
                    output = "";
                    string imgpath;
                    if (fbtxt != "")
                    {
                        imgpath = fbtxt[..fbtxt.LastIndexOf('/')] + "/images";
                        string fbparts = FileHelper.Readtxt(fbtxt);
                        char[] charSeparators = new char[] { '\r', '\n' };
                        string[] fbflashparts = fbparts.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                        //机型识别
                        int c = 0;
                        if (fbflashparts[c].Contains("codename"))
                        {
                            string codename = sukiViewModel.CodeName;
                            string[] lines = fbflashparts[c].Split(':');
                            string devicename = lines[1];
                            c = 1;
                            if (codename != devicename)
                            {
                                WiredflashLog.Text = GetTranslation("Wiredflash_ModelError");
                                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_ModelErrorCantFlash")).Dismiss().ByClickingBackground().TryShow();
                                succ = false;
                                TXTFlashBusy(false);
                                return;
                            }
                        }
                        for (int i = 0 + c; i < fbflashparts.Length; i++)
                        {
                            if (fbflashparts[i].Contains(' '))
                            {
                                string[] partandpath = fbflashparts[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                string shell = string.Format($"-s {Global.thisdevice} flash {partandpath[0]} \"{fbtxt[..fbtxt.LastIndexOf('/')]}{partandpath[1]}\"");
                                await Fastboot(shell);
                            }
                            else
                            {
                                if (fbflashparts[i] == "boot" && (bool)AddRoot.IsChecked)
                                {
                                    WiredflashLog.Text += GetTranslation("Wiredflash_RepairBoot");
                                    Global.Bootinfo = await BootDetect.Boot_Detect($"{imgpath}/{fbflashparts[i]}.img");
                                    Global.Zipinfo = await ZipDetect.Zip_Detect(Path.Combine(Global.runpath, "APK", "Magisk.apk"));
                                    string newboot = await MagiskPatch.Magisk_Patch(Global.Zipinfo, Global.Bootinfo);
                                    string shell = string.Format($"-s {Global.thisdevice} flash boot {newboot}");
                                    await Fastboot(shell);
                                }
                                else
                                {
                                    string shell = string.Format($"-s {Global.thisdevice} flash {fbflashparts[i]} \"{imgpath}/{fbflashparts[i]}.img\"");
                                    await Fastboot(shell);
                                }
                            }
                            FileHelper.Write(adb_log_path, output);
                            if (output.Contains("FAILED") || output.Contains("error"))
                            {
                                succ = false;
                                break;
                            }
                        }
                    }
                    if (fbdtxt != "" && succ)
                    {
                        if (sukiViewModel.Status != "Fastbootd")
                        {
                            await Fastboot($"-s {Global.thisdevice} reboot fastboot");
                            FileHelper.Write(adb_log_path, output);
                            if (output.Contains("FAILED") || output.Contains("error"))
                            {
                                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_FaildRestart")).Dismiss().ByClickingBackground().TryShow();
                                succ = false;
                                TXTFlashBusy(false);
                                return;
                            }
                            _ = await GetDevicesInfo.SetDevicesInfoLittle();
                        }
                        imgpath = fbdtxt[..fbdtxt.LastIndexOf('/')] + "/images";
                        string fbdparts = FileHelper.Readtxt(fbdtxt);
                        char[] charSeparators = new char[] { '\r', '\n' };
                        string[] fbdflashparts = fbdparts.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                        int c = 0;
                        if (fbdflashparts[c].Contains("codename"))
                        {
                            string codename = sukiViewModel.CodeName;
                            string[] lines = fbdflashparts[c].Split(':');
                            string devicename = lines[1];
                            c = 1;
                            if (codename != devicename)
                            {
                                WiredflashLog.Text = GetTranslation("Wiredflash_ModelError");
                                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_ModelErrorCantFlash")).Dismiss().ByClickingBackground().TryShow();
                                succ = false;
                                TXTFlashBusy(false);
                                return;
                            }
                        }
                        string slot = "";
                        string active = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} getvar current-slot");
                        if (active.Contains("current-slot: a"))
                        {
                            slot = "_a";
                        }
                        else if (active.Contains("current-slot: b"))
                        {
                            slot = "_b";
                        }
                        else if (active.Contains("FAILED"))
                        {
                            slot = null;
                        }
                        string cow = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} getvar all");
                        string[] parts = { "odm", "system", "system_ext", "product", "vendor", "mi_ext" };
                        for (int i = 0; i < parts.Length; i++)
                        {
                            string cowpart = string.Format("{0}{1}-cow", parts[i], slot);
                            if (cow.Contains(cowpart))
                            {
                                string shell = string.Format($"-s {Global.thisdevice} delete-logical-partition {cowpart}");
                                await Fastboot(shell);
                            }
                            FileHelper.Write(adb_log_path, output);
                            if (output.Contains("FAILED") || output.Contains("error"))
                            {
                                succ = false;
                                break;
                            }
                        }
                        if (slot != null && succ)
                        {
                            string deleteslot = "";
                            if (slot == "_a")
                            {
                                deleteslot = "_b";
                            }
                            else if (slot == "_b")
                            {
                                deleteslot = "_a";
                            }
                            string part = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} getvar all");
                            for (int i = 0; i < parts.Length; i++)
                            {
                                string deletepart = string.Format("{0}{1}", parts[i], deleteslot);
                                string find = string.Format(":{0}:", deletepart);
                                if (part.Contains(find))
                                {
                                    string shell = string.Format($"-s {Global.thisdevice} delete-logical-partition {deletepart}");
                                    await Fastboot(shell);
                                }
                                FileHelper.Write(adb_log_path, output);
                                if (output.Contains("FAILED") || output.Contains("error"))
                                {
                                    succ = false;
                                    break;
                                }
                            }
                        }
                        if (succ)
                        {
                            for (int i = 0 + c; i < fbdflashparts.Length; i++)
                            {
                                if (fbdflashparts[i].Contains(' '))
                                {
                                    string[] partandpath = fbdflashparts[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                    string deletepart = string.Format("{0}{1}", partandpath[0], slot);
                                    string shell = string.Format($"-s {Global.thisdevice} delete-logical-partition {deletepart}");
                                    await Fastboot(shell);
                                }
                                else
                                {
                                    string deletepart = string.Format("{0}{1}", fbdflashparts[i], slot);
                                    string shell = string.Format($"-s {Global.thisdevice} delete-logical-partition {deletepart}");
                                    await Fastboot(shell);
                                }
                                FileHelper.Write(adb_log_path, output);
                                if (output.Contains("FAILED") || output.Contains("error"))
                                {
                                    succ = false;
                                    break;
                                }
                            }
                        }
                        if (succ)
                        {
                            for (int i = 0 + c; i < fbdflashparts.Length; i++)
                            {
                                if (fbdflashparts[i].Contains(' '))
                                {
                                    string[] partandpath = fbdflashparts[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                    string makepart = string.Format("{0}{1}", partandpath[0], slot);
                                    string shell = string.Format($"-s {Global.thisdevice} create-logical-partition {makepart} 00");
                                    await Fastboot(shell);
                                }
                                else
                                {
                                    string makepart = string.Format("{0}{1}", fbdflashparts[i], slot);
                                    string shell = string.Format($"-s {Global.thisdevice} create-logical-partition {makepart} 00");
                                    await Fastboot(shell);
                                }
                                FileHelper.Write(adb_log_path, output);
                                if (output.Contains("FAILED") || output.Contains("error"))
                                {
                                    succ = false;
                                    break;
                                }
                            }
                        }
                        if (succ)
                        {
                            for (int i = 0 + c; i < fbdflashparts.Length; i++)
                            {
                                if (fbdflashparts[i].Contains(' '))
                                {
                                    string[] partandpath = fbdflashparts[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                    string shell = string.Format($"-s {Global.thisdevice} flash {partandpath[0]} \"{fbdtxt[..fbdtxt.LastIndexOf('/')]}{partandpath[1]}\"");
                                    await Fastboot(shell);
                                }
                                else
                                {
                                    string shell = string.Format($"-s {Global.thisdevice} flash {fbdflashparts[i]} \"{imgpath}/{fbdflashparts[i]}.img\"");
                                    await Fastboot(shell);
                                }
                                FileHelper.Write(adb_log_path, output);
                                if (output.Contains("FAILED") || output.Contains("error"))
                                {
                                    succ = false;
                                    break;
                                }
                            }
                        }
                    }
                    if (succ)
                    {
                        if (ErasData.IsChecked == true)
                        {
                            await Fastboot($"-s {Global.thisdevice} erase metadata");
                            await Fastboot($"-s {Global.thisdevice} erase userdata");
                        }
                        Global.MainDialogManager.CreateDialog()
                            .WithTitle(GetTranslation("Common_Succ"))
                            .WithContent(GetTranslation("Wiredflash_ROMFlash"))
                            .OfType(NotificationType.Success)
                            .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ => await Fastboot($"-s {Global.thisdevice} reboot"), true)
                            .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                            .TryShow();
                    }
                    else
                    {
                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_FlashError")).Dismiss().ByClickingBackground().TryShow();
                    }
                    TXTFlashBusy(false);
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_SelectFlashFile")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    public static FilePickerFileType ZIP { get; } = new("ZIP")
    {
        Patterns = new[] { "*.zip" },
        AppleUniformTypeIdentifiers = new[] { "*.zip" }
    };

    public static FilePickerFileType Bat { get; } = new("Bat")
    {
        Patterns = new[] { "flash*.bat", "flash*.sh" },
        AppleUniformTypeIdentifiers = new[] { "flash*.bat", "flash*.sh" }
    };

    private async void OpenSideloadFile(object sender, RoutedEventArgs args)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            FileTypeFilter = new[] { ZIP },
            Title = "Open ZIP File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            AdbSideloadFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    private async void OpenUpdatedFile(object sender, RoutedEventArgs args)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            FileTypeFilter = new[] { ZIP },
            Title = "Open ZIP File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            FastbootUpdatedFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }
    private async void OpenBatFile(object sender, RoutedEventArgs args)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            FileTypeFilter = new[] { Bat },
            Title = "Open Bat File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            BatFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    private async void SetToA(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Fastboot")
            {
                await Fastboot($"-s {Global.thisdevice} set_active a");
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async void SetToB(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == "Fastboot")
            {
                await Fastboot($"-s {Global.thisdevice} set_active b");
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    public void MoreFlashBusy(bool is_busy)
    {
        if (is_busy)
        {
            Global.checkdevice = false;
            BusyFlash.IsBusy = true;
            MoreWiredFlash.IsEnabled = false;
        }
        else
        {
            Global.checkdevice = true;
            BusyFlash.IsBusy = false;
            MoreWiredFlash.IsEnabled = true;
        }
    }

    private async void StartFlash(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (AdbSideloadFile.Text != "" && FastbootUpdatedFile.Text == "" && BatFile.Text == "")
            {
                if (sukiViewModel.Status == GetTranslation("Home_Recovery"))
                {
                    string output = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell twrp sideload");
                    if (output.Contains("not found"))
                    {
                        await CallExternalProgram.ADB($"-s {Global.thisdevice} reboot sideload");
                    }
                    await Task.Delay(2000);
                    await GetDevicesInfo.SetDevicesInfoLittle();
                }
                if (sukiViewModel.Status == "Sideload")
                {
                    MoreFlashBusy(true);
                    output = "";
                    WiredflashLog.Text = "";
                    string shell = string.Format($"-s {Global.thisdevice} sideload \"{AdbSideloadFile.Text}\"");
                    await ADB(shell);
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                    MoreFlashBusy(false);
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterSideload")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else if (AdbSideloadFile.Text == "" && FastbootUpdatedFile.Text != "" && BatFile.Text == "")
            {
                if (sukiViewModel.Status == "Fastboot")
                {
                    MoreFlashBusy(true);
                    output = "";
                    WiredflashLog.Text = "";
                    string shell = string.Format($"-s {Global.thisdevice} update \"{FastbootUpdatedFile.Text}\"");
                    await Fastboot(shell);
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                    MoreFlashBusy(false);
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else if (AdbSideloadFile.Text == "" && FastbootUpdatedFile.Text == "" && BatFile.Text != "")
            {
                if (sukiViewModel.Status == "Fastboot")
                {
                    MoreFlashBusy(true);
                    output = "";
                    WiredflashLog.Text = "";
                    if (Global.System == "Windows")
                    {
                        await RunBat(BatFile.Text);
                    }
                    else
                    {
                        await RunSH(BatFile.Text);
                    }
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                    MoreFlashBusy(false);
                }
                else
                {
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                _ = AdbSideloadFile.Text == "" && FastbootUpdatedFile.Text == "" && BatFile.Text == ""
                    ? Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_SelectFlashFile")).Dismiss().ByClickingBackground().TryShow()
                    : Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_NoMul")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }
}