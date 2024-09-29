using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using SukiUI.Controls;
using System.IO;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Home;

public partial class WirelessADB : SukiWindow
{
    public static Bitmap ConvertToBitmap(byte[] imageData)
    {
        using (var stream = new MemoryStream(imageData))
        {
            return new Bitmap(stream);
        }
    }
    public WirelessADB()
    {
        InitializeComponent();
        string serviceID = "studio-" + StringHelper.RandomString(8);
        string password = StringHelper.RandomString(8);
        QRCode.Source = ConvertToBitmap(ADBPairHelper.QRCodeInit(serviceID, password));
    }

    private async void WConnect(object sender, RoutedEventArgs args)
    {
        IPAndPort.Text = "1111111";
        PairingCode.Text = "11111111";
    }
}