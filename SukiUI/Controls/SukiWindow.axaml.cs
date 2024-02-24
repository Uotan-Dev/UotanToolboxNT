using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Collections;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;

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

    public static readonly StyledProperty<bool> BackgroundAnimationEnabledProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(BackgroundAnimationEnabled), defaultValue: false);

    public bool BackgroundAnimationEnabled
    {
        get => GetValue(BackgroundAnimationEnabledProperty);
        set => SetValue(BackgroundAnimationEnabledProperty, value);
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

    public SukiWindow()
    {
        MenuItems = new AvaloniaList<MenuItem>();
    }

    private IDisposable? _subscriptionDisposables;

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

        var stateObs = this.GetObservable(WindowStateProperty)
            .Do(OnWindowStateChanged)
            .Select(_ => Unit.Default);

        // Create handlers for buttons
        if (e.NameScope.Get<Button>("PART_MaximizeButton") is { } maximize)
        {
            maximize.Click += (_, _) =>
            {
                if (!CanResize) return;
                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            };
        }

        if (e.NameScope.Get<Button>("PART_MinimizeButton") is { } minimize)
            minimize.Click += (_, _) => WindowState = WindowState.Minimized;

        if (e.NameScope.Get<Button>("PART_CloseButton") is { } close)
            close.Click += (_, _) => Close();

        if (e.NameScope.Get<GlassCard>("PART_TitleBarBackground") is { } titleBar)
            titleBar.PointerPressed += OnTitleBarPointerPressed;

        if (e.NameScope.Get<SukiBackground>("PART_Background") is { } background)
        {
            background.SetAnimationEnabled(BackgroundAnimationEnabled);
            var bgObs = this.GetObservable(BackgroundAnimationEnabledProperty)
                .Do(enabled => background.SetAnimationEnabled(enabled))
                .Select(_ => Unit.Default)
                .Merge(stateObs)
                .ObserveOn(new AvaloniaSynchronizationContext());

            _subscriptionDisposables = bgObs.Subscribe();
        }
    }

    private void OnWindowStateChanged(WindowState state)
    {
        if (state == WindowState.FullScreen)
            CanResize = CanMove = false;
        else
            CanResize = CanMove = true;
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.ClickCount >= 2 && CanResize)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
        else if (CanMove)
            BeginMoveDrag(e);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _subscriptionDisposables?.Dispose();
    }
}