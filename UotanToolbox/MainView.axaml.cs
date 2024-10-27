using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SukiUI.Controls;
using SukiUI.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UotanToolbox.Common;
using UotanToolbox.Features.Settings;

namespace UotanToolbox;

public partial class MainView : SukiWindow
{
    public MainView()
    {
        InitializeComponent();
        Bitmap bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://UotanToolbox/Assets/OIG.N5o-removebg-preview.png")));
        Icon = new WindowIcon(bitmap);
        SetSystemDecorationsBasedOnPlatform();
    }

    private void SetSystemDecorationsBasedOnPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            this.SystemDecorations = SystemDecorations.BorderOnly;
        }
        else
        {
            this.SystemDecorations = SystemDecorations.Full;
        }
    }

    private void MenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        if (e.Source is not MenuItem mItem)
        {
            return;
        }

        if (mItem.DataContext is not SukiColorTheme cTheme)
        {
            return;
        }

        vm.ChangeTheme(cTheme);
    }

    private void InputElement_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        IsMenuVisible = !IsMenuVisible;
    }

    private void OpenTerminal(object sender, RoutedEventArgs e)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _ = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                WorkingDirectory = Path.Combine(Global.bin_path, "platform-tools"),
                UseShellExecute = true
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string[] terminalCommands = new string[]
            {
            "x-terminal-emulator",  // Generic terminal emulator
            "gnome-terminal",       // GNOME terminal
            "deepin-terminal",      // deepin terminal
            "konsole",              // KDE Konsole
            "xfce4-terminal",       // XFCE terminal
            "mate-terminal",        // MATE terminal
            "lxterminal",           // LXDE terminal
            "tilix",                // Tilix terminal
            "alacritty",            // Alacritty terminal
            "xterm",                 // Xterm as fallback
            "kitty",                // Kitty terminal
            "wezterm"              // Wezterm terminal
            };

            foreach (string terminal in terminalCommands)
            {
                try
                {
                    _ = Process.Start(new ProcessStartInfo
                    {
                        FileName = terminal,
                        Arguments = $"--working-directory={Path.Combine(Global.bin_path, "platform-tools", "adb")}",
                        UseShellExecute = false
                    });
                    break;  // If successful, break out of the loop
                }
                catch
                {
                    // Continue trying other terminals if one fails
                }
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _ = Process.Start("open", "-a Terminal " + Path.Combine(Global.bin_path, "platform-tools", "adb"));
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        Closing += (s, e) =>
        {
            SettingsViewModel settingsViewModel = new SettingsViewModel();
            Settings.Default.IsLightTheme = settingsViewModel.IsLightTheme;
            Settings.Default.Save();
        };
    }
}
