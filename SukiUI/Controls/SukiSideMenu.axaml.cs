using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Interactivity;
using Avalonia.Layout;
using SukiUI.Enums;

namespace SukiUI.Controls;

public class SukiSideMenu : SelectingItemsControl
{
    public static readonly StyledProperty<bool> IsSearchEnabledProperty =
        AvaloniaProperty.Register<SukiSideMenu, bool>(nameof(IsSearchEnabled), defaultValue: false);

    public bool IsSearchEnabled
    {
        get => GetValue(IsSearchEnabledProperty);
        set => SetValue(IsSearchEnabledProperty, value);
    }
    
    public static readonly StyledProperty<bool> IsToggleButtonVisibleProperty =
        AvaloniaProperty.Register<SukiSideMenu, bool>(nameof(IsToggleButtonVisible), defaultValue: true);

    public bool IsToggleButtonVisible
    {
        get => GetValue(IsToggleButtonVisibleProperty);
        set => SetValue(IsToggleButtonVisibleProperty, value);
    }
    
    public static readonly StyledProperty<bool> IsMenuExpandedProperty =
        AvaloniaProperty.Register<SukiSideMenu, bool>(nameof(IsMenuExpanded), defaultValue: true);

    public bool IsMenuExpanded
    {
        get => GetValue(IsMenuExpandedProperty);
        set => SetValue(IsMenuExpandedProperty, value);
    }
    
    public static readonly StyledProperty<int> OpenPaneLengthProperty =
        AvaloniaProperty.Register<SukiSideMenu, int>(nameof(OpenPaneLength), defaultValue: 220);

    public int OpenPaneLength
    {
        get => GetValue(OpenPaneLengthProperty);
        set => SetValue(OpenPaneLengthProperty, value switch
        {
            >= 200 => value,
            _ => throw new ArgumentOutOfRangeException($"OpenPaneLength must be greater than or equal to 200, but was {value}")
        });
    }
    
    public static readonly StyledProperty<HorizontalAlignment> TogglePaneButtonPositionProperty =
        AvaloniaProperty.Register<SukiSideMenu, HorizontalAlignment>(nameof(TogglePaneButtonPosition), defaultValue: HorizontalAlignment.Right);

    public SideMenuTogglePaneButtonPositionOptions TogglePaneButtonPosition
    {
        get => GetValue(TogglePaneButtonPositionProperty) switch
        {
            HorizontalAlignment.Right => SideMenuTogglePaneButtonPositionOptions.Right,
            HorizontalAlignment.Left => SideMenuTogglePaneButtonPositionOptions.Left,
            _ => SideMenuTogglePaneButtonPositionOptions.Right
        };
        set => SetValue(TogglePaneButtonPositionProperty, value switch
        {
            SideMenuTogglePaneButtonPositionOptions.Right => HorizontalAlignment.Right,
            SideMenuTogglePaneButtonPositionOptions.Left => HorizontalAlignment.Left,
            _ => HorizontalAlignment.Right
        });
    }

    public static readonly StyledProperty<bool> IsSelectedItemContentMovableProperty =
        AvaloniaProperty.Register<SukiSideMenu, bool>(nameof(IsSelectedItemContentMovable), defaultValue: true);

    public bool IsSelectedItemContentMovable
    {
        get => GetValue(IsSelectedItemContentMovableProperty);
        set => SetValue(IsSelectedItemContentMovableProperty, value);
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

    private bool IsSpacerVisible => !IsMenuExpanded;

    private IDisposable? _subscriptionDisposable;
    private IDisposable? _contentDisposable;
    
    public SukiSideMenu()
    {
        SelectionMode = SelectionMode.Single | SelectionMode.AlwaysSelected;
    }


    private void MenuExpandedClicked()
    {
        IsMenuExpanded = !IsMenuExpanded;
        
        if(_sideMenuItems.Any())
            foreach (var item in _sideMenuItems)
                item.IsTopMenuExpanded = IsMenuExpanded;
        
        else if(Items.FirstOrDefault() is SukiSideMenuItem)
            foreach (SukiSideMenuItem? item in Items)
                item!.IsTopMenuExpanded = IsMenuExpanded;
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (Items.Any())
        {
            SelectedItem = Items.First();
        }

        /*e.NameScope.Get<Button>("PART_SidebarToggleButton").Click += (_, _) =>
            MenuExpandedClicked();*/
      

        if (e.NameScope.Get<Grid>("PART_Spacer") is { } spacer)
        {
            spacer.IsVisible = IsSpacerVisible;
            var menuObservable = this.GetObservable(IsMenuExpandedProperty)
                .Select(_ => Unit.Default);
           
            _subscriptionDisposable = menuObservable
               
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
        SukiSideMenuItem menuItem =
            (ItemTemplate != null && ItemTemplate.Match(item) &&
             ItemTemplate.Build(item) is SukiSideMenuItem sukiMenuItem)
                ? sukiMenuItem
                : new SukiSideMenuItem();
        menuItem.IsContentMovable = IsSelectedItemContentMovable;
        _sideMenuItems.Add(menuItem);
        return menuItem;
    }

    private List<SukiSideMenuItem> _sideMenuItems = new();

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
