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

    public SukiColorTheme(string displayName, Color primary, Color accent)
    {
        DisplayName = displayName;
        Primary = primary;
        Accent = accent;
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