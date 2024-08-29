using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace SukiUI.ColorTheme;

public static class NotificationColor
{
    public static readonly LinearGradientBrush InfoIconForeground = new LinearGradientBrush()
    {
        EndPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        StartPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
        GradientStops = new GradientStops()
        {
            new() { Color = Color.FromRgb(89,126,247), Offset = 0.3 },
            new() { Color = Color.FromRgb(47,84,235), Offset = 1 },
            
        }
    };
    public static readonly LinearGradientBrush SuccessIconForeground =  new LinearGradientBrush()
    {
        EndPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        StartPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
        GradientStops = new GradientStops()
        {
            new() { Color = Color.FromRgb(82,196,26), Offset = 0.3 },
            new() { Color = Color.FromRgb(56,158,13), Offset = 1 },
            
        }
    };
    public static readonly LinearGradientBrush WarningIconForeground = new LinearGradientBrush()
    {
        EndPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        StartPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
        GradientStops = new GradientStops()
        {
            new() { Color = Color.FromRgb(255,169,64), Offset = 0.3 },
            new() { Color = Color.FromRgb(250,140,22), Offset = 1 },
            
        }
    };
    public static readonly LinearGradientBrush ErrorIconForeground = new LinearGradientBrush()
    {
        EndPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        StartPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
        GradientStops = new GradientStops()
        {
            new() { Color = Color.FromRgb(255,77,79), Offset = 0.3 },
            new() { Color = Color.FromRgb(245,34,45), Offset = 1 },
            
        }
    };
}