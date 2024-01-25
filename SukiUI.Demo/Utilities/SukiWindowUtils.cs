using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;
using SukiUI.Controls;

namespace SukiUI.Demo.Utilities
{
    public static class SukiWindowUtils
    {
        /// <summary>
        /// For the record this is gross and all the theme, background, toasts and dialog stack needs to be rewritten.
        /// Providing each of them via discrete services with only a service locator singleton is a better long term strategy.
        /// </summary>
        public static SukiWindow Get() =>
            (SukiWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;

        /// <summary>
        /// I hate this too.
        /// </summary>
        public static SukiToast GetSukiHost(this SukiWindow window) =>
            window.FindDescendantOfType<SukiToast>()!;
    }
}