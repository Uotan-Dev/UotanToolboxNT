using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace UotanToolbox.Common;

public class ViewLocator : IDataTemplate
{
    readonly Dictionary<object, Control> _controlCache = [];

    public Control Build(object data)
    {
        var fullName = data?.GetType().FullName;

        if (fullName is null)
        {
            return new TextBlock { Text = "Data is null or has no name." };
        }

        if (fullName.Contains("ApplicationInfo"))
        {
            fullName = "UotanToolbox.Features.Appmgr.AppmgrViewModel";
        }

        var name = fullName.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type is null)
        {
            return new TextBlock { Text = $"No View For {name}." };
        }

        if (!_controlCache.TryGetValue(data!, out Control res))
        {
            res ??= (Control)Activator.CreateInstance(type)!;
            _controlCache[data!] = res;
        }

        res.DataContext = data;
        return res;
    }

    public bool Match(object data) => data is INotifyPropertyChanged;
}