using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace SukiUI.Controls;

public class SukiSideMenuItem : ListBoxItem
{
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, object?>(nameof(Icon));

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, string?>(nameof(Header));

    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly StyledProperty<object> PageContentProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, object>(nameof(PageContent));

    public object PageContent
    {
        get => GetValue(PageContentProperty);
        set => SetValue(PageContentProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (e.NameScope.Get<ContentPresenter>("PART_AltDisplay") is { } contentControl)
        {
            if (Header is not null || Icon is not null)
            {
                contentControl.IsVisible = false;
            }
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.Handled)
            return;

        if (!e.Handled && ItemsControl.ItemsControlFromItemContaner(this) is SukiSideMenu owner)
        {
            var p = e.GetCurrentPoint(this);

            if (p.Properties.PointerUpdateKind is PointerUpdateKind.LeftButtonPressed or
                PointerUpdateKind.RightButtonPressed)
            {
                if (p.Pointer.Type == PointerType.Mouse)
                {
                    // If the pressed point comes from a mouse, perform the selection immediately.
                    e.Handled = owner.UpdateSelectionFromPointerEvent(this);
                }
            }
        }
    }
}