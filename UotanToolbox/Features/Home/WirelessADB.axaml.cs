using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Microsoft.VisualBasic;
using SukiUI.Controls;
using System;
using System.IO;
using System.Text.RegularExpressions;
using UotanToolbox.Common;

using UotanToolbox.Features.Components;


namespace UotanToolbox.Features.Home;

public partial class WirelessADB : SukiWindow
{
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public static Bitmap ConvertToBitmap(byte[] imageData)
    {
        using (var stream = new MemoryStream(imageData))
        {
            return new Bitmap(stream);
        }
    }
    public  WirelessADB()
    {
        InitializeComponent();
        QRCode.Source = ConvertToBitmap(ADBPairHelper.QRCodeInit(Global.serviceID, Global.password));
    }
    private async void WConnect(object sender, RoutedEventArgs args)
    {
        string input = IPAndPort.Text;
        string password = PairingCode.Text;
        if (string.IsNullOrEmpty(input) && string.IsNullOrEmpty(password))
        {
            string result = await CallExternalProgram.ADB($"pair {input} {password}");
            if (result.Contains("Successfully paired to "))
            {
                SukiHost.ShowDialog(this, new PureDialog(GetTranslation("WirelessADB_Connect")), allowBackgroundClose: true);
            }
            else
            {
                SukiHost.ShowDialog(this, new PureDialog(result), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(this, new PureDialog(GetTranslation("Common_EnterAll")), allowBackgroundClose: true);
        }
    }
    private async void TWConnect(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
            if (sukiViewModel.Status == GetTranslation("Home_System"))
            {
                string output = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell ip addr show to 0.0.0.0/0 scope global");
                string pattern = @"inet\s+(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})/";

                Match match = Regex.Match(output, pattern);

                if (!match.Success)
                {
                    SukiHost.ShowDialog(this, new PureDialog(output), allowBackgroundClose: true);
                    return;
                }
                output = await CallExternalProgram.ADB($"-s {Global.thisdevice} tcpip 5555");
                if (output.Contains("restarting"))
                {
                    string output2 = await CallExternalProgram.ADB($"connect {match.Groups[1].Value}");
                    if (output2.Contains("connected"))
                    {
                        SukiHost.ShowDialog(this, new PureDialog(GetTranslation("WirelessADB_Connect")), allowBackgroundClose: true);
                    }
                    else
                    {
                        SukiHost.ShowDialog(this, new PureDialog(output + "\n" + output2), allowBackgroundClose: true);
                    }
                }
            }
            else
            {
                SukiHost.ShowDialog(this, new PureDialog(GetTranslation("Common_OpenADB")), allowBackgroundClose: true);
            }
        }
        else
        {
            SukiHost.ShowDialog(this, new PureDialog(GetTranslation("Common_NotConnected")), allowBackgroundClose: true);
        }
    }
}