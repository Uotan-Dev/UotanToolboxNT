using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using SukiUI.Models;
using System;
using System.Timers;
using Avalonia.Media;
using SukiUI.Content;
using SukiUI.Enums;

namespace SukiUI.Controls;

public class SukiToast : ContentControl
{
    protected override Type StyleKeyOverride => typeof(SukiToast);
    
    internal SukiHost Host { get; private set; }

    private readonly Timer _timer = new();

    private Action? _onClickedCallback;

    public SukiToast()
    {
        _timer.Elapsed += TimerOnElapsed;
    }

    private async void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        _timer.Stop();
        await SukiHost.ClearToast(this);
    }
    
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<SukiToast, object?>(nameof(Icon));

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SukiToast, string>(nameof(Title));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        e.NameScope.Get<Border>("PART_ToastCard").PointerPressed += ToastCardClickedHandler;
    }

    private async void ToastCardClickedHandler(object o, PointerPressedEventArgs pointerPressedEventArgs)
    {
        _onClickedCallback?.Invoke();
        _onClickedCallback = null;
        await SukiHost.ClearToast(this);
    }
    
    // Icon Foreground Brushes
    // Note: it would be better to place them into a resource dictionary, but findResource performs slightly slower
    private readonly SolidColorBrush _infoIconForeground = new(Color.FromRgb(47,84,235));
    private readonly SolidColorBrush _successIconForeground = new(Color.FromRgb(35,143,35));
    private readonly SolidColorBrush _warningIconForeground = new(Color.FromRgb(177,113,20));
    private readonly SolidColorBrush _errorIconForeground = new(Color.FromRgb(216,63,48));

    public void Initialize(SukiToastModel model, SukiHost host)
    {
        Host = host;
        Title = model.Title;
        Content = model.Content;
        Icon = model.Type switch
        {
            SukiToastType.Info => Icons.InformationOutline,
            SukiToastType.Success => Icons.CircleOutlineCheck,
            SukiToastType.Warning => Icons.AlertOutline,
            SukiToastType.Error => Icons.CircleOutlineMinus,
            _ => Icons.InformationOutline
        };
        Foreground = model.Type switch
        {
            SukiToastType.Info => _infoIconForeground,
            SukiToastType.Success => _successIconForeground,
            SukiToastType.Warning => _warningIconForeground,
            SukiToastType.Error => _errorIconForeground,
            _ => _infoIconForeground
        };
        _onClickedCallback = model.OnClicked;
        _timer.Interval = model.Lifetime.TotalMilliseconds;
        _timer.Start();
        DockPanel.SetDock(this, Dock.Bottom);
    }
}