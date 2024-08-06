using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Material.Icons;

namespace UotanToolbox.Converters;

public static class BoolToIconConverters
{
    public static readonly BoolToIconConverter Animation = new(MaterialIconKind.Pause, MaterialIconKind.Play);
    public static readonly BoolToIconConverter WindowLock = new(MaterialIconKind.Unlocked, MaterialIconKind.Lock);
    public static readonly BoolToIconConverter Visibility = new(MaterialIconKind.EyeClosed, MaterialIconKind.Eye);
    public static readonly BoolToIconConverter Simple = new(MaterialIconKind.Close, MaterialIconKind.Ticket);
}

public class BoolToIconConverter(MaterialIconKind trueIcon, MaterialIconKind falseIcon) : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool b) return null;
        return b ? trueIcon : falseIcon;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}