using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UotanToolbox.Features.Settings;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}