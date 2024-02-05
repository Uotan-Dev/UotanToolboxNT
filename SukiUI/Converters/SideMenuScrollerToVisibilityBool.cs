using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SukiUI.Converters
{
    public class SideMenuScrollerToVisibilityBool : IMultiValueConverter
    {
        public static SideMenuScrollerToVisibilityBool Up { get; } = new((x,y) => x > y);

        public static SideMenuScrollerToVisibilityBool Down { get; } = new((x, y) => x < y);

        private readonly Func<double, double, bool> _converter;
        
        public SideMenuScrollerToVisibilityBool(Func<double, double, bool> converter)
        {
            _converter = converter;
        }
        
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values[0] is not double firstVal) return null;
            if (values[1] is not double secondVal) return null;
            return _converter(firstVal,secondVal);
        }
    }
}