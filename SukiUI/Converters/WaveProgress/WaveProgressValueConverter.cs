using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SukiUI.Converters.WaveProgress;

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