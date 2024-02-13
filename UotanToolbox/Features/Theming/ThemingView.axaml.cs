using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UotanToolbox.Features.Theming;

public partial class ThemingView : UserControl
{
    public ThemingView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}