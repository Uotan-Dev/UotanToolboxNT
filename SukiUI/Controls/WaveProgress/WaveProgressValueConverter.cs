using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;
using SukiUI.Extensions;
using System;
using System.Globalization;

namespace SukiUI.Controls;

public class WaveProgressValueConverter : IValueConverter
{
    public static readonly WaveProgressValueConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double i) return 0;
        return 155 - i * 2.1;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class WaveProgressValueColorConverter : IValueConverter
{
    public static readonly WaveProgressValueColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double d) return Brushes.Black;
        if (d > 50) return Brushes.GhostWhite;
        return Application.Current?.ActualThemeVariant == ThemeVariant.Dark ? Brushes.GhostWhite : Brushes.Black;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class WaveProgressValueTextConverter : IValueConverter
{
    public static readonly WaveProgressValueTextConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not double d ? "0%" : $"{d:#0}%";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class WaveProgressGradientOffsetConverter : IValueConverter
{
    public static readonly WaveProgressGradientOffsetConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double v)
            return Brushes.Blue;

        Color PrimaryColor = Colors.DodgerBlue;
        Color AccentColor = Colors.Transparent;

        try
        {
            PrimaryColor = (Color)Application.Current.FindRequiredResource("SukiPrimaryColor");
            AccentColor = (Color)Application.Current.FindRequiredResource("SukiAccentColor");
        }
        catch { }

        v /= 100;
        v += Application.Current.RequestedThemeVariant == ThemeVariant.Light ? 0.2 : 0.4;
        if (v > 1)
            v = 1;

        return new LinearGradientBrush()
        {
            EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
            GradientStops = new GradientStops()
            {
                new GradientStop() { Color = PrimaryColor, Offset = 0 },
                new GradientStop() { Color = Application.Current.RequestedThemeVariant == ThemeVariant.Light ? Colors.Transparent: AccentColor, Offset = v }
            }
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}