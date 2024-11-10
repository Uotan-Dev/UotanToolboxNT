using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Material.Icons;
using SukiUI.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Features.Appmgr;

namespace UotanToolbox.Features.Filemgr;

public partial class FilemgrViewModel : MainPageBase
{
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }
    public AvaloniaList<Node> TreeViewContent { get; } = [];

    public FilemgrViewModel() : base("文件管理", MaterialIconKind.ChartPieOutline, -250)
    {
        TreeViewContent.AddRange(
            Enumerable.Range(1, 10).Select(x => new Node($"Outer {x}",
                Enumerable.Range(1, 5).Select(y => new Node($"Inner {y}",
                    Enumerable.Range(1, 2).Select(z => new Node($"Innermost {z}",
                        Enumerable.Range(1, 2).Select(n => new Node($"1234567 {n}")))))))));
    }
}

public partial class Project : ObservableObject
{
    [ObservableProperty]
    private string projectName;

    [ObservableProperty]
    private bool isSelected;
}

public partial class Node(string value, IEnumerable<Node>? subNodes = null) : ObservableObject
{
    public AvaloniaList<Node> SubNodes { get; } = new(subNodes ?? Array.Empty<Node>());
    [ObservableProperty] private string _value = value;
}