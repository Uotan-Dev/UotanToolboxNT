using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Appmgr;

public partial class AppmgrViewModel : MainPageBase
{
    [ObservableProperty]private ObservableCollection<ApplicationInfo> applications;

    public AppmgrViewModel() : base("应用管理", MaterialIconKind.ViewGridPlusOutline, -700)
    {
        Applications = new ObservableCollection<ApplicationInfo>
        {
            new ApplicationInfo
            {
                Name = "Application 1",
                Size = "182MB",
                InstalledDate = "2017-01-03"
            },
            new ApplicationInfo
            {
                Name = "Application 2",
                Size = "250MB",
                InstalledDate = "2023-05-15"
            }
        };
    }

    [RelayCommand]
    public async Task Connect()
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var fullApplicationsList = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm list packages -f");
            Debug.WriteLine(fullApplicationsList);

            var lines = fullApplicationsList.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var applicationInfos = new List<ApplicationInfo>();

            var tasks = lines.Select(async line =>
            {
                var parts = line.Split('=');
                if (parts.Length >= 2)
                {
                    var packageName = parts[1].Substring(parts[1].LastIndexOf('/') + 1);

                    if (string.IsNullOrEmpty(packageName))
                        return;

                    var combinedOutput = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dumpsys package {packageName} && dumpsys meminfo -a {packageName}");
                    var lineOutput = combinedOutput.Split('\n');

                    var sizeLine = lineOutput.FirstOrDefault(line => line.Contains("TOTAL"));
                    var size = GetPackageSize(sizeLine);

                    var installedDate = GetInstalledDate(lineOutput);

                    var applicationInfo = new ApplicationInfo
                    {
                        Name = packageName,
                        Size = size,
                        InstalledDate = installedDate
                    };

                    lock (applicationInfos)
                    {
                        applicationInfos.Add(applicationInfo);
                    }
                }
            });

            await Task.WhenAll(tasks);

            Applications = new ObservableCollection<ApplicationInfo>(applicationInfos);
        }
    }

    private string GetPackageSize(string sizeLine)
    {
        if (!string.IsNullOrEmpty(sizeLine))
        {
            string[] parts = sizeLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int sizeInKB = int.Parse(parts[1]);
            int sizeInMB = sizeInKB / 1024;
            return $"{sizeInMB}MB";
        }

        return "未知大小";
    }

    private string GetInstalledDate(string[] lines)
    {
        var installedDateLine = lines.FirstOrDefault(x => x.Contains("firstInstallTime"));
        if (installedDateLine != null)
        {
            var installedDate = installedDateLine.Substring(installedDateLine.IndexOf('=') + 1).Trim();
            return installedDate;
        }
        return "未知时间";
    }
}

public class ApplicationInfo
{
    public string Name { get; set; }
    public string Size { get; set; }
    public string InstalledDate { get; set; }
}