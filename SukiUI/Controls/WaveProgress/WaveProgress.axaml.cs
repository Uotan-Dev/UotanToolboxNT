using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
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

    private double _value = 50;

    public double Value
    {
        get => _value;
        set
        {
            if (value is < 0 or > 100)
                return;

            SetValue(ValueProperty,value);
        }
    }

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<CircleProgressBar, double>(nameof(Value), defaultValue: 50);
    
   

    public static readonly StyledProperty<bool> IsTextVisibleProperty = AvaloniaProperty.Register<WaveProgress, bool>(nameof(IsTextVisible), defaultValue: true);

    public bool IsTextVisible
    {
        get { return GetValue(IsTextVisibleProperty); }
        set
        {
            SetValue(IsTextVisibleProperty, value);
        }
    }
}