using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace UotanToolbox.Common
{
    public class DeviceConfig
    {
        public int Version { get; set; }
        public string Cm { get; set; }
        public int ParamMode { get; set; }
    }


    public class OnePlus(string ModelVerifyPrjName, int serial, int ATOBuild = 0, int Flash_Mode = 0, int cf = 0, string[] supported_functions = null)
    {
        private readonly Dictionary<string, DeviceConfig> _deviceConfigs = new Dictionary<string, DeviceConfig>
            {
            // OP5, cheeseburger
            {"16859", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // OP5t, dumpling
            {"17801", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // OP6, enchilada
            {"17819", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // OP6t, fajita
            {"18801", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // OP6t T-Mo, fajitat
            {"18811", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // Oneplus 7 Pro 5G Sprint, guacamoles
            {"18825", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // Oneplus 7 Pro 5G EE and Elisa, guacamoleg
            {"18827", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // Oneplus 7 Pro T-Mo, guacamolet
            {"18831", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // Oneplus 7t, hotdogb
            {"18865", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // Oneplus 7t T-Mo, hotdogt
            {"19863", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // Oneplus 7t Pro, hotdog
            {"19801", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // Oneplus 7t Pro 5G T-Mo, hotdogg
            {"19861", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // OP8, instantnoodle
            {"19821", new DeviceConfig {Version = 2, Cm = "0cffee8a", ParamMode = 0}},
            // OP8 T-Mo, instantnoodlet
            {"19855", new DeviceConfig {Version = 2, Cm = "0cffee8a", ParamMode = 0}},
            // OP8, instantnoodle
            {"19821", new DeviceConfig {Version = 2, Cm = "6d9215b4", ParamMode = 0}},
            // OP8 Verizon, instantnoodlev
            {"19867", new DeviceConfig {Version = 2, Cm = "4107b2d4", ParamMode = 0}},
            // OP8 Visible, instantnoodlevis
            {"19868", new DeviceConfig {Version = 2, Cm = "178d8213", ParamMode = 0}},
            // OP8 Pro, instantnoodlep
            {"19811", new DeviceConfig {Version = 2, Cm = "40217c07", ParamMode = 0}},
            // OP8t, kebab
            {"19805", new DeviceConfig {Version = 2, Cm = "1a5ec176", ParamMode = 0}},
            // OP8t T-Mo, kebabt
            {"20809", new DeviceConfig {Version = 2, Cm = "d6bc8c36", ParamMode = 0}},
            // OP9 Pro, lemonadep
            {"19815", new DeviceConfig {Version = 2, Cm = "9c151c7f", ParamMode = 0}},
            {"20859", new DeviceConfig {Version = 2, Cm = "9c151c7f", ParamMode = 0}},
            {"20857", new DeviceConfig {Version = 2, Cm = "9c151c7f", ParamMode = 0}},
            // OP9, lemonade
            {"19825", new DeviceConfig {Version = 2, Cm = "0898dcd6", ParamMode = 0}},
            {"20851", new DeviceConfig {Version = 2, Cm = "0898dcd6", ParamMode = 0}},
            {"20852", new DeviceConfig {Version = 2, Cm = "0898dcd6", ParamMode = 0}},
            {"20853", new DeviceConfig {Version = 2, Cm = "0898dcd6", ParamMode = 0}},
            // OP9R, lemonades
            {"20828", new DeviceConfig {Version = 2, Cm = "f498b60f", ParamMode = 0}},
            {"20838", new DeviceConfig {Version = 2, Cm = "f498b60f", ParamMode = 0}},
            // OP9 TMO, lemonadet
            {"20854", new DeviceConfig {Version = 2, Cm = "16225d4e", ParamMode = 0}},
            // OP9 Pro TMO, lemonadept
            {"2085A", new DeviceConfig {Version = 2, Cm = "7f19519a", ParamMode = 0}},
            // OP Nord, avicii
            {"20801", new DeviceConfig {Version = 2, Cm = "eacf50e7", ParamMode = 0}},
            // OP N10 5G Metro, billie8t
            {"20885", new DeviceConfig {Version = 3, Cm = "3a403a71", ParamMode = 1}},
            // OP N10 5G Global, billie8
            {"20886", new DeviceConfig {Version = 3, Cm = "b8bd9e39", ParamMode = 1}},
            // OP N10 5G TMO, billie8t
            {"20888", new DeviceConfig {Version = 3, Cm = "142f1bd7", ParamMode = 1}},
            // OP N10 5G Europe, billie8
            {"20889", new DeviceConfig {Version = 3, Cm = "f2056ae1", ParamMode = 1}},
            // OP N100 Metro, billie2t
            {"20880", new DeviceConfig {Version = 3, Cm = "6ccf5913", ParamMode = 1}},
            // OP N100 Global, billie2
            {"20881", new DeviceConfig {Version = 3, Cm = "fa9ff378", ParamMode = 1}},
            // OP N100 TMO, billie2t
            {"20882", new DeviceConfig {Version = 3, Cm = "4ca1e84e", ParamMode = 1}},
            // OP N100 Europe, billie2
            {"20883", new DeviceConfig {Version = 3, Cm = "ad9dba4a", ParamMode = 1}},
            // dre8t
            {"20818", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // dre8m
            {"2083C", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // dre9
            {"2083D", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // op nord ce, ebba
            {"20813", new DeviceConfig {Version = 2, Cm = "48ad7b61", ParamMode = 0}},
        };
        /// <summary>
        /// 根据设备代号来检索对应的算法序号，CM值与param模式
        /// </summary>
        /// <param name="ModelVerifyPrjName"></param>
        /// <returns>设备配置</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public DeviceConfig GetConfig(string ModelVerifyPrjName)
        {
            if (_deviceConfigs.TryGetValue(ModelVerifyPrjName, out DeviceConfig config))
            {
                return config;
            }
            else
            {
                throw new KeyNotFoundException($"The device configuration for key '{ModelVerifyPrjName}' was not found.");
            }
        }
        public string GetProdKey(int projId)
        {
            string prodKey;
            if (projId == 18825 || projId == 18801) // key_guacamoles, fajiita
            {
                prodKey = "b2fad511325185e5";
            }
            else // key_op7t/op8/N10
            {
                prodKey = "7016147d58e8c038";
            }
            return prodKey;
        }
        public class OnePlus1(string version, string ModelVerifyPrjName, int serial, string pk = "", string prodkey = "", string cf = "0")
        {
            private readonly string _pk = pk;
            private readonly string _prodkey = prodkey;
            private readonly string _ModelVerifyPrjName = ModelVerifyPrjName;
            private readonly string _randomPostfix = "0iyFR00pPnoqjVNL";
            private readonly string _Version = version;
            private readonly string _cf = cf;
            private readonly string _socSn = serial.ToString();
            public static byte[] StringToByteArray(string hex)
            {
                int NumberChars = hex.Length;
                byte[] bytes = new byte[NumberChars / 2];
                for (int i = 0; i < NumberChars; i += 2)
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                return bytes;
            }
            public static string CryptToken(string data, string pk, bool decrypt = false, bool demacia = false)
            {
                byte[] aesKey, aesIV;

                if (demacia)
                {
                    aesKey = [0x01, 0x63, 0xA0, 0xD1, 0xFD, 0xE2, 0x67, 0x11, .. Encoding.UTF8.GetBytes(pk), .. new byte[] { 0x48, 0x27, 0xC2, 0x08, 0xFB, 0xB0, 0xE6, 0xF0 }];
                    aesIV = [0x96, 0xE0, 0x79, 0x0C, 0xAE, 0x2B, 0xB4, 0xAF, 0x68, 0x4C, 0x36, 0xCB, 0x0B, 0xEC, 0x49, 0xCE];
                }
                else
                {
                    aesKey = [0x10, 0x45, 0x63, 0x87, 0xE3, 0x7E, 0x23, 0x71, .. Encoding.UTF8.GetBytes(pk), .. new byte[] { 0xA2, 0xD4, 0xA0, 0x74, 0x0f, 0xD3, 0x28, 0x96 }];
                    aesIV = [0x9D, 0x61, 0x4A, 0x1E, 0xAC, 0x81, 0xC9, 0xB2, 0xD3, 0x76, 0xD7, 0x49, 0x31, 0x03, 0x63, 0x79];
                }

                if (decrypt)
                {
                    byte[] cdata = CryptoHelper.Unhexlify(data);
                    string hexString = "3930376865617679776f726b6c6f61644e0260e5250c941b413dba383d7532f01a4a287b0c2e83410f694b9d96e9509d";
                    cdata = StringToByteArray(hexString);
                    byte[] result = CryptoHelper.AESCBC(cdata, aesKey, aesIV, true);
                    Console.WriteLine(result);
                    result = result.SkipWhile(b => b == 0).ToArray();
                    if (result.Length >= 16 && result.Take(16).SequenceEqual(Encoding.UTF8.GetBytes("907heavyworkload")))
                    {
                        return Encoding.UTF8.GetString(result);
                    }
                    else
                    {
                        return string.Join(",", Encoding.UTF8.GetString(result).Split(','));
                    }
                }
                else
                {
                    byte[] pdata = Encoding.UTF8.GetBytes(data.PadRight(256, demacia ? '\0' : ' '));
                    byte[] result = CryptoHelper.AESCBC(pdata, aesKey, aesIV, false);
                    string rdata = BitConverter.ToString(result).Replace("-", "").ToUpper();
                    return rdata;
                }
            }
            public static (string, string) GenerateToken(string prodKey, string modelVerifyPrjName, string randomPostfix, string cf, string socSn, string version, bool program = false)
            {
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                // 计算ModelVerifyHashToken
                var h1 = $"{prodKey}{modelVerifyPrjName}{randomPostfix}";
                var modelVerifyHashTokenBytes = SHA256.HashData(Encoding.UTF8.GetBytes(h1));
                var modelVerifyHashToken = BitConverter.ToString(modelVerifyHashTokenBytes).Replace("-", "").ToUpper();
                // 计算secret
                var h2 = $"c4b95538c57df231{modelVerifyPrjName}{cf}{socSn}{version}{timestamp}{modelVerifyHashToken}5b0217457e49381b";
                var secretBytes = SHA256.HashData(Encoding.UTF8.GetBytes(h2));
                var secret = BitConverter.ToString(secretBytes).Replace("-", "").ToUpper();
                var items = program
                    ? new[] { timestamp, secret }
                    : new[] { modelVerifyPrjName, randomPostfix, modelVerifyHashToken, version, cf, socSn, timestamp, secret };
                var data = string.Join(",", items);
                var token = CryptToken(data, prodKey);
                return (prodKey, token);
            }
        }
        /*
        public class Program
        {
            public static int Main(string[] args)
            {
                string pk = "l8TEcLb3nFPmOE2O";
                string ModelVerifyPrjName = "18821";
                string socSn = "753799659";
                string prodkey;
                string version = "guacamoles_21_O.22_191107";
                if (args.Length == 3)
                {
                    version = args[2];
                }
                if (new[] { "18825", "18801" }.Contains(ModelVerifyPrjName)) // key_guacamoles, fajiita
                {
                    prodkey = "b2fad511325185e5";
                }
                else // key_op7t/op8/N10
                {
                    prodkey = "7016147d58e8c038";
                }
                OnePlus1 onePlus1 = new OnePlus1(version, ModelVerifyPrjName, Convert.ToInt32(socSn), pk, prodkey);
                (string token, string secret) = onePlus1.GenerateToken(prodkey, ModelVerifyPrjName, "0iyFR00pPnoqjVNL", "0", socSn, version, false);
                Console.WriteLine($"pk: {pk}");
                Console.WriteLine($"Secret: {secret}");
                Console.WriteLine($"version: {version}");
                Console.WriteLine($"Token: {token}");
                return 0;
            }
        }*/

    }
}




