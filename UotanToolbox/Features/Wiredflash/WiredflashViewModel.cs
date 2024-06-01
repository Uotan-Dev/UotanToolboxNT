using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using ReactiveUI;
using SukiUI.Controls;
using SukiUI.MessageBox;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Features.Wiredflash;

public partial class WiredflashViewModel : MainPageBase
{
    [ObservableProperty] private string _fastbootFile, _fastbootdFile, _adbsideloadFile, _fastbootupdatedFile;

    public WiredflashViewModel() : base("线刷", MaterialIconKind.WrenchCogOutline, -300)
    {
    }
}