using Avalonia.Controls;
using Avalonia.Threading;
using Makaretu.Dns;
using QRCoder;
using SukiUI.Controls;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UotanToolbox.Features.Components;
using UotanToolbox.Features.Home;

namespace UotanToolbox.Common
{

    internal class ADBPairHelper
    {
        private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
        public static byte[] QRCodeInit(string serviceID, string password)
        {
            string QRData = "WIFI:T:ADB;S:" + serviceID + ";P:" + password + ";;";
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(QRData, QRCodeGenerator.ECCLevel.H))
            using (BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData))
            {
                return qrCode.GetGraphic(20);
            }
        }

        public static async Task ScanmDNS(string serviceID, string password, Window window)
        {
            string ip;
            int port = 5554;
            string target = null;
            var mdns = new MulticastService();
            var sd = new ServiceDiscovery(mdns);
            mdns.AnswerReceived += (s, e) =>
            {
                var servers = e.Message.Answers.OfType<SRVRecord>();
                foreach (var server in servers)
                {
                    if (server.Name.ToString().Contains(serviceID))
                    {
                        target = server.Target.ToString();
                        port = server.Port;
                    }
                    //Console.WriteLine($"host '{server.Target}' for '{server.Name}' {server.Port}");
                    mdns.SendQuery(server.Target, type: DnsType.A);
                }
                var addresses = e.Message.Answers.OfType<AddressRecord>();
                foreach (var address in addresses)
                {
                    if (address.Name.ToString() == target && StringHelper.IsIPv4(address.Address.ToString()))
                    {
                        string result = CallExternalProgram.ADB($"pair {address.Address}:{port} {password}").Result;
                        if (result.Contains("Successfully paired to "))
                        {
                            SukiHost.ShowDialog(window, new PureDialog(GetTranslation("WirelessADB_Connect")), allowBackgroundClose: true);
                        }
                    }
                    //Console.WriteLine($"host '{address.Name}' at {address.Address}");
                }
            };
            try
            {
                mdns.Start();
            }
            finally
            {
                sd.Dispose();
                mdns.Stop();
            }
        }
        public static async Task<bool> Pair(string input, string password)
        {
            string result = await CallExternalProgram.ADB($"pair {input} {password}");
            if (result.Contains("Successfully paired to "))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //string serviceID = "studio-" + StringHelper.RandomString(8);
        //string password = StringHelper.RandomString(8);
        //QRCodeInit(serviceID, password);
    }
}

