using Avalonia;
using ShowMeTheXaml;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UotanToolbox.Common;

namespace UotanToolbox;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        _ = BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        Global.runpath = System.AppDomain.CurrentDomain.BaseDirectory;//获取工具运行路径
        Global.tmp_path = Path.GetTempPath();
        if (OperatingSystem.IsLinux())
        {
            if (RuntimeInformation.OSArchitecture == Architecture.X64)
            {
                Global.System = "Linux_AMD64";
            }
            else if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                Global.System = "Linux_AArch64";
            }
            else if (RuntimeInformation.OSArchitecture == Architecture.LoongArch64)
            {
                Global.System = "Linux_LoongArch64";
            }
            Global.log_path = Global.tmp_path;
        }
        else if (OperatingSystem.IsMacOS())
        {
            Global.System = "macOS";
            Global.log_path = Path.Combine(Global.runpath, "Log");
            if (!File.Exists(Global.log_path))
            {
                _ = Directory.CreateDirectory(Global.log_path);
            }
            Global.backup_path = Path.Combine(Global.runpath, "Backup");
            if (!File.Exists(Global.backup_path))
            {
                _ = Directory.CreateDirectory(Global.backup_path);
            }
        }
        else if (OperatingSystem.IsWindows())
        {
            Global.log_path = Path.Combine(Global.runpath, "Log");
            if (!File.Exists(Global.log_path))
            {
                _ = Directory.CreateDirectory(Global.log_path);
            }
            Global.backup_path = Path.Combine(Global.runpath, "Backup");
            if (!File.Exists(Global.backup_path))
            {
                _ = Directory.CreateDirectory(Global.backup_path);
            }
        }
        Global.bin_path = Path.Combine(Global.runpath, "Bin");
        Global.serviceID = "studio-" + StringHelper.RandomString(8);
        Global.password = StringHelper.RandomString(8);
        // No need to set default for Windows
        var app = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseXamlDisplay();
        return app;
    }
}