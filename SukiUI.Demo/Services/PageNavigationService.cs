using SukiUI.Demo.Features;
using System;

namespace SukiUI.Demo.Services;

public class PageNavigationService
{
    public Action<Type>? NavigationRequested { get; set; }

    public void RequestNavigation<T>() where T : DemoPageBase
    {
        NavigationRequested?.Invoke(typeof(T));
    }
}