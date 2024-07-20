using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using Avalonia.Collections;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using SukiUI.Enums;
using SukiUI.Utilities;

namespace SukiUI.Controls;

public class SukiWindow : Window
{
    protected override Type StyleKeyOverride => typeof(SukiWindow);

    public static readonly StyledProperty<double> TitleFontSizeProperty =
        AvaloniaProperty.Register<SukiWindow, double>(nameof(TitleFontSize), defaultValue: 13);

    public double TitleFontSize
    {
        get => GetValue(TitleFontSizeProperty);
        set => SetValue(TitleFontSizeProperty, value);
    }

    public static readonly StyledProperty<FontWeight> TitleFontWeightProperty =
        AvaloniaProperty.Register<SukiWindow, FontWeight>(nameof(TitleFontWeight),
            defaultValue: FontWeight.Bold);

    public FontWeight TitleFontWeight
    {
        get => GetValue(TitleFontWeightProperty);
        set => SetValue(TitleFontWeightProperty, value);
    }

    public static readonly StyledProperty<Control?> LogoContentProperty =
        AvaloniaProperty.Register<SukiWindow, Control?>(nameof(LogoContent));

    public Control? LogoContent
    {
        get => GetValue(LogoContentProperty);
        set => SetValue(LogoContentProperty, value);
    }

    public static readonly StyledProperty<bool> ShowBottomBorderProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(ShowBottomBorder), defaultValue: true);

    public bool ShowBottomBorder
    {
        get => GetValue(ShowBottomBorderProperty);
        set => SetValue(ShowBottomBorderProperty, value);
    }

    public static readonly StyledProperty<bool> IsTitleBarVisibleProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(IsTitleBarVisible), defaultValue: true);

    public bool IsTitleBarVisible
    {
        get => GetValue(IsTitleBarVisibleProperty);
        set => SetValue(IsTitleBarVisibleProperty, value);
    }

    public static readonly StyledProperty<bool> IsMenuVisibleProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(IsMenuVisible), defaultValue: false);

    public bool IsMenuVisible
    {
        get => GetValue(IsMenuVisibleProperty);
        set => SetValue(IsMenuVisibleProperty, value);
    }

    public static readonly StyledProperty<AvaloniaList<MenuItem>?> MenuItemsProperty =
        AvaloniaProperty.Register<SukiWindow, AvaloniaList<MenuItem>?>(nameof(MenuItems));

    public AvaloniaList<MenuItem>? MenuItems
    {
        get => GetValue(MenuItemsProperty);
        set => SetValue(MenuItemsProperty, value);
    }


    public static readonly StyledProperty<bool> CanMinimizeProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(CanMinimize), defaultValue: true);

    public bool CanMinimize
    {
        get => GetValue(CanMinimizeProperty);
        set => SetValue(CanMinimizeProperty, value);
    }

    public static readonly StyledProperty<bool> CanMoveProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(CanMove), defaultValue: true);

    public bool CanMove
    {
        get => GetValue(CanMoveProperty);
        set => SetValue(CanMoveProperty, value);
    }

    // BACKGROUND PROPERTIES
    public static readonly StyledProperty<bool> BackgroundAnimationEnabledProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(BackgroundAnimationEnabled), defaultValue: false);

    public bool BackgroundAnimationEnabled
    {
        get => GetValue(BackgroundAnimationEnabledProperty);
        set => SetValue(BackgroundAnimationEnabledProperty, value);
    }

    public static readonly StyledProperty<SukiBackgroundStyle> BackgroundStyleProperty =
        AvaloniaProperty.Register<SukiWindow, SukiBackgroundStyle>(nameof(BackgroundStyle),
            defaultValue: SukiBackgroundStyle.Bubble);

    
    /// <inheritdoc cref="SukiBackground.Style"/>
    public SukiBackgroundStyle BackgroundStyle
    {
        get => GetValue(BackgroundStyleProperty);
        set => SetValue(BackgroundStyleProperty, value);
    }

    public static readonly StyledProperty<string?> BackgroundShaderFileProperty =
        AvaloniaProperty.Register<SukiWindow, string?>(nameof(BackgroundShaderFile));

    /// <inheritdoc cref="SukiBackground.ShaderFile"/>
    public string? BackgroundShaderFile
    {
        get => GetValue(BackgroundShaderFileProperty);
        set => SetValue(BackgroundShaderFileProperty, value);
    }

    public static readonly StyledProperty<string?> BackgroundShaderCodeProperty =
        AvaloniaProperty.Register<SukiWindow, string?>(nameof(BackgroundShaderCode));

    
    /// <inheritdoc cref="SukiBackground.ShaderCode"/>
    public string? BackgroundShaderCode
    {
        get => GetValue(BackgroundShaderCodeProperty);
        set => SetValue(BackgroundShaderCodeProperty, value);
    }
    
    public static readonly StyledProperty<bool> BackgroundTransitionsEnabledProperty =
        AvaloniaProperty.Register<SukiBackground, bool>(nameof(BackgroundTransitionsEnabled), defaultValue: false);
    
    /// <inheritdoc cref="SukiBackground.TransitionsEnabled"/>
    public bool BackgroundTransitionsEnabled
    {
        get => GetValue(BackgroundTransitionsEnabledProperty);
        set => SetValue(BackgroundTransitionsEnabledProperty, value);
    }

    public static readonly StyledProperty<double> BackgroundTransitionTimeProperty =
        AvaloniaProperty.Register<SukiBackground, double>(nameof(BackgroundTransitionTime), defaultValue: 1.0);
    
    /// <inheritdoc cref="SukiBackground.TransitionTime"/>
    public double BackgroundTransitionTime
    {
        get => GetValue(BackgroundTransitionTimeProperty);
        set => SetValue(BackgroundTransitionTimeProperty, value);
    }

    public static readonly StyledProperty<Avalonia.Controls.Controls> RightWindowTitleBarControlsProperty = AvaloniaProperty.Register<SukiWindow, Avalonia.Controls.Controls>(nameof(RightWindowTitleBarControls), defaultValue: new Avalonia.Controls.Controls());

    /// <summary>
    /// Controls that are displayed on the right side of the title bar, to the left of the normal window control buttons. (Displays provided controls right-to-left)
    /// </summary>
    public Avalonia.Controls.Controls RightWindowTitleBarControls
    {
        get => GetValue(RightWindowTitleBarControlsProperty);
        set => SetValue(RightWindowTitleBarControlsProperty, value);
    }

    public SukiWindow()
    {
        MenuItems = new AvaloniaList<MenuItem>();
    }

    private IDisposable? _subscriptionDisposables;
    private SukiBackground? _background;

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;
        if (desktop.MainWindow is SukiWindow s && s != this)
        {
            if (Icon == null) Icon = s.Icon;
            // This would be nice to do, but obviously LogoContent is a control and you can't attach it twice.
            // if (LogoContent is null) LogoContent = s.LogoContent;
        }
    }


    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _subscriptionDisposables = this.GetObservable(WindowStateProperty)
            .Do(OnWindowStateChanged)
            .Select(_ => Unit.Default).ObserveOn(new AvaloniaSynchronizationContext())
            .Subscribe();
        try
        {
            // Create handlers for buttons
            if (e.NameScope.Get<Button>("PART_MaximizeButton") is { } maximize)
            {
                maximize.Click += OnMaximizeButtonClicked;
                bool pointerOnMaxButton = false;
                var  setter             = typeof(Button).GetProperty("IsPointerOver");

                var proc = (IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled) =>
                {
                    switch (msg)
                    {
                        case 533:
                            if (!pointerOnMaxButton) break;
                            if (!CanResize) break;
                            WindowState = WindowState == WindowState.Maximized
                                ? WindowState.Normal
                                : WindowState.Maximized;
                            break;
                        case 0x0084:
                            var point = new PixelPoint(
                                (short)(ToInt32(lParam) & 0xffff),
                                (short)(ToInt32(lParam) >> 16));
                            var buttonLeftTop = maximize.PointToScreen(new(0, 0));
                            var x             = (buttonLeftTop.X - point.X)         / RenderScaling;
                            var y             = (point.Y         - buttonLeftTop.Y) / RenderScaling;
                            if (new Rect(0, 0,
                                    maximize.DesiredSize.Width,
                                    maximize.DesiredSize.Height)
                                .Contains(new Point(x, y)))
                            {
                                setter?.SetValue(maximize, true);
                                pointerOnMaxButton     = true;
                                handled                = true;
                                return (IntPtr)9;
                            }

                            pointerOnMaxButton = false;
                            setter?.SetValue(maximize, false);
                            break;
                    }

                    return IntPtr.Zero;
                    
                    static int ToInt32(IntPtr ptr) => IntPtr.Size == 4 ? ptr.ToInt32() : (int)(ptr.ToInt64() & 0xffffffff);
                };

                

                Win32Properties.AddWndProcHookCallback(this, new Win32Properties.CustomWndProcHookCallback(proc));
            } 

            if (e.NameScope.Get<Button>("PART_MinimizeButton") is { } minimize)
                minimize.Click += (_, _) =>
                {
           
                    WindowState = WindowState.Minimized;
                   
                };

            if (e.NameScope.Get<Button>("PART_CloseButton") is { } close)
                close.Click += (_, _) => Close();

            if (e.NameScope.Get<GlassCard>("PART_TitleBarBackground") is { } titleBar)
            {
                titleBar.PointerPressed += OnTitleBarPointerPressed;
                titleBar.DoubleTapped += OnMaximizeButtonClicked;
            }
        }
        catch
        {
        }
    }

    private void OnMaximizeButtonClicked(object? sender, RoutedEventArgs args)
    {
        if (!CanResize) return;
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void OnWindowStateChanged(WindowState state)
    {
        if (state == WindowState.FullScreen)
            CanResize = CanMove = false;
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        BeginMoveDrag(e);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _subscriptionDisposables?.Dispose();
    }

}