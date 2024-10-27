using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SukiUI.Dialogs;
using SukiUI.Toasts;
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
        // Load Language settings
        CultureInfo CurCulture = Settings.Default.Language is not null and not ""
            ? new CultureInfo(Settings.Default.Language, false)
            : CultureInfo.CurrentCulture;
        Assets.Resources.Culture = CurCulture;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            IDataTemplate viewLocator = _provider?.GetRequiredService<IDataTemplate>();
            MainViewModel mainVm = _provider?.GetRequiredService<MainViewModel>();

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
        IDataTemplate viewlocator = Current?.DataTemplates.First(x => x is ViewLocator);
        ServiceCollection services = new ServiceCollection();

        if (viewlocator is not null)
        {
            _ = services.AddSingleton(viewlocator);
        }

        _ = services.AddSingleton<PageNavigationService>();
        services.AddSingleton<ISukiToastManager, SukiToastManager>();
        services.AddSingleton<ISukiDialogManager, SukiDialogManager>();
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