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
    public class Oneplus
    {
        public string fh { get; set; }
        public string projid { get; set; }
        public long serial { get; set; }
        public int ATOBuild { get; set; }
        public int Flash_Mode { get; set; }
        public int cf { get; set; }
        public Oneplus(string FH, string Projid, long Serial, int AtoBuild, int Flash_mode, int CF)
        {
            fh = FH; 
            projid = Projid; 
            serial = Serial; 
            ATOBuild = AtoBuild;
            Flash_Mode = Flash_mode;
            cf = CF;
        }
    }
    internal class Oneplus_One
    {
        public Dictionary<string, DeviceConfig> deviceConfig = new Dictionary<string, DeviceConfig>
        {
            // OP5, cheeseburger
            { "16859", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            // OP5t, dumpling
            { "17801", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            // OP6, enchilada
            { "17819", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            // OP6t, fajita
            { "18801", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            // OP6t T-Mo, fajitat
            { "18811", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            // Oneplus 7, guacamoleb
            { "18857", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            // Oneplus 7 Pro, guacamole
            { "18821", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            // Oneplus 7 Pro 5G Sprint, guacamoles
            { "18825", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            // Oneplus 7 Pro 5G EE and Elisa, guacamoleg
            { "18827", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            // Oneplus 7 Pro T-Mo, guacamolet
            { "18831", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            // Oneplus 7t, hotdogb
            { "18865", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            // Oneplus 7t T-Mo, hotdogt
            { "19863", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            // Oneplus 7t Pro, hotdog
            { "19801", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            // Oneplus 7t Pro 5G T-Mo, hotdogg
            { "19861", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },

            // OP8, instantnoodle
            { "19821", new DeviceConfig { Version = 2, Cm = "0cffee8a", ParamMode = 0 } },
            // OP8 T-Mo, instantnoodlet
            { "19855", new DeviceConfig { Version = 2, Cm = "6d9215b4", ParamMode = 0 } },
            // OP8 Verizon, instantnoodlev
            { "19867", new DeviceConfig { Version = 2, Cm = "4107b2d4", ParamMode = 0 } },
            // OP8 Visible, instantnoodlevis
            { "19868", new DeviceConfig { Version = 2, Cm = "178d8213", ParamMode = 0 } },
            // OP8 Pro, instantnoodlep
            { "19811", new DeviceConfig { Version = 2, Cm = "40217c07", ParamMode = 0 } },
            // OP8t, kebab
            { "19805", new DeviceConfig { Version = 2, Cm = "1a5ec176", ParamMode = 0 } },
            // OP8t T-Mo, kebabt
            { "20809", new DeviceConfig { Version = 2, Cm = "d6bc8c36", ParamMode = 0 } },

            // OP Nord, avicii
            { "20801", new DeviceConfig { Version = 2, Cm = "eacf50e7", ParamMode = 0 } },

            // OP N10 5G Metro, billie8t
            { "20885", new DeviceConfig { Version = 3, Cm = "3a403a71", ParamMode = 1 } },
            // OP N10 5G Global, billie8
            { "20886", new DeviceConfig { Version = 3, Cm = "b8bd9e39", ParamMode = 1 } },
            // billie8t, OP N10 5G TMO
            { "20888", new DeviceConfig { Version = 3, Cm = "142f1bd7", ParamMode = 1 } },
            // OP N10 5G Europe, billie8
            { "20889", new DeviceConfig { Version = 3, Cm = "f2056ae1", ParamMode = 1 } },

            // OP N100 Metro, billie2t
            { "20880", new DeviceConfig { Version = 3, Cm = "6ccf5913", ParamMode = 1 } },
            // OP N100 Global, billie2
            { "20881", new DeviceConfig { Version = 3, Cm = "fa9ff378", ParamMode = 1 } },
            // OP N100 TMO, billie2t
            { "20882", new DeviceConfig { Version = 3, Cm = "4ca1e84e", ParamMode = 1 } },
            // OP N100 Europe, billie2
            { "20883", new DeviceConfig { Version = 3, Cm = "ad9dba4a", ParamMode = 1 } },

            // OP9 Pro, lemonadep
            { "19815", new DeviceConfig { Version = 2, Cm = "9c151c7f", ParamMode = 0 } },
            { "20859", new DeviceConfig { Version = 2, Cm = "9c151c7f", ParamMode = 0 } },
            { "20857", new DeviceConfig { Version = 2, Cm = "9c151c7f", ParamMode = 0 } },
            // OP9, lemonade
            { "19825", new DeviceConfig { Version = 2, Cm = "0898dcd6", ParamMode = 0 } },
            { "20851", new DeviceConfig { Version = 2, Cm = "0898dcd6", ParamMode = 0 } },
            { "20852", new DeviceConfig { Version = 2, Cm = "0898dcd6", ParamMode = 0 } },
            { "20853", new DeviceConfig { Version = 2, Cm = "0898dcd6", ParamMode = 0 } },
            // OP9R, lemonades
            { "20828", new DeviceConfig { Version = 2, Cm = "f498b60f", ParamMode = 0 } },
            { "20838", new DeviceConfig { Version = 2, Cm = "f498b60f", ParamMode = 0 } },
            // OP9 TMO, lemonadet
            { "20854", new DeviceConfig { Version = 2, Cm = "16225d4e", ParamMode = 0 } },
            // OP9 Pro TMO, lemonadept
            { "2085A", new DeviceConfig { Version = 2, Cm = "7f19519a", ParamMode = 0 } },

            // dre8t
            { "20818", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            // dre8m
            { "2083C", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },
            // dre9
            { "2083D", new DeviceConfig { Version = 1, Cm = null, ParamMode = 0 } },

            // op nord ce, ebba
            { "20813", new DeviceConfig { Version = 2, Cm = "48ad7b61", ParamMode = 0 } }
        };
        private static string GeneratePK()
        {
            string val = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            StringBuilder pk = new StringBuilder();
            Random random = new Random();
            for (int i = 0; i < 16; i++)
            {
                int rand = random.Next(0, 0x100);
                int nr = (rand & 0xFF) % 0x3E;
                pk.Append(val[nr]);
            }
            return pk.ToString();
        }

        public static string CryptToken(byte[] data, string pk, bool decrypt = false, bool demacia = false)
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
    }
}
