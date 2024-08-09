using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Styling;
using SkiaSharp;

namespace SukiUI.Utilities.Effects
{
    public abstract class EffectDrawBase : ICustomDrawOperation
    {
        public Rect Bounds { get; set; }

        private SukiEffect? _effect;

        public SukiEffect? Effect
        {
            get => _effect;
            set
            {
                var old = _effect;
                if (Equals(old, value)) return;
                _effect = value;
                EffectChanged(old, _effect);
            }
        }

        private bool _animationEnabled = true;
        public bool AnimationEnabled
        {
            get => _animationEnabled;
            set
            {
                if (value) _animationTick.Start();
                else _animationTick.Stop();
                _animationEnabled = value;
            }
        }

        protected float AnimationSpeedScale { get; set; } = 0.1f;
        
        protected ThemeVariant ActiveVariant { get; private set; }
        
        protected float AnimationSeconds => (float)_animationTick.Elapsed.TotalSeconds;
        
        private readonly Stopwatch _animationTick = Stopwatch.StartNew();

        protected EffectDrawBase(Rect bounds)
        {
            Bounds = bounds;
            var sTheme = SukiTheme.GetInstance();
            sTheme.OnBaseThemeChanged += v => ActiveVariant = v;
            ActiveVariant = SukiTheme.GetInstance().ActiveBaseTheme;
        }

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature is null) throw new InvalidOperationException("Unable to lease Skia API");
            using var lease = leaseFeature.Lease();
            Render(lease.SkCanvas, SKRect.Create((float)Bounds.Width, (float)Bounds.Height));
        }

        protected abstract void Render(SKCanvas canvas, SKRect rect);

        protected SKShader? EffectWithUniforms(float alpha = 1f) => 
            EffectWithUniforms(Effect, alpha);

        protected SKShader? EffectWithUniforms(SukiEffect? effect, float alpha = 1f) => 
            effect?.ToShaderWithUniforms(AnimationSeconds, ActiveVariant, Bounds, AnimationSpeedScale, alpha);

        protected virtual void EffectChanged(SukiEffect? oldValue, SukiEffect? newValue)
        {
            // no-op
        }
        
        public virtual void Dispose()
        {
            // no-op
        }
        
        public virtual bool Equals(ICustomDrawOperation other) => false;
        
        public virtual bool HitTest(Point p) => false;
    }
}