using Avalonia.Controls;
using Avalonia.Interactivity;
using UotanToolbox.Common;
using UotanToolbox.Features.Home;


namespace UotanToolbox.Features.Rom;

public partial class RomView : UserControl
{
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public RomView()
    {
        InitializeComponent();
    }

    private void OpenProject(object sender, RoutedEventArgs args) => new RomProject().Show();
}