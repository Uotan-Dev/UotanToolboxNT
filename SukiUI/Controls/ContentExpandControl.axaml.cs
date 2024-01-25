using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace SukiUI.Controls
{
    public class ContentExpandControl : ContentControl
    {
        public static readonly StyledProperty<double> MultiplierProperty =
            AvaloniaProperty.Register<ContentExpandControl, double>(nameof(Multiplier));

        public double Multiplier
        {
            get => GetValue(MultiplierProperty);
            set => SetValue(MultiplierProperty, value);
        }
        
        public static readonly StyledProperty<Orientation> OrientationProperty =
            AvaloniaProperty.Register<ContentExpandControl, Orientation>(nameof(Orientation));
        
        public Orientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }
        
        static ContentExpandControl()
        {
            AffectsArrange<ContentExpandControl>(MultiplierProperty, OrientationProperty);

            AffectsMeasure<ContentExpandControl>(MultiplierProperty, OrientationProperty);
        }

        protected override Size MeasureCore(Size availableSize)
        {
            var result = base.MeasureCore(availableSize);
            return result;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var result = base.ArrangeOverride(finalSize);
            // A very gross way to actually get the bloody thing to draw...
            if (Parent is Control c) c.Margin = new Thickness(1);
            return result;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var result = base.MeasureOverride(availableSize);

            var w = result.Width;
            var h = result.Height;

            switch (Orientation)
            {
                case Orientation.Horizontal:
                    w *= Multiplier;
                    break;

                case Orientation.Vertical:
                    h *= Multiplier;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // A very gross way to actually get the bloody thing to draw...
            if (Parent is Control c) c.Margin = new Thickness(0);
            return new Size(w, h);
        }
    }
}