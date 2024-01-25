using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SukiUI.Demo.Common;

public class ViewLocator : IDataTemplate
{
    private readonly Dictionary<object, Control> _controlCache;

    public ViewLocator()
    {
        _controlCache = new Dictionary<object, Control>();
    }

    public Control Build(object? data)
    {
        var fullName = data?.GetType().FullName;
        if (fullName is null)
            return new TextBlock { Text = "Data is null or has no name." };
        var name = fullName.Replace("ViewModel", "View");
        var type = Type.GetType(name);
        if (type is null)
            return new TextBlock { Text = $"No View For {name}." };

        if (!_controlCache.TryGetValue(data!, out var res))
        {
            res ??= (Control)Activator.CreateInstance(type)!;
            _controlCache[data!] = res;
        }

        res.DataContext = data;
        return res;
    }

    public bool Match(object? data) => data is INotifyPropertyChanged;
}