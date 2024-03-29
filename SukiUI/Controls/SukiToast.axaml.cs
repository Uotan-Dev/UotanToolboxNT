using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using SukiUI.Models;
using System;
using System.Timers;

namespace SukiUI.Controls;

public class SukiToast : ContentControl
{
    protected override Type StyleKeyOverride => typeof(SukiToast);

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SukiToast, string>(nameof(Title));
    
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

    public void Initialize(SukiToastModel model, SukiHost host)
    {
        Host = host;
        Title = model.Title;
        Content = model.Content;
        _onClickedCallback = model.OnClicked;
        _timer.Interval = model.Lifetime.TotalMilliseconds;
        _timer.Start();
        DockPanel.SetDock(this, Dock.Bottom);
    }
}