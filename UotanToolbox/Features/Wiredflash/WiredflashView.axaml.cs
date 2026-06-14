using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using UotanToolbox.Common;
using UotanToolbox.Common.Devices;
using UotanToolbox.Common.PatchHelper;
using ZstdSharp;


namespace UotanToolbox.Features.Wiredflash;

public partial class WiredflashView : UserControl
{
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    private readonly string fastboot_log_path = Path.Combine(Global.log_path, "fastboot.txt");
    private readonly string update_status = Path.Combine(Global.log_path, "update.txt");
    private string output = "";

    public WiredflashView()
    {
        InitializeComponent();
    }

    private async Task AppendCommandOutputAsync(string commandOutput)
    {
        if (commandOutput == null)
        {
            return;
        }

        string normalizedOutput = commandOutput
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Replace("\0", string.Empty);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            WiredflashLog.Text += normalizedOutput;
            WiredflashLog.CaretIndex = WiredflashLog.Text.Length;

            output += normalizedOutput;
        });
    }

    // helper to run fastboot via device manager or fallback
    private async Task<string> Fastboot(string cmd)
    {
        string commandOutput;
        if (Global.DeviceManager != null)
        {
            var dev = Global.DeviceManager.Devices.FirstOrDefault(d => d.Id == Global.thisdevice && d.Transport == TransportType.Fastboot);
            if (dev != null)
            {
                commandOutput = await Global.DeviceManager.ExecuteStreamingAsync(dev, cmd, chunk => _ = AppendCommandOutputAsync(chunk));
                return commandOutput;
            }
        }

        commandOutput = await CallExternalProgram.Fastboot(cmd, chunk => _ = AppendCommandOutputAsync(chunk));
        return commandOutput;
    }

    // helper to run adb via device manager or fallback
    private async Task<string> Adb(string cmd)
    {
        string commandOutput;
        if (Global.DeviceManager != null)
        {
            var dev = Global.DeviceManager.Devices.FirstOrDefault(d => d.Id == Global.thisdevice && d.Transport == TransportType.Adb);
            if (dev != null)
            {
                commandOutput = await Global.DeviceManager.ExecuteStreamingAsync(dev, cmd, chunk => _ = AppendCommandOutputAsync(chunk));
                return commandOutput;
            }
        }

        commandOutput = await CallExternalProgram.ADB(cmd, chunk => _ = AppendCommandOutputAsync(chunk));
        return commandOutput;
    }

    private async Task<string> RunProcessWithStreamingAsync(ProcessStartInfo startInfo)
    {
        using Process process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        _ = process.Start();

        var stdoutBuilder = new StringBuilder();
        var stderrBuilder = new StringBuilder();
        char[] buffer = new char[1024];

        async Task ReadStreamAsync(StreamReader reader, StringBuilder capture)
        {
            int read;
            while ((read = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                string chunk = new string(buffer, 0, read);
                _ = capture.Append(chunk);
                await AppendCommandOutputAsync(chunk);
            }
        }

        Task readStdoutTask = ReadStreamAsync(process.StandardOutput, stdoutBuilder);
        Task readStderrTask = ReadStreamAsync(process.StandardError, stderrBuilder);
        await Task.WhenAll(process.WaitForExitAsync(), readStdoutTask, readStderrTask);

        string stdout = stdoutBuilder.ToString().TrimEnd();
        string stderr = stderrBuilder.ToString().TrimEnd();
        if (string.IsNullOrWhiteSpace(stderr))
        {
            return stdout;
        }

        if (string.IsNullOrWhiteSpace(stdout))
        {
            return stderr;
        }

        return string.Concat(stdout, Environment.NewLine, stderr);
    }

    public async Task<string> RunBat(string batpath)
    {
        string wkdir = Path.Combine(Global.bin_path, "platform-tools");
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = batpath,
            WorkingDirectory = wkdir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        return await RunProcessWithStreamingAsync(startInfo);
    }

    public async Task<string> RunSH(string shpath)
    {
        string wkdir = Path.Combine(Global.bin_path, "platform-tools");
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{shpath}\"",
            WorkingDirectory = wkdir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        return await RunProcessWithStreamingAsync(startInfo);
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
                            .WithViewModel(_ => new SetMagiskDialogViewModel())
                            .TryShow();
    }

    private async void SetCommand(object sender, RoutedEventArgs args)
    {
        Global.MainDialogManager.CreateDialog()
                            .WithViewModel(_ => new SetVbmetaDialogViewModel())
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
        TXTFlashBusy(true);
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status is "Fastboot" or "Fastbootd")
            {
                if (!string.IsNullOrEmpty(FastbootFile.Text) || !string.IsNullOrEmpty(FastbootdFile.Text))
                {
                    bool succ = true;
                    string fbtxt = FastbootFile.Text;
                    string fbdtxt = FastbootdFile.Text;
                    WiredflashLog.Text = "";
                    output = "";
                    string imgpath;
                    if (!string.IsNullOrEmpty(fbtxt))
                    {
                        imgpath = fbtxt[..fbtxt.LastIndexOf('/')] + "/images";
                        string fbparts = FileHelper.Readtxt(fbtxt);
                        char[] charSeparators = ['\r', '\n'];
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
                                string filepath = fbtxt[..fbtxt.LastIndexOf('/')] + partandpath[1];
                                if (Path.GetExtension(filepath) == ".zst")
                                {
                                    WiredflashLog.Text += GetTranslation("Wiredflash_ZST");
                                    var zstfile = File.OpenRead(filepath);
                                    string zstname = Path.GetFileNameWithoutExtension(filepath);
                                    if (!zstname.Contains(".img"))
                                    {
                                        zstname = zstname + ".img";
                                    }
                                    string outfile = Path.Combine(Path.GetDirectoryName(filepath), zstname);
                                    var zstout = File.OpenWrite(outfile);
                                    var decompress = new DecompressionStream(zstfile);
                                    await decompress.CopyToAsync(zstout);
                                    decompress.Close();
                                    zstout.Close();
                                    zstfile.Close();
                                    filepath = outfile;
                                }
                                if (partandpath[0].Contains("vbmeta") && (bool)DisVbmeta.IsChecked)
                                {
                                    await Fastboot($"-s {Global.thisdevice} {Global.VbmetaCommand} flash {partandpath[0]} \"{filepath}\"");
                                }
                                else
                                {
                                    await Fastboot($"-s {Global.thisdevice} flash {partandpath[0]} \"{filepath}\"");
                                }
                            }
                            else
                            {
                                if ((fbflashparts[i] == Global.SetBoot) && (bool)AddRoot.IsChecked && !string.IsNullOrEmpty(Global.MagiskAPKPath))
                                {
                                    WiredflashLog.Text += GetTranslation("Wiredflash_RepairBoot");
                                    Global.Bootinfo = await ImageDetect.Boot_Detect($"{imgpath}/{fbflashparts[i]}.img");
                                    Global.Zipinfo = await PatchDetect.Patch_Detect(Global.MagiskAPKPath);
                                    string newboot = null;
                                    switch (Global.Zipinfo.Mode)
                                    {
                                        case PatchMode.Magisk:
                                            newboot = await MagiskPatch.Magisk_Patch_Mouzei(Global.Zipinfo, Global.Bootinfo);
                                            break;
                                        case PatchMode.GKI:
                                            newboot = await KernelSUPatch.GKI_Patch(Global.Zipinfo, Global.Bootinfo);
                                            break;
                                        case PatchMode.LKM:
                                            newboot = await KernelSUPatch.LKM_Patch(Global.Zipinfo, Global.Bootinfo);
                                            break;
                                    }
                                    await Fastboot($"-s {Global.thisdevice} flash {Global.SetBoot} \"{newboot}\"");
                                }
                                else if (fbflashparts[i].Contains("vbmeta") && (bool)DisVbmeta.IsChecked)
                                {
                                    await Fastboot($"-s {Global.thisdevice} {Global.VbmetaCommand} flash {fbflashparts[i]} \"{imgpath}/{fbflashparts[i]}.img\"");
                                }
                                else
                                {
                                    await Fastboot($"-s {Global.thisdevice} flash {fbflashparts[i]} \"{imgpath}/{fbflashparts[i]}.img\"");
                                }
                            }
                            FileHelper.Write(fastboot_log_path, output);
                            if (output.Contains("FAILED") || output.Contains("error"))
                            {
                                succ = false;
                                break;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(fbdtxt) && succ)
                    {
                        if (sukiViewModel.Status != "Fastbootd")
                        {
                            await Fastboot($"-s {Global.thisdevice} reboot fastboot");
                            FileHelper.Write(fastboot_log_path, output);
                            if (output.Contains("FAILED") || output.Contains("error"))
                            {
                                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_FaildRestart")).Dismiss().ByClickingBackground().TryShow();
                                succ = false;
                                TXTFlashBusy(false);
                                return;
                            }
                            await GetDevicesInfo.SetDevicesInfoLittle();
                            await Task.Delay(5000);
                            await GetDevicesInfo.SetDevicesInfoLittle();
                        }
                        imgpath = fbdtxt[..fbdtxt.LastIndexOf('/')] + "/images";
                        string fbdparts = FileHelper.Readtxt(fbdtxt);
                        char[] charSeparators = ['\r', '\n'];
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
                        if (succ)
                        {
                            for (int i = 0 + c; i < fbdflashparts.Length; i++)
                            {
                                if (fbdflashparts[i].Contains(' '))
                                {
                                    string[] partandpath = fbdflashparts[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                    if ((!partandpath[1].Contains("delete")) && (!partandpath[1].Contains("create")))
                                    {
                                        if (partandpath[0].Contains("super_empty"))
                                        {
                                            await Fastboot($"-s {Global.thisdevice} wipe-super \"{fbdtxt[..fbdtxt.LastIndexOf('/')]}{partandpath[1]}\"");
                                        }
                                    }
                                }
                                else
                                {
                                    if (fbdflashparts[i].Contains("super_empty"))
                                    {
                                        await Fastboot($"-s {Global.thisdevice} wipe-super \"{imgpath}/{fbdflashparts[i]}.img\"");
                                    }
                                }
                                FileHelper.Write(fastboot_log_path, output);
                                if (output.Contains("FAILED") || output.Contains("error"))
                                {
                                    succ = false;
                                    break;
                                }
                            }
                        }
                        string slot = "";
                        FileHelper.Write(update_status, await Fastboot($"-s {Global.thisdevice} getvar snapshot-update-status"));
                        string active = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} getvar current-slot");
                        if (active.Contains("current-slot: a"))
                        {
                            slot = "_a";
                        }
                        else if (active.Contains("current-slot: b"))
                        {
                            slot = "_b";
                        }
                        else
                        {
                            slot = null;
                        }
                        string cow = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} getvar all");
                        string[] cowparts = FeaturesHelper.GetVPartList(cow);
                        for (int i = 0; i < cowparts.Length; i++)
                        {
                            if (cowparts[i].Contains("-cow"))
                            {
                                await Fastboot($"-s {Global.thisdevice} delete-logical-partition {cowparts[i]}");
                            }
                            FileHelper.Write(fastboot_log_path, output);
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
                            string[] deleteslotparts = FeaturesHelper.GetVPartList(part);
                            for (int i = 0; i < deleteslotparts.Length; i++)
                            {
                                if (deleteslotparts[i].EndsWith(deleteslot))
                                {
                                    await Fastboot($"-s {Global.thisdevice} delete-logical-partition {deleteslotparts[i]}");
                                }
                                FileHelper.Write(fastboot_log_path, output);
                                if (output.Contains("FAILED") || output.Contains("error"))
                                {
                                    succ = false;
                                    break;
                                }
                            }
                        }
                        if (succ)
                        {
                            string part = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} getvar all");
                            string[] vparts = FeaturesHelper.GetVPartList(part);
                            for (int i = 0 + c; i < fbdflashparts.Length; i++)
                            {
                                if (fbdflashparts[i].Contains(' '))
                                {
                                    string[] partandpath = fbdflashparts[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                    string dmpart = string.Format("{0}{1}", partandpath[0], slot);
                                    if (Array.Exists(vparts, element => element == dmpart) && (!partandpath[1].Contains("create")))
                                    {
                                        await Fastboot($"-s {Global.thisdevice} delete-logical-partition {dmpart}");
                                    }
                                    if ((Array.Exists(vparts, element => element == dmpart) && (!partandpath[1].Contains("delete")) && (!partandpath[1].Contains("create"))) || (!Array.Exists(vparts, element => element == dmpart) && partandpath[1].Contains("create") && (!partandpath[1].StartsWith('/'))))
                                    {
                                        await Fastboot($"-s {Global.thisdevice} create-logical-partition {dmpart} 00");
                                    }
                                }
                                else
                                {
                                    string dmpart = string.Format("{0}{1}", fbdflashparts[i], slot);
                                    if (Array.Exists(vparts, element => element == dmpart))
                                    {
                                        await Fastboot($"-s {Global.thisdevice} delete-logical-partition {dmpart}");
                                        await Fastboot($"-s {Global.thisdevice} create-logical-partition {dmpart} 00");
                                    }
                                }
                                FileHelper.Write(fastboot_log_path, output);
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
                                    if ((!partandpath[1].Contains("delete")) && (!partandpath[1].Contains("create")))
                                    {
                                        if (!partandpath[0].Contains("super_empty"))
                                        {
                                            if (partandpath[0].Contains("vbmeta") && (bool)DisVbmeta.IsChecked)
                                            {
                                                await Fastboot($"-s {Global.thisdevice} {Global.VbmetaCommand} flash {partandpath[0]} \"{fbdtxt[..fbdtxt.LastIndexOf('/')]}{partandpath[1]}\"");
                                            }
                                            else
                                            {
                                                await Fastboot($"-s {Global.thisdevice} flash {partandpath[0]} \"{fbdtxt[..fbdtxt.LastIndexOf('/')]}{partandpath[1]}\"");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (!fbdflashparts[i].Contains("super_empty"))
                                    {
                                        if (fbdflashparts[i].Contains("vbmeta") && (bool)DisVbmeta.IsChecked)
                                        {
                                            await Fastboot($"-s {Global.thisdevice} {Global.VbmetaCommand} flash {fbdflashparts[i]} \"{imgpath}/{fbdflashparts[i]}.img\"");
                                        }
                                        else
                                        {
                                            await Fastboot($"-s {Global.thisdevice} flash {fbdflashparts[i]} \"{imgpath}/{fbdflashparts[i]}.img\"");
                                        }
                                    }
                                }
                                FileHelper.Write(fastboot_log_path, output);
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
                    TXTFlashBusy(false);
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
                TXTFlashBusy(false);
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_NotConnected")).Dismiss().ByClickingBackground().TryShow();
            TXTFlashBusy(false);
        }
    }

    public static FilePickerFileType ZIP { get; } = new("ZIP")
    {
        Patterns = new[] { "*.zip" },
        AppleUniformTypeIdentifiers = new[] { "*.zip" }
    };

    public static FilePickerFileType Bat { get; } = new("Bat")
    {
        Patterns = new[] { "flash*.bat" },
        AppleUniformTypeIdentifiers = new[] { "flash*.bat" }
    };

    public static FilePickerFileType Sh { get; } = new("Sh")
    {
        Patterns = new[] { "flash*.sh" },
        AppleUniformTypeIdentifiers = new[] { "flash*.sh" }
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
            AdbSideloadFile.Text = files[0].TryGetLocalPath();
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
            FastbootUpdatedFile.Text = files[0].TryGetLocalPath();
        }
    }
    private async void OpenBatFile(object sender, RoutedEventArgs args)
    {
        if (Global.System == "Windows")
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
                BatFile.Text = files[0].TryGetLocalPath();
                string batfile = FileHelper.Readtxt(BatFile.Text);
                if (batfile.Contains("oem lock"))
                {
                    Global.MainDialogManager.CreateDialog()
                          .WithTitle(GetTranslation("Common_Warn"))
                          .WithContent(GetTranslation("Wiredflash_RelockTip"))
                          .OfType(NotificationType.Warning)
                          .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), _ => { }, true)
                          .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => BatFile.Text = null, true)
                          .TryShow();
                }
            }
        }
        else
        {
            TopLevel topLevel = TopLevel.GetTopLevel(this);
            System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                FileTypeFilter = new[] { Sh },
                Title = "Open Sh File",
                AllowMultiple = false
            });
            if (files.Count >= 1)
            {
                BatFile.Text = files[0].TryGetLocalPath();
                string batfile = FileHelper.Readtxt(BatFile.Text);
                if (batfile.Contains("oem lock"))
                {
                    Global.MainDialogManager.CreateDialog()
                          .WithTitle(GetTranslation("Common_Warn"))
                          .WithContent(GetTranslation("Wiredflash_RelockTip"))
                          .OfType(NotificationType.Warning)
                          .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), _ => { }, true)
                          .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => BatFile.Text = null, true)
                          .TryShow();
                }
            }
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
            if (!string.IsNullOrEmpty(AdbSideloadFile.Text) && string.IsNullOrEmpty(FastbootUpdatedFile.Text) && string.IsNullOrEmpty(BatFile.Text))
            {
                if (SoluTwo.IsChecked == true)
                {
                    if (sukiViewModel.Status == GetTranslation("Home_Recovery"))
                    {
                        MoreFlashBusy(true);
                        output = "";
                        WiredflashLog.Text = "推送 Rom 中可能需要一些时间，请稍候... \r\nPushing Rom may take some time, please wait...\r\n";
                        await Adb($"-s {Global.thisdevice} push -p \"{AdbSideloadFile.Text}\" /data/media/0/update.zip");
                        await Adb($"-s {Global.thisdevice} push \"{Path.Combine(Global.runpath, "Push", "testScript.txt")}\" /cache/recovery/openrecoveryscript");
                        await Adb($"-s {Global.thisdevice} reboot recovery");
                        WiredflashLog.Text += "自动重启后将自动进行安装！ \r\nAfter automatic reboot, the installation will start automatically！\r\n";
                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                        MoreFlashBusy(false);
                    }
                }
                else
                {
                    if (sukiViewModel.Status == GetTranslation("Home_Recovery"))
                    {
                        string precheckOutput = await Adb($"-s {Global.thisdevice} shell twrp sideload");
                        if (precheckOutput.Contains("not found", StringComparison.OrdinalIgnoreCase))
                        {
                            await Adb($"-s {Global.thisdevice} reboot sideload");
                        }
                        await Task.Delay(2000);
                        await GetDevicesInfo.SetDevicesInfoLittle();
                    }
                    if (sukiViewModel.Status == GetTranslation("Home_Sideload"))
                    {
                        MoreFlashBusy(true);
                        output = "";
                        WiredflashLog.Text = "";
                        string shell = string.Format($"-s {Global.thisdevice} sideload \"{AdbSideloadFile.Text}\"");
                        await Adb(shell);
                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Execution")).OfType(NotificationType.Information).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
                        MoreFlashBusy(false);
                    }
                    else
                    {
                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterSideload")).Dismiss().ByClickingBackground().TryShow();
                    }
                }
            }
            else if (string.IsNullOrEmpty(AdbSideloadFile.Text) && !string.IsNullOrEmpty(FastbootUpdatedFile.Text) && string.IsNullOrEmpty(BatFile.Text))
            {
                if (sukiViewModel.Status == GetTranslation("Home_Fastboot"))
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
            else if (string.IsNullOrEmpty(AdbSideloadFile.Text) && string.IsNullOrEmpty(FastbootUpdatedFile.Text) && !string.IsNullOrEmpty(BatFile.Text))
            {
                if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
                {
                    MoreFlashBusy(true);
                    output = "";
                    WiredflashLog.Text = "";
                    if (Global.System == "Windows")
                    {
                        _ = await RunBat(BatFile.Text);
                    }
                    else
                    {
                        _ = await RunSH(BatFile.Text);
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
                _ = string.IsNullOrEmpty(AdbSideloadFile.Text) && string.IsNullOrEmpty(FastbootUpdatedFile.Text) && string.IsNullOrEmpty(BatFile.Text)
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