using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
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
        CultureInfo CurCulture = Settings.Default.Language is not null and not ""
            ? new CultureInfo(Settings.Default.Language, false)
            : CultureInfo.CurrentCulture;
        Assets.Resources.Culture = CurCulture;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            IDataTemplate viewLocator = _provider?.GetRequiredService<IDataTemplate>();
            MainViewModel mainVm = _provider?.GetRequiredService<MainViewModel>();

            desktop.MainWindow = viewLocator?.Build(mainVm) as Window;
            desktop.MainWindow.Width = 1240;
            desktop.MainWindow.Height = 840;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider ConfigureServices()
    {
        IDataTemplate viewlocator = Current?.DataTemplates.First(x => x is ViewLocator);
        ServiceCollection services = new ServiceCollection();

        if (viewlocator is not null)
        {
            _ = services.AddSingleton(viewlocator);
        }

        _ = services.AddSingleton<PageNavigationService>();

        // Viewmodels
        _ = services.AddSingleton<MainViewModel>();
        System.Collections.Generic.IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => !p.IsAbstract && typeof(MainPageBase).IsAssignableFrom(p));
        foreach (Type type in types)
        {
            _ = services.AddSingleton(typeof(MainPageBase), type);
        }

        return services.BuildServiceProvider();
    }
}