using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Dialogs;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Wiredflash;

public partial class WiredflashView : UserControl
{
    ISukiDialogManager dialogManager;
    static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);

    readonly string adb_log_path = Path.Combine(Global.log_path, "adb.txt");
    string output = "";

    public WiredflashView() => InitializeComponent();

    public async Task Fastboot(string fbshell)//Fastboot实时输出
    {
        await Task.Run(() =>
        {
            var cmd = Path.Combine(Global.bin_path, "platform-tools", "fastboot");

            var fastboot = new ProcessStartInfo(cmd, fbshell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var fb = new Process();
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
            var cmd = Path.Combine(Global.bin_path, "platform-tools", "adb");

            var adbexe = new ProcessStartInfo(cmd, adbshell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var adb = new Process();
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
            var wkdir = Path.Combine(Global.bin_path, "platform-tools");
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
            var wkdir = Path.Combine(Global.bin_path, "platform-tools");
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

    async void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (!string.IsNullOrEmpty(outLine.Data))
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var sb = new StringBuilder(WiredflashLog.Text);
                WiredflashLog.Text = sb.AppendLine(outLine.Data).ToString();
                WiredflashLog.ScrollToLine(StringHelper.TextBoxLine(WiredflashLog.Text));
                var op = new StringBuilder(output);
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

    async void OpenFastbootFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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

    async void OpenFastbootdFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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

    async void StartTXTFlash(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

            if (sukiViewModel.Status is "Fastboot" or "Fastbootd")
            {
                if (FastbootFile.Text != "" || FastbootdFile.Text != "")
                {
                    TXTFlashBusy(true);
                    var succ = true;
                    var fbtxt = FastbootFile.Text;
                    var fbdtxt = FastbootdFile.Text;
                    WiredflashLog.Text = "";
                    output = "";
                    string imgpath;

                    if (fbtxt != "")
                    {
                        imgpath = fbtxt[..fbtxt.LastIndexOf("/")] + "/images";
                        var fbparts = FileHelper.Readtxt(fbtxt);
                        var charSeparators = new char[] { '\r', '\n' };
                        var fbflashparts = fbparts.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                        // 机型识别
                        var c = 0;

                        if (fbflashparts[c].Contains("codename"))
                        {
                            var codename = sukiViewModel.CodeName;
                            var lines = fbflashparts[c].Split(':');
                            var devicename = lines[1];
                            c = 1;

                            if (codename != devicename)
                            {
                                WiredflashLog.Text = GetTranslation("Wiredflash_ModelError");
                                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_ModelErrorCantFlash")).Dismiss().ByClickingBackground().TryShow();
                                succ = false;
                                TXTFlashBusy(false);
                                return;
                            }
                        }

                        for (int i = 0 + c; i < fbflashparts.Length; i++)
                        {
                            var shell = string.Format($"-s {Global.thisdevice} flash {fbflashparts[i]} \"{imgpath}/{fbflashparts[i]}.img\"");
                            await Fastboot(shell);
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
                                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_FaildRestart")).Dismiss().ByClickingBackground().TryShow();
                                succ = false;
                                TXTFlashBusy(false);
                                return;
                            }

                            _ = await GetDevicesInfo.SetDevicesInfoLittle();
                        }

                        imgpath = fbdtxt[..fbdtxt.LastIndexOf("/")] + "/images";
                        var fbdparts = FileHelper.Readtxt(fbdtxt);
                        var charSeparators = new char[] { '\r', '\n' };
                        var fbdflashparts = fbdparts.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                        var c = 0;

                        if (fbdflashparts[c].Contains("codename"))
                        {
                            var codename = sukiViewModel.CodeName;
                            var lines = fbdflashparts[c].Split(':');
                            var devicename = lines[1];
                            c = 1;

                            if (codename != devicename)
                            {
                                WiredflashLog.Text = GetTranslation("Wiredflash_ModelError");
                                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_ModelErrorCantFlash")).Dismiss().ByClickingBackground().TryShow();
                                succ = false;
                                TXTFlashBusy(false);
                                return;
                            }
                        }

                        var slot = "";
                        var active = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} getvar current-slot");

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

                        var cow = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} getvar all");
                        string[] parts = { "odm", "system", "system_ext", "product", "vendor", "mi_ext" };

                        for (int i = 0; i < parts.Length; i++)
                        {
                            var cowpart = string.Format("{0}{1}-cow", parts[i], slot);

                            if (cow.Contains(cowpart))
                            {
                                var shell = string.Format($"-s {Global.thisdevice} delete-logical-partition {cowpart}");
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
                            var deleteslot = "";

                            if (slot == "_a")
                            {
                                deleteslot = "_b";
                            }
                            else if (slot == "_b")
                            {
                                deleteslot = "_a";
                            }

                            var part = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} getvar all");

                            for (int i = 0; i < parts.Length; i++)
                            {
                                var deletepart = string.Format("{0}{1}", parts[i], deleteslot);
                                var find = string.Format(":{0}:", deletepart);

                                if (part.Contains(find))
                                {
                                    var shell = string.Format($"-s {Global.thisdevice} delete-logical-partition {deletepart}");
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
                                var deletepart = string.Format("{0}{1}", fbdflashparts[i], slot);
                                var shell = string.Format($"-s {Global.thisdevice} delete-logical-partition {deletepart}");
                                await Fastboot(shell);
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
                                var makepart = string.Format("{0}{1}", fbdflashparts[i], slot);
                                var shell = string.Format($"-s {Global.thisdevice} create-logical-partition {makepart} 00");
                                await Fastboot(shell);
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
                                var shell = string.Format($"-s {Global.thisdevice} flash {fbdflashparts[i]} \"{imgpath}/{fbdflashparts[i]}.img\"");
                                await Fastboot(shell);
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

                        var result = false;

                        _ = dialogManager.CreateDialog()
        .WithTitle("Warn")
        .WithContent(GetTranslation("Wiredflash_ROMFlash"))
        .WithActionButton("Yes", _ => result = true, true)
        .WithActionButton("No", _ => result = false, true)
        .TryShow();

                        if (result == true)
                        {
                            await Fastboot($"-s {Global.thisdevice} reboot");
                        }
                    }
                    else
                    {
                        _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_FlashError")).Dismiss().ByClickingBackground().TryShow();
                    }

                    TXTFlashBusy(false);
                }
                else
                {
                    _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_SelectFlashFile")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
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

    async void OpenSideloadFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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

    async void OpenUpdatedFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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

    async void OpenBatFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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

    async void SetToA(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

            if (sukiViewModel.Status == "Fastboot")
            {
                await Fastboot($"-s {Global.thisdevice} set_active a");
            }
            else
            {
                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    async void SetToB(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

            if (sukiViewModel.Status == "Fastboot")
            {
                await Fastboot($"-s {Global.thisdevice} set_active b");
            }
            else
            {
                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
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

    async void StartFlash(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

            if (AdbSideloadFile.Text != "" && FastbootUpdatedFile.Text == "" && BatFile.Text == "")
            {
                if (sukiViewModel.Status == "Sideload")
                {
                    MoreFlashBusy(true);
                    output = "";
                    WiredflashLog.Text = "";
                    var shell = string.Format($"-s {Global.thisdevice} sideload \"{AdbSideloadFile.Text}\"");
                    await ADB(shell);
                    _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                    MoreFlashBusy(false);
                }
                else
                {
                    _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterSideload")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else if (AdbSideloadFile.Text == "" && FastbootUpdatedFile.Text != "" && BatFile.Text == "")
            {
                if (sukiViewModel.Status == "Fastboot")
                {
                    MoreFlashBusy(true);
                    output = "";
                    WiredflashLog.Text = "";
                    var shell = string.Format($"-s {Global.thisdevice} update \"{FastbootUpdatedFile.Text}\"");
                    await Fastboot(shell);
                    _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                    MoreFlashBusy(false);
                }
                else
                {
                    _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
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

                    _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                    MoreFlashBusy(false);
                }
                else
                {
                    _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                _ = AdbSideloadFile.Text == "" && FastbootUpdatedFile.Text == "" && BatFile.Text == ""
                    ? dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_SelectFlashFile")).Dismiss().ByClickingBackground().TryShow()
                    : dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_NoMul")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
        }
    }
}