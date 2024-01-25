using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;

namespace SukiUI.Controls
{
    // TODO: This needs fairly significant work to make a bit more bomb proof
    // There are probably some more gains that can be made in terms of performance.
    // Unfortunately we're still bound by the arrange of controls having to happen on the main thread.
    public class SukiTransitioningContentControl : TemplatedControl
    {
        internal static readonly StyledProperty<object?> FirstBufferProperty =
            AvaloniaProperty.Register<SukiTransitioningContentControl, object?>(nameof(FirstBuffer));

        internal object? FirstBuffer
        {
            get => GetValue(FirstBufferProperty);
            set => SetValue(FirstBufferProperty, value);
        }

        internal static readonly StyledProperty<object?> SecondBufferProperty =
            AvaloniaProperty.Register<SukiTransitioningContentControl, object?>(nameof(SecondBuffer));

        internal object? SecondBuffer
        {
            get => GetValue(SecondBufferProperty);
            set => SetValue(SecondBufferProperty, value);
        }

        public static readonly StyledProperty<object?> ContentProperty = AvaloniaProperty.Register<SukiTransitioningContentControl, object?>(nameof(Content));

        public object? Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        private bool _isFirstBufferActive;

        private ContentPresenter _firstBuffer = null!;
        private ContentPresenter _secondBuffer = null!;

        private IDisposable? _disposable;

        private static readonly Animation FadeIn;
        private static readonly Animation FadeOut;
        
        private ContentPresenter To => _isFirstBufferActive ? _secondBuffer : _firstBuffer;
        private ContentPresenter From => _isFirstBufferActive ? _firstBuffer : _secondBuffer;

        static SukiTransitioningContentControl()
        {
            FadeIn = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(400),
                Children =
                {
                    new KeyFrame()
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = OpacityProperty,
                                Value = 0d
                            }
                        },
                        Cue = new Cue(0d)
                    },
                    new KeyFrame()
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = OpacityProperty,
                                Value = 1d
                            }
                        },
                        Cue = new Cue(1d)
                    }
                },
                FillMode = FillMode.Forward
            };
            FadeOut = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(400),
                Children =
                {
                    new KeyFrame()
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = OpacityProperty,
                                Value = 1d
                            }
                        },
                        Cue = new Cue(0d)
                    },
                    new KeyFrame()
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = OpacityProperty,
                                Value = 0d
                            }
                        },
                        Cue = new Cue(1d)
                    }
                },
                FillMode = FillMode.Forward
            };
            FadeIn.Duration = FadeOut.Duration = TimeSpan.FromMilliseconds(250);
        }

        private CancellationTokenSource _animCancellationToken = new();

        private IDisposable? _contentDisposable;

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            _contentDisposable = this.GetObservable(ContentProperty)
                .ObserveOn(new AvaloniaSynchronizationContext())
                .Subscribe(PushContent);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            if (e.NameScope.Get<ContentPresenter>("PART_FirstBufferControl") is { } fBuff)
                _firstBuffer = fBuff;
            if (e.NameScope.Get<ContentPresenter>("PART_SecondBufferControl") is { } sBuff)
                _secondBuffer = sBuff;
        }

        public void PushContent(object? content)
        {
            if (content is null) return;
            
            _animCancellationToken.Cancel();
            _animCancellationToken.Dispose();
            _animCancellationToken = new CancellationTokenSource();
            
            if (_isFirstBufferActive) SecondBuffer = content;
            else FirstBuffer = content;
            try
            {
                FadeOut.RunAsync(From, _animCancellationToken.Token);
                FadeIn.RunAsync(To, _animCancellationToken.Token);
            }
            catch
            {
                // ignored
            }

            To.IsHitTestVisible = true;
            From.IsHitTestVisible = false;
            _isFirstBufferActive = !_isFirstBufferActive;
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            _disposable?.Dispose();
            _contentDisposable?.Dispose();
            _animCancellationToken.Dispose();
        }
    }
}