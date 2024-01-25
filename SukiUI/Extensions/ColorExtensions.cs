using Avalonia.Media;

namespace SukiUI.Extensions;

public static class ColorExtensions
{
    public static Color WithAlpha(this Color c, byte alpha) =>
        new(alpha, c.R, c.G, c.B);

    public static Color WithAlpha(this Color c, double alpha) =>
        WithAlpha(c, (byte)(255 * alpha));
}