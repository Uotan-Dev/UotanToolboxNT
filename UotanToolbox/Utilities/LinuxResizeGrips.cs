using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UotanToolbox.Utilities;

/// <summary>
/// Avalonia 11.x 的 X11 后端会把 SystemDecorations.BorderOnly 归一化成 None，
/// 因此 Linux 上窗口既没有系统边框，也没有系统提供的缩放热区。
/// 这里在窗口根面板四周铺 8 个透明 Border，按下后由我们自己接管拖拽并实时改尺寸。
///
/// 为什么不直接用 Window.BeginResizeDrag（EWMH _NET_WM_MOVERESIZE）：
/// 交给窗口管理器缩放时，拖动期间几何由 WM 独占，客户端设置 Width/Height 会被忽略，
/// 本项目的固定宽高比联动就会失效（长宽各拖各的）。自己接管才能让比例始终锁死。
///
/// Avalonia 11.x normalizes SystemDecorations.BorderOnly to None on the X11 backend, so
/// Linux windows have neither a system frame nor system-provided resize grips. This adds
/// 8 transparent borders around the window root panel and drives the resize itself.
///
/// Why not Window.BeginResizeDrag (_NET_WM_MOVERESIZE): while the window manager owns the
/// geometry during the drag it ignores our Width/Height requests, which breaks the fixed
/// aspect ratio of this project. Driving the resize ourselves keeps the ratio locked.
/// </summary>
internal static class LinuxResizeGrips
{
    private const double EdgeThickness = 6;
    private const double CornerSize = 8;
    private const double MinWidth = 640;

    private static readonly (WindowEdge Edge, HorizontalAlignment H, VerticalAlignment V,
        StandardCursorType Cursor, bool IsCorner)[] Definitions =
    {
        (WindowEdge.North, HorizontalAlignment.Stretch, VerticalAlignment.Top, StandardCursorType.SizeNorthSouth, false),
        (WindowEdge.South, HorizontalAlignment.Stretch, VerticalAlignment.Bottom, StandardCursorType.SizeNorthSouth, false),
        (WindowEdge.West, HorizontalAlignment.Left, VerticalAlignment.Stretch, StandardCursorType.SizeWestEast, false),
        (WindowEdge.East, HorizontalAlignment.Right, VerticalAlignment.Stretch, StandardCursorType.SizeWestEast, false),
        (WindowEdge.NorthWest, HorizontalAlignment.Left, VerticalAlignment.Top, StandardCursorType.TopLeftCorner, true),
        (WindowEdge.NorthEast, HorizontalAlignment.Right, VerticalAlignment.Top, StandardCursorType.TopRightCorner, true),
        (WindowEdge.SouthWest, HorizontalAlignment.Left, VerticalAlignment.Bottom, StandardCursorType.BottomLeftCorner, true),
        (WindowEdge.SouthEast, HorizontalAlignment.Right, VerticalAlignment.Bottom, StandardCursorType.BottomRightCorner, true),
    };

    /// <summary>
    /// 当前平台是否需要自绘缩放热区。
    /// Whether the current platform needs self-drawn resize grips.
    /// </summary>
    public static bool IsNeeded => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    /// <summary>
    /// 热区拖拽是否正在进行。窗口自身的比例联动逻辑在拖拽期间应让位，
    /// 否则会和这里的实时尺寸计算互相拉扯。
    /// True while a grip drag is in progress. The window's own ratio enforcement should
    /// stand down during the drag so it does not fight this code.
    /// </summary>
    public static bool IsDragging { get; private set; }

    /// <summary>
    /// 清掉已经挂上的热区。
    /// Removes grips previously attached.
    /// </summary>
    public static void Detach(List<Border> grips)
    {
        foreach (Border grip in grips)
        {
            grip.PointerPressed -= OnGripPressed;
            (grip.Parent as Panel)?.Children.Remove(grip);
        }

        grips.Clear();
        IsDragging = false;
    }

    /// <summary>
    /// 在窗口根面板四周挂上 8 个缩放热区。
    /// Attaches 8 resize grips around the given window root panel.
    /// </summary>
    /// <param name="aspectRatio">窗口固定宽高比（宽 / 高），小于等于 0 表示不锁比例。</param>
    public static void Attach(Panel rootPanel, List<Border> grips, double aspectRatio)
    {
        Log($"Attach called, needed={IsNeeded}, panel={rootPanel.GetType().Name}, existing={rootPanel.Children.Count}, ratio={aspectRatio}");

        if (!IsNeeded)
        {
            return;
        }

        foreach (var (edge, h, v, cursor, isCorner) in Definitions)
        {
            var grip = new Border
            {
                Tag = new GripTag(edge, aspectRatio),
                Background = Brushes.Transparent,
                HorizontalAlignment = h,
                VerticalAlignment = v,
                Cursor = new Cursor(cursor),
            };

            if (isCorner)
            {
                grip.Width = CornerSize;
                grip.Height = CornerSize;
            }
            else
            {
                if (v == VerticalAlignment.Stretch)
                {
                    grip.Width = EdgeThickness;
                }

                if (h == HorizontalAlignment.Stretch)
                {
                    grip.Height = EdgeThickness;
                }
            }

            grip.PointerPressed += OnGripPressed;
            grips.Add(grip);
            rootPanel.Children.Add(grip);
        }

        Log($"attached {grips.Count} grips, panel children={rootPanel.Children.Count}");
    }

    private sealed record GripTag(WindowEdge Edge, double AspectRatio);

    private static void OnGripPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border grip || grip.Tag is not GripTag tag)
        {
            return;
        }

        if (grip.FindAncestorOfType<Window>() is not { } window)
        {
            return;
        }

        // 尊重“是否允许缩放”开关，并只在普通窗口状态下生效。
        // Honour the resize toggle and only act in the normal window state.
        if (!window.CanResize || window.WindowState != WindowState.Normal)
        {
            Log("resize disabled by CanResize / WindowState");
            return;
        }

        if (!e.GetCurrentPoint(window).Properties.IsLeftButtonPressed)
        {
            return;
        }

        WindowEdge edge = tag.Edge;
        double ratio = tag.AspectRatio;

        double scaling = window.RenderScaling > 0 ? window.RenderScaling : 1;
        PixelPoint startScreen = window.PointToScreen(e.GetPosition(window));
        double startWidth = window.ClientSize.Width > 0 ? window.ClientSize.Width : window.Width;
        double startHeight = window.ClientSize.Height > 0 ? window.ClientSize.Height : window.Height;
        PixelPoint startPosition = window.Position;

        bool west = edge is WindowEdge.West or WindowEdge.NorthWest or WindowEdge.SouthWest;
        bool east = edge is WindowEdge.East or WindowEdge.NorthEast or WindowEdge.SouthEast;
        bool north = edge is WindowEdge.North or WindowEdge.NorthWest or WindowEdge.NorthEast;
        bool south = edge is WindowEdge.South or WindowEdge.SouthWest or WindowEdge.SouthEast;

        // 屏幕可用区上限（换算成逻辑像素）。撞到边界时必须两轴一起回退，
        // 否则窗口管理器会单独截断某一维，比例就失真了。
        // Screen working-area limits in logical pixels. When hitting the edge both axes must
        // shrink together, otherwise the window manager truncates a single axis and the
        // ratio breaks.
        double maxWidth = double.PositiveInfinity;
        double maxHeight = double.PositiveInfinity;
        if (window.Screens.ScreenFromWindow(window) is { } screen)
        {
            maxWidth = screen.WorkingArea.Width / scaling;
            maxHeight = screen.WorkingArea.Height / scaling;
        }

        IsDragging = true;
        Log($"drag start {edge} size={startWidth}x{startHeight} pos={startPosition} scale={scaling}");

        void OnMoved(object? _, PointerEventArgs args)
        {
            PixelPoint current = window.PointToScreen(args.GetPosition(window));
            double dx = (current.X - startScreen.X) / scaling;
            double dy = (current.Y - startScreen.Y) / scaling;

            double newWidth = startWidth;
            double newHeight = startHeight;

            if (east) newWidth = startWidth + dx;
            if (west) newWidth = startWidth - dx;
            if (south) newHeight = startHeight + dy;
            if (north) newHeight = startHeight - dy;

            if (ratio > 0)
            {
                // 锁定宽高比：只有横向边时以宽度为准，只有纵向边时以高度为准，
                // 角上取换算后变化更大的一轴作为主导。
                // Lock the aspect ratio: horizontal-only edges follow the width, vertical-only
                // edges follow the height, corners follow whichever axis moved more.
                bool widthDrives;
                if ((east || west) && (north || south))
                {
                    widthDrives = Math.Abs(newWidth - startWidth) >= Math.Abs((newHeight - startHeight) * ratio);
                }
                else
                {
                    widthDrives = east || west;
                }

                if (widthDrives)
                {
                    newHeight = newWidth / ratio;
                }
                else
                {
                    newWidth = newHeight * ratio;
                }

                if (newWidth < MinWidth)
                {
                    newWidth = MinWidth;
                    newHeight = newWidth / ratio;
                }

                // 撞到屏幕边界时按比例整体回退，保持比例不变。
                // Clamp to the screen while keeping the ratio intact.
                if (newWidth > maxWidth)
                {
                    newWidth = maxWidth;
                    newHeight = newWidth / ratio;
                }

                if (newHeight > maxHeight)
                {
                    newHeight = maxHeight;
                    newWidth = newHeight * ratio;
                }
            }
            else
            {
                newWidth = Math.Max(newWidth, MinWidth);
                newHeight = Math.Max(newHeight, MinWidth);
            }

            window.Width = newWidth;
            window.Height = newHeight;

            // 从左边 / 上边缩放时窗口原点要跟着移动，右边和下边才是锚点。
            // When resizing from the west/north edge the origin must follow so that the
            // opposite edge stays anchored.
            if (west || north)
            {
                double px = west ? startPosition.X + (startWidth - newWidth) * scaling : startPosition.X;
                double py = north ? startPosition.Y + (startHeight - newHeight) * scaling : startPosition.Y;
                window.Position = new PixelPoint((int)Math.Round(px), (int)Math.Round(py));
            }
        }

        void OnReleased(object? _, PointerReleasedEventArgs args)
        {
            grip.PointerMoved -= OnMoved;
            grip.PointerReleased -= OnReleased;
            args.Pointer.Capture(null);
            IsDragging = false;
            Log($"drag end size={window.ClientSize.Width}x{window.ClientSize.Height}");
        }

        grip.PointerMoved += OnMoved;
        grip.PointerReleased += OnReleased;
        e.Pointer.Capture(grip);

        e.Handled = true;
    }

    private static void Log(string message)
    {
        if (Environment.GetEnvironmentVariable("UOTAN_GRIP_LOG") != "1")
        {
            return;
        }

        try
        {
            System.IO.File.AppendAllText("/tmp/grip.log", $"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}");
        }
        catch
        {
            // 日志失败不影响功能。
            // Logging failures must not break the feature.
        }
    }
}
