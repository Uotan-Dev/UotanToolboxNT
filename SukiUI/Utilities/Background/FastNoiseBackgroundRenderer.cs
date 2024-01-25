using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using SukiUI.Models;
using System;
using System.Threading.Tasks;

namespace SukiUI.Utilities.Background;

public sealed class FastNoiseBackgroundRenderer : ISukiBackgroundRenderer
{
    public bool SupportsAnimation => true;

    private static readonly Random Rand = new();
    private static readonly FastNoiseLite NoiseGen = new();

    private readonly object _lockObj = new();

    private bool _isRedrawing;

    private uint _themeColor;
    private uint _accentColor;
    private uint _baseColor;

    private float _pOffsetX;
    private float _pOffsetY;

    private float _aOffsetX;
    private float _aOffsetY;

    private readonly float _scale;
    private readonly float _xAnim;
    private readonly float _yAnim;
    private readonly float _primaryAlpha;
    private readonly float _accentAlpha;

    public FastNoiseBackgroundRenderer(FastNoiseRendererOptions? options = null)
    {
        var opt = options ?? new FastNoiseRendererOptions(FastNoiseLite.NoiseType.OpenSimplex2);
        NoiseGen.SetNoiseType(opt.Type);
        _scale = opt.NoiseScale * 100f;
        _xAnim = opt.XAnimSpeed;
        _yAnim = opt.YAnimSpeed;
        _primaryAlpha = opt.PrimaryAlpha;
        _accentAlpha = opt.AccentAlpha;
    }

    public void UpdateValues(SukiColorTheme colorTheme, ThemeVariant baseTheme)
    {
        _themeColor = colorTheme.Primary.ToUInt32();
        _accentColor = colorTheme.Accent.ToUInt32();
        _baseColor = baseTheme == ThemeVariant.Light
            ? new Color(255, 241, 241, 241).ToUInt32()
            : GetBackgroundColor(colorTheme.Primary);

        _pOffsetX = Rand.Next(1000);
        _pOffsetY = Rand.Next(1000);

        _aOffsetY = Rand.Next(1000);
        _aOffsetX = Rand.Next(1000);
    }

    public async Task Render(WriteableBitmap bitmap)
    {
        _pOffsetX += _xAnim;
        _pOffsetY += _yAnim;
        _aOffsetX -= _xAnim;
        _aOffsetY -= _yAnim;

        if (_isRedrawing) return;
        lock (_lockObj) { _isRedrawing = true; }

        await Task.Run(() =>
        {
            using var frameBuffer = bitmap.Lock();
            var frameSize = frameBuffer.Size;
            var frameScale = (1f / frameSize.Height) * _scale;
            unsafe
            {
                var backBuffer = (uint*)frameBuffer.Address.ToPointer();
                var stride = frameBuffer.RowBytes / 4;

                Parallel.For((long)0, frameSize.Height, (scanline) =>
                {
                    var dest = backBuffer + scanline * stride + 0;
                    for (var x = 0; x < frameSize.Width; x++)
                    {
                        var noise = NoiseGen.GetNoise((_pOffsetX + x) * frameScale, (_pOffsetY + scanline) * frameScale);
                        noise = (noise + 1f) / 2f * _primaryAlpha; // noise returns -1 to +1 which isn't useful.
                        var alpha = (byte)(noise * 255);
                        var firstLayer = BlendPixelOverlay(WithAlpha(_themeColor, alpha), _baseColor);

                        noise = NoiseGen.GetNoise((_aOffsetX + x) * frameScale, (_aOffsetY + scanline) * frameScale);
                        noise = (noise + 1f) / 2f * _accentAlpha;
                        alpha = (byte)(noise * 255);

                        dest[x] = BlendPixel(WithAlpha(_accentColor, alpha), firstLayer);
                    }
                });
            }
        });
        lock (_lockObj) { _isRedrawing = false; }
    }

    private static uint GetBackgroundColor(Color input)
    {
        int r = input.R;
        int g = input.G;
        int b = input.B;

        var minValue = Math.Min(Math.Min(r, g), b);
        var maxValue = Math.Max(Math.Max(r, g), b);

        r = (r == minValue) ? 30 : ((r == maxValue) ? 30 : 22);
        g = (g == minValue) ? 30 : ((g == maxValue) ? 30 : 22);
        b = (b == minValue) ? 30 : ((b == maxValue) ? 30 : 22);
        return ARGB(255, (byte)r, (byte)g, (byte)b);
    }

    private static uint ARGB(byte a, byte r, byte g, byte b) =>
        (uint)(a << 24 | r << 16 | g << 8 | b << 0);

    private static byte A(uint col) => (byte)(col >> 24);

    private static byte R(uint col) => (byte)(col >> 16);

    private static byte G(uint col) => (byte)(col >> 8);

    private static byte B(uint col) => (byte)col;

    private static uint WithAlpha(uint col, byte a) => (col & 0x00FFFFFF) | (uint)(a << 24);

    private static uint BlendPixel(uint fore, uint back)
    {
        var alphaF = A(fore) / 255.0f;

        var resultR = (byte)(R(fore) * alphaF + R(back) * (1 - alphaF));
        var resultG = (byte)(G(fore) * alphaF + G(back) * (1 - alphaF));
        var resultB = (byte)(B(fore) * alphaF + B(back) * (1 - alphaF));
        var resultA = A(back);

        return ARGB(resultA, resultR, resultG, resultB);
    }

    private static uint BlendPixelOverlay(uint fore, uint back)
    {
        var alphaF = A(fore) / 255.0f;

        var resultR = OverlayComponentBlend(R(fore), R(back), alphaF);
        var resultG = OverlayComponentBlend(G(fore), G(back), alphaF);
        var resultB = OverlayComponentBlend(B(fore), B(back), alphaF);

        return ARGB(A(back), resultR, resultG, resultB);
    }

    private static byte OverlayComponentBlend(byte componentF, byte componentB, float alphaF)
    {
        var result = componentB <= 128
            ? 2 * componentF * componentB / 255.0f
            : 255 - 2 * (255 - componentF) * (255 - componentB) / 255.0f;

        return (byte)(result * alphaF + componentB * (1 - alphaF));
    }
}