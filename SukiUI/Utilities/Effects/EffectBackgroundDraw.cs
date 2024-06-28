using System;
using System.Diagnostics;
using Avalonia;
using SkiaSharp;

namespace SukiUI.Utilities.Effects
{
    internal class EffectBackgroundDraw : EffectDrawBase
    {
        internal bool TransitionsEnabled { get; set; }
        internal double TransitionTime { get; set; }

        private static readonly Stopwatch TransitionTick = Stopwatch.StartNew();

        private static float TransitionSeconds => (float)TransitionTick.Elapsed.TotalSeconds;

        private SukiEffect? _oldEffect;
        private float _transitionStartTime;
        private float _transitionEndTime;

        public EffectBackgroundDraw(Rect bounds) : base(bounds)
        {
        }

        protected override void EffectChanged(SukiEffect? oldValue, SukiEffect? newValue)
        {
            if (!TransitionsEnabled) return;
            if (oldValue is null || Equals(oldValue, newValue)) return;
            _oldEffect = oldValue;
            _transitionStartTime = TransitionSeconds;
            _transitionEndTime = TransitionSeconds + (float)Math.Max(0, TransitionTime);
        }

        protected override void Render(SKCanvas canvas, SKRect rect)
        {
            canvas.Clear(SKColors.Transparent);

            if (Effect is not null)
            {
                using var paint = new SKPaint();
                using var shader = EffectWithUniforms();
                paint.Shader = shader;
                canvas.DrawRect(rect, paint);
            }
            if (_oldEffect is not null)
            {
                
                using var paint = new SKPaint();
                // TODO: Investigate how to blend the shaders better - currently the only problem with this system.
                // Blend modes effect the transition quite heavily, only these 3 seem to work in any reasonable way.
                // paint.BlendMode = SKBlendMode.ColorBurn; // - Okay
                // paint.BlendMode = SKBlendMode.Overlay; // - Not Great
                paint.BlendMode = SKBlendMode.Darken; // - Best
                var lerped = InverseLerp(_transitionStartTime, _transitionEndTime, TransitionSeconds);
                using var shader = EffectWithUniforms(_oldEffect, (float)(1 - lerped));
                paint.Shader = shader;
                if (lerped < 1)
                    canvas.DrawRect(rect, paint);
                else
                    _oldEffect = null;
            }
        }

        private static double InverseLerp(double start, double end, double value) =>
            Math.Max(0, Math.Min(1, (value - start) / (end - start)));
    }
}