using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Linq;
using UotanToolbox.Common;
using UotanToolbox.Features;
using UotanToolbox.Services;

namespace UotanToolbox;

public partial class App : Application
{
    private IServiceProvider _provider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        _provider = ConfigureServices();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        CultureInfo CurCulture;
        if (Settings.Default.Language != null && Settings.Default.Language != "") CurCulture = new CultureInfo(Settings.Default.Language, false);
        else CurCulture = CultureInfo.CurrentCulture;
        Assets.Resources.Culture = CurCulture;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var viewLocator = _provider?.GetRequiredService<IDataTemplate>();
            var mainVm = _provider?.GetRequiredService<MainViewModel>();

            desktop.MainWindow = viewLocator?.Build(mainVm) as Window;
            if (OperatingSystem.IsWindows())
            {
                desktop.MainWindow.MinWidth = 1220;
                desktop.MainWindow.MaxWidth = 1220;
                desktop.MainWindow.Width = 1220;
            }
            else
            {
                desktop.MainWindow.MinWidth = 1235;
                desktop.MainWindow.MaxWidth = 1235;
                desktop.MainWindow.Width = 1235;
            }
            desktop.MainWindow.MaxHeight = 840;
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