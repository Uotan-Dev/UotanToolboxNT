using System;
using Avalonia.Media;
using SukiUI.Enums;

namespace SukiUI.Models;

public record SukiColorTheme
{
    public string DisplayName { get; }

    public Color Primary { get; }

    public IBrush PrimaryBrush => new SolidColorBrush(Primary);

    public Color Accent { get; }

    public IBrush AccentBrush => new SolidColorBrush(Accent);

    // Used in shaders to save calculating them per-frame.
    internal Color BackgroundPrimary { get; }
    internal Color BackgroundAccent { get; }
    internal Color Background { get; }

    public SukiColorTheme(string displayName, Color primary, Color accent)
    {
        DisplayName = displayName;
        Primary = primary;
        Accent = accent;
        Background = GetBackgroundColor(Primary);
        BackgroundPrimary = new Color(primary.A, (byte)(primary.R / 1.3), (byte)(primary.G / 1.3), (byte)(primary.B / 1.3));
        BackgroundAccent = new Color(accent.A, (byte)(accent.R / 1.3), (byte)(accent.G / 1.3), (byte)(accent.B / 1.3));
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash *= 31 + Primary.GetHashCode();
            hash *= 31 + Accent.GetHashCode();
            hash *= 31 + DisplayName.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
    {
        return DisplayName;
    }

    private static Color GetBackgroundColor(Color input)
    {
        int r = input.R;
        int g = input.G;
        int b = input.B;

        var minValue = Math.Min(Math.Min(r, g), b);
        var maxValue = Math.Max(Math.Max(r, g), b);

        r = (r == minValue) ? 37 : ((r == maxValue) ? 37 : 26);
        g = (g == minValue) ? 37 : ((g == maxValue) ? 37 : 26);
        b = (b == minValue) ? 37 : ((b == maxValue) ? 37 : 26);
        return new Color(255, (byte)r, (byte)g, (byte)b);
    }
}

internal record DefaultSukiColorTheme : SukiColorTheme
{
    internal SukiColor ThemeColor { get; }

    internal DefaultSukiColorTheme(SukiColor themeColor, Color primary, Color accent)
        : base(themeColor.ToString(), primary, accent)
    {
        ThemeColor = themeColor;
    }
}