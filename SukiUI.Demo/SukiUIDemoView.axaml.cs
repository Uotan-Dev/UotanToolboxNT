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
using SukiUI.Demo.Utilities;
using SukiUI.Models;

namespace SukiUI.Demo;

public partial class SukiUIDemoView : SukiWindow
{
    public SukiUIDemoView()
    {
        InitializeComponent();
        var bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://SukiUI.Demo/Assets/OIG.N5o-removebg-preview.png")));
        Icon = new WindowIcon(bitmap);
    }

    private void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SukiUIDemoViewModel vm) return;
        if (e.Source is not MenuItem mItem) return;
        if (mItem.DataContext is not SukiColorTheme cTheme) return;
        vm.ChangeTheme(cTheme);
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        IsMenuVisible = !IsMenuVisible;
    }
}