using System;
using Avalonia;
using Avalonia.Controls;

namespace SukiUI.Helpers
{
    public static class AnimationExtensions
    {

        public static readonly AttachedProperty<double> FadeInProperty = AvaloniaProperty.RegisterAttached<Control, double>("FadeIn", typeof(Control), defaultValue: 0);    
 

        public static double GetFadeIn(Control wrap)
        {
            return wrap.GetValue(FadeInProperty);
        }

        public static void SetFadeIn(Control interactElem, double value)
        {
            if (value > 0)
            {
                interactElem.Opacity = 0;
                interactElem.AttachedToVisualTree += (sender, args) =>
                {
                    interactElem.Animate<double>(Control.OpacityProperty, 0,1, TimeSpan.FromMilliseconds(value));
                };
            }
            interactElem.SetValue(FadeInProperty, value);
        }
    
  
    }
}