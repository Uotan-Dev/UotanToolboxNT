using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace SukiUI.Controls
{
    public class DateTimePickerSelectedDateConverter : IValueConverter
    {
        public static readonly DateTimePickerSelectedDateConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not (DateTime or DateTimeOffset)) return null;

            try
            {
                switch (value)
                {
                    // It is not allowed to add positive/negative LocalTimeOffset to a min/max Value of non-UTC DateTime
                    case DateTime dateTime when dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue:
                        return new DateTimeOffset(dateTime, TimeSpan.Zero);
                    case DateTime dateTime:
                        return new DateTimeOffset(dateTime);
                    case DateTimeOffset dateTimeOffset:
                        return dateTimeOffset;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                null => null,
                DateTimeOffset dateTimeOffset when targetType == typeof(DateTimeOffset) => dateTimeOffset,
                DateTimeOffset dateTimeOffset when targetType == typeof(DateTimeOffset?) => dateTimeOffset,
                DateTimeOffset dateTimeOffset when targetType == typeof(DateTime) => dateTimeOffset.DateTime,
                DateTimeOffset dateTimeOffset when targetType == typeof(DateTime?) => dateTimeOffset.DateTime,
                DateTime dateTime when targetType == typeof(DateTimeOffset) => new DateTimeOffset(dateTime),
                DateTime dateTime when targetType == typeof(DateTimeOffset?) => new DateTimeOffset(dateTime),
                DateTime dateTime when targetType == typeof(DateTime) => dateTime,
                DateTime dateTime when targetType == typeof(DateTime?) => dateTime,
                _ => throw new NotSupportedException()
            };
        }
    }
}