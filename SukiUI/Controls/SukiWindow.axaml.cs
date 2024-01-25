using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
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

    public static readonly StyledProperty<Control> LogoContentProperty =
        AvaloniaProperty.Register<SukiWindow, Control>(nameof(LogoContent),
            defaultValue: new Border());

    public Control LogoContent
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

    public static readonly StyledProperty<bool> IsMinimizeButtonEnabledProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(IsMinimizeButtonEnabled),
            defaultValue: true);

    public bool IsMinimizeButtonEnabled
    {
        get => GetValue(IsMinimizeButtonEnabledProperty);
        set => SetValue(IsMinimizeButtonEnabledProperty, value);
    }

    public static readonly StyledProperty<bool> IsMaximizeButtonEnabledProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(IsMaximizeButtonEnabled),
            defaultValue: true);

    public bool IsMaximizeButtonEnabled
    {
        get => GetValue(IsMaximizeButtonEnabledProperty);
        set => SetValue(IsMaximizeButtonEnabledProperty, value);
    }

    public static readonly StyledProperty<bool> IsMenuVisibleProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(IsMenuVisible), defaultValue: false);

    public bool IsMenuVisible
    {
        get => GetValue(IsMenuVisibleProperty);
        set => SetValue(IsMenuVisibleProperty, value);
    }

    public static readonly StyledProperty<List<MenuItem>> MenuItemsProperty =
        AvaloniaProperty.Register<SukiWindow, List<MenuItem>>(nameof(MenuItems),
            defaultValue: new List<MenuItem>());

    public List<MenuItem> MenuItems
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

    private IDisposable? _subscribtionDisposables;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Apply a style only on windows to offset oversizing.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var maxStyle = new Style(
                x => x.OfType<SukiWindow>()
                    .PropertyEquals(WindowStateProperty, WindowState.Maximized)
                    .Template()
                    .OfType<VisualLayerManager>());
            maxStyle.Setters.Add(new Setter(PaddingProperty, new Thickness(8)));
            Application.Current!.Styles.Add(maxStyle);
        }

        // Create handlers for buttons
        if (e.NameScope.Find<Button>("PART_MaximizeButton") is { } maximize)
            maximize.Click += (_, _) =>
                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;

        if (e.NameScope.Find<Button>("PART_MinimizeButton") is { } minimize)
            minimize.Click += (_, _) => WindowState = WindowState.Minimized;

        if (e.NameScope.Find<Button>("PART_CloseButton") is { } close)
            close.Click += (_, _) => Close();

        if (e.NameScope.Find<GlassCard>("PART_TitleBarBackground") is { } titleBar)
            titleBar.PointerPressed += OnTitleBarPointerPressed;

        if (e.NameScope.Find<SukiBackground>("PART_Background") is { } background)
        {
            background.SetAnimationEnabled(BackgroundAnimationEnabled);
            _subscribtionDisposables = this.GetObservable(BackgroundAnimationEnabledProperty)
                .Do(enabled => background.SetAnimationEnabled(enabled))
                .ObserveOn(new AvaloniaSynchronizationContext())
                .Subscribe();
        }
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.ClickCount >= 2)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
        else
            BeginMoveDrag(e);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _subscribtionDisposables?.Dispose();
    }
}