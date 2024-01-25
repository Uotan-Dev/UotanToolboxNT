using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace SukiUI.Controls;

public class SukiSideMenu : SelectingItemsControl
{
    public static readonly StyledProperty<bool> IsMenuExpandedProperty =
        AvaloniaProperty.Register<SukiSideMenu, bool>(nameof(IsMenuExpanded), defaultValue: true);

    public bool IsMenuExpanded
    {
        get => GetValue(IsMenuExpandedProperty);
        set => SetValue(IsMenuExpandedProperty, value);
    }

    public static readonly StyledProperty<bool> HeaderContentOverlapsToggleButtonProperty =
        AvaloniaProperty.Register<SukiSideMenu, bool>(nameof(HeaderContentOverlapsToggleButton), defaultValue: false);

    public bool HeaderContentOverlapsToggleButton
    {
        get => GetValue(HeaderContentOverlapsToggleButtonProperty);
        set => SetValue(HeaderContentOverlapsToggleButtonProperty, value);
    }

    public static readonly StyledProperty<double> HeaderMinHeightProperty =
        AvaloniaProperty.Register<SukiSideMenu, double>(nameof(HeaderMinHeight));

    public double HeaderMinHeight
    {
        get => GetValue(HeaderMinHeightProperty);
        set => SetValue(HeaderMinHeightProperty, value);
    }

    public static readonly StyledProperty<object?> HeaderContentProperty =
        AvaloniaProperty.Register<SukiSideMenu, object?>(nameof(HeaderContent));

    public object? HeaderContent
    {
        get => GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }

    public static readonly StyledProperty<object?> FooterContentProperty =
        AvaloniaProperty.Register<SukiSideMenu, object?>(nameof(FooterContent));

    public object? FooterContent
    {
        get => GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

    private bool IsSpacerVisible => HeaderContentOverlapsToggleButton && !IsMenuExpanded;

    private IDisposable? _subscriptionDisposable;
    private IDisposable? _contentDisposable;
    
    public SukiSideMenu()
    {
        SelectionMode = SelectionMode.Single | SelectionMode.AlwaysSelected;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (Items.Any())
        {
            SelectedItem = Items.First();
        }

        e.NameScope.Get<Button>("PART_SidebarToggleButton").Click += (_, _) =>
            IsMenuExpanded = !IsMenuExpanded;
        e.NameScope.Get<Button>("PART_SidebarToggleButtonOverlay").Click += (_, _) =>
            IsMenuExpanded = !IsMenuExpanded;

        if (e.NameScope.Get<Grid>("PART_Spacer") is { } spacer)
        {
            spacer.IsVisible = IsSpacerVisible;
            var menuObservable = this.GetObservable(IsMenuExpandedProperty)
                .Select(_ => Unit.Default);
            var headerContentObservable = this.GetObservable(HeaderContentOverlapsToggleButtonProperty)
                .Select(_ => Unit.Default);
            _subscriptionDisposable = menuObservable
                .Merge(headerContentObservable)
                .ObserveOn(new AvaloniaSynchronizationContext())
                .Subscribe(_ => spacer.IsVisible = IsSpacerVisible);
        }

        if (e.NameScope.Get<SukiTransitioningContentControl>("PART_TransitioningContentControl") is { } contentControl)
        {
            _contentDisposable = this.GetObservable(SelectedItemProperty)
                .ObserveOn(new AvaloniaSynchronizationContext())
                .Do(obj =>
                {
                    contentControl.Content = obj switch
                    {
                        SukiSideMenuItem { PageContent: { } sukiMenuPageContent } => sukiMenuPageContent,
                        _ => obj
                    };
                })
                .Subscribe();
        }
    }

    public bool UpdateSelectionFromPointerEvent(Control source)
    {
        return UpdateSelectionFromEventSource(source);
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        if (ItemTemplate != null && ItemTemplate.Match(item) && ItemTemplate.Build(item) is SukiSideMenuItem sukiMenuItem)
            return sukiMenuItem;
        return new SukiSideMenuItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<SukiSideMenuItem>(item, out recycleKey);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _contentDisposable?.Dispose();
        _subscriptionDisposable?.Dispose();
    }
}