using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace UotanToolbox.Common.QcomHelper.modules
{
    public class DeviceConfig
    {
        public int Version { get; set; }
        public string Cm { get; set; }
        public int ParamMode { get; set; }
    }
    internal class Oneplus
    {
        public string Fh { get; set; }
        public string Projid { get; set; }
        public int Version { get; set; }
        public int Serial { get; set; }
        public int ATOBuild { get; set; }
        public int FlashMode { get; set; }
        public int Cf { get; set; }
        public string[] Supported_functions { get; set; }

        public Oneplus(string fh, string projid, int version, int serial, int atoBuild, int flashMode, int cf, string[] supported_functions)
        {
            Fh = fh;
            Version = version;
            Projid = projid;
            Serial = serial;
            ATOBuild = atoBuild;
            FlashMode = flashMode;
            Cf = cf;
            Supported_functions = supported_functions;
        }
        public static Dictionary<string, DeviceConfig> DeviceConfig { get; } = new Dictionary<string, DeviceConfig>
        {
            { "16859", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            { "17801", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            { "17819", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            { "18801", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            { "18811", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            { "18857", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            { "18821", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            { "18825", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            { "18827", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            { "18831", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            { "18865", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            { "19863", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            { "19801", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            { "19861", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            { "19821", new DeviceConfig { Version = 2, Cm = "0cffee8a", ParamMode = 0 } },
            { "19855", new DeviceConfig { Version = 2, Cm = "6d9215b4", ParamMode = 0 } },
            { "19867", new DeviceConfig { Version = 2, Cm = "4107b2d4", ParamMode = 0 } },
            { "19868", new DeviceConfig { Version = 2, Cm = "178d8213", ParamMode = 0 } },
            { "19811", new DeviceConfig { Version = 2, Cm = "40217c07", ParamMode = 0 } },
            { "19805", new DeviceConfig { Version = 2, Cm = "1a5ec176", ParamMode = 0 } },
            { "20809", new DeviceConfig { Version = 2, Cm = "d6bc8c36", ParamMode = 0 } },
            { "20801", new DeviceConfig { Version = 2, Cm = "eacf50e7", ParamMode = 0 } },
            { "20885", new DeviceConfig { Version = 3, Cm = "3a403a71", ParamMode = 1 } },
            { "20886", new DeviceConfig { Version = 3, Cm = "b8bd9e39", ParamMode = 1 } },
            { "20888", new DeviceConfig { Version = 3, Cm = "142f1bd7", ParamMode = 1 } },
            { "20889", new DeviceConfig { Version = 3, Cm = "f2056ae1", ParamMode = 1 } },
            { "20880", new DeviceConfig { Version = 3, Cm = "6ccf5913", ParamMode = 1 } },
            { "20881", new DeviceConfig { Version = 3, Cm = "fa9ff378", ParamMode = 1 } },
            { "20882", new DeviceConfig { Version = 3, Cm = "4ca1e84e", ParamMode = 1 } },
            { "20883", new DeviceConfig { Version = 3, Cm = "ad9dba4a", ParamMode = 1 } },
            { "19815", new DeviceConfig { Version = 2, Cm = "9c151c7f", ParamMode = 0 } },
            { "20859", new DeviceConfig { Version = 2, Cm = "9c151c7f", ParamMode = 0 } },
            { "20857", new DeviceConfig { Version = 2, Cm = "9c151c7f", ParamMode = 0 } },
            { "19825", new DeviceConfig { Version = 2, Cm = "0898dcd6", ParamMode = 0 } },
            { "20851", new DeviceConfig { Version = 2, Cm = "0898dcd6", ParamMode = 0 } },
            { "20852", new DeviceConfig { Version = 2, Cm = "0898dcd6", ParamMode = 0 } },
            { "20853", new DeviceConfig { Version = 2, Cm = "0898dcd6", ParamMode = 0 } },
            { "20828", new DeviceConfig { Version = 2, Cm = "f498b60f", ParamMode = 0 } },
            { "20838", new DeviceConfig { Version = 2, Cm = "f498b60f", ParamMode = 0 } },
            { "20854", new DeviceConfig { Version = 2, Cm = "16225d4e", ParamMode = 0 } },
            { "2085A", new DeviceConfig { Version = 2, Cm = "7f19519a", ParamMode = 0 } },
            { "20818", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            { "2083C", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            { "2083D", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            { "20813", new DeviceConfig { Version = 2, Cm = "48ad7b61", ParamMode = 0 } }
        };
        public static Oneplus Init(string projid, int serial, int atoBuild = 0, int flashMode = 0, int cf = 0)
        {
            Oneplus oneplus = new Oneplus("", "18825", 1, 123456, 0, 0, 0, null);
            oneplus.Projid = projid;
            oneplus.Serial = serial;
            DeviceConfig.TryGetValue(projid, out DeviceConfig deviceConfig);
            oneplus.Version = deviceConfig.Version;
            oneplus.Cf = cf;
            oneplus.FlashMode = flashMode;
            oneplus.ATOBuild = atoBuild;
            if (deviceConfig.Cm != null)
            {
                oneplus.Projid = deviceConfig.Cm;
            }
            return oneplus;
        }
        private static string GeneratePK()
        {
            const string val = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var pk = new StringBuilder(16);
            var random = new Random();
            for (int i = 0; i < 16; i++)
            {
                int nr = random.Next(val.Length);
                pk.Append(val[nr]);
            }
            return pk.ToString();
        }

        public static string CryptToken1(byte[] data, string pk, bool decrypt = false, bool demacia = false)
        {
            using (Aes aes = Aes.Create())
            {
                byte[] aesKey;
                byte[] aesIv;
                if (demacia)
                {
                    aesKey = CryptoHelper.Combine(new byte[] { 0x01, 0x63, 0xA0, 0xD1, 0xFD, 0xE2, 0x67, 0x11 }, Encoding.UTF8.GetBytes(pk), new byte[] { 0x48, 0x27, 0xC2, 0x08, 0xFB, 0xB0, 0xE6, 0xF0 });
                    aesIv = new byte[] { 0x96, 0xE0, 0x79, 0x0C, 0xAE, 0x2B, 0xB4, 0xAF, 0x68, 0x4C, 0x36, 0xCB, 0x0B, 0xEC, 0x49, 0xCE };
                }
                else
                {
                    aesKey = CryptoHelper.Combine(new byte[] { 0x10, 0x45, 0x63, 0x87, 0xE3, 0x7E, 0x23, 0x71 }, Encoding.UTF8.GetBytes(pk), new byte[] { 0xA2, 0xD4, 0xA0, 0x74, 0x0F, 0xD3, 0x28, 0x96 });
                    aesIv = new byte[] { 0x9D, 0x61, 0x4A, 0x1E, 0xAC, 0x81, 0xC9, 0xB2, 0xD3, 0x76, 0xD7, 0x49, 0x31, 0x03, 0x63, 0x79 };
                }

                if (decrypt)
                {
                    byte[] cdata = CryptoHelper.HexStringToByteArray(BitConverter.ToString(data).Replace("-", "").ToUpper());
                    byte[] result = CryptoHelper.AesCbcDecrypt(aesKey, aesIv, cdata);
                    result = result.TakeWhile(b => b != 0).ToArray();

                    if (Encoding.UTF8.GetString(result.Take(16).ToArray()) == "907heavyworkload")
                    {
                        return BitConverter.ToString(result).Replace("-", "").ToUpper();
                    }
                    else
                    {
                        return Encoding.UTF8.GetString(result).Split(',').ToString();
                    }
                }
                else
                {
                    if (!demacia)
                    {
                        if (data.Length < 256)
                        {
                            Array.Resize(ref data, 256);
                        }
                    }
                    else
                    {
                        List<byte> dataList = new List<byte>(data);
                        while (dataList.Count < 256)
                        {
                            dataList.Add(0x00);
                        }
                        data = dataList.ToArray();
                    }

                }
                byte[] encryptedData = CryptoHelper.Encrypt(aes, aesKey, aesIv, data);
                return BitConverter.ToString(encryptedData).Replace("-", "").ToUpper();
            }
        }

        public static string CryptToken2(byte[] data, string pk, bool decrypt = false, bool demacia = false)
        {
            using (Aes aes = Aes.Create())
            {
                byte[] aesKey;
                byte[] aesIv;
                if (demacia)
                {
                    aesKey = CryptoHelper.Combine(new byte[] { 0x01, 0x63, 0xA0, 0xD1, 0xFD, 0xE2, 0x67, 0x11 }, Encoding.UTF8.GetBytes(pk), new byte[] { 0x48, 0x27, 0xC2, 0x08, 0xFB, 0xB0, 0xE6, 0xF0 });
                    aesIv = new byte[] { 0x96, 0xE0, 0x79, 0x0C, 0xAE, 0x2B, 0xB4, 0xAF, 0x68, 0x4C, 0x36, 0xCB, 0x0B, 0xEC, 0x49, 0xCE };
                }
                else
                {
                    aesKey = CryptoHelper.Combine(new byte[] { 0x10, 0x45, 0x63, 0x87, 0xE3, 0x7E, 0x23, 0x71 }, Encoding.UTF8.GetBytes(pk), new byte[] { 0xA2, 0xD4, 0xA0, 0x74, 0x0F, 0xD3, 0x28, 0x96 });
                    aesIv = new byte[] { 0x9D, 0x61, 0x4A, 0x1E, 0xAC, 0x81, 0xC9, 0xB2, 0xD3, 0x76, 0xD7, 0x49, 0x31, 0x03, 0x63, 0x79 };
                }

                if (decrypt)
                {
                    byte[] cdata = CryptoHelper.HexStringToByteArray(BitConverter.ToString(data).Replace("-", "").ToUpper());
                    byte[] result = CryptoHelper.AesCbcDecrypt(aesKey, aesIv, cdata);
                    result = result.TakeWhile(b => b != 0).ToArray();

                    if (Encoding.UTF8.GetString(result.Take(16).ToArray()) == "907heavyworkload")
                    {
                        return BitConverter.ToString(result).Replace("-", "").ToUpper();
                    }
                    else
                    {
                        return Encoding.UTF8.GetString(result).Split(',').ToString();
                    }
                }
                else
                {
                    if (!demacia)
                    {
                        if (data.Length < 256)
                        {
                            Array.Resize(ref data, 256);
                        }
                    }
                    else
                    {
                        List<byte> dataList = new List<byte>(data);
                        while (dataList.Count < 256)
                        {
                            dataList.Add(0x00);
                        }
                        data = dataList.ToArray();
                    }

                }
                byte[] encryptedData = CryptoHelper.Encrypt(aes, aesKey, aesIv, data);
                return BitConverter.ToString(encryptedData).Replace("-", "").ToUpper();
            }
        }
        public static string generatetoken1(string prodkey, string ModelVerifyPrjName, string random_postfix, string Version, int cf, string soc_sn, string timestamp, string pk, bool program = false)
        {
            string h1 = prodkey + ModelVerifyPrjName + random_postfix;
            string ModelVerifyHashToken = CryptoHelper.ComputeSha256Hash(h1).ToUpper();
            string h2 = "c4b95538c57df231" + ModelVerifyPrjName + cf.ToString() + soc_sn + Version + timestamp + ModelVerifyHashToken + "5b0217457e49381b";
            Console.WriteLine(h2);
            string secret = CryptoHelper.ComputeSha256Hash(h2).ToUpper();
            if (program)
            {
                string[] items = { timestamp, secret };
                string data = string.Join(",", items);
                string token = CryptToken1(Encoding.UTF8.GetBytes(data), pk);
                return token;
            }
            else
            {
                string[] items = { ModelVerifyPrjName, random_postfix, ModelVerifyHashToken, Version, cf.ToString(), soc_sn, timestamp, secret };
                string data = string.Join(",", items);
                string token = CryptToken1(Encoding.UTF8.GetBytes(data), pk);
                return token;
            }
        }
    }
}
