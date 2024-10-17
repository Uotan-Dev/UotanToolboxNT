using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SukiUI.Controls;
using SukiUI.Toasts;

namespace UotanToolbox.Common;

public class ViewLocator : IDataTemplate
{
    private readonly Dictionary<object, Control> _controlCache = [];

    public Control Build(object data)
    {
        string fullName = data?.GetType().FullName;
        if (fullName is null)
        {
            return new TextBlock { Text = "Data is null or has no name." };
        }

        if (fullName.Contains("ApplicationInfo"))
        {
            fullName = "UotanToolbox.Features.Appmgr.AppmgrViewModel";
        }

        string name = fullName.Replace("ViewModel", "View");
        Type type = Type.GetType(name);
        if (type is null)
        {
            return new TextBlock { Text = $"No View For {name}." };
        }

        if (fullName.Contains("UotanToolbox.Features.Home.HomeView"))
        {
            if (!_controlCache.TryGetValue(data!, out Control ress))
            {
                ress ??= Global.homeView!;
                _controlCache[data!] = ress;
                _ = FeaturesHelper.CheckEnvironment(Global.HomeDialogManager);
            }
        }

        if (fullName.Contains("UotanToolbox.Features.Basicflash.BasicflashView"))
        {
            if (!_controlCache.TryGetValue(data!, out Control ress))
            {
                ress ??= Global.basicflashView!;
                _controlCache[data!] = ress;
            }
        }

        if (fullName.Contains("UotanToolbox.Features.Appmgr.AppmgrView"))
        {
            if (!_controlCache.TryGetValue(data!, out Control ress))
            {
                ress ??= Global.appmgrView!;
                _controlCache[data!] = ress;
            }
        }

        if (fullName.Contains("UotanToolbox.Features.Wiredflash.WiredflashView"))
        {
            if (!_controlCache.TryGetValue(data!, out Control ress))
            {
                ress ??= Global.wiredflashView!;
                _controlCache[data!] = ress;
            }
        }

        if (fullName.Contains("UotanToolbox.Features.Customizedflash.CustomizedflashView"))
        {
            if (!_controlCache.TryGetValue(data!, out Control ress))
            {
                ress ??= Global.customizedflashView!;
                _controlCache[data!] = ress;
            }
        }

        if (fullName.Contains("UotanToolbox.Features.Scrcpy.ScrcpyView"))
        {
            if (!_controlCache.TryGetValue(data!, out Control ress))
            {
                ress ??= Global.scrcpyView!;
                _controlCache[data!] = ress;
            }
        }

        if (fullName.Contains("UotanToolbox.Features.FormatExtract.FormatExtractView"))
        {
            if (!_controlCache.TryGetValue(data!, out Control ress))
            {
                ress ??= Global.formatExtractView!;
                _controlCache[data!] = ress;
            }
        }

        if (fullName.Contains("UotanToolbox.Features.Others.OthersView"))
        {
            if (!_controlCache.TryGetValue(data!, out Control ress))
            {
                ress ??= Global.othersView!;
                _controlCache[data!] = ress;
            }
        }

        if (fullName.Contains("UotanToolbox.Features.Modifypartition.ModifypartitionView"))
        {
            if (!_controlCache.TryGetValue(data!, out Control ress))
            {
                ress ??= Global.modifypartitionView!;
                _controlCache[data!] = ress;
                _ = FeaturesHelper.LoadMassage(Global.ModPartDialogManager);
            }
        }

        if (!_controlCache.TryGetValue(data!, out Control res))
        {
            res ??= (Control)Activator.CreateInstance(type)!;
            _controlCache[data!] = res;
        }

        res.DataContext = data;
        return res;
    }

    public bool Match(object data)
    {
        return data is INotifyPropertyChanged;
    }
}