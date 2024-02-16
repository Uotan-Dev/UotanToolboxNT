using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using UotanToolbox.Common;
using UotanToolbox.Features;
using UotanToolbox.Services;
using System;
using System.Linq;
using System.Globalization;
using System.Diagnostics;

namespace UotanToolbox;

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
            var mainVm = _provider?.GetRequiredService<MainViewModel>();

            desktop.MainWindow = viewLocator?.Build(mainVm) as Window;
            desktop.MainWindow.Width = 1240;
            desktop.MainWindow.Height = 840;
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
        services.AddSingleton<MainViewModel>();
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => !p.IsAbstract && typeof(MainPageBase).IsAssignableFrom(p));
        foreach (var type in types)
            services.AddSingleton(typeof(MainPageBase), type);

        return services.BuildServiceProvider();
    }
}