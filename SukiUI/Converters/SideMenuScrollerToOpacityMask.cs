using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace SukiUI.Converters
{
    public class SideMenuScrollerToOpacityMask : IMultiValueConverter
    {
        private readonly Func<double, double, IBrush?> _func;
        
        public static SideMenuScrollerToOpacityMask Top { get; } = new((x,y) => x > y ? TopBrush : Brushes.White);
        public static SideMenuScrollerToOpacityMask Bottom { get; } = new((x,y) => x < y ? BottomBrush : Brushes.White);

        private static readonly LinearGradientBrush BottomBrush = new()
        {
            StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5, 0.95, RelativeUnit.Relative),
            GradientStops = new GradientStops()
            {
                new(Colors.Black, 0.9),
                new(Colors.Transparent,1 )
            }
        };
        
        private static readonly LinearGradientBrush TopBrush = new()
        {
            StartPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5, 0.05, RelativeUnit.Relative),
            GradientStops = new GradientStops()
            {
                new(Colors.Black, 0.9),
                new(Colors.Transparent,1 )
            }
        };
        
        public SideMenuScrollerToOpacityMask(Func<double, double, IBrush?> func)
        {
            _func = func;
        }
        
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count != 2) return null;
            if (values[0] is not double valOne) return null;
            if (values[1] is not double valTwo) return null;
            return _func(valOne, valTwo);
        }
    }
}