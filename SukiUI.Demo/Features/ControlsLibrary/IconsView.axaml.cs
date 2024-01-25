using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using SukiUI.Controls;

namespace SukiUI.Demo.Features.ControlsLibrary;

public partial class IconsView : UserControl
{
    public IconsView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (VisualRoot is not Window win) return;
        if (sender is not Control control) return;
        if (control.DataContext is not KeyValuePair<string, StreamGeometry> kvp) return;
        win.Clipboard!.SetTextAsync($"<PathIcon Data=\"{{x:Static content:Icons.{kvp.Key}}}\" />");
        SukiHost.ShowToast("Copied To Clipboard", $"Copied the XAML for {kvp.Key} to clipboard.");
    }
}