using Avalonia.Controls.Notifications;
using QRCoder;
using SukiUI.Dialogs;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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


        //Todo:使用原生Zeroconf做网络mdns扫描
        public static async Task ScanmDNS(string serviceID, string password, ISukiDialogManager dialogManager)
        {
            while (true)
            {
                string result = await CallExternalProgram.ADB("mdns services");
                if (result.Contains("List of discovered mdns services"))
                {
                    var lineRegex = "([^\\t]+)\\t*_adb-tls-pairing._tcp.\\t*([^:]+):([0-9]+)";
                    var match = Regex.Match(result, lineRegex);
                    string deviceIP = match.Groups[2].Value;
                    if (match.Success)
                    {
                        result = await CallExternalProgram.ADB($"pair {match.Groups[2].Value}:{match.Groups[3].Value} {password}");
                        if (result.Contains("Successfully paired to "))
                        {
                            dialogManager.CreateDialog().WithTitle(GetTranslation("Common_Succ")).OfType(NotificationType.Success).WithContent(GetTranslation("WirelessADB_Connect")).Dismiss().ByClickingBackground().TryShow();
                        }
                    }
                }
                await Task.Delay(1000);
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

