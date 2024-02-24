using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SukiUI.Controls;
using SukiUI.Models;

namespace UotanToolbox;

public partial class MainView : SukiWindow
{
    public MainView()
    {
        InitializeComponent();
        var bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://UotanToolbox/Assets/OIG.N5o-removebg-preview.png")));
        Icon = new WindowIcon(bitmap);
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