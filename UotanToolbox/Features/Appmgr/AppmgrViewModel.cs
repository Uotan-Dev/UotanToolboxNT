using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using ReactiveUI;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Appmgr;

public partial class AppmgrViewModel : MainPageBase
{
    [ObservableProperty]
    private ObservableCollection<ApplicationInfo> applications = [];
    [ObservableProperty]
    private bool isBusy = false, hasItems = false, sBoxEnabled = true;
    [ObservableProperty]
    private bool isSystemAppDisplayed = false, isInstalling = false;
    [ObservableProperty]
    private string _apkFile;
    [ObservableProperty]
    private string _search;
    [ObservableProperty]
    private string _sBoxWater = GetTranslation("Appmgr_SearchApp");
    ApplicationInfo[] allApplicationInfos;
    List<ApplicationInfo> applicationInfos;

    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public AppmgrViewModel() : base(GetTranslation("Sidebar_Appmgr"), MaterialIconKind.ViewGridPlusOutline, -700)
    {
        _ = this.WhenAnyValue(app => app.Search)
            .Subscribe(option =>
            {
                if (applicationInfos != null && allApplicationInfos != null)
                {
                    if (!string.IsNullOrEmpty(Search))
                    {
                        applicationInfos.Clear();
                        applicationInfos.AddRange(allApplicationInfos.Where(app => app.DisplayName.Contains(Search) || app.Name.Contains(Search))
                                                                     .OrderByDescending(app => app.Size)
                                                                     .ThenBy(app => app.Name)
                                                                     .ToList());
                        Applications = new ObservableCollection<ApplicationInfo>(applicationInfos);
                    }
                    else
                    {
                        applicationInfos.Clear();
                        applicationInfos.AddRange(allApplicationInfos.Where(info => info != null)
                                                                     .OrderByDescending(app => app.Size)
                                                                     .ThenBy(app => app.Name)
                                                                     .ToList());
                        Applications = new ObservableCollection<ApplicationInfo>(applicationInfos);
                    }
                }
            });
    }

    private static readonly char[] separatorArray = ['\r', '\n'];

    public static string ExtractPackageName(string line)
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

    [RelayCommand]
    public async Task Connect()
    {
        IsBusy = true;
        SBoxEnabled = false;
        SBoxWater = GetTranslation("Appmgr_SearchWait");
        await Task.Run(async () =>
        {
            if (await GetDevicesInfo.SetDevicesInfoLittle())
            {
                MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
                if (sukiViewModel.Status == GetTranslation("Home_Android"))
                {
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} push \"{Path.Join(Global.runpath, "Push", "list_apps")}\" /data/local/tmp/");
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell chmod 777 /data/local/tmp/list_apps");
                    string fulllists = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell /data/local/tmp/list_apps ");
                    List<ApplicationInfo> fullapplications = StringHelper.ParseApplicationInfo(fulllists);
                    string fullApplicationsList = !IsSystemAppDisplayed
                        ? await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm list packages -3")
                        : await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm list packages");
                    if (fullApplicationsList.Contains("cannot connect to daemon"))
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Global.MainDialogManager.CreateDialog()
                                        .OfType(NotificationType.Error)
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .WithContent(GetTranslation("Common_DeviceFailedToConnect"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
                        });
                    }
                    string[] lines = fullApplicationsList.Split(separatorArray, StringSplitOptions.RemoveEmptyEntries);
                    HasItems = lines.Length > 0;
                    IEnumerable<Task<ApplicationInfo>> applicationInfosTasks = lines.Select(async line =>
                    {
                        string displayName = null;
                        string packageName = ExtractPackageName(line);
                        foreach (ApplicationInfo app in fullapplications)
                        {
                            if (app.Name == packageName)
                            {
                                displayName = app.DisplayName;
                                break;
                            }
                        }
                        if (string.IsNullOrEmpty(packageName))
                        {
                            return null;
                        }
                        string combinedOutput = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dumpsys package {packageName}");
                        string[] splitOutput = combinedOutput.Split('\n', ' ');
                        string otherInfo = GetVersionName(splitOutput) + " | " + GetInstalledDate(splitOutput) + " | " + GetSdkVersion(splitOutput);
                        return new ApplicationInfo { Name = packageName, DisplayName = displayName, OtherInfo = otherInfo };
                    });
                    allApplicationInfos = await Task.WhenAll(applicationInfosTasks);
                    applicationInfos = allApplicationInfos.Where(info => info != null)
                                                             .OrderByDescending(app => app.Size)
                                                             .ThenBy(app => app.Name)
                                                             .ToList();
                    Applications = new ObservableCollection<ApplicationInfo>(applicationInfos);
                    await CallExternalProgram.ADB($"-s {Global.thisdevice} shell rm /data/local/tmp/list_apps");
                }
                else if (sukiViewModel.Status == GetTranslation("Home_OpenHOS"))
                {
                    string[] applist = StringHelper.OHAppList(await CallExternalProgram.HDC($"-t {Global.thisdevice} shell bm dump -a"));
                    HasItems = applist.Length > 2;
                    ApplicationInfo[] applicationInfo = new ApplicationInfo[applist.Length - 2];
                    ApplicationInfo[] OHApplicationInfos;
                    List<ApplicationInfo> OHApplicationList = null;
                    for (int i = 2; i < applist.Length; i++)
                    {
                        string[] appinfo = StringHelper.OHAppInfo(await CallExternalProgram.HDC($"-t {Global.thisdevice} shell bm dump -n {applist[i]}"));
                        applicationInfo[i - 2] = new ApplicationInfo { Name = applist[i], DisplayName = appinfo[1], OtherInfo = appinfo[2] + "|API:" + appinfo[0] };
                        if (i == applicationInfo.Length % 10)
                        {
                            OHApplicationInfos = applicationInfo;
                            OHApplicationList = OHApplicationInfos.Where(info => info != null)
                                                              .OrderByDescending(app => app.Size)
                                                              .ThenBy(app => app.Name)
                                                              .ToList();
                            Applications = new ObservableCollection<ApplicationInfo>(OHApplicationList);
                            IsBusy = false;
                        }
                        if (i % (applicationInfo.Length % 10) == 0 && i != applicationInfo.Length % 10)
                        {
                            OHApplicationInfos = applicationInfo.Skip(i - (applicationInfo.Length % 10)).Take(i).ToArray();
                            OHApplicationList.AddRange(OHApplicationInfos.Where(info => info != null)
                                                              .OrderByDescending(app => app.Size)
                                                              .ThenBy(app => app.Name)
                                                              .ToList());
                            Applications = new ObservableCollection<ApplicationInfo>(OHApplicationList);
                        }
                    }
                    allApplicationInfos = applicationInfo;
                    applicationInfos = allApplicationInfos.Where(info => info != null)
                                                      .OrderByDescending(app => app.Size)
                                                      .ThenBy(app => app.Name)
                                                      .ToList();
                }
                else
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Global.MainDialogManager.CreateDialog()
                                    .OfType(NotificationType.Error)
                                    .WithTitle(GetTranslation("Common_Error"))
                                    .WithContent(GetTranslation("Common_OpenADBOrHDC"))
                                    .Dismiss().ByClickingBackground()
                                    .TryShow();
                    });
                }
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Global.MainDialogManager.CreateDialog()
                                .OfType(NotificationType.Error)
                                .WithTitle(GetTranslation("Common_Error"))
                                .WithContent(GetTranslation("Common_NotConnected"))
                                .Dismiss().ByClickingBackground()
                                .TryShow();
                });
            }
        });
        SBoxEnabled = true;
        SBoxWater = GetTranslation("Appmgr_SearchApp");
        IsBusy = false;
    }

    [RelayCommand]
    public async Task InstallApk()
    {
        IsInstalling = true;
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            if (!string.IsNullOrEmpty(ApkFile))
            {
                MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
                if (sukiViewModel.Status == GetTranslation("Home_Android"))
                {
                    string[] fileArray = ApkFile.Split("|||");
                    for (int i = 0; i < fileArray.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(fileArray[i]))
                        {
                            string output = await CallExternalProgram.ADB($"-s {Global.thisdevice} install -r \"{fileArray[i]}\"");
                            _ = output.Contains("Success")
                                ? Global.MainToastManager.CreateToast()
                                                         .WithTitle(GetTranslation("Common_Succ"))
                                                         .WithContent(GetTranslation("Common_InstallSuccess"))
                                                         .OfType(NotificationType.Success)
                                                         .Dismiss().ByClicking()
                                                         .Dismiss().After(TimeSpan.FromSeconds(3))
                                                         .Queue()
                                : Global.MainToastManager.CreateToast()
                                                         .WithTitle(GetTranslation("Common_Error"))
                                                         .WithContent(GetTranslation("Common_InstallFailed"))
                                                         .OfType(NotificationType.Error)
                                                         .Dismiss().ByClicking()
                                                         .Dismiss().After(TimeSpan.FromSeconds(5))
                                                         .Queue();
                        }
                    }
                }
                else if (sukiViewModel.Status == GetTranslation("Home_OpenHOS"))
                {
                    string[] fileArray = ApkFile.Split("|||");
                    for (int i = 0; i < fileArray.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(fileArray[i]) && File.Exists(fileArray[i]))
                        {
                            try
                            {
                                File.Copy(fileArray[i], Path.Combine(Global.runpath, "APK", Path.GetFileName(fileArray[i])));
                                string output = await CallExternalProgram.HDC($"-t {Global.thisdevice} app install \"{Path.Combine("APK", Path.GetFileName(fileArray[i]))}\"");
                                _ = output.Contains("successfully")
                                    ? Global.MainToastManager.CreateToast()
                                                             .WithTitle(GetTranslation("Common_Succ"))
                                                             .WithContent(GetTranslation("Common_InstallSuccess"))
                                                             .OfType(NotificationType.Success)
                                                             .Dismiss().ByClicking()
                                                             .Dismiss().After(TimeSpan.FromSeconds(3))
                                                             .Queue()
                                    : Global.MainToastManager.CreateToast()
                                                             .WithTitle(GetTranslation("Common_Error"))
                                                             .WithContent(GetTranslation("Common_InstallFailed") + "\r\n" + StringHelper.OHApp(output))
                                                             .OfType(NotificationType.Error)
                                                             .Dismiss().ByClicking()
                                                             .Dismiss().After(TimeSpan.FromSeconds(5))
                                                             .Queue();
                                File.Delete(Path.Combine(Global.runpath, "APK", Path.GetFileName(fileArray[i])));
                            }
                            catch
                            {
                                Global.MainToastManager.CreateToast()
                                                             .WithTitle(GetTranslation("Common_Error"))
                                                             .WithContent(GetTranslation("Common_InstallFailed"))
                                                             .OfType(NotificationType.Error)
                                                             .Dismiss().ByClicking()
                                                             .Dismiss().After(TimeSpan.FromSeconds(5))
                                                             .Queue();
                            }
                        }
                    }
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                                .OfType(NotificationType.Error)
                                .WithTitle(GetTranslation("Common_Error"))
                                .WithContent(GetTranslation("Common_OpenADBOrHDC"))
                                .Dismiss().ByClickingBackground()
                                .TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                          .OfType(NotificationType.Error)
                          .WithTitle(GetTranslation("Common_Error"))
                          .WithContent(GetTranslation("Appmgr_NoApkFileSelected"))
                          .Dismiss().ByClickingBackground()
                          .TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                    .OfType(NotificationType.Error)
                                    .WithTitle(GetTranslation("Common_Error"))
                                    .WithContent(GetTranslation("Common_NotConnected"))
                                    .Dismiss().ByClickingBackground()
                                    .TryShow();
        }
        IsInstalling = false;
    }

    public string SelectedApplication()
    {
        return Applications.FirstOrDefault(app => app.IsSelected)?.Name ?? "";
    }

    [RelayCommand]
    public async Task RunApp()
    {
        IsBusy = true;
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                if (SelectedApplication() != "")
                {
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell monkey -p {SelectedApplication()} 1");
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                                                .OfType(NotificationType.Error)
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .WithContent(GetTranslation("Appmgr_AppIsNotSelected"))
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                            .OfType(NotificationType.Error)
                            .WithTitle(GetTranslation("Common_Error"))
                            .WithContent(GetTranslation("Common_OpenADB"))
                            .Dismiss().ByClickingBackground()
                            .TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                        .OfType(NotificationType.Error)
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .WithContent(GetTranslation("Common_NotConnected"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
        }
        IsBusy = false;
    }

    [RelayCommand]
    public async Task DisableApp()
    {
        IsBusy = true;
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                if (SelectedApplication() != "")
                {
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm disable {SelectedApplication()}");
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                                                .OfType(NotificationType.Error)
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .WithContent(GetTranslation("Appmgr_AppIsNotSelected"))
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                            .OfType(NotificationType.Error)
                            .WithTitle(GetTranslation("Common_Error"))
                            .WithContent(GetTranslation("Common_OpenADB"))
                            .Dismiss().ByClickingBackground()
                            .TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                        .OfType(NotificationType.Error)
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .WithContent(GetTranslation("Common_NotConnected"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
        }
        IsBusy = false;
    }

    [RelayCommand]
    public async Task EnableApp()
    {
        IsBusy = true;
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                string selectedApp = SelectedApplication();
                if (!string.IsNullOrEmpty(selectedApp))
                {
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm enable {selectedApp}");
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                                                .OfType(NotificationType.Error)
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .WithContent(GetTranslation("Appmgr_AppIsNotSelected"))
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                            .OfType(NotificationType.Error)
                            .WithTitle(GetTranslation("Common_Error"))
                            .WithContent(GetTranslation("Common_OpenADB"))
                            .Dismiss().ByClickingBackground()
                            .TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                        .OfType(NotificationType.Error)
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .WithContent(GetTranslation("Common_NotConnected"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
        }
        IsBusy = false;
    }

    [RelayCommand]
    public async Task UninstallApp()
    {
        IsBusy = true;
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                string selectedApp = SelectedApplication();
                if (!string.IsNullOrEmpty(selectedApp))
                {
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm uninstall {selectedApp}");
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                                                .OfType(NotificationType.Error)
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .WithContent(GetTranslation("Appmgr_AppIsNotSelected"))
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
                }
            }
            else if (sukiViewModel.Status == GetTranslation("Home_OpenHOS"))
            {
                string selectedApp = SelectedApplication();
                if (!string.IsNullOrEmpty(selectedApp))
                {
                    _ = await CallExternalProgram.HDC($"-t {Global.thisdevice} app uninstall {selectedApp}");
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                                                .OfType(NotificationType.Error)
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .WithContent(GetTranslation("Appmgr_AppIsNotSelected"))
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                            .OfType(NotificationType.Error)
                            .WithTitle(GetTranslation("Common_Error"))
                            .WithContent(GetTranslation("Common_OpenADBOrHDC"))
                            .Dismiss().ByClickingBackground()
                            .TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                        .OfType(NotificationType.Error)
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .WithContent(GetTranslation("Common_NotConnected"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
        }
        IsBusy = false;
    }

    [RelayCommand]
    public async Task UninstallAppWithData()
    {
        IsBusy = true;
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                string selectedApp = SelectedApplication();
                if (!string.IsNullOrEmpty(selectedApp))
                {
                    // Note: This command may vary depending on the requirements and platform specifics.
                    // The following is a general example and may not work as is.
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm uninstall -k {selectedApp}");
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                                                .OfType(NotificationType.Error)
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .WithContent(GetTranslation("Appmgr_AppIsNotSelected"))
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                            .OfType(NotificationType.Error)
                            .WithTitle(GetTranslation("Common_Error"))
                            .WithContent(GetTranslation("Common_OpenADB"))
                            .Dismiss().ByClickingBackground()
                            .TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                        .OfType(NotificationType.Error)
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .WithContent(GetTranslation("Common_NotConnected"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
        }
        IsBusy = false;
    }

    [RelayCommand]
    public async Task ExtractInstaller()
    {
        IsBusy = true;
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
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
                    Global.MainDialogManager.CreateDialog()
                                                .OfType(NotificationType.Error)
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .WithContent(GetTranslation("Appmgr_AppIsNotSelected"))
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                            .OfType(NotificationType.Error)
                            .WithTitle(GetTranslation("Common_Error"))
                            .WithContent(GetTranslation("Common_OpenADB"))
                            .Dismiss().ByClickingBackground()
                            .TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                        .OfType(NotificationType.Error)
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .WithContent(GetTranslation("Common_NotConnected"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
        }
        IsBusy = false;
    }

    [RelayCommand]
    public async Task ClearApp()
    {
        IsBusy = true;
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                string selectedApp = SelectedApplication();
                if (!string.IsNullOrEmpty(selectedApp))
                {
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm clear {selectedApp}");
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                                                .OfType(NotificationType.Error)
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .WithContent(GetTranslation("Appmgr_AppIsNotSelected"))
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
                }
            }
            else if (sukiViewModel.Status == GetTranslation("Home_OpenHOS"))
            {
                string selectedApp = SelectedApplication();
                if (!string.IsNullOrEmpty(selectedApp))
                {
                    _ = await CallExternalProgram.HDC($"-t {Global.thisdevice} shell bm clean -n {selectedApp} -d");
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                                                .OfType(NotificationType.Error)
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .WithContent(GetTranslation("Appmgr_AppIsNotSelected"))
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                            .OfType(NotificationType.Error)
                            .WithTitle(GetTranslation("Common_Error"))
                            .WithContent(GetTranslation("Common_OpenADBOrHDC"))
                            .Dismiss().ByClickingBackground()
                            .TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                        .OfType(NotificationType.Error)
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .WithContent(GetTranslation("Common_NotConnected"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
        }
        IsBusy = false;
    }

    [RelayCommand]
    public async Task ForceStopApp()
    {
        IsBusy = true;
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                string selectedApp = SelectedApplication();
                if (!string.IsNullOrEmpty(selectedApp))
                {
                    _ = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell am force-stop {selectedApp}");
                }
                else
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Global.MainDialogManager.CreateDialog()
                                                    .OfType(NotificationType.Error)
                                                    .WithTitle(GetTranslation("Common_Error"))
                                                    .WithContent(GetTranslation("Appmgr_AppIsNotSelected"))
                                                    .Dismiss().ByClickingBackground()
                                                    .TryShow();
                    });
                }
            }
            else if (sukiViewModel.Status == GetTranslation("Home_OpenHOS"))
            {
                string selectedApp = SelectedApplication();
                if (!string.IsNullOrEmpty(selectedApp))
                {
                    _ = await CallExternalProgram.HDC($"-t {Global.thisdevice} shell aa force-stop {selectedApp}");
                }
                else
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Global.MainDialogManager.CreateDialog()
                                                    .OfType(NotificationType.Error)
                                                    .WithTitle(GetTranslation("Common_Error"))
                                                    .WithContent(GetTranslation("Appmgr_AppIsNotSelected"))
                                                    .Dismiss().ByClickingBackground()
                                                    .TryShow();
                    });
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                            .OfType(NotificationType.Error)
                            .WithTitle(GetTranslation("Common_Error"))
                            .WithContent(GetTranslation("Common_OpenADBOrHDC"))
                            .Dismiss().ByClickingBackground()
                            .TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                        .OfType(NotificationType.Error)
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .WithContent(GetTranslation("Common_NotConnected"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
        }
        IsBusy = false;
    }

    [RelayCommand]
    public async Task ActivateApp()
    {
        IsBusy = true; // Assuming this sets a flag that indicates the operation is in progress.
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_Android"))
            {
                string focus_name, package_name;
                string dumpsys = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell \"dumpsys window | grep mCurrentFocus\"");
                string text = await FeaturesHelper.ActiveApp(dumpsys);
                Global.MainToastManager.CreateToast()
                                           .OfType(NotificationType.Information)
                                           .WithTitle(GetTranslation("Appmgr_AppActivactor"))
                                           .WithContent($"{text}")
                                           .Dismiss().ByClicking()
                                           .Dismiss().After(TimeSpan.FromSeconds(3))
                                           .Queue();
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                            .OfType(NotificationType.Error)
                            .WithTitle(GetTranslation("Common_Error"))
                            .WithContent(GetTranslation("Common_OpenADB"))
                            .Dismiss().ByClickingBackground()
                            .TryShow();
            }
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                        .OfType(NotificationType.Error)
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .WithContent(GetTranslation("Common_NotConnected"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
        }
        IsBusy = false;
    }

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
    private string? displayName;

    [ObservableProperty]
    private string size;

    [ObservableProperty]
    private string otherInfo;
}