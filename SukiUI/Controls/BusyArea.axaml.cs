using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls;

public partial class BusyArea : UserControl
{
    public BusyArea()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static readonly StyledProperty<bool> IsBusyProperty =
        AvaloniaProperty.Register<BusyArea, bool>(nameof(IsBusy), defaultValue: false);

    public bool IsBusy
    {
        get { return GetValue(IsBusyProperty); }
        set { SetValue(IsBusyProperty, value); }
    }

    public static readonly StyledProperty<string?> BusyTextProperty =
        AvaloniaProperty.Register<BusyArea, string?>(nameof(BusyText), defaultValue: null);

    public string? BusyText
    {
        get => GetValue(BusyTextProperty);
        set => SetValue(BusyTextProperty, value);
    }
}