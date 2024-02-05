using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Material.Icons;

namespace SukiUI.Demo.Converters
{
    public static class BoolToIconConverters
    {
        public static readonly BoolToIconConverter Animation = new(MaterialIconKind.Pause, MaterialIconKind.Play);
        public static readonly BoolToIconConverter WindowLock = new(MaterialIconKind.Unlocked, MaterialIconKind.Lock);
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
}