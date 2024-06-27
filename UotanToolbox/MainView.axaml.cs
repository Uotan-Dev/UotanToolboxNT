using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SukiUI.Controls;
using SukiUI.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UotanToolbox.Common;

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

    private void OpenTerminal(object? sender, RoutedEventArgs e)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                WorkingDirectory = Path.Combine(Global.bin_path, "platform-tools"),
                UseShellExecute = true
            });
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            Process.Start(new ProcessStartInfo
            {
                FileName = "/usr/bin/gnome-terminal",
                Arguments = $"--working-directory={Path.Combine(Global.bin_path, "platform-tools", "adb")}",
                UseShellExecute = false
            });
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            Process.Start("open", "-a Terminal " + Path.Combine(Global.bin_path, "platform-tools", "adb"));
    }
}