using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SukiUI.Content;
using SukiUI.Controls;
using UotanToolbox.Common;
using UotanToolbox.Utilities;
using SukiUI.Models;

namespace UotanToolbox;

public partial class MainView : SukiWindow
{
    public MainView()
    {
        InitializeComponent();
        var bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://UotanToolbox/Assets/OIG.N5o-removebg-preview.png")));
        Icon = new WindowIcon(bitmap);
        if (Global.System == "Linux")
        {
            this.SystemDecorations = SystemDecorations.BorderOnly;
        }
    }

    private void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        if (e.Source is not MenuItem mItem) return;
        if (mItem.DataContext is not SukiColorTheme cTheme) return;
        vm.ChangeTheme(cTheme);
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        IsMenuVisible = !IsMenuVisible;
    }
}