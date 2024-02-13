using Avalonia;
using Avalonia.Media;
using ShowMeTheXaml;
using UotanToolbox.Common;
using System;

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
        FontManagerOptions options = new();
        if (OperatingSystem.IsLinux())
        {
            Global.System = "Linux";
            options.DefaultFamilyName = "Quicksand";
        }
        else if (OperatingSystem.IsMacOS())
        {
            Global.System = "MacOS";
            options.DefaultFamilyName = "Quicksand";
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