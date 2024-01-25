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

    public void Initialize(SukiToastModel model)
    {
        Title = model.Title;
        Content = model.Content;
        _onClickedCallback = model.OnClicked;
        _timer.Interval = model.Lifetime.TotalMilliseconds;
        _timer.Start();
        DockPanel.SetDock(this, Dock.Bottom);
    }

    internal void InitializeInvisible()
    {
        Title = "Invisible";
        Content = "Invisible Content";
        Opacity = 0;
        _timer.Interval = 5;
        _timer.Elapsed -= TimerOnElapsed;
        _timer.Elapsed += async (_, _) => await SukiHost.ClearInvisibleToast(this);
        _timer.Start();
    }
}