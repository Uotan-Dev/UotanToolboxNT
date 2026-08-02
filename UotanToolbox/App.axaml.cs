using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
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
    private IServiceProvider _provider = null!; // initialized in Initialize()

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

        // set up global device manager reference
        if (_provider is null)
            throw new InvalidOperationException("Service provider not initialized");
        Global.DeviceManager = _provider.GetRequiredService<UotanToolbox.Common.Devices.DeviceManager>();
        // perform initial scan in background
        _ = Global.DeviceManager.ScanAsync();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var viewLocator = _provider.GetRequiredService<IDataTemplate>();
            var mainVm = _provider.GetRequiredService<MainViewModel>();

            var window = viewLocator.Build(mainVm) as Window;
            if (window == null)
                throw new InvalidOperationException("Failed to build main window");
            desktop.MainWindow = window;
            // MainWindow is guaranteed non-null because we just assigned it from 'window'
            desktop.MainWindow!.Width = 1235;
            desktop.MainWindow!.Height = 840;
            // 屏幕分辨率 ≤1080P（1920×1080）时切换为系统微软雅黑，保证低分辨率下文本清晰。
            // 合规说明：微软雅黑为微软专有字体，不随应用分发字体文件，仅按名称引用系统自带字体。
            ApplyResolutionBasedFont(desktop.MainWindow);
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// 屏幕分辨率 ≤1080P（1920×1080，含 1080P 本身）时，将全局默认字体切换为系统微软雅黑，
    /// 以获得低分辨率下更清晰的文本渲染；高于 1080P 时保持默认的 MiSans。
    /// 合规：微软雅黑是微软专有字体，不可随应用打包分发，这里只按字体名称引用系统已安装字体；
    /// 仅在 Windows 且字体存在时生效，其它平台继续使用随应用分发的 MiSans。
    /// </summary>
    private void ApplyResolutionBasedFont(Window window)
    {
        try
        {
            Screen? primary = window.Screens.Primary ?? window.Screens.All.FirstOrDefault();
            if (primary is null)
            {
                return;
            }
            // Screen.Bounds 为物理像素，1080P 对应 1920×1080
            bool isLowResolution = primary.Bounds.Width <= 1920 && primary.Bounds.Height <= 1080;
            if (isLowResolution && OperatingSystem.IsWindows())
            {
                FontFamily yahei = new("Microsoft YaHei UI");
                // 更新全局资源，使所有引用 DefaultFontFamily 的控件生效
                Resources["DefaultFontFamily"] = yahei;
                // 同时直接设置窗口字体，确保即使样式未及时刷新也能立即应用
                TextElement.SetFontFamily(window, yahei);
            }
        }
        catch
        {
            // 无法获取屏幕信息时保持默认字体（MiSans），不影响启动
        }
    }

    private static ServiceProvider ConfigureServices()
    {
        IDataTemplate? viewlocator = Current?.DataTemplates.First(x => x is ViewLocator);
        ServiceCollection services = new ServiceCollection();

        if (viewlocator is not null)
        {
            _ = services.AddSingleton(viewlocator);
        }

        _ = services.AddSingleton<PageNavigationService>();
        services.AddSingleton<ISukiToastManager, SukiToastManager>();
        services.AddSingleton<ISukiDialogManager, SukiDialogManager>();

        // transport implementations for devices
        services.AddSingleton<UotanToolbox.Common.Devices.IDeviceTransport, UotanToolbox.Common.Devices.AdbTransport>();
        services.AddSingleton<UotanToolbox.Common.Devices.IDeviceTransport, UotanToolbox.Common.Devices.FastbootTransport>();
        services.AddSingleton<UotanToolbox.Common.Devices.IDeviceTransport, UotanToolbox.Common.Devices.HdcTransport>();
        services.AddSingleton<UotanToolbox.Common.Devices.IDeviceTransport, UotanToolbox.Common.Devices.EdlTransport>();

        // device manager singleton
        services.AddSingleton<UotanToolbox.Common.Devices.DeviceManager>(sp =>
            new UotanToolbox.Common.Devices.DeviceManager(sp.GetServices<UotanToolbox.Common.Devices.IDeviceTransport>()));

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