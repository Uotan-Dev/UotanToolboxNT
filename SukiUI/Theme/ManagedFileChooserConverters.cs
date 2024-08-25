using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using SukiUI.Content;

namespace SukiUI.Theme
{
    public class TextToPathConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var pathes = (value as string).Split('\\');
            if (pathes.Length > 3)
            {
                pathes = pathes.Skip(1).ToArray();
            }

            var stackPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal,
                Margin = new Thickness(5)
            };

            for (var i = 0; i < pathes.Length; i++)
            {
                var t = new TextBlock
                {
                    Text = pathes[i],
                    FontWeight =
                        Application.Current!.TryGetResource("DefaultDemiBold", Application.Current!.ActualThemeVariant,
                            out var fontWeight)
                            ? (FontWeight)fontWeight!
                            : FontWeight.DemiBold,
                    FontSize = 14, VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.ForegroundProperty] = i == pathes.Length - 1
                        ? new DynamicResourceExtension("SukiText")
                        : new DynamicResourceExtension("SukiLowText")
                };

                stackPanel.Children.Add(t);

                var p = new PathIcon
                {
                    Height = 6, Margin = new Thickness(12, 4, 12, 2), Width = 5, Data = Icons.ChevronRight,
                    IsVisible = i != pathes.Length - 1, VerticalAlignment = VerticalAlignment.Center
                };

                p[!TemplatedControl.ForegroundProperty] = new DynamicResourceExtension("SukiLowText");

                stackPanel.Children.Add(p);
            }

            return stackPanel;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class WindowManagedConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var w = value as Window;
            w.SystemDecorations = SystemDecorations.BorderOnly;
            //    w.ExtendClientAreaToDecorationsHint = true;


            return "";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}