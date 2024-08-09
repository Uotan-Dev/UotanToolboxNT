using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Threading;
using SukiUI.Enums;
using SukiUI.Helpers;
using SukiUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.VisualTree;
using SukiUI.Content;

namespace SukiUI.Controls;

/// <summary>
/// Hosts both Dialogs and Notifications
/// </summary>
public class SukiHost : ContentControl
{
    
  

    protected override Type StyleKeyOverride => typeof(SukiHost);

    public static readonly StyledProperty<bool> IsDialogOpenProperty =
        AvaloniaProperty.Register<SukiHost, bool>(nameof(IsDialogOpen), defaultValue: false);

    public bool IsDialogOpen
    {
        get => GetValue(IsDialogOpenProperty);
        set => SetValue(IsDialogOpenProperty, value);
    }

    public static readonly StyledProperty<Control> DialogContentProperty =
        AvaloniaProperty.Register<SukiHost, Control>(nameof(DialogContent));

    public Control DialogContent
    {
        get => GetValue(DialogContentProperty);
        set => SetValue(DialogContentProperty, value);
    }

    public static readonly StyledProperty<bool> AllowBackgroundCloseProperty =
        AvaloniaProperty.Register<SukiHost, bool>(nameof(AllowBackgroundClose), defaultValue: true);

    public bool AllowBackgroundClose
    {
        get => GetValue(AllowBackgroundCloseProperty);
        set => SetValue(AllowBackgroundCloseProperty, value);
    }

    public static readonly AttachedProperty<ToastLocation> ToastLocationProperty =
        AvaloniaProperty.RegisterAttached<SukiHost, Window, ToastLocation>("ToastLocation",
            defaultValue: ToastLocation.BottomRight);

    public static void SetToastLocation(Control element, ToastLocation value) =>
        element.SetValue(ToastLocationProperty, value);

    public static ToastLocation GetToastLocation(Control element) =>
        element.GetValue(ToastLocationProperty);

    public static readonly AttachedProperty<int> ToastLimitProperty =
        AvaloniaProperty.RegisterAttached<SukiHost, Window, int>("ToastLimit", defaultValue: 5);

    public static int GetToastLimit(Control element) => element.GetValue(ToastLimitProperty);

    public static void SetToastLimit(Control element, int value) =>
        element.SetValue(ToastLimitProperty, value);

    public static readonly StyledProperty<AvaloniaList<SukiToast>?> ToastsCollectionProperty =
        AvaloniaProperty.Register<SukiHost, AvaloniaList<SukiToast>?>(nameof(ToastsCollection));

    public AvaloniaList<SukiToast>? ToastsCollection
    {
        get => GetValue(ToastsCollectionProperty);
        set => SetValue(ToastsCollectionProperty, value);
    }

    private static Window? _mainWindow;
    private static readonly Dictionary<Window, SukiHost> Instances = new();

    private int _maxToasts;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (VisualRoot is not Window w) return;
        Instances.Add(w, this);
        _mainWindow ??= w;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (VisualRoot is not Window window)
            throw new InvalidOperationException("SukiHost must be hosted inside a Window or SukiWindow");
        ToastsCollection ??= new AvaloniaList<SukiToast>();
        _maxToasts = GetToastLimit(window);
        var toastLoc = GetToastLocation(window);

        e.NameScope.Get<Border>("PART_DialogBackground").PointerPressed += (_, _) => BackgroundRequestClose(this);

        e.NameScope.Get<ItemsControl>("PART_ToastPresenter").HorizontalAlignment =
            toastLoc == ToastLocation.BottomLeft
                ? HorizontalAlignment.Left
                : HorizontalAlignment.Right;
        
        var b = e.NameScope.Get<Border>("PART_DialogBackground");
        b.Loaded += (sender, args) =>
        {
            var v = ElementComposition.GetElementVisual(b);
            CompositionAnimationHelper.MakeOpacityAnimated(v, 400);
        }; 
        
     
    }
    

    // TODO: Dialog API desperately needs to support a result or on-close callback.
    // TODO: Toasts and dialogs should be dragged out into their own discrete service and provided by a higher level service locator.
    // TODO: Currently not possible to switch the toast side at runtime, in reality there should be multiple anchors and toasts can be displayed on them arbitrarily.
    // Giving devs direct access to this object like this is messy and there really needs to be a standard abstraction above all these features.
    // This goes for other APIs like the background and theming.

    /// <summary>
    /// Shows a dialog in the <see cref="SukiHost"/>
    /// Can display ViewModels if provided, if a suitable ViewLocator has been registered with Avalonia.
    /// </summary>
    /// <param name="window">The window who's SukiHost should be used to display the toast.</param>
    /// <param name="content">Content to display.</param>
    /// <param name="showCardBehind">Whether or not to show a card behind the content.</param>
    /// <param name="allowBackgroundClose">Allows the dialog to be closed by clicking outside of it.</param>
    /// <exception cref="InvalidOperationException">Thrown if there is no SukiHost associated with the specified window.</exception>
    public static void ShowDialog(Window window, object? content, bool showCardBehind = true,
        bool allowBackgroundClose = false)
    {
        if (!Instances.TryGetValue(window, out var host))
            throw new InvalidOperationException("No SukiHost present in this window");
        var control = content as Control ?? ViewLocator.TryBuild(content);
        host.IsDialogOpen = true;
        host.DialogContent = control;
        host.AllowBackgroundClose = allowBackgroundClose;
        host.GetTemplateChildren().First(n => n.Name == "BorderDialog1").Opacity = showCardBehind ? 1 : 0;
    }

    public static Task ShowDialogAsync(Window window, object? content, bool showCardBehind = true, bool allowBackgroundClose = false)
    {
        if (!Instances.TryGetValue(window, out var host))
            throw new InvalidOperationException("No SukiHost present in this window");

        var control = content as Control ?? ViewLocator.TryBuild(content);
        host.IsDialogOpen = true;
        host.DialogContent = control;
        host.AllowBackgroundClose = allowBackgroundClose;
        host.GetTemplateChildren().First(n => n.Name == "BorderDialog1").Opacity = showCardBehind ? 1 : 0;

        var tcs = new TaskCompletionSource<object>();
        EventHandler<AvaloniaPropertyChangedEventArgs> dialogOpenChanged = null;
        dialogOpenChanged = (sender, args) =>
        {
            if (!host.IsDialogOpen)
            {
                tcs.TrySetResult(null);
                host.PropertyChanged -= dialogOpenChanged;
            }
        };
        host.PropertyChanged += dialogOpenChanged;
        return tcs.Task;
    }

    /// <summary>
    /// <inheritdoc cref="ShowDialog(Avalonia.Controls.Window,object?,bool,bool)"/>
    /// </summary>
    /// <param name="content">Content to display.</param>
    /// <param name="showCardBehind">Whether or not to show a card behind the content.</param>
    /// <param name="allowBackgroundClose">Allows the dialog to be closed by clicking outside of it.</param>
    public static void ShowDialog(object? content, bool showCardBehind = true, bool allowBackgroundClose = false) =>
        ShowDialog(_mainWindow, content, showCardBehind, allowBackgroundClose);

    public static async Task ShowDialogAsync(object? content, bool showCardBehind = true, bool allowBackgroundClose = false)
    {
        await ShowDialogAsync(_mainWindow, content, showCardBehind, allowBackgroundClose);
    }

    public static void ShowMessageBox(MessageBoxModel model, bool allowbackgroundclose = true)
    {
        
        
        SukiHost.ShowDialog(new MessageBox(){
            _onActionCallback = model.ActionButton,
            Title = model.Title, Content = model.Content, ShowActionButton = model.ActionButtonContent != null, 
            ActionButtonContent = model.ActionButtonContent, 
            Icon = model.Type switch
            {
                NotificationType.Info => Icons.InformationOutline,
                NotificationType.Success => Icons.Check,
                NotificationType.Warning => Icons.AlertOutline,
                NotificationType.Error => Icons.AlertOutline,
                _ => Icons.InformationOutline
            }
            , Foreground = model.Type switch
            {
                NotificationType.Info => SukiHost.GetGradient(Color.FromRgb(47,84,235)),
                NotificationType.Success => SukiHost.GetGradient(Color.FromRgb(82,196,26)),
                NotificationType.Warning => SukiHost.GetGradient(Color.FromRgb(240,140,22)),
                NotificationType.Error => SukiHost.GetGradient(Color.FromRgb(245,34,45)),
                _ => SukiHost.GetGradient(Color.FromRgb(89,126,255))
            }}, false, allowbackgroundclose);

    }

    private static LinearGradientBrush GetGradient(Color c1)
    {
        return new LinearGradientBrush()
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(){Color = c1, Offset = 0},
                new GradientStop(){Color = Color.FromArgb(140, c1.R,c1.G,c1.B), Offset = 1}
            }
        };
    }
    
    /// <summary>
    /// Attempts to close a dialog if one is shown in a specific window.
    /// </summary>
    public static void CloseDialog(Window window)
    {
        if (!Instances.TryGetValue(window, out var host))
            throw new InvalidOperationException("No SukiHost present in this window");
        host.IsDialogOpen = false;
       
    }

    /// <summary>
    /// Attempts to close a dialog if one is shown in the earliest of any opened windows.
    /// </summary>
    public static void CloseDialog() => CloseDialog(_mainWindow);
    
    /// <summary>
    /// Used to close the open dialog when the background is clicked, if this is allowed.
    /// </summary>
    private static void BackgroundRequestClose(SukiHost host)
    {
        if (!host.AllowBackgroundClose) return;
        host.IsDialogOpen = false;
    }
    
    /// <summary>
    /// Shows a toast in the SukiHost - The default location is in the bottom right.
    /// This can be changed with an attached property in SukiWindow.
    /// </summary>
    /// <param name="window">The window who's SukiHost should be used to display the toast.</param>
    /// <param name="model">A pre-constructed <see cref="SukiToastModel"/>.</param>
    /// <exception cref="InvalidOperationException">Thrown if there is no SukiHost associated with the specified window.</exception>
    public static async Task ShowToast(Window window, ToastModel model)
    {
        try
        {
            if (!Instances.TryGetValue(window, out var host))
                throw new InvalidOperationException("No SukiHost present in this window");

            var toast = ToastPool.Get();
            toast.Initialize(model, host);
            if (host.ToastsCollection.Count >= host._maxToasts)
                await ClearToast(host.ToastsCollection.First());
            Dispatcher.UIThread.Invoke(() =>
            {
                host.ToastsCollection.Add(toast);
                toast.Animate(OpacityProperty, 0d, 1d, TimeSpan.FromMilliseconds(500));
                toast.Animate(MarginProperty, new Thickness(0, 10, 0, -10), new Thickness(),
                    TimeSpan.FromMilliseconds(500));
            });
        }catch{}
    }
    
    /// <summary>
    /// <inheritdoc cref="ShowToast(Window, SukiToastModel)"/>
    /// This method will show the toast in the earliest opened window.
    /// </summary>
    /// <param name="model">A pre-constructed <see cref="SukiToastModel"/>.</param>
    public static Task ShowToast(ToastModel model) => 
        ShowToast(_mainWindow, model);

    /// <summary>
    /// <inheritdoc cref="ShowToast(Window, SukiToastModel)"/>
    /// This method will show the toast in the earliest opened window.
    /// </summary>
    /// <param name="title">The title to display in the toast.</param>
    /// <param name="content">The content of the toast, this can be any control or ViewModel.</param>
    /// <param name="type">The type of the toast, including Info, Success, Warning and Error</param>
    /// <param name="duration">Duration for this toast to be active. Default is 2 seconds.</param>
    /// <param name="onClicked">A callback that will be fired if the Toast is cleared by clicking.</param>
    public static Task ShowToast(string title, object content, NotificationType? type = NotificationType.Info, TimeSpan? duration = null, Action? onClicked = null) =>
        ShowToast(new ToastModel(
            title,
            content as Control ?? ViewLocator.TryBuild(content),
            type ?? NotificationType.Info,
            duration ?? TimeSpan.FromSeconds(4),
            onClicked));

    /// <summary>
    /// <inheritdoc cref="ShowToast(Window, SukiToastModel)"/>
    /// This method will show the toast in a specific window.
    /// </summary>
    /// <param name="window">The window who's SukiHost should be used to display the toast.</param>
    /// <param name="title">The title to display in the toast.</param>
    /// <param name="content">The content of the toast, this can be any control or ViewModel.</param>
    /// <param name="type">The type of the toast, including Info, Success, Warning and Error</param>
    /// <param name="duration">Duration for this toast to be active. Default is 2 seconds.</param>
    /// <param name="onClicked">A callback that will be fired if the Toast is cleared by clicking.</param>
    public static Task ShowToast(Window window, string title, object content, NotificationType? type = NotificationType.Info, TimeSpan? duration = null,
        Action? onClicked = null) =>
        ShowToast(window, new ToastModel(
            title,
            content as Control ?? ViewLocator.TryBuild(content),
            type ?? NotificationType.Info,
            duration ?? TimeSpan.FromSeconds(4),
            onClicked));

    /// <summary>
    /// Clears a specific toast from display (if it is still currently being displayed).
    /// </summary>
    /// <param name="toast">The toast to clear.</param>
    public static async Task ClearToast(SukiToast toast)
    {
        var wasRemoved = await Task.Run(async () =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                toast.Animate(OpacityProperty, 1d, 0d, TimeSpan.FromMilliseconds(300));
                toast.Animate(MarginProperty, new Thickness(), new Thickness(0, 50, 0, -50),
                    TimeSpan.FromMilliseconds(300));
            });
            await Task.Delay(300);
            return Dispatcher.UIThread.Invoke(() => toast.Host.ToastsCollection.Remove(toast));
        });

        if (!wasRemoved) return;
        ToastPool.Return(toast);
    }

    /// <summary>
    /// Clears all active toasts in a specific window immediately.
    /// </summary>
    public static void ClearAllToasts(Window window)
    {
        if (!Instances.TryGetValue(window, out var host))
            throw new InvalidOperationException("No SukiHost present in this window");
        ToastPool.Return(host.ToastsCollection);
        Dispatcher.UIThread.Invoke(() => host.ToastsCollection.Clear());
    }

    /// <summary>
    /// Clears all active toasts in the earliest open window immediately.
    /// </summary>
    public static void ClearAllToasts() => ClearAllToasts(_mainWindow);

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        if (VisualRoot is not Window w) return;
        Instances.Remove(w);
        _mainWindow = Instances.FirstOrDefault().Key;
    }
}