using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UotanToolbox.Features.About;

public partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}