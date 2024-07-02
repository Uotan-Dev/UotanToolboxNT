using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Features.Appmgr;

public partial class AppmgrViewModel : MainPageBase
{
    [ObservableProperty]
    private ObservableCollection<ApplicationInfo> applications = new ObservableCollection<ApplicationInfo>();
    [ObservableProperty]
    private bool isBusy = false, hasItems = false;
    [ObservableProperty]
    private bool isSystemAppDisplayed = false, isInstalling = false;
    [ObservableProperty]
    private string _apkFile;
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public AppmgrViewModel() : base(GetTranslation("Sidebar_Appmgr"), MaterialIconKind.ViewGridPlusOutline, -700)
    {
    }

    [RelayCommand]
    public async Task Connect()
    {
        hasItems = false;
        MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
        IsBusy = true;
        try
        {
            await Task.Run(async () =>
            {
                if (!await GetDevicesInfo.SetDevicesInfoLittle())
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        SukiHost.ShowDialog(new PureDialog("设备未连接"), allowBackgroundClose: true);
                    });
                    return;
                }
                string fullApplicationsList;
                if (!isSystemAppDisplayed)
                    fullApplicationsList = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm list packages -3");
                else
                    fullApplicationsList = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm list packages");
                if (fullApplicationsList.Contains("cannot connect to daemon"))
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        SukiHost.ShowDialog(new PureDialog("设备连接失败"), allowBackgroundClose: true);
                    });
                    return;
                }
                if (!(sukiViewModel.Status == GetTranslation("Home_System")))
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        SukiHost.ShowDialog(new PureDialog("请在系统内执行"), allowBackgroundClose: true);
                    });
                    return;
                }
                var lines = fullApplicationsList.Split(separatorArray, StringSplitOptions.RemoveEmptyEntries);
                HasItems = lines.Length > 0;
                var applicationInfosTasks = lines.Select(async line =>
                {
                    var packageName = ExtractPackageName(line);
                    if (string.IsNullOrEmpty(packageName)) return null;
                    var combinedOutput = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dumpsys package {packageName}");
                    var installedDate = GetInstalledDate(combinedOutput.Split('\n'));
                    return installedDate != null
                        ? new ApplicationInfo { Name = packageName, InstalledDate = installedDate }
                        : null;
                });
                ApplicationInfo[] allApplicationInfos = await Task.WhenAll(applicationInfosTasks);
                var applicationInfos = allApplicationInfos.Where(info => info != null)
                                                         .OrderByDescending(app => app.Size)
                                                         .ThenBy(app => app.Name)
                                                         .ToList();
                Applications = new ObservableCollection<ApplicationInfo>(applicationInfos);
            });
        }
        catch (Exception ex)
        {
            SukiHost.ShowDialog(new ErrorDialog(ex.Message));
        }
        finally
        {
            IsBusy = false;
        }
        static string ExtractPackageName(string line)
        {
            var parts = line.Split(':');
            if (parts.Length < 2) return null;
            var packageNamePart = parts[1];
            var packageNameStartIndex = packageNamePart.LastIndexOf('/') + 1;
            return packageNameStartIndex < packageNamePart.Length
                ? packageNamePart.Substring(packageNameStartIndex)
                : null;
        }
    }

    [RelayCommand]
    public async Task InstallApk()
    {
        IsInstalling = true;
        if (!string.IsNullOrEmpty(ApkFile))
        {
            string output = await CallExternalProgram.ADB($"-s {Global.thisdevice} install -r \"{ApkFile}\"");
            if (output.Contains("Success"))
            {
                SukiHost.ShowDialog(new PureDialog("安装成功！"), allowBackgroundClose: true);
            }
            else
            {
                SukiHost.ShowDialog(new ErrorDialog($"安装失败：\r\n{output}"));
            }
        }
        else
        {
            SukiHost.ShowDialog(new PureDialog("未选择APK文件!"), allowBackgroundClose: true);
        }
        IsInstalling = false;
    }

    [RelayCommand]
    public async Task RunApp()
    {
        await Task.Run(async () =>
        {
            IsBusy = true;
            if (SelectedApplication() != "")
                await CallExternalProgram.ADB($"-s {Global.thisdevice} shell monkey -p {SelectedApplication()} 1");
            IsBusy = false;
        });
    }

    [RelayCommand]
    public async Task DisableApp()
    {
        await Task.Run(async () =>
        {
            IsBusy = true;
            if (SelectedApplication() != "")
                await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm disable {SelectedApplication()}");
            IsBusy = false;
        });
    }

    [RelayCommand]
    public async Task EnableApp()
    {
        IsBusy = true;
        var selectedApp = SelectedApplication();
        if (!string.IsNullOrEmpty(selectedApp))
        {
            await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm enable {selectedApp}");
        }
        IsBusy = false;
    }
    [RelayCommand]
    public async Task UninstallApp()
    {
        IsBusy = true; var selectedApp = SelectedApplication(); if (!string.IsNullOrEmpty(selectedApp))
        {
            await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm uninstall {selectedApp}");
        }
        IsBusy = false;
    }

    [RelayCommand]
    public async Task UninstallAppWithData()
    {
        IsBusy = true;
        var selectedApp = SelectedApplication();
        if (!string.IsNullOrEmpty(selectedApp))
        {
            // Note: This command may vary depending on the requirements and platform specifics.
            // The following is a general example and may not work as is.
            await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm uninstall -k {selectedApp}");
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
        IsBusy = true; var selectedApp = SelectedApplication();
        if (!string.IsNullOrEmpty(selectedApp))
        {
            // Get the apk file of the selected app, and save it to the user's desktop.
            var apkFile = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm path {selectedApp}");
            apkFile = apkFile[(apkFile.IndexOf(':') + 1)..].Trim();
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            await CallExternalProgram.ADB($"-s {Global.thisdevice} pull {apkFile} {desktopPath}");
        }
        IsBusy = false;
    }

    private static readonly char[] separatorArray = ['\r', '\n'];

    private static string GetInstalledDate(string[] lines)
    {
        var installedDateLine = lines.FirstOrDefault(x => x.Contains("firstInstallTime"));
        if (installedDateLine != null)
        {
            var installedDate = installedDateLine[(installedDateLine.IndexOf('=') + 1)..].Trim();
            return installedDate;
        }
        return "未知时间";
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
    private string installedDate;
}