using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SukiUI.Demo.Common;
using SukiUI.Demo.Features;
using SukiUI.Demo.Services;
using System;
using System.Linq;

namespace SukiUI.Demo;

public partial class App : Application
{
    private IServiceProvider? _provider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        _provider = ConfigureServices();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var viewLocator = _provider?.GetRequiredService<IDataTemplate>();
            var mainVm = _provider?.GetRequiredService<SukiUIDemoViewModel>();

            desktop.MainWindow = viewLocator?.Build(mainVm) as Window;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider ConfigureServices()
    {
        var viewlocator = Current?.DataTemplates.First(x => x is ViewLocator);
        var services = new ServiceCollection();

        if (viewlocator is not null)
            services.AddSingleton(viewlocator);
        services.AddSingleton<PageNavigationService>();

        // Viewmodels
        services.AddSingleton<SukiUIDemoViewModel>();
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => !p.IsAbstract && typeof(DemoPageBase).IsAssignableFrom(p));
        foreach (var type in types)
            services.AddSingleton(typeof(DemoPageBase), type);

        return services.BuildServiceProvider();
    }
}