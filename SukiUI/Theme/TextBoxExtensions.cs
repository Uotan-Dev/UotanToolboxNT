using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Globalization;

namespace SukiUI.Theme;

public static class TextBoxExtensions
{
    public static readonly AttachedProperty<string> PrefixProperty =
        AvaloniaProperty.RegisterAttached<TextBox, string>("Prefix", typeof(TextBox), defaultValue: "");

    public static string GetPrefix(TextBox textBox)
    {
        return textBox.GetValue(PrefixProperty);
    }

    public static void SetPrefix(TextBox textBox, string value)
    {
        textBox.SetValue(PrefixProperty, value);
    }

    public static readonly AttachedProperty<bool> AddDeleteButtonProperty =
        AvaloniaProperty.RegisterAttached<TextBox, bool>("AddDeleteButton", typeof(TextBox), defaultValue: false);

    public static bool GetAddDeleteButton(TextBox textBox)
    {
        return textBox.GetValue(AddDeleteButtonProperty);
    }

    public static void SetAddDeleteButton(TextBox textBox, bool value)
    {
        textBox.SetValue(AddDeleteButtonProperty, value);
    }

    public static void Error(this TextBox textbox, string message)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            textbox.BorderBrush = Brushes.IndianRed;

            textbox.ContextFlyout = new Flyout()
            {
                Placement = PlacementMode.BottomEdgeAlignedLeft,
                Content = new TextBlock() { Text = message, FontWeight = FontWeight.Thin, Foreground = Brushes.IndianRed }
            };
            textbox.ContextFlyout.ShowAt(textbox);
            textbox.Vibrate(TimeSpan.FromMilliseconds(400));
        });
    }
}

public static class ButtonExtensions
{
    public static readonly AttachedProperty<bool> ShowProgressProperty =
        AvaloniaProperty.RegisterAttached<Button, bool>("ShowProgress", typeof(Button), defaultValue: false);

    public static bool GetShowProgress(Button textBox)
    {
        return textBox.GetValue(ShowProgressProperty);
    }

    public static void SetShowProgress(Button textBox, bool value)
    {
        textBox.SetValue(ShowProgressProperty, value);
    }

    public static void ShowProgress(this Button textBox)
    {
        textBox.SetValue(ShowProgressProperty, true);
    }

    public static void HideProgress(this Button textBox)
    {
        textBox.SetValue(ShowProgressProperty, false);
    }
}

public class StringToDoubleConverter : IValueConverter
{
    public static readonly StringToDoubleConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return 0;

        return ((string)value).Length > 0 ? 1 : 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class BoolToWidthProgressConverter : IValueConverter
{
    public static readonly BoolToWidthProgressConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return 0;

        return ((bool)value) ? 40 : 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}