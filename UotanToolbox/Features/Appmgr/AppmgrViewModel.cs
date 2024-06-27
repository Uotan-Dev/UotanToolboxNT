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
    [ObservableProperty] private ObservableCollection<ApplicationInfo> applications;
    [ObservableProperty] private bool isBusy = false, hasItems = false;
    [ObservableProperty] private bool isSystemAppDisplayed = false, isInstalling = false;
    [ObservableProperty] private string _apkFile;

    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public AppmgrViewModel() : base(GetTranslation("Sidebar_Appmgr"), MaterialIconKind.ViewGridPlusOutline, -700)
    {
        if (Global.load_times == 0)
        {
            Applications = [];
            Global.load_times = 1;
        }
    }

    [RelayCommand]
    private async Task<bool> IsDeviceConnected()
    {
        try
        {
            var devicesListOutput = await CallExternalProgram.ADB("devices");
            return devicesListOutput.Contains($"{Global.thisdevice} device");
        }
        catch
        {
            return false;
        }
    }
    public async Task Connect()
    {
        IsBusy = true;
        try
        {
            if (!await IsDeviceConnected()) 
            {
                SukiHost.ShowDialog(new ConnectionDialog("Device not found."));
                return;
            }
            if (!await GetDevicesInfo.SetDevicesInfoLittle())
                return;
            string fullApplicationsList;
            if (!isSystemAppDisplayed)
                fullApplicationsList = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm list packages -3");
            else
                fullApplicationsList = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm list packages");

            var lines = fullApplicationsList.Split(separatorArray, StringSplitOptions.RemoveEmptyEntries);
            HasItems = lines.Length > 0;
            var applicationInfosTasks = lines.AsParallel()
                                           .Select(async line =>
                                           {
                                               var packageName = ExtractPackageName(line);
                                               if (string.IsNullOrEmpty(packageName)) return null;
                                               var combinedOutput = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dumpsys package {packageName}");
                                               var installedDate = GetInstalledDate(combinedOutput.Split('\n'));

                                               return packageName != null && installedDate != null
                                                     ? new ApplicationInfo { Name = packageName, InstalledDate = installedDate }
                                                     : null;
                                           });
            var applicationInfos = (await Task.WhenAll(applicationInfosTasks))
                                             .Where(info => info != null)
                                             .OrderByDescending(app => app.Size)
                                             .ThenBy(app => app.Name)
                                             .ToList();
            Applications = new ObservableCollection<ApplicationInfo>(applicationInfos);
        }
        catch (Exception ex)
        {
            SukiHost.ShowDialog(new ConnectionDialog(ex.Message));
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
            await CallExternalProgram.ADB($"-s {Global.thisdevice} install -r {ApkFile}");
        }
        else
        {
            SukiHost.ShowDialog(new ConnectionDialog("未选择APK文件!"));
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
        var selectedApp = Applications.FirstOrDefault(app => app.IsSelected);
        if (selectedApp != null)
        {
            return selectedApp.Name;
        }
        else
        {
            return "";
        }
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

public class ApplicationInfo : ObservableObject
{
    private bool isSelected;

    public string? Name { get; set; }
    public string? Size { get; set; }
    public string? InstalledDate { get; set; }

    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }
}