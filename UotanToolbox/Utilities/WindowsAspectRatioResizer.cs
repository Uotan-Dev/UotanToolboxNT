using Avalonia.Controls;
using System;
using System.Runtime.InteropServices;

namespace UotanToolbox.Utilities;

/// <summary>
/// 在 Windows 上通过拦截 Win32 WM_SIZING 消息，在窗口实际改变尺寸和重绘之前直接约束拖拽矩形的宽高比。
///
/// 为什么不能在 ClientSizeProperty 变更回调里改 Width/Height：
/// Windows 拖拽边框时处于模态缩放循环中，窗口会先按鼠标位置完成一次非等比缩放并触发重绘，
/// 随后 Avalonia 回调再调用 SetWindowPos 强行改回等比尺寸；下一次鼠标移动时系统又会拉回非等比矩形，
/// 两套尺寸在每一帧来回拉扯，导致 DWM/渲染表面反复重建，窗口和 Viewbox 出现严重闪烁。
/// 拦截 WM_SIZING 直接修改 LPRECT 可在单次布局/渲染内一步到位，彻底消除闪烁。
/// </summary>
internal static class WindowsAspectRatioResizer
{
    private const uint WM_SIZING = 0x0214;

    private const int WMSZ_LEFT = 1;
    private const int WMSZ_RIGHT = 2;
    private const int WMSZ_TOP = 3;
    private const int WMSZ_TOPLEFT = 4;
    private const int WMSZ_TOPRIGHT = 5;
    private const int WMSZ_BOTTOM = 6;
    private const int WMSZ_BOTTOMLEFT = 7;
    private const int WMSZ_BOTTOMRIGHT = 8;

    private const double MinLogicalWidth = 640;

    public static bool IsSupported => OperatingSystem.IsWindows();

    public static IDisposable? Attach(Window window, double aspectRatio)
    {
        if (!IsSupported || aspectRatio <= 0)
        {
            return null;
        }

        Win32Properties.CustomWndProcHookCallback hook = (IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled) =>
            WndProc(window, aspectRatio, hWnd, msg, wParam, lParam, ref handled);

        Win32Properties.AddWndProcHookCallback(window, hook);
        return new HookSubscription(window, hook);
    }

    private static IntPtr WndProc(
        Window window,
        double aspectRatio,
        IntPtr hWnd,
        uint msg,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled)
    {
        if (msg != WM_SIZING || lParam == IntPtr.Zero)
        {
            return IntPtr.Zero;
        }

        if (!window.CanResize || window.WindowState != WindowState.Normal)
        {
            return IntPtr.Zero;
        }

        if (!GetWindowRect(hWnd, out RECT currentWindowRect) || !GetClientRect(hWnd, out RECT currentClientRect))
        {
            return IntPtr.Zero;
        }

        int currentClientWidth = currentClientRect.Right - currentClientRect.Left;
        int currentClientHeight = currentClientRect.Bottom - currentClientRect.Top;
        int frameWidth = (currentWindowRect.Right - currentWindowRect.Left) - currentClientWidth;
        int frameHeight = (currentWindowRect.Bottom - currentWindowRect.Top) - currentClientHeight;

        RECT rect = Marshal.PtrToStructure<RECT>(lParam);
        int proposedClientWidth = (rect.Right - rect.Left) - frameWidth;
        int proposedClientHeight = (rect.Bottom - rect.Top) - frameHeight;

        double scaling = window.RenderScaling > 0 ? window.RenderScaling : 1.0;
        int minClientWidth = (int)Math.Round(MinLogicalWidth * scaling);
        int minClientHeight = (int)Math.Round(minClientWidth / aspectRatio);

        int edge = unchecked((int)wParam.ToInt64());
        int targetClientWidth;
        int targetClientHeight;

        switch (edge)
        {
            case WMSZ_LEFT:
            case WMSZ_RIGHT:
                targetClientWidth = Math.Max(proposedClientWidth, minClientWidth);
                targetClientHeight = (int)Math.Round(targetClientWidth / aspectRatio);
                break;

            case WMSZ_TOP:
            case WMSZ_BOTTOM:
                targetClientHeight = Math.Max(proposedClientHeight, minClientHeight);
                targetClientWidth = (int)Math.Round(targetClientHeight * aspectRatio);
                break;

            case WMSZ_TOPLEFT:
            case WMSZ_TOPRIGHT:
            case WMSZ_BOTTOMLEFT:
            case WMSZ_BOTTOMRIGHT:
                // 将鼠标拖拽矩形正交投影到宽高比直线上，避免在两轴切换阈值附近来回跳变（抽搐），
                // 无论沿对角线放大还是缩小都保持连续平滑。
                double projectedHeight = (proposedClientWidth * aspectRatio + proposedClientHeight)
                                         / (aspectRatio * aspectRatio + 1.0);
                targetClientHeight = Math.Max((int)Math.Round(projectedHeight), minClientHeight);
                targetClientWidth = (int)Math.Round(targetClientHeight * aspectRatio);
                if (targetClientWidth < minClientWidth)
                {
                    targetClientWidth = minClientWidth;
                    targetClientHeight = (int)Math.Round(targetClientWidth / aspectRatio);
                }
                break;

            default:
                return IntPtr.Zero;
        }

        int targetWindowWidth = targetClientWidth + frameWidth;
        int targetWindowHeight = targetClientHeight + frameHeight;

        switch (edge)
        {
            case WMSZ_LEFT:
                rect.Left = rect.Right - targetWindowWidth;
                rect.Bottom = rect.Top + targetWindowHeight;
                break;
            case WMSZ_RIGHT:
                rect.Right = rect.Left + targetWindowWidth;
                rect.Bottom = rect.Top + targetWindowHeight;
                break;
            case WMSZ_TOP:
                rect.Top = rect.Bottom - targetWindowHeight;
                rect.Right = rect.Left + targetWindowWidth;
                break;
            case WMSZ_BOTTOM:
                rect.Bottom = rect.Top + targetWindowHeight;
                rect.Right = rect.Left + targetWindowWidth;
                break;
            case WMSZ_TOPLEFT:
                rect.Left = rect.Right - targetWindowWidth;
                rect.Top = rect.Bottom - targetWindowHeight;
                break;
            case WMSZ_TOPRIGHT:
                rect.Right = rect.Left + targetWindowWidth;
                rect.Top = rect.Bottom - targetWindowHeight;
                break;
            case WMSZ_BOTTOMLEFT:
                rect.Left = rect.Right - targetWindowWidth;
                rect.Bottom = rect.Top + targetWindowHeight;
                break;
            case WMSZ_BOTTOMRIGHT:
                rect.Right = rect.Left + targetWindowWidth;
                rect.Bottom = rect.Top + targetWindowHeight;
                break;
            default:
                return IntPtr.Zero;
        }

        Marshal.StructureToPtr(rect, lParam, false);
        handled = true;
        return new IntPtr(1);
    }

    private sealed class HookSubscription(Window window, Win32Properties.CustomWndProcHookCallback hook) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Win32Properties.RemoveWndProcHookCallback(window, hook);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
}
