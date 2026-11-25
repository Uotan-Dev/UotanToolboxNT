using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SukiUI.Controls;
using SukiUI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UotanToolbox.Common;
using UotanToolbox.Features.Settings;
using UotanToolbox.Utilities;

namespace UotanToolbox;

public partial class MainView : SukiWindow
{
    internal const double AspectRatio = 1235.0 / 840.0;
    private const double DesignContentWidth = 1235.0;
    private Size _lastSize = new Size(1235, 840);
    private bool _isAdjustingSize;
    private IDisposable? _windowsResizerHook;

    // Linux 上 Avalonia 11.x 没有系统边框，缩放热区需要自己挂。
    // Avalonia 11.x has no system border on Linux, so resize grips must be added manually.
    private readonly List<Border> _resizeGrips = new();

    public MainView()
    {
        InitializeComponent();
        Bitmap bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://UotanToolbox/Assets/OIG.N5o-removebg-preview.png")));
        Icon = new WindowIcon(bitmap);
        SetSystemDecorationsBasedOnPlatform();
        _windowsResizerHook = WindowsAspectRatioResizer.Attach(this, AspectRatio);
        this.GetObservable(ClientSizeProperty).Subscribe(OnClientSizeChanged);
        AttachViewboxHeightSync();
    }

    /// <summary>
    /// 标题栏位于 Viewbox 之外且高度固定，窗口放大/缩小时内容区宽高比会轻微变化；
    /// 同时不同子页面的内容宽度不一致，若 SukiSideMenu 只写死 Height="795"，
    /// Viewbox 等比缩放后会在左侧菜单栏下方露出缝隙。
    /// 这里固定设计宽度 1235，并根据 Viewbox 实际可用区域动态同步 SukiSideMenu.Height，
    /// 使 Viewbox 缩放后的高度始终严丝合缝贴齐窗口底部。
    /// </summary>
    private void AttachViewboxHeightSync()
    {
        // 注意：不能监听 Viewbox 自身的 Bounds，因为 Avalonia 的 Viewbox.ArrangeOverride
        // 返回的是按子元素宽高比缩放后的矩形，其宽高比永远等于子元素自身比例。
        // 必须监听外层拉伸填充的 Panel（MainContentHost）的 Bounds，才能拿到标题栏下方的真实可用尺寸。
        if (this.FindControl<Panel>("MainContentHost") is not { } contentHost ||
            this.FindControl<SukiSideMenu>("MainSideMenu") is not { } mainSideMenu)
        {
            return;
        }

        contentHost.GetObservable(BoundsProperty).Subscribe(bounds =>
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            double targetHeight = bounds.Height * (DesignContentWidth / bounds.Width);
            if (Math.Abs(mainSideMenu.Height - targetHeight) > 0.5)
            {
                mainSideMenu.Height = targetHeight;
            }
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        _windowsResizerHook?.Dispose();
        _windowsResizerHook = null;
        base.OnClosed(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // 模板重新应用时先清掉旧热区，避免重复叠加。
        // Drop previous grips before adding new ones when the template is re-applied.
        LinuxResizeGrips.Detach(_resizeGrips);

        if (e.NameScope.Find<Panel>("PART_Root") is not { } rootPanel)
        {
            return;
        }

        LinuxResizeGrips.Attach(rootPanel, _resizeGrips, AspectRatio);
    }

    private void OnClientSizeChanged(Size newSize)
    {
        // Windows 上由 WM_SIZING 钩子在系统缩放阶段直接锁定宽高比，
        // 这里不能再改 Width/Height，否则会与系统拖拽循环互相拉扯导致剧烈闪烁。
        if (WindowsAspectRatioResizer.IsSupported)
        {
            _lastSize = newSize;
            return;
        }

        // 自绘热区拖拽时已经按比例算好尺寸，这里不要再插手，
        // 否则两套逻辑会互相拉扯，长宽变成各自独立变化。
        // The grip drag already applies the ratio; stay out of its way here or the two
        // implementations will fight and the axes drift apart.
        if (LinuxResizeGrips.IsDragging)
        {
            _lastSize = newSize;
            return;
        }

        if (_isAdjustingSize || newSize.Width <= 0 || newSize.Height <= 0)
        {
            return;
        }

        double deltaWidth = Math.Abs(newSize.Width - _lastSize.Width);
        double deltaHeight = Math.Abs(newSize.Height - _lastSize.Height);

        _isAdjustingSize = true;
        try
        {
            if (deltaWidth > deltaHeight)
            {
                double expectedHeight = newSize.Width / AspectRatio;
                if (Math.Abs(newSize.Height - expectedHeight) > 1)
                {
                    this.Height = expectedHeight;
                    _lastSize = new Size(newSize.Width, expectedHeight);
                    return;
                }
            }
            else
            {
                double expectedWidth = newSize.Height * AspectRatio;
                if (Math.Abs(newSize.Width - expectedWidth) > 1)
                {
                    this.Width = expectedWidth;
                    _lastSize = new Size(expectedWidth, newSize.Height);
                    return;
                }
            }

            _lastSize = newSize;
        }
        finally
        {
            _isAdjustingSize = false;
        }
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

    private void SetResize(object sender, RoutedEventArgs e)
    {
        Global.SetResize = this.CanResize;
        Settings.Default.WindowUnlockResize = this.CanResize;
        Settings.Default.Save();
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
            string[] terminalCommands =
            [
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
            ];

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
            double saveWidth = ClientSize.Width > 0 ? ClientSize.Width : Width;
            double saveHeight = ClientSize.Height > 0 ? ClientSize.Height : Height;
            Settings.Default.WindowScale = $"{Math.Round(saveWidth, 0)}x{Math.Round(saveHeight, 0)}";
            Settings.Default.Save();
            KillProcess("adb");
            KillProcess("hdc");
        };
    }

    private static void KillProcess(string processName)
    {
        foreach (var process in Process.GetProcessesByName(processName))
        {
            try
            {
                process.Kill();
                process.WaitForExit();
            }
            catch (Win32Exception ex) { }
            catch (InvalidOperationException) { }
        }
    }
}
