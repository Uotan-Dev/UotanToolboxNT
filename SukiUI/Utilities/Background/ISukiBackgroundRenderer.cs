using Avalonia.Media.Imaging;
using Avalonia.Styling;
using SukiUI.Controls;
using SukiUI.Models;
using System.Threading.Tasks;

namespace SukiUI.Utilities.Background;

/// <summary>
/// Provides an interface for background renderers to be implemented behind.
/// </summary>
public interface ISukiBackgroundRenderer
{
    /// <summary>
    /// Tells the <see cref="SukiBackground"/> control if this renderer should be animated.
    /// </summary>
    public bool SupportsAnimation { get; }

    /// <summary>
    /// Updates the values from the main thread, allowing the generator to keep drawing in the background.
    /// </summary>
    /// <param name="colorTheme"></param>
    /// <param name="baseTheme"></param>
    public void UpdateValues(SukiColorTheme colorTheme, ThemeVariant baseTheme);

    /// <summary>
    /// Called every time the Background control attempts to render the background.
    /// This is called once at startup and...
    /// If animation is enabled this will be called per-frame at 60fps.
    /// If animation is disabled, this will be called every time the theme is changed.
    /// </summary>
    /// <param name="bitmap">Bitmap to draw the background to.</param>
    /// <returns></returns>
    public Task Render(WriteableBitmap bitmap);
}