using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using System;

namespace SukiUI;

public static class ControlAnimationHelper
{
    public static void Jump(this Control control)
    {
        var currentMargin = control.Margin;

        new Avalonia.Animation.Animation
        {
            Duration = TimeSpan.FromMilliseconds(800),
            FillMode = FillMode.Forward,
            Easing = new QuadraticEaseOut(),
            IterationCount = new IterationCount(1),
            PlaybackDirection = PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame()
                {
                    Setters =
                    {
                        new Setter { Property = Control.MarginProperty, Value = new Thickness(currentMargin.Left, currentMargin.Top, currentMargin.Right, currentMargin.Bottom)},
                    },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame()
                {
                    Setters =
                    {
                        new Setter { Property = Control.MarginProperty, Value = new Thickness(currentMargin.Left, currentMargin.Top - 8, currentMargin.Right, currentMargin.Bottom +8)},
                    },
                    KeyTime = TimeSpan.FromMilliseconds(300)
                },
                new KeyFrame()
                {
                    Setters =
                    {
                        new Setter { Property = Control.MarginProperty, Value = new Thickness(currentMargin.Left, currentMargin.Top, currentMargin.Right, currentMargin.Bottom)},
                    },
                    KeyTime = TimeSpan.FromMilliseconds(600)
                },
                new KeyFrame()
                {
                    Setters =
                    {
                        new Setter { Property = Control.MarginProperty, Value = new Thickness(currentMargin.Left, currentMargin.Top-2, currentMargin.Right, currentMargin.Bottom +2)},
                    },
                    KeyTime = TimeSpan.FromMilliseconds(700)
                },new KeyFrame()
                {
                    Setters =
                    {
                        new Setter { Property = Control.MarginProperty, Value = new Thickness(currentMargin.Left, currentMargin.Top, currentMargin.Right, currentMargin.Bottom)},
                    },
                    KeyTime = TimeSpan.FromMilliseconds(800)
                }
            }
        }.RunAsync(control);
    }

    public static void Vibrate(this Animatable control, TimeSpan duration)
    {
        var count = duration.TotalMilliseconds / 75;
        new Avalonia.Animation.Animation
        {
            Duration = TimeSpan.FromMilliseconds(75),
            FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount((ulong)(count)),
            PlaybackDirection = PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame()
                {
                    Setters =
                    {
                        new Setter { Property = ScaleTransform.ScaleXProperty, Value = 0.995},
                        new Setter { Property = ScaleTransform.ScaleYProperty, Value = 0.995},
                        new Setter { Property = RotateTransform.AngleProperty, Value = 0.4}
                    },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame()
                {
                    Setters =
                    {
                        new Setter { Property = ScaleTransform.ScaleXProperty, Value = 1.005},
                        new Setter { Property = ScaleTransform.ScaleYProperty, Value = 1.005},
                        new Setter { Property = RotateTransform.AngleProperty, Value = -0.3}
                    },
                    KeyTime = TimeSpan.FromMilliseconds(75)
                }
            }
        }.RunAsync(control);
    }

    public static void Animate<T>(this Animatable control, AvaloniaProperty Property, T from, T to, TimeSpan duration, ulong count = 1)
    {
        new Avalonia.Animation.Animation
        {
            Duration = duration,
            FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount(count),
            PlaybackDirection = PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame()
                {
                    Setters = { new Setter { Property = Property, Value = from } },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame()
                {
                    Setters = { new Setter { Property = Property, Value = to } },
                    KeyTime = duration
                }
            }
        }.RunAsync(control);
    }

    public static void Animate<T>(this Animatable control, AvaloniaProperty Property, T from, T to)
    {
        new Avalonia.Animation.Animation
        {
            Duration = TimeSpan.FromMilliseconds(500),
            FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount(1),
            PlaybackDirection = PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame()
                {
                    Setters = { new Setter { Property = Property, Value = from } },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame()
                {
                    Setters = { new Setter { Property = Property, Value = to } },
                    KeyTime = TimeSpan.FromMilliseconds(500)
                }
            }
        }.RunAsync(control);
    }
}