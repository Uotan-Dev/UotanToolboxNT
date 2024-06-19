using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Controls;
using System;
using System.Collections.Generic;
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

    public AppmgrViewModel() : base("应用管理", MaterialIconKind.ViewGridPlusOutline, -700)
    {
        Applications = [];
    }

    [RelayCommand]
    public async Task Connect()
    {
        IsBusy = true;
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var fullApplicationsList = "";
            if (isSystemAppDisplayed == false)
                fullApplicationsList = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm list packages -3");
            else
                fullApplicationsList = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm list packages");

            var lines = fullApplicationsList.Split(separatorArray, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length != 0) HasItems = true;
            var applicationInfos = new List<ApplicationInfo>();

            var tasks = lines.Select(async line =>
            {
                var parts = line.Split(':');
                if (parts.Length >= 2)
                {
                    var packageName = parts[1][(parts[1].LastIndexOf('/') + 1)..];
                    if (string.IsNullOrEmpty(packageName)) return;
                    var combinedOutput = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dumpsys package {packageName}");
                    var lineOutput = combinedOutput.Split('\n');
                    //var sizeLine = lineOutput.FirstOrDefault(line => line.Contains("TOTAL"));
                    //var size = GetPackageSize(sizeLine);
                    var installedDate = GetInstalledDate(lineOutput);
                    var applicationInfo = new ApplicationInfo
                    {
                        Name = packageName,
                        //Size = size,
                        InstalledDate = installedDate
                    };
                    lock (applicationInfos)
                    {
                        applicationInfos.Add(applicationInfo);
                    }
                }
            });

            await Task.WhenAll(tasks);
            applicationInfos = [.. applicationInfos.OrderByDescending(app => app.Size).ThenBy(app => app.Name)];
            Applications = new ObservableCollection<ApplicationInfo>(applicationInfos);
        }
        IsBusy = false;
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

    /*private static string GetPackageSize(string sizeLine)
    {
        if (!string.IsNullOrEmpty(sizeLine))
        {
            string[] parts = sizeLine.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            int sizeInKB = int.Parse(parts[1]);
            int sizeInMB = sizeInKB / 1024;
            return $"{sizeInMB}MB";
        }

        return "未知大小";
    }*/

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