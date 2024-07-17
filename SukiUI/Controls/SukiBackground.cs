using System;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using SukiUI.Enums;
using SukiUI.Utilities;
using SukiUI.Utilities.Effects;

namespace SukiUI.Controls
{
    public class SukiBackground : Control
    {
        public static readonly StyledProperty<SukiBackgroundStyle> StyleProperty =
            AvaloniaProperty.Register<SukiWindow, SukiBackgroundStyle>(nameof(Style),
                defaultValue: SukiBackgroundStyle.Bubble);

        /// <summary>
        /// Which of the default background styles to use - DEFAULT: Gradient
        /// </summary>
        public SukiBackgroundStyle Style
        {
            get => GetValue(StyleProperty);
            set => SetValue(StyleProperty, value);
        }

        public static readonly StyledProperty<string?> ShaderFileProperty =
            AvaloniaProperty.Register<SukiWindow, string?>(nameof(ShaderFile));

        /// <summary>
        /// Specify a filename of an EMBEDDED RESOURCE file of type `.SkSL` with or without extension and it will be loaded and displayed.
        /// This takes priority over the <see cref="ShaderCode"/> property, which in turns takes priority over <see cref="Style"/>.
        /// </summary>
        public string? ShaderFile
        {
            get => GetValue(ShaderFileProperty);
            set => SetValue(ShaderFileProperty, value);
        }

        public static readonly StyledProperty<string?> ShaderCodeProperty =
            AvaloniaProperty.Register<SukiWindow, string?>(nameof(ShaderCode));

        /// <summary>
        /// Specify the shader code to use directly, simpler if you don't want to create an .SkSL file or want to generate the shader effect at runtime in some way.
        /// This takes priority over the <see cref="Style"/> property, but is second in priority to <see cref="ShaderFile"/> if it is set.
        /// </summary>
        public string? ShaderCode
        {
            get => GetValue(ShaderCodeProperty);
            set => SetValue(ShaderCodeProperty, value);
        }

        public static readonly StyledProperty<bool> AnimationEnabledProperty =
            AvaloniaProperty.Register<SukiWindow, bool>(nameof(AnimationEnabled), defaultValue: false);
        
        /// <summary>
        /// Enables/disables animations - DEFAULT: False
        /// </summary>
        public bool AnimationEnabled
        {
            get => GetValue(AnimationEnabledProperty);
            set => SetValue(AnimationEnabledProperty, value);
        }

        public static readonly StyledProperty<bool> TransitionsEnabledProperty =
            AvaloniaProperty.Register<SukiBackground, bool>(nameof(TransitionsEnabled), defaultValue: false);
        
        /// <summary>
        /// Enables/disables transition animations when switching backgrounds - DEFAULT: False
        /// </summary>
        public bool TransitionsEnabled
        {
            get => GetValue(TransitionsEnabledProperty);
            set => SetValue(TransitionsEnabledProperty, value);
        }

        public static readonly StyledProperty<double> TransitionTimeProperty =
            AvaloniaProperty.Register<SukiBackground, double>(nameof(TransitionTime), defaultValue: 1.0);
        
        /// <summary>
        /// The amount of time in seconds the background transition will take - DEFAULT: 1.0
        /// </summary>
        public double TransitionTime
        {
            get => GetValue(TransitionTimeProperty);
            set => SetValue(TransitionTimeProperty, value);
        }
        
        private readonly EffectBackgroundDraw _draw;
        private readonly IDisposable _observables;

        public SukiBackground()
        {
            IsHitTestVisible = false;
            _draw = new EffectBackgroundDraw(new Rect(0, 0, Bounds.Width, Bounds.Height));
            var transEnabledObs = this.GetObservable(TransitionsEnabledProperty)
                .Do(enabled => _draw.TransitionsEnabled = enabled)
                .Select(_ => Unit.Default);
            var transTime = this.GetObservable(TransitionTimeProperty)
                .Do(time => _draw.TransitionTime = time)
                .Select(_ => Unit.Default)
                .Merge(transEnabledObs);
            var animObs = this.GetObservable(AnimationEnabledProperty)
                .Do(enabled => _draw.AnimationEnabled = enabled)
                .Select(_ => Unit.Default)
                .Merge(transTime);
            var bgStyleObs = this.GetObservable(StyleProperty)
                .Select(_ => Unit.Default)
                .Merge(animObs);
            var bgShaderFileObs = this.GetObservable(ShaderFileProperty)
                .Select(_ => Unit.Default)
                .Merge(bgStyleObs);
            var bgShaderCodeObs = this.GetObservable(ShaderCodeProperty)
                .Select(_ => Unit.Default)
                .Merge(bgShaderFileObs)
                .Do(_ => HandleBackgroundStyleChanges())
                .ObserveOn(new AvaloniaSynchronizationContext());
            _observables = bgShaderCodeObs.Subscribe();
        }

        public override void Render(DrawingContext context)
        {
            _draw.Bounds = Bounds;
            context.Custom(_draw);
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
        }

        private void HandleBackgroundStyleChanges()
        {
            if (ShaderFile is not null)
                _draw.Effect = SukiEffect.FromEmbeddedResource(ShaderFile);
            else if (ShaderCode is not null)
                _draw.Effect = SukiEffect.FromString(ShaderCode);
            else
                _draw.Effect = SukiEffect.FromEmbeddedResource(Style.ToString());
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            _observables.Dispose();
        }
    }
}