using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UotanToolbox.Common;
using UotanToolbox.Features.Appmgr;

namespace UotanToolbox.Features.Filemgr;

public partial class FilemgrViewModel : MainPageBase
{
    [ObservableProperty]
    private ObservableCollection<File> files = [];
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public FilemgrViewModel() : base("文件管理", MaterialIconKind.FolderOutline, -625)
    {
    }
}

public partial class File : ObservableObject
{
    [ObservableProperty]
    private string fileName;

    [ObservableProperty]
    private string fileTime;
}