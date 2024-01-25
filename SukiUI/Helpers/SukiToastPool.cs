using SukiUI.Controls;
using System.Collections.Generic;

namespace SukiUI.Helpers;

internal static class SukiToastPool
{
    private static readonly Stack<SukiToast> Pool = new();

    internal static SukiToast Get()
    {
        return Pool.Count >= 1 ? Pool.Pop() : new SukiToast();
    }

    internal static void Return(SukiToast toast) => Pool.Push(toast);

    internal static void Return(IEnumerable<SukiToast> toasts)
    {
        foreach (var toast in toasts)
            Pool.Push(toast);
    }
}