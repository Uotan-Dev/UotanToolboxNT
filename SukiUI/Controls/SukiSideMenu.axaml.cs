using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
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
        
        if(_SideMenuItems.Any())
            foreach (SukiSideMenuItem item in _SideMenuItems)
                item.IsTopMenuExpanded = IsMenuExpanded;
        
        else if(Items.First() is SukiSideMenuItem)
            foreach (SukiSideMenuItem item in Items)
                item.IsTopMenuExpanded = IsMenuExpanded;
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (Items.Any())
        {
            SelectedItem = Items.First();
        }

        e.NameScope.Get<Button>("PART_SidebarToggleButton").Click += (_, _) =>
            MenuExpandedClicked();
      

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
        
        _SideMenuItems.Add(menuItem);
        return menuItem;
    }

    private List<SukiSideMenuItem> _SideMenuItems = new List<SukiSideMenuItem>();

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