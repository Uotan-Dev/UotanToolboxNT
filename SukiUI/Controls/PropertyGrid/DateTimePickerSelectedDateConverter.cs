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
            if (value is null)
            {
                return null;
            }

            try
            {
                if (value is DateTime dateTime)
                {
                    return new DateTimeOffset(dateTime);
                }
                else if (value is DateTimeOffset dateTimeOffset)
                {
                    return dateTimeOffset;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }

            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return null!;
            }

            if (value is DateTimeOffset dateTimeOffset)
            {
                if (targetType == typeof(DateTimeOffset))
                {
                    return dateTimeOffset;
                }

                if (targetType == typeof(DateTimeOffset?))
                {
                    return dateTimeOffset;
                }

                if (targetType == typeof(DateTime))
                {
                    return dateTimeOffset.DateTime;
                }

                if (targetType == typeof(DateTime?))
                {
                    return dateTimeOffset.DateTime;
                }
            }

            if (value is DateTime dateTime)
            {
                if (targetType == typeof(DateTimeOffset))
                {
                    return new DateTimeOffset(dateTime);
                }

                if (targetType == typeof(DateTimeOffset?))
                {
                    return new DateTimeOffset(dateTime);
                }

                if (targetType == typeof(DateTime))
                {
                    return dateTime;
                }

                if (targetType == typeof(DateTime?))
                {
                    return dateTime;
                }
            }

            throw new NotSupportedException();
        }
    }
}