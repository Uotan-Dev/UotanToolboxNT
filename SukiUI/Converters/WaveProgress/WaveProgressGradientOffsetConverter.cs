using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;

namespace SukiUI.Converters.WaveProgress;

internal class WaveProgressGradientOffsetConverter : IValueConverter
{
    public static readonly WaveProgressGradientOffsetConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double v)
            return Brushes.Blue;
            
        var primaryColor = (Color)(Application.Current!.FindResource("SukiPrimaryColor") ?? Colors.DodgerBlue);
        var accentColor = (Color)(Application.Current!.FindResource("SukiAccentColor") ?? Colors.Transparent);

        v /= 100;
        v += Application.Current!.RequestedThemeVariant == ThemeVariant.Light ? 0.2 : 0.4;
        if (v > 1)
            v = 1;

        return new LinearGradientBrush()
        {
            EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
            GradientStops = new GradientStops()
            {
                new() { Color = primaryColor, Offset = 0 },
                new() { Color = Application.Current.RequestedThemeVariant == ThemeVariant.Light ? Colors.Transparent: accentColor, Offset = v }
            }
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}