using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Styling;
using SkiaSharp;
using System;

namespace SukiUI.Controls.GlassMorphism;

public class BlurBackground : Control
{
    public static readonly StyledProperty<ExperimentalAcrylicMaterial> MaterialProperty =
        AvaloniaProperty.Register<BlurBackground, ExperimentalAcrylicMaterial>(
            "Material");

    public ExperimentalAcrylicMaterial Material
    {
        get => GetValue(MaterialProperty);
        set => SetValue(MaterialProperty, value);
    }

    private static readonly ImmutableExperimentalAcrylicMaterial DefaultAcrylicMaterialDark =
        (ImmutableExperimentalAcrylicMaterial)new ExperimentalAcrylicMaterial()
        {
            MaterialOpacity = 0.25,
            TintColor = Colors.Black,
            TintOpacity = 0.7,
            PlatformTransparencyCompensationLevel = 0
        }.ToImmutable();

    private static readonly ImmutableExperimentalAcrylicMaterial DefaultAcrylicMaterialLight =
        (ImmutableExperimentalAcrylicMaterial)new ExperimentalAcrylicMaterial()
        {
            MaterialOpacity = 0.0,
            TintColor = Colors.White,
            TintOpacity = 0.3,
            PlatformTransparencyCompensationLevel = 0
        }.ToImmutable();

    static BlurBackground()
    {
        AffectsRender<BlurBackground>(MaterialProperty);
    }

    public static SKBlendMode blendmodedark = SKBlendMode.Clear;

    private static SKShader s_acrylicNoiseShader;

    private class BlurBehindRenderOperation : ICustomDrawOperation
    {
        private readonly ImmutableExperimentalAcrylicMaterial _material;
        private readonly Rect _bounds;

        public BlurBehindRenderOperation(ImmutableExperimentalAcrylicMaterial material, Rect bounds)
        {
            _material = material;
            _bounds = bounds;
        }

        public void Dispose()
        {
        }

        public bool HitTest(Point p) => _bounds.Contains(p);

        static SKColorFilter CreateAlphaColorFilter(double opacity)
        {
            if (opacity > 1)
                opacity = 1;
            var c = new byte[256];
            var a = new byte[256];
            for (var i = 0; i < 256; i++)
            {
                c[i] = (byte)i;
                a[i] = (byte)(i * opacity);
            }

            return SKColorFilter.CreateTable(a, c, c, c);
        }

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            if (!canvas.TotalMatrix.TryInvert(out var currentInvertedTransform))
                return;

            using var backgroundSnapshot = lease.SkSurface.Snapshot();

            using var backdropShader = SKShader.CreateImage(backgroundSnapshot, SKShaderTileMode.Clamp,
                SKShaderTileMode.Clamp, currentInvertedTransform);

            using var blurred = SKSurface.Create(lease.GrContext, false, new SKImageInfo(
                (int)Math.Ceiling(_bounds.Width),
                (int)Math.Ceiling(_bounds.Height), SKImageInfo.PlatformColorType, SKAlphaType.Premul));
            using (var filter = SKImageFilter.CreateBlur(3, 3))
            using (var blurPaint = new SKPaint
            {
                Shader = backdropShader,
                ImageFilter = filter
            })
                blurred.Canvas.DrawRect(0, 0, (float)_bounds.Width, (float)_bounds.Height, blurPaint);

            using (var blurSnap = blurred.Snapshot())
            using (var blurSnapShader = SKShader.CreateImage(blurSnap))
            {
                using var blurSnapPaint = new SKPaint
                {
                    Shader = blurSnapShader,
                    IsAntialias = false,
                };

                canvas.DrawRect(0, 0, (float)_bounds.Width, (float)_bounds.Height, blurSnapPaint);
            }

            //return;

            using var acrylliPaint = new SKPaint();
            acrylliPaint.IsAntialias = true;

            const double noiseOpacity = 0.01;

            var tintColor = _material.TintColor;
            var tint = new SKColor(tintColor.R, tintColor.G, tintColor.B, tintColor.A);

            if (s_acrylicNoiseShader == null)
            {
                using var stream =
                       typeof(SkiaPlatform).Assembly.GetManifestResourceStream(
                           "Avalonia.Skia.Assets.NoiseAsset_256X256_PNG.png");
                using var bitmap = SKBitmap.Decode(stream);
                s_acrylicNoiseShader = SKShader.CreateBitmap(bitmap, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp)
                    .WithColorFilter(CreateAlphaColorFilter(noiseOpacity));
            }

            using var backdrop = SKShader.CreateColor(new SKColor(_material.MaterialColor.R, _material.MaterialColor.G,
                       _material.MaterialColor.B, _material.MaterialColor.A));
            using var tintShader = SKShader.CreateColor(tint);
            using var effectiveTint = SKShader.CreateCompose(backdrop, tintShader);
            using var compose = SKShader.CreateCompose(effectiveTint, s_acrylicNoiseShader);
            acrylliPaint.Shader = compose;
            acrylliPaint.IsAntialias = true;

            canvas.DrawRect(0, 0, (float)_bounds.Width, (float)_bounds.Height, acrylliPaint);
        }

        public Rect Bounds => _bounds.Inflate(4);

        public bool Equals(ICustomDrawOperation? other)
        {
            return other is BlurBehindRenderOperation op && op._bounds == _bounds && op._material.Equals(_material);
        }
    }

    public override void Render(DrawingContext context)
    {
        var mat = Material != null
            ? (ImmutableExperimentalAcrylicMaterial)Material.ToImmutable()
            : Application.Current.ActualThemeVariant == ThemeVariant.Dark
                ? DefaultAcrylicMaterialDark
                : DefaultAcrylicMaterialLight;
        context.Custom(new BlurBehindRenderOperation(mat, new Rect(default, Bounds.Size)));
    }
}