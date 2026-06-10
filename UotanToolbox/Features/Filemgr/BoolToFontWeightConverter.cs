using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace UotanToolbox.Features.Filemgr;

/// <summary>
/// <para>将布尔值转换为 FontWeight 的值转换器。true 时返回 Bold，false 时返回 Normal。</para>
/// Value converter that converts a boolean to FontWeight. Returns Bold for true, Normal for false.
/// </summary>
public class BoolToFontWeightConverter : IValueConverter
{
    /// <summary>
    /// <para>转换器单例实例。</para>
    /// Singleton instance of the converter.
    /// </summary>
    public static BoolToFontWeightConverter Instance { get; } = new();

    /// <summary>
    /// <para>将布尔值转换为 FontWeight。</para>
    /// Converts a boolean to FontWeight.
    /// </summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? FontWeight.Bold : FontWeight.Normal;
    }

    /// <summary>
    /// <para>将 FontWeight 转换回布尔值（不支持）。</para>
    /// Converts FontWeight back to boolean (not supported).
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Avalonia.Data.BindingOperations.DoNothing;
    }
}
