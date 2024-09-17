using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Appmgr;

public partial class AppmgrViewModel : MainPageBase
{
    [ObservableProperty]
    private ObservableCollection<ApplicationInfo> applications = [];
    [ObservableProperty]
    private bool isBusy = false, hasItems = false;
    [ObservableProperty]
    private bool isSystemAppDisplayed = false, isInstalling = false;
    [ObservableProperty]
    private string _apkFile;
    private ISukiDialogManager dialogManager;
    private ISukiToastManager toastManager;
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public AppmgrViewModel() : base(GetTranslation("Sidebar_Appmgr"), MaterialIconKind.ViewGridPlusOutline, -700)
    {
    }

    [RelayCommand]
    public async Task Connect()
    {
        HasItems = false;
        MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
        IsBusy = true;
        await Task.Run(async () =>
        {
            if (!await GetDevicesInfo.SetDevicesInfoLittle())
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _ = dialogManager.CreateDialog()
                                .OfType(NotificationType.Error)
                                .WithTitle("Error")
                                .WithContent(GetTranslation("Common_NotConnected"))
                                .Dismiss().ByClickingBackground()
                                .TryShow();
                });
                IsBusy = false; return;
            }
            string fullApplicationsList = !IsSystemAppDisplayed
                ? await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm list packages -3")
                : await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm list packages");
            if (fullApplicationsList.Contains("cannot connect to daemon"))
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _ = dialogManager.CreateDialog()
                                .OfType(NotificationType.Error)
                                .WithTitle("Error")
                                .WithContent(GetTranslation("Common_DeviceFailedToConnect"))
                                .Dismiss().ByClickingBackground()
                                .TryShow();
                });
                IsBusy = false; return;
            }
            if (!(sukiViewModel.Status == GetTranslation("Home_System")))
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _ = dialogManager.CreateDialog()
                                .OfType(NotificationType.Error)
                                .WithTitle("Error")
                                .WithContent(GetTranslation("Appmgr_PleaseExecuteInSystem"))
                                .Dismiss().ByClickingBackground()
                                .TryShow();
                });
                IsBusy = false; return;
            }
            string[] lines = fullApplicationsList.Split(separatorArray, StringSplitOptions.RemoveEmptyEntries);
            HasItems = lines.Length > 0;
            System.Collections.Generic.IEnumerable<Task<ApplicationInfo>> applicationInfosTasks = lines.Select(async line =>
            {
                string packageName = ExtractPackageName(line);
                if (string.IsNullOrEmpty(packageName))
                {
                    return null;
                }

                string combinedOutput = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dumpsys package {packageName}");
                string[] splitOutput = combinedOutput.Split('\n', ' ');
                string otherInfo = GetVersionName(splitOutput) + " | " + GetInstalledDate(splitOutput) + " | " + GetSdkVersion(splitOutput);
                return new ApplicationInfo { Name = packageName, OtherInfo = otherInfo };
            });
            ApplicationInfo[] allApplicationInfos = await Task.WhenAll(applicationInfosTasks);
            System.Collections.Generic.List<ApplicationInfo> applicationInfos = allApplicationInfos.Where(info => info != null)
                                                     .OrderByDescending(app => app.Size)
                                                     .ThenBy(app => app.Name)
                                                     .ToList();
            Applications = new ObservableCollection<ApplicationInfo>(applicationInfos);
            IsBusy = false;
        });


        static string ExtractPackageName(string line)
        {
            string[] parts = line.Split(':');
            if (parts.Length < 2)
            {
                return null;
            }

            string packageNamePart = parts[1];
            int packageNameStartIndex = packageNamePart.LastIndexOf('/') + 1;
            return packageNameStartIndex < packageNamePart.Length
                ? packageNamePart[packageNameStartIndex..]
                : null;
        }
    }

    [RelayCommand]
    public async Task InstallApk()
    {
        IsInstalling = true;
        if (!string.IsNullOrEmpty(ApkFile))
        {
            if (!await GetDevicesInfo.SetDevicesInfoLittle())
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _ = dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent(GetTranslation("Common_NotConnected"))
            .Dismiss().ByClickingBackground()
            .TryShow();
                });
                IsInstalling = false; return;
            }
            string[] fileArray = ApkFile.Split("|||");
            for (int i = 0; i < fileArray.Length; i++)
            {
                if (!string.IsNullOrEmpty(fileArray[i]))
                {
                    string output = await CallExternalProgram.ADB($"-s {Global.thisdevice} install -r \"{fileArray[i]}\"");
                    _ = output.Contains("Success")
                        ? toastManager.CreateToast()
    .WithTitle("Info")
    .WithContent(GetTranslation("Common_InstallSuccess"))
    .OfType(NotificationType.Success)
    .Dismiss().ByClicking()
    .Dismiss().After(TimeSpan.FromSeconds(3))
    .Queue()
                        : toastManager.CreateToast()
.WithTitle("Info")
.WithContent(GetTranslation("Common_InstallFailed"))
.OfType(NotificationType.Error)
.Dismiss().ByClicking()
.Dismiss().After(TimeSpan.FromSeconds(3))
.Queue();
                }
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent(GetTranslation("Appmgr_NoApkFileSelected"))
            .Dismiss().ByClickingBackground()
            .TryShow();
        }
        IsInstalling = false;
    }

    [RelayCommand]
    public async Task RunApp()
    {
        await Task.Run(async () =>
        {
            IsBusy = true;
            if (!await GetDevicesInfo.SetDevicesInfoLittle())
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _ = dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent(GetTranslation("Common_NotConnected"))
            .Dismiss().ByClickingBackground()
            .TryShow();
                });
                IsBusy = false; return;
            }
            if (SelectedApplication() != "")
            {
                _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell monkey -p {SelectedApplication()} 1");
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _ = dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent(GetTranslation("Appmgr_AppIsNotSelected"))
            .Dismiss().ByClickingBackground()
            .TryShow();
                });
            }

            IsBusy = false;
        });
    }

    [RelayCommand]
    public async Task DisableApp()
    {
        await Task.Run(async () =>
        {
            IsBusy = true;
            if (!await GetDevicesInfo.SetDevicesInfoLittle())
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _ = dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent(GetTranslation("Common_NotConnected"))
            .Dismiss().ByClickingBackground()
            .TryShow();
                });
                IsBusy = false; return;
            }
            if (SelectedApplication() != "")
            {
                _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm disable {SelectedApplication()}");
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _ = dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent(GetTranslation("Appmgr_AppIsNotSelected"))
            .Dismiss().ByClickingBackground()
            .TryShow();
                });
            }
            IsBusy = false;
        });
    }

    [RelayCommand]
    public async Task EnableApp()
    {
        IsBusy = true;
        if (!await GetDevicesInfo.SetDevicesInfoLittle())
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _ = dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent(GetTranslation("Common_NotConnected"))
            .Dismiss().ByClickingBackground()
            .TryShow();
            });
            IsBusy = false; return;
        }
        string selectedApp = SelectedApplication();
        if (!string.IsNullOrEmpty(selectedApp))
        {
            _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm enable {selectedApp}");
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _ = dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent(GetTranslation("Appmgr_AppIsNotSelected"))
            .Dismiss().ByClickingBackground()
            .TryShow();
            });
        }
        IsBusy = false;
    }
    [RelayCommand]
    public async Task UninstallApp()
    {
        IsBusy = true;
        if (!await GetDevicesInfo.SetDevicesInfoLittle())
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _ = dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent(GetTranslation("Common_NotConnected"))
            .Dismiss().ByClickingBackground()
            .TryShow();
            });
            IsBusy = false; return;
        }
        string selectedApp = SelectedApplication();
        if (!string.IsNullOrEmpty(selectedApp))
        {
            _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm uninstall {selectedApp}");
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _ = dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent(GetTranslation("Appmgr_AppIsNotSelected"))
            .Dismiss().ByClickingBackground()
            .TryShow();
            });
        }
        IsBusy = false;
    }

    [RelayCommand]
    public async Task UninstallAppWithData()
    {
        IsBusy = true;
        if (!await GetDevicesInfo.SetDevicesInfoLittle())
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _ = dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent(GetTranslation("Common_NotConnected"))
            .Dismiss().ByClickingBackground()
            .TryShow();
            });
            IsBusy = false; return;
        }
        string selectedApp = SelectedApplication();
        if (!string.IsNullOrEmpty(selectedApp))
        {
            // Note: This command may vary depending on the requirements and platform specifics.
            // The following is a general example and may not work as is.
            _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm uninstall -k {selectedApp}");
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _ = dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent(GetTranslation("Appmgr_AppIsNotSelected"))
            .Dismiss().ByClickingBackground()
            .TryShow();
            });
        }
        IsBusy = false;
    }

    public string SelectedApplication()
    {
        return Applications.FirstOrDefault(app => app.IsSelected)?.Name ?? "";
    }

    [RelayCommand]
    public async Task ExtractInstaller()
    {
        IsBusy = true;
        if (!await GetDevicesInfo.SetDevicesInfoLittle())
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _ = dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent(GetTranslation("Common_NotConnected"))
            .Dismiss().ByClickingBackground()
            .TryShow();
            });
            IsBusy = false; return;
        }
        string selectedApp = SelectedApplication();
        if (!string.IsNullOrEmpty(selectedApp))
        {
            // Get the apk file of the selected app, and save it to the user's desktop.
            string apkFile = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm path {selectedApp}");
            apkFile = apkFile[(apkFile.IndexOf(':') + 1)..].Trim();
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} pull {apkFile} {desktopPath}");
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _ = dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent(GetTranslation("Appmgr_AppIsNotSelected"))
            .Dismiss().ByClickingBackground()
            .TryShow();
            });
        }
        IsBusy = false;
    }

    [RelayCommand]
    public async Task ClearApp()
    {
        IsBusy = true;
        if (!await GetDevicesInfo.SetDevicesInfoLittle())
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _ = dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent(GetTranslation("Common_NotConnected"))
            .Dismiss().ByClickingBackground()
            .TryShow();
            });
            IsBusy = false; return;
        }
        string selectedApp = SelectedApplication();
        if (!string.IsNullOrEmpty(selectedApp))
        {
            _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm clear {selectedApp}");
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _ = dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent(GetTranslation("Appmgr_AppIsNotSelected"))
            .Dismiss().ByClickingBackground()
            .TryShow();
            });
        }
        IsBusy = false;
    }

    [RelayCommand]
    public async Task ForceStopApp()
    {
        IsBusy = true;
        if (!await GetDevicesInfo.SetDevicesInfoLittle())
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _ = dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent(GetTranslation("Common_NotConnected"))
            .Dismiss().ByClickingBackground()
            .TryShow();
            });
            IsBusy = false; return;
        }
        string selectedApp = SelectedApplication();
        if (!string.IsNullOrEmpty(selectedApp))
        {
            _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell am force-stop {selectedApp}");
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _ = dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent(GetTranslation("Appmgr_AppIsNotSelected"))
            .Dismiss().ByClickingBackground()
            .TryShow();
            });
        }
        IsBusy = false;
    }

    [RelayCommand]
    public async Task ActivateApp()
    {
        IsBusy = true; // Assuming this sets a flag that indicates the operation is in progress.
        if (!await GetDevicesInfo.SetDevicesInfoLittle())
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _ = dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent(GetTranslation("Common_NotConnected"))
            .Dismiss().ByClickingBackground()
            .TryShow();
            });
            IsBusy = false; return;
        }
        string selectedApp = SelectedApplication();
        if (string.IsNullOrEmpty(selectedApp))
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _ = dialogManager.CreateDialog()
            .OfType(NotificationType.Error)
            .WithTitle("Error")
            .WithContent(GetTranslation("Appmgr_AppIsNotSelected"))
            .Dismiss().ByClickingBackground()
            .TryShow();
            });
            return;
        }
        string focus_name, package_name;
        string dumpsys = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell \"dumpsys window | grep mCurrentFocus\"");
        string text = await FeaturesHelper.ActiveApp(dumpsys);
        _ = toastManager.CreateToast()
.WithTitle("Info")
.WithContent(GetTranslation("Appmgr_AppActivactor") + $"\r\n{text}")
.OfType(NotificationType.Information)
.Dismiss().ByClicking()
.Dismiss().After(TimeSpan.FromSeconds(3))
.Queue();
        IsBusy = false;
    }

    private static readonly char[] separatorArray = ['\r', '\n'];

    private static string GetInstalledDate(string[] lines)
    {
        string installedDateLine = lines.FirstOrDefault(x => x.Contains("lastUpdateTime"));
        if (installedDateLine != null)
        {
            string installedDate = installedDateLine[(installedDateLine.IndexOf('=') + 1)..].Trim();
            return installedDate;
        }
        return GetTranslation("Appmgr_UnknownTime");
    }

    private static string GetSdkVersion(string[] lines)
    {
        string sdkVersion = lines.FirstOrDefault(x => x.Contains("targetSdk"));
        if (sdkVersion != null)
        {
            string installedDate = "SDK" + sdkVersion[(sdkVersion.IndexOf('=') + 1)..].Trim();
            return installedDate;
        }
        return GetTranslation("Appmgr_UnknownSDKVersion");
    }

    private static string GetVersionName(string[] lines)
    {
        string versionName = lines.FirstOrDefault(x => x.Contains("versionName"));
        if (versionName != null)
        {
            string installedDate = versionName[(versionName.IndexOf('=') + 1)..].Trim();
            return installedDate;
        }
        return GetTranslation("Appmgr_UnknownAppVersion");
    }
}

public partial class ApplicationInfo : ObservableObject
{
    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string size;

    [ObservableProperty]
    private string otherInfo;
}