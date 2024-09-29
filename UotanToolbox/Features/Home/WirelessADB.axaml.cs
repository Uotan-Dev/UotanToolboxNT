using Avalonia.Interactivity;
using SukiUI.Controls;

namespace UotanToolbox.Features.Home;

public partial class WirelessADB : SukiWindow
{
    public WirelessADB()
    {
        InitializeComponent();
    }

    private async void WConnect(object sender, RoutedEventArgs args)
    {
        IPAndPort.Text = "1111111";
        PairingCode.Text = "11111111";
    }
}