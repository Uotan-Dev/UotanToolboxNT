using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls;

public partial class WaveProgress : UserControl
{
    public WaveProgress()
    {
        InitializeComponent();
        
        var theme = SukiTheme.GetInstance();
        theme.OnBaseThemeChanged += _ =>
        {
            Value++;
            Value--;
        };
        theme.OnColorThemeChanged += _ =>
        {
            Value++;
            Value--;
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<WaveProgress, double>(nameof(Value), defaultValue: 50);

    public double Value
    {
        get => GetValue(ValueProperty);
        set
        {
            if (value is >= 0 and <= 100)
                SetValue(ValueProperty, value);
        }
    }
    
    public static readonly StyledProperty<bool> IsTextVisibleProperty = AvaloniaProperty.Register<WaveProgress, bool>(nameof(IsTextVisible), defaultValue: true);

    public bool IsTextVisible
    {
        get => GetValue(IsTextVisibleProperty);
        set => SetValue(IsTextVisibleProperty, value);
    }
}