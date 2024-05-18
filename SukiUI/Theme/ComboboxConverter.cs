using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Globalization;

namespace SukiUI.Theme
{
    public class PlusNineConverter : IValueConverter
    {
        public static readonly PlusNineConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            double x = (double)value;
            if (x == 0)
                return 0;

            x += 9;
            return x;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BiggestItemConverter : IValueConverter
    {
        public static readonly BiggestItemConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            IEnumerable? x = (IEnumerable?)value;
            if (x is null)
                return "";

            var s = "";
            foreach (var o in x)
            {
                if (o?.ToString()?.Length > s.ToString().Length)
                    s = o.ToString();
            }

            return s;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BiggestItemListBoxConverter : IValueConverter
    {
        public static readonly BiggestItemConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            IEnumerable? x = (IEnumerable?)value;
            if (x is null)
                return "";

            var s = "";
            foreach (var o in x)
            {
                if (o?.ToString()?.Length > s.ToString().Length)
                    s = o.ToString();
            }

            return s;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
