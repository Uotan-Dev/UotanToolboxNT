using Avalonia;
using Avalonia.Media;
using ShowMeTheXaml;
using System;
using UotanToolbox.Common;

namespace UotanToolbox;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        Global.runpath = System.IO.Directory.GetCurrentDirectory();//获取工具运行路径
        FontManagerOptions options = new();
        if (OperatingSystem.IsLinux())
        {
            Global.System = "Linux";
            options.DefaultFamilyName = "MiSans";
        }
        else if (OperatingSystem.IsMacOS())
        {
            Global.System = "MacOS";
            options.DefaultFamilyName = "MiSans";
        }
        // No need to set default for Windows
        return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseXamlDisplay()
                .With(options);
    }
}