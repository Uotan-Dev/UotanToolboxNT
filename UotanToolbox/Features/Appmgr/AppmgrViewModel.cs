using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
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
    private string _apkFile = string.Empty;
    [ObservableProperty]
    private string _search = string.Empty;
    [ObservableProperty]
    private string _sBoxWater = GetTranslation("Appmgr_SearchApp");
    ApplicationInfo[] allApplicationInfos = [];
    List<ApplicationInfo> applicationInfos = [];

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
                        applicationInfos.AddRange(allApplicationInfos.Where(app => app.DisplayName.Contains(Search, StringComparison.OrdinalIgnoreCase) || app.Name.Contains(Search, StringComparison.OrdinalIgnoreCase))
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

    public static string? ExtractPackageName(string line)
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
        // Reset stale state from any previous fetch.
        allApplicationInfos = [];
        applicationInfos = [];
        Applications.Clear();
        HasItems = false;
        await Task.Run(async () =>
        {
            if (await GetDevicesInfo.SetDevicesInfoLittle())
            {
                MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
                if (sukiViewModel.Status == GetTranslation("Home_Android"))
                {
                    await FeaturesHelper.AdbCmd(Global.thisdevice, $"push \"{Path.Join(Global.runpath, "Push", "list_apps")}\" /data/local/tmp/");
                    await FeaturesHelper.AdbCmd(Global.thisdevice, "shell chmod 777 /data/local/tmp/list_apps");
                    string fulllists = await FeaturesHelper.AdbCmd(Global.thisdevice, "shell /data/local/tmp/list_apps ");
                    List<ApplicationInfo> fullapplications = StringHelper.ParseApplicationInfo(fulllists);
                    string fullApplicationsList = !IsSystemAppDisplayed
                        ? await FeaturesHelper.AdbCmd(Global.thisdevice, "shell pm list packages -3")
                        : await FeaturesHelper.AdbCmd(Global.thisdevice, "shell pm list packages");
                    // Always clean up the helper binary, even if parsing fails below.
                    try
                    {
                        await FeaturesHelper.AdbCmd(Global.thisdevice, "shell rm /data/local/tmp/list_apps");
                    }
                    catch
                    {
                        // cleanup is best-effort; don't mask the real error
                    }
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
                        return;
                    }
                    string[] lines = fullApplicationsList.Split(separatorArray, StringSplitOptions.RemoveEmptyEntries);
                    await Dispatcher.UIThread.InvokeAsync(() => HasItems = lines.Length > 0);
                    IEnumerable<Task<ApplicationInfo?>> applicationInfosTasks = lines.Select(async line =>
                    {
                        string? displayName = null;
                        string? packageName = ExtractPackageName(line);
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
                        string combinedOutput = await FeaturesHelper.AdbCmd(Global.thisdevice, $"shell dumpsys package {packageName}");
                        string[] splitOutput = combinedOutput.Split('\n', ' ');
                        string otherInfo = GetVersionName(splitOutput) + " | " + GetInstalledDate(splitOutput) + " | " + GetSdkVersion(splitOutput);
                        string enabledState = ParseEnabledState(combinedOutput);
                        return new ApplicationInfo { Name = packageName, DisplayName = StringHelper.RemoveLineFeed(displayName ?? string.Empty), OtherInfo = otherInfo, EnabledState = enabledState };
                    });
                    allApplicationInfos = [.. (await Task.WhenAll(applicationInfosTasks)).Where(info => info != null).Select(info => info!)];
                    applicationInfos = [.. allApplicationInfos.Where(info => info != null)
                                                             .OrderByDescending(app => app.Size)
                                                             .ThenBy(app => app.Name)];
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Applications = new ObservableCollection<ApplicationInfo>(applicationInfos);
                    });
                }
                else if (sukiViewModel.Status == GetTranslation("Home_OpenHOS"))
                {
                    string[] applist = StringHelper.OHAppList(await FeaturesHelper.HdcCmd(Global.thisdevice, "shell bm dump -a"));
                    await Dispatcher.UIThread.InvokeAsync(() => HasItems = applist.Length > 2);
                    // applist[0] is a header line, applist[1] may be blank/total — real package names start at index 2.
                    List<ApplicationInfo> ohApplicationList = [];
                    for (int i = 2; i < applist.Length; i++)
                    {
                        string rawAppInfo = await FeaturesHelper.HdcCmd(Global.thisdevice, $"shell bm dump -n {applist[i]}");
                        string[] appinfo = StringHelper.OHAppInfo(rawAppInfo);
                        ohApplicationList.Add(new ApplicationInfo { Name = applist[i], DisplayName = appinfo[1], OtherInfo = appinfo[2] + "|API:" + appinfo[0], EnabledState = ParseOHEnabledState(rawAppInfo) });
                        // Refresh the visible list every 10 apps so the UI populates progressively.
                        if (ohApplicationList.Count % 10 == 0)
                        {
                            var snapshot = ohApplicationList
                                .OrderByDescending(app => app.Size)
                                .ThenBy(app => app.Name)
                                .ToList();
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Applications = new ObservableCollection<ApplicationInfo>(snapshot);
                            });
                        }
                    }
                    allApplicationInfos = [.. ohApplicationList];
                    applicationInfos = [.. allApplicationInfos.Where(info => info != null)
                                                      .OrderByDescending(app => app.Size)
                                                      .ThenBy(app => app.Name)];
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Applications = new ObservableCollection<ApplicationInfo>(applicationInfos);
                    });
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
                            string output = await FeaturesHelper.AdbCmd(Global.thisdevice, $"install -r \"{fileArray[i]}\"");
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
                        if (!string.IsNullOrEmpty(fileArray[i]))
                        {
                            string hdcCommand = $"install \"{fileArray[i]}\"";
                            try
                            {
                                string output = await FeaturesHelper.HdcCmd(Global.thisdevice, hdcCommand);
                                if (output.Contains("successfully"))
                                {
                                    Global.MainToastManager.CreateToast()
                                                         .WithTitle(GetTranslation("Common_Succ"))
                                                         .WithContent(GetTranslation("Common_InstallSuccess"))
                                                         .OfType(NotificationType.Success)
                                                         .Dismiss().ByClicking()
                                                         .Dismiss().After(TimeSpan.FromSeconds(3))
                                                         .Queue();
                                }
                                else
                                {
                                    // Keep the original hdc call and its raw output in the
                                    // error dialog so installation failures can be diagnosed
                                    // (hdc prints error reason to stdout/stderr).
                                    ShowErrorDialog(
                                        GetTranslation("Common_InstallFailed"),
                                        BuildHdcInstallDetails(hdcCommand, output));
                                }
                                File.Delete(Path.Combine(Global.runpath, "APK", Path.GetFileName(fileArray[i])));
                            }
                            catch (Exception ex)
                            {
                                ShowErrorDialog(
                                    GetTranslation("Common_InstallFailed"),
                                    BuildHdcInstallDetails(hdcCommand, ex.Message));
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
        if (!await GetDevicesInfo.SetDevicesInfoLittle())
        {
            ShowErrorDialog(GetTranslation("Common_NotConnected"));
            IsBusy = false;
            return;
        }

        MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
        if (sukiViewModel.Status != GetTranslation("Home_Android"))
        {
            ShowErrorDialog(GetTranslation("Common_OpenADB"));
            IsBusy = false;
            return;
        }

        string selectedApp = SelectedApplication();
        if (string.IsNullOrEmpty(selectedApp))
        {
            ShowErrorDialog(GetTranslation("Appmgr_AppIsNotSelected"));
            IsBusy = false;
            return;
        }

        string output = await FeaturesHelper.AdbCmd(Global.thisdevice, $"shell monkey -p {selectedApp} 1");
        // monkey prints "Events injected" on success; bail out messages start with "//" or "No activities found".
        bool started = output.Contains("Events injected", StringComparison.OrdinalIgnoreCase) ||
                       output.Contains("monkey abort", StringComparison.OrdinalIgnoreCase) == false;
        Global.MainToastManager.CreateToast()
                               .OfType(started ? NotificationType.Success : NotificationType.Error)
                               .WithTitle(started ? GetTranslation("Common_Succ") : GetTranslation("Common_Error"))
                               .WithContent(started ? GetTranslation("Appmgr_RunSucc") : GetTranslation("Appmgr_RunFail"))
                               .Dismiss().ByClicking()
                               .Dismiss().After(TimeSpan.FromSeconds(3))
                               .Queue();
        IsBusy = false;
    }

    [RelayCommand]
    public async Task DisableApp()
    {
        await ToggleAppEnabledAsync(enable: false);
    }

    [RelayCommand]
    public async Task EnableApp()
    {
        await ToggleAppEnabledAsync(enable: true);
    }

    private async Task ToggleAppEnabledAsync(bool enable)
    {
        IsBusy = true;
        try
        {
            if (!await GetDevicesInfo.SetDevicesInfoLittle())
            {
                ShowErrorDialog(GetTranslation("Common_NotConnected"));
                return;
            }

            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            string status = sukiViewModel.Status;
            bool isAndroid = status == GetTranslation("Home_Android");
            bool isOH = status == GetTranslation("Home_OpenHOS");
            if (!isAndroid && !isOH)
            {
                ShowErrorDialog(GetTranslation("Common_OpenADBOrHDC"));
                return;
            }

            string selectedApp = SelectedApplication();
            if (string.IsNullOrEmpty(selectedApp))
            {
                ShowErrorDialog(GetTranslation("Appmgr_AppIsNotSelected"));
                return;
            }

            string output;
            if (isAndroid)
            {
                // disable-user keeps the change scoped to the current user and is reversible;
                // pm enable re-enables a user-disabled package. There is no "enable-user" command.
                output = enable
                    ? await FeaturesHelper.AdbCmd(Global.thisdevice, $"shell pm enable {selectedApp}")
                    : await FeaturesHelper.AdbCmd(Global.thisdevice, $"shell pm disable-user --user 0 {selectedApp}");
            }
            else
            {
                // OpenHarmony: bm enable -n / bm disable -n (per awesome-hdc bm tool docs).
                output = enable
                    ? await FeaturesHelper.HdcCmd(Global.thisdevice, $"shell bm enable -n {selectedApp}")
                    : await FeaturesHelper.HdcCmd(Global.thisdevice, $"shell bm disable -n {selectedApp}");
            }

            // pm enable/disable-user print a literal "Success" line on success and surface
            // an exception/usage message on failure. bm enable-app/disable-app report the
            // resulting enabled state ("app is enabled" / "app is disabled") or an error.
            string trimmedOutput = output.Trim();
            bool ok = trimmedOutput.Contains("Success", StringComparison.OrdinalIgnoreCase) ||
                      trimmedOutput.Contains("enabled", StringComparison.OrdinalIgnoreCase) ||
                      trimmedOutput.Contains("disabled", StringComparison.OrdinalIgnoreCase) ||
                      trimmedOutput.Contains("changed", StringComparison.OrdinalIgnoreCase);
            // An empty/whitespace-only output is not a success signal — treat it as a failure.
            if (string.IsNullOrWhiteSpace(trimmedOutput))
            {
                ok = false;
            }
            if (ok)
            {
                Global.MainToastManager.CreateToast()
                                       .WithTitle(GetTranslation("Common_Succ"))
                                       .WithContent(enable ? GetTranslation("Appmgr_EnableSucc") : GetTranslation("Appmgr_DisableSucc"))
                                       .OfType(NotificationType.Success)
                                       .Dismiss().ByClicking()
                                       .Dismiss().After(TimeSpan.FromSeconds(3))
                                       .Queue();
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                                        .OfType(NotificationType.Warning)
                                        .WithTitle(GetTranslation("Common_Warn"))
                                        .WithContent(enable ? GetTranslation("Appmgr_EnableFail") : GetTranslation("Appmgr_DisableFail"))
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task UninstallApp()
    {
        await UninstallApplicationAsync(keepData: false);
    }

    [RelayCommand]
    public async Task UninstallAppWithData()
    {
        await UninstallApplicationAsync(keepData: true);
    }

    private async Task UninstallApplicationAsync(bool keepData)
    {
        IsBusy = true;
        if (!await GetDevicesInfo.SetDevicesInfoLittle())
        {
            ShowErrorDialog(GetTranslation("Common_NotConnected"));
            IsBusy = false;
            return;
        }

        MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
        bool isAndroid = sukiViewModel.Status == GetTranslation("Home_Android");
        bool isOH = sukiViewModel.Status == GetTranslation("Home_OpenHOS");
        if (!isAndroid && !isOH)
        {
            ShowErrorDialog(GetTranslation("Common_OpenADBOrHDC"));
            IsBusy = false;
            return;
        }

        string selectedApp = SelectedApplication();
        if (string.IsNullOrEmpty(selectedApp))
        {
            ShowErrorDialog(GetTranslation("Appmgr_AppIsNotSelected"));
            IsBusy = false;
            return;
        }

        string output;
        if (isAndroid)
        {
            string cmd = keepData ? $"shell pm uninstall -k {selectedApp}" : $"shell pm uninstall {selectedApp}";
            output = await FeaturesHelper.AdbCmd(Global.thisdevice, cmd);
        }
        else
        {
            output = await FeaturesHelper.HdcCmd(Global.thisdevice, $"app uninstall {selectedApp}");
        }

        bool uninstalled = output.Contains("Success", StringComparison.OrdinalIgnoreCase) ||
                           output.Contains("successfully", StringComparison.OrdinalIgnoreCase) ||
                           output.Contains("Deleted", StringComparison.OrdinalIgnoreCase);
        Global.MainToastManager.CreateToast()
                               .OfType(uninstalled ? NotificationType.Success : NotificationType.Error)
                               .WithTitle(uninstalled ? GetTranslation("Common_Succ") : GetTranslation("Common_Error"))
                               .WithContent(uninstalled ? GetTranslation("Appmgr_UninstallSucc") : GetTranslation("Appmgr_UninstallFail"))
                               .Dismiss().ByClicking()
                               .Dismiss().After(TimeSpan.FromSeconds(3))
                               .Queue();
        IsBusy = false;
    }

    [RelayCommand]
    public async Task ExtractInstaller()
    {
        IsBusy = true;
        try
        {
            if (!await GetDevicesInfo.SetDevicesInfoLittle())
            {
                ShowErrorDialog(GetTranslation("Common_NotConnected"));
                return;
            }

            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status != GetTranslation("Home_Android"))
            {
                ShowErrorDialog(GetTranslation("Common_OpenADB"));
                return;
            }

            string selectedApp = SelectedApplication();
            if (string.IsNullOrEmpty(selectedApp))
            {
                ShowErrorDialog(GetTranslation("Appmgr_AppIsNotSelected"));
                return;
            }

            // Resolve the on-device apk path(s). Some apps are split (base + splits).
            string pathOutput = await FeaturesHelper.AdbCmd(Global.thisdevice, $"shell pm path {selectedApp}");
            if (string.IsNullOrWhiteSpace(pathOutput) ||
                pathOutput.Contains("Package not found", StringComparison.OrdinalIgnoreCase) ||
                pathOutput.Contains("Unknown package", StringComparison.OrdinalIgnoreCase))
            {
                ShowErrorDialog(GetTranslation("Appmgr_ExtractFailed"));
                return;
            }

            // Look up a friendly display name for renaming the extracted apk.
            ApplicationInfo? appInfo = Applications.FirstOrDefault(a => a.Name == selectedApp);
            string friendlyName = SanitizeFileName(appInfo?.DisplayName);
            if (string.IsNullOrWhiteSpace(friendlyName) || string.Equals(friendlyName, "package", StringComparison.OrdinalIgnoreCase))
            {
                friendlyName = SanitizeFileName(selectedApp);
            }
            if (string.IsNullOrWhiteSpace(friendlyName))
            {
                friendlyName = "app";
            }

            // Let the user choose where to extract the installer(s).
            string? targetDir = await PickExtractFolderAsync();
            if (string.IsNullOrEmpty(targetDir))
            {
                return; // user cancelled
            }

            string[] apkPaths = pathOutput.Split(separatorArray, StringSplitOptions.RemoveEmptyEntries);
            bool anySuccess = false;
            bool anyFailure = false;
            int index = 0;
            foreach (string rawPath in apkPaths)
            {
                string trimmed = rawPath.Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    continue;
                }
                // pm path lines look like "package:/data/app/.../base.apk"
                string onDevicePath = trimmed[(trimmed.IndexOf(':') + 1)..].Trim();
                if (string.IsNullOrWhiteSpace(onDevicePath))
                {
                    continue;
                }

                string originalName = Path.GetFileName(onDevicePath);
                string extension = Path.GetExtension(originalName);
                if (string.IsNullOrEmpty(extension))
                {
                    extension = ".apk";
                }
                string newName;
                if (apkPaths.Length == 1)
                {
                    newName = $"{friendlyName}{extension}";
                }
                else
                {
                    // For split apks keep the original base/split qualifier for clarity.
                    string stem = Path.GetFileNameWithoutExtension(originalName);
                    newName = $"{friendlyName}_{stem}{extension}";
                }
                string destPath = Path.Combine(targetDir, newName);

                string pullOutput = await FeaturesHelper.AdbCmd(Global.thisdevice, $"pull \"{onDevicePath}\" \"{destPath}\"");
                bool ok = pullOutput.Contains("1 file pulled", StringComparison.OrdinalIgnoreCase) ||
                          pullOutput.Contains("bytes in", StringComparison.OrdinalIgnoreCase) ||
                          pullOutput.Contains("file pulled", StringComparison.OrdinalIgnoreCase);
                if (ok && File.Exists(destPath))
                {
                    anySuccess = true;
                }
                else
                {
                    anyFailure = true;
                }
                index++;
            }

            if (anySuccess && !anyFailure)
            {
                Global.MainToastManager.CreateToast()
                                       .WithTitle(GetTranslation("Common_Succ"))
                                       .WithContent(GetTranslation("Appmgr_ExtractSuccess"))
                                       .OfType(NotificationType.Success)
                                       .Dismiss().ByClicking()
                                       .Dismiss().After(TimeSpan.FromSeconds(4))
                                       .Queue();
            }
            else if (anySuccess && anyFailure)
            {
                Global.MainToastManager.CreateToast()
                                       .WithTitle(GetTranslation("Common_Warn"))
                                       .WithContent(GetTranslation("Appmgr_ExtractPartial"))
                                       .OfType(NotificationType.Warning)
                                       .Dismiss().ByClicking()
                                       .Dismiss().After(TimeSpan.FromSeconds(5))
                                       .Queue();
            }
            else
            {
                ShowErrorDialog(GetTranslation("Appmgr_ExtractFailed"));
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<string?> PickExtractFolderAsync()
    {
        var mainWindow = (Avalonia.Application.Current?.ApplicationLifetime
            as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (mainWindow is null)
        {
            return null;
        }
        IReadOnlyList<IStorageFolder> folders = await mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = GetTranslation("Appmgr_SelectExtractFolder"),
            AllowMultiple = false
        });
        if (folders.Count < 1)
        {
            return null;
        }
        return folders[0].TryGetLocalPath();
    }

    private static string SanitizeFileName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }
        char[] invalid = Path.GetInvalidFileNameChars();
        string cleaned = string.Join("_", name.Split(invalid)).Trim();
        return cleaned;
    }

    private static void ShowErrorDialog(string content, string? details = null)
    {
        string fullContent = details == null
            ? content
            : $"{content}\r\n\r\n{details}";
        Global.MainDialogManager.CreateDialog()
                    .OfType(NotificationType.Error)
                    .WithTitle(GetTranslation("Common_Error"))
                    .WithContent(fullContent)
                    .Dismiss().ByClickingBackground()
                    .TryShow();
    }

    // Build a diagnostic details block preserving the exact hdc command invoked
    // and its raw stdout/stderr, so installation failures can be debugged from
    // the error dialog instead of a disappearing toast.
    private static string BuildHdcInstallDetails(string hdcCommand, string rawOutput)
    {
        string trimmedOutput = string.IsNullOrWhiteSpace(rawOutput) ? "(empty)" : rawOutput.TrimEnd();
        return $"hdc {hdcCommand}\r\n---\r\n{trimmedOutput}";
    }

    [RelayCommand]
    public async Task ClearApp()
    {
        IsBusy = true;
        if (!await GetDevicesInfo.SetDevicesInfoLittle())
        {
            ShowErrorDialog(GetTranslation("Common_NotConnected"));
            IsBusy = false;
            return;
        }

        MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
        bool isAndroid = sukiViewModel.Status == GetTranslation("Home_Android");
        bool isOH = sukiViewModel.Status == GetTranslation("Home_OpenHOS");
        if (!isAndroid && !isOH)
        {
            ShowErrorDialog(GetTranslation("Common_OpenADBOrHDC"));
            IsBusy = false;
            return;
        }

        string selectedApp = SelectedApplication();
        if (string.IsNullOrEmpty(selectedApp))
        {
            ShowErrorDialog(GetTranslation("Appmgr_AppIsNotSelected"));
            IsBusy = false;
            return;
        }

        string output = isAndroid
            ? await FeaturesHelper.AdbCmd(Global.thisdevice, $"shell pm clear {selectedApp}")
            : await FeaturesHelper.HdcCmd(Global.thisdevice, $"shell bm clean -n {selectedApp} -d");

        bool cleared = output.Contains("Success", StringComparison.OrdinalIgnoreCase) ||
                       output.Contains("cleared", StringComparison.OrdinalIgnoreCase) ||
                       output.Contains("clean", StringComparison.OrdinalIgnoreCase);
        Global.MainToastManager.CreateToast()
                               .OfType(cleared ? NotificationType.Success : NotificationType.Error)
                               .WithTitle(cleared ? GetTranslation("Common_Succ") : GetTranslation("Common_Error"))
                               .WithContent(cleared ? GetTranslation("Appmgr_ClearSucc") : GetTranslation("Appmgr_ClearFail"))
                               .Dismiss().ByClicking()
                               .Dismiss().After(TimeSpan.FromSeconds(3))
                               .Queue();
        IsBusy = false;
    }

    [RelayCommand]
    public async Task ForceStopApp()
    {
        IsBusy = true;
        if (!await GetDevicesInfo.SetDevicesInfoLittle())
        {
            ShowErrorDialog(GetTranslation("Common_NotConnected"));
            IsBusy = false;
            return;
        }

        MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
        bool isAndroid = sukiViewModel.Status == GetTranslation("Home_Android");
        bool isOH = sukiViewModel.Status == GetTranslation("Home_OpenHOS");
        if (!isAndroid && !isOH)
        {
            ShowErrorDialog(GetTranslation("Common_OpenADBOrHDC"));
            IsBusy = false;
            return;
        }

        string selectedApp = SelectedApplication();
        if (string.IsNullOrEmpty(selectedApp))
        {
            ShowErrorDialog(GetTranslation("Appmgr_AppIsNotSelected"));
            IsBusy = false;
            return;
        }

        string output = isAndroid
            ? await FeaturesHelper.AdbCmd(Global.thisdevice, $"shell am force-stop {selectedApp}")
            : await FeaturesHelper.HdcCmd(Global.thisdevice, $"shell aa force-stop {selectedApp}");

        // am force-stop is silent on success. Treat any explicit failure marker as error.
        bool stopped = !output.Contains("Error", StringComparison.OrdinalIgnoreCase) &&
                       !output.Contains("Exception", StringComparison.OrdinalIgnoreCase);
        Global.MainToastManager.CreateToast()
                               .OfType(stopped ? NotificationType.Success : NotificationType.Error)
                               .WithTitle(stopped ? GetTranslation("Common_Succ") : GetTranslation("Common_Error"))
                               .WithContent(stopped ? GetTranslation("Appmgr_ForceStopSucc") : GetTranslation("Appmgr_ForceStopFail"))
                               .Dismiss().ByClicking()
                               .Dismiss().After(TimeSpan.FromSeconds(3))
                               .Queue();
        IsBusy = false;
    }

    [RelayCommand]
    public async Task ActivateApp()
    {
        IsBusy = true;
        if (!await GetDevicesInfo.SetDevicesInfoLittle())
        {
            ShowErrorDialog(GetTranslation("Common_NotConnected"));
            IsBusy = false;
            return;
        }

        MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
        bool isAndroid = sukiViewModel.Status == GetTranslation("Home_Android");
        bool isOH = sukiViewModel.Status == GetTranslation("Home_OpenHOS");
        if (!isAndroid && !isOH)
        {
            ShowErrorDialog(GetTranslation("Common_OpenADBOrHDC"));
            IsBusy = false;
            return;
        }

        string dumpsys = await FeaturesHelper.AdbCmd(Global.thisdevice, $"shell \"dumpsys window | grep mCurrentFocus\"");
        string text = await FeaturesHelper.ActiveApp(dumpsys);
        Global.MainToastManager.CreateToast()
                               .OfType(NotificationType.Information)
                               .WithTitle(GetTranslation("Appmgr_AppActivactor"))
                               .WithContent($"{text}")
                               .Dismiss().ByClicking()
                               .Dismiss().After(TimeSpan.FromSeconds(3))
                               .Queue();
        IsBusy = false;
    }

    private static string GetInstalledDate(string[] lines)
    {
        string? installedDateLine = lines.FirstOrDefault(x => x.Contains("lastUpdateTime"));
        if (installedDateLine != null)
        {
            string installedDate = installedDateLine[(installedDateLine.IndexOf('=') + 1)..].Trim();
            return installedDate;
        }
        return GetTranslation("Appmgr_UnknownTime");
    }

    private static string GetSdkVersion(string[] lines)
    {
        string? sdkVersion = lines.FirstOrDefault(x => x.Contains("targetSdk"));
        if (sdkVersion != null)
        {
            string installedDate = "SDK" + sdkVersion[(sdkVersion.IndexOf('=') + 1)..].Trim();
            return installedDate;
        }
        return GetTranslation("Appmgr_UnknownSDKVersion");
    }

    private static string GetVersionName(string[] lines)
    {
        string? versionName = lines.FirstOrDefault(x => x.Contains("versionName"));
        if (versionName != null)
        {
            string installedDate = versionName[(versionName.IndexOf('=') + 1)..].Trim();
            return installedDate;
        }
        return GetTranslation("Appmgr_UnknownAppVersion");
    }

    /// <summary>
    /// Parses the enabled state of a package from its <c>dumpsys package</c> output.
    /// <c>dumpsys package</c> emits a <c>User 0: ... enabled=...</c> (or
    /// <c>enabledComponents</c> / <c>disabledComponents</c> blocks) that reflects whether
    /// the package is enabled, disabled, disabled-user, etc.
    /// </summary>
    private static string ParseEnabledState(string dumpsysOutput)
    {
        if (string.IsNullOrWhiteSpace(dumpsysOutput))
        {
            return GetTranslation("Appmgr_StateUnknown");
        }

        // Look for the per-user enabled flag, e.g. "User 0: ceUserId=0 installed=true hidden=false suspended=false stopped=false notLaunched=false enabled=0"
        string[] lines = dumpsysOutput.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (!trimmed.Contains("enabled=", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            // Skip the COMPONENT-enabling summary lines; we want the package-level user line.
            if (trimmed.StartsWith("enabledComponents", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("disabledComponents", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            int eqIdx = trimmed.IndexOf("enabled=", StringComparison.OrdinalIgnoreCase);
            int valueStart = eqIdx + "enabled=".Length;
            if (valueStart >= trimmed.Length)
            {
                continue;
            }
            // The value is the numeric constant immediately after "enabled=".
            int end = valueStart;
            while (end < trimmed.Length && (char.IsDigit(trimmed[end]) || trimmed[end] == '-'))
            {
                end++;
            }
            string rawValue = trimmed.AsSpan(valueStart, end - valueStart).ToString();
            if (int.TryParse(rawValue, out int enabledState))
            {
                return enabledState switch
                {
                    0 or 1 => GetTranslation("Appmgr_StateEnabled"),     // DEFAULT(0)/ENABLED(1)
                    2 => GetTranslation("Appmgr_StateDisabled"),         // DISABLED
                    3 => GetTranslation("Appmgr_StateDisabledUser"),     // DISABLED_USER
                    4 => GetTranslation("Appmgr_StateDisabledUntilUsed"),// DISABLED_UNTIL_USED
                    _ => GetTranslation("Appmgr_StateUnknown"),
                };
            }
        }

        // Fallback: some Android versions surface a "stopType" / "firstInstallTime" section
        // but not the per-user enabled flag. Treat absence as unknown rather than guessing.
        return GetTranslation("Appmgr_StateUnknown");
    }

    /// <summary>
    /// Parses the enabled state of a HarmonyOS/OpenHarmony app from its
    /// <c>bm dump -n</c> output, which contains an <c>"enabled": true/false</c> JSON field.
    /// </summary>
    private static string ParseOHEnabledState(string bmDumpOutput)
    {
        if (string.IsNullOrWhiteSpace(bmDumpOutput))
        {
            return GetTranslation("Appmgr_StateUnknown");
        }

        int idx = bmDumpOutput.IndexOf("\"enabled\"", StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
        {
            return GetTranslation("Appmgr_StateUnknown");
        }

        // Capture the token after the colon, tolerating whitespace.
        int colon = bmDumpOutput.IndexOf(':', idx);
        if (colon < 0)
        {
            return GetTranslation("Appmgr_StateUnknown");
        }
        int scan = colon + 1;
        while (scan < bmDumpOutput.Length && char.IsWhiteSpace(bmDumpOutput[scan]))
        {
            scan++;
        }
        if (scan >= bmDumpOutput.Length)
        {
            return GetTranslation("Appmgr_StateUnknown");
        }

        // Match true/false (possibly quoted), else fall back to unknown.
        if (bmDumpOutput.AsSpan(scan).StartsWith("true", StringComparison.OrdinalIgnoreCase))
        {
            return GetTranslation("Appmgr_StateEnabled");
        }
        if (bmDumpOutput.AsSpan(scan).StartsWith("false", StringComparison.OrdinalIgnoreCase))
        {
            return GetTranslation("Appmgr_StateDisabled");
        }
        return GetTranslation("Appmgr_StateUnknown");
    }
}

public partial class ApplicationInfo : ObservableObject
{
    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string? displayName;

    [ObservableProperty]
    private string size = string.Empty;

    [ObservableProperty]
    private string otherInfo = string.Empty;

    /// <summary>
    /// Human-readable enabled state label (Enabled/Disabled/Disabled-User/...),
    /// populated during <see cref="AppmgrViewModel.Connect"/>. Empty until known.
    /// </summary>
    [ObservableProperty]
    private string enabledState = string.Empty;
}