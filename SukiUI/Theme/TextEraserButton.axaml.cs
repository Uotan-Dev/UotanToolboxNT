using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SukiUI.Theme;

public partial class TextEraserButton : UserControl
{
    public TextEraserButton()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<TextEraserButton, string>(nameof(Text), defaultValue: "");

    public string Text
    {
        get { return GetValue(TextProperty); }
        set
        {
            SetValue(TextProperty, value);
        }
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        Text = "_";
        Text = "";
    }
}