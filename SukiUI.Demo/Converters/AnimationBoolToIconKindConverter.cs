using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Material.Icons;

namespace SukiUI.Demo.Converters
{
    public class AnimationBoolToIconKindConverter : IValueConverter
    {
        public static AnimationBoolToIconKindConverter Instance { get; } = new();
        
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not bool b) return null;
            return b ? MaterialIconKind.Pause : MaterialIconKind.Play;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}