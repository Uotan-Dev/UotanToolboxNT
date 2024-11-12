using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UotanToolbox.Common.ROMHelper
{
    internal class Ofp_QC_Helper
    {
        private static byte[] FromHex(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException("Hex string must have an even length");
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        // {version , key , iv}
        private static readonly string[] Array0 = ["V1.4.17/1.4.27", "64313534616665656161666139353866", "32633034306635373836383239323037"];
        private static readonly string[] Array1 = ["V1.6.17", "32653936643766343632353931613066", "31376363363332323463323038373038"];
        private static readonly string[] Array2 = ["V1.5.13", "39346436326538333163663161316130", "37616235653333626435306438316361"];
        private static readonly string[] Array3 = ["V1.6.6/1.6.9/1.6.17/1.6.24/1.6.26/1.7.6", "34613833373232396536666337376434", "30306265643437623830656563396437"];
        private static readonly string[] Array4 = ["V1.7.2", "33333938363939616365626461306461", "62333961343666356363346630643435"];
        private static readonly string[] Array5 = ["V2.0.3", "62346237333538656561323230393931", "65393037376532366162313032643162"];

        private static (int pageSize, byte[] key, byte[] iv, byte[] data) GenerateKey2(string filename)
        {
            var keys = new List<string[]>
        {
            Array0,
            Array1,
            Array2,
            Array3,
            Array4,
            Array5
        };

            foreach (var dkey in keys)
            {
                byte[] key = FromHex(dkey[1]);

                byte[] iv = FromHex(dkey[2]);

                Console.WriteLine($"trying {dkey[0]} Key: {BitConverter.ToString(key)} IV: {BitConverter.ToString(iv)}");
                var (pageSize, data) = ExtractXml(filename, key, iv);
                if (pageSize != 0)
                {
                    return (pageSize, key, iv, data);
                }
            }
            return (0, null, null, null);
        }
        private static byte[] Deobfuscate(byte[] data, byte[] mask)
        {
            byte[] ret = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                byte v = (byte)ROL((uint)(data[i] ^ mask[i]), 4, 8);
                ret[i] = v;
            }
            return ret;
        }
        private static uint ROL(uint x, int n, int bits = 32)
        {
            n = bits - n;
            uint mask = (uint)((1 << n) - 1);
            uint maskBits = x & mask;
            return (x >> n) | (maskBits << (bits - n));
        }/*
    
    private static byte[] AesCfbDecrypt(byte[] data, byte[] key, byte[] iv)
    {
        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CFB;
        aes.Padding = PaddingMode.None;
        using ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        return decryptor.TransformFinalBlock(data, 0, data.Length);
    }*/

        private static (int pagesize, byte[] data) ExtractXml(string filename, byte[] key, byte[] iv)
        {
            long filesize = new FileInfo(filename).Length;
            using FileStream rf = new(filename, FileMode.Open, FileAccess.Read);
            int pagesize = 512;
            foreach (int x in new int[] { 0x200, 0x1000 })
            {
                rf.Seek(filesize - x + 0x10, SeekOrigin.Begin);
                byte[] buffer = new byte[4];
                rf.Read(buffer, 0, 4);
                if (BitConverter.ToUInt32(buffer, 0) == 0x7CEF)
                {
                    pagesize = x;
                    break;
                }
            }
            if (pagesize == 0)
            {
                Console.WriteLine("Unknown pagesize. Aborting");
                Environment.Exit(0);
            }
            long xmloffset = filesize - pagesize;
            Console.WriteLine(xmloffset);
            rf.Seek(xmloffset + 0x14, SeekOrigin.Begin);
            byte[] offsetBuffer = new byte[4];
            rf.Read(offsetBuffer, 0, 4);
            long temp = BitConverter.ToInt32(offsetBuffer, 0);
            long offset = temp * pagesize;
            rf.Seek(xmloffset + 0x18, SeekOrigin.Begin);
            byte[] lengthBuffer = new byte[4];
            rf.Read(lengthBuffer, 0, 4);
            int length = BitConverter.ToInt32([.. lengthBuffer], 0);
            if (length < 200) // A57 hack
            {
                length = (int)(xmloffset - offset - 0x57);
            }
            rf.Seek(offset, SeekOrigin.Begin);
            byte[] data = new byte[length];
            rf.Read(data, 0, length);
            byte[] dec = AesCfbDecrypt(data, key, iv);
            Console.WriteLine(Convert.ToHexString(dec));
            if (Encoding.UTF8.GetString(dec).Contains("<?xml"))
            {
                return (pagesize, dec);
            }
            else
            {
                return (0, []);
            }
        }

        //主要问题就是这个函数，我不知道怎么改。C#好像不支持指定块大小的解密
        private static byte[] AesCfbDecrypt(byte[] data, byte[] key, byte[] iv)
        {
            using Aes aes = Aes.Create();
            Console.WriteLine(data.Length);
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CFB;
            aes.Padding = PaddingMode.None;
            using ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using MemoryStream ms = new(data);
            using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
            byte[] decrypted = new byte[data.Length];
            cs.Read(decrypted, 0, decrypted.Length);
            return decrypted;
        }
        private static long CopySub(FileStream rf, FileStream wf, long start, long length)
        {
            rf.Seek(start, SeekOrigin.Begin);
            long rlen = 0;
            byte[] buffer = new byte[0x100000];

            while (length > 0)
            {
                int size = (int)Math.Min(length, buffer.Length);
                int bytesRead = rf.Read(buffer, 0, size);
                wf.Write(buffer, 0, bytesRead);
                rlen += bytesRead;
                length -= bytesRead;
            }

            return rlen;
        }

        private static void Copy(string filename, string wfilename, string path, long start, long length, string[] checksums)
        {
            Console.WriteLine($"\nExtracting {wfilename}");
            string outputPath = Path.Combine(path, wfilename);

            using (FileStream rf = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (FileStream wf = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    rf.Seek(start, SeekOrigin.Begin);
                    byte[] data = new byte[length];
                    rf.Read(data, 0, (int)length);
                    wf.Write(data, 0, data.Length);
                }
            }

            CheckHashFile(outputPath, checksums, true);
        }

        private static void DecryptFile(byte[] key, byte[] iv, string filename, string path, string wfilename, long start, long length, long rlength, string[] checksums, int decryptsize = 0x40000)
        {
            Console.WriteLine($"\nExtracting {wfilename}");
            if (rlength == length)
            {
                long tlen = length;
                length = (length / 0x4 * 0x4);
                if (tlen % 0x4 != 0)
                {
                    length += 0x4;
                }
            }

            string outputPath = Path.Combine(path, wfilename);

            using (FileStream rf = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (FileStream wf = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    rf.Seek(start, SeekOrigin.Begin);
                    int size = decryptsize;
                    if (rlength < decryptsize)
                    {
                        size = (int)rlength;
                    }
                    byte[] data = new byte[size];
                    rf.Read(data, 0, size);
                    if (size % 4 != 0)
                    {
                        Array.Resize(ref data, size + (4 - (size % 4)));
                    }
                    byte[] outp = AesCfbDecrypt(data, key, iv);
                    wf.Write(outp, 0, size);

                    if (rlength > decryptsize)
                    {
                        CopySub(rf, wf, start + size, rlength - size);
                    }

                    if (rlength % 0x1000 != 0)
                    {
                        byte[] fill = new byte[0x1000 - (rlength % 0x1000)];
                        // wf.Write(fill, 0, fill.Length);
                    }
                }
            }

            CheckHashFile(outputPath, checksums, false);
        }

        private static void CheckHashFile(string wfilename, string[] checksums, bool isCopy)
        {
            string sha256sum = checksums[0];
            string md5sum = checksums[1];
            string prefix = isCopy ? "Copy: " : "Decrypt: ";

            using (FileStream rf = new FileStream(wfilename, FileMode.Open, FileAccess.Read))
            {
                long size = new FileInfo(wfilename).Length;
                using (MD5 md5 = MD5.Create())
                {
                    byte[] md5Hash = md5.ComputeHash(rf);
                    rf.Seek(0, SeekOrigin.Begin);

                    bool sha256bad = false;
                    bool md5bad = false;
                    string md5status = "empty";
                    string sha256status = "empty";

                    if (!string.IsNullOrEmpty(sha256sum))
                    {
                        foreach (long x in new long[] { 0x40000, size })
                        {
                            rf.Seek(0, SeekOrigin.Begin);
                            using (SHA256 sha256 = SHA256.Create())
                            {
                                if (x == 0x40000)
                                {
                                    byte[] buffer = new byte[x];
                                    rf.Read(buffer, 0, buffer.Length);
                                    sha256.TransformFinalBlock(buffer, 0, buffer.Length);
                                }
                                else if (x == size)
                                {
                                    byte[] buffer = new byte[128 * sha256.HashSize];
                                    int bytesRead;
                                    while ((bytesRead = rf.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        sha256.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                                    }
                                    sha256.TransformFinalBlock(buffer, 0, 0);
                                }

                                if (!sha256sum.Equals(BitConverter.ToString(sha256.Hash).Replace("-", ""), StringComparison.InvariantCultureIgnoreCase))
                                {
                                    sha256bad = true;
                                    sha256status = "bad";
                                }
                                else
                                {
                                    sha256status = "verified";
                                    break;
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(md5sum))
                    {
                        if (md5sum != BitConverter.ToString(md5Hash).Replace("-", "").ToLowerInvariant())
                        {
                            md5bad = true;
                            md5status = "bad";
                        }
                        else
                        {
                            md5status = "verified";
                        }
                    }

                    if ((sha256bad && md5bad) || (sha256bad && string.IsNullOrEmpty(md5sum)) || (md5bad && string.IsNullOrEmpty(sha256sum)))
                    {
                        Console.WriteLine($"{prefix}error on hashes. File might be broken!");
                    }
                    else
                    {
                        Console.WriteLine($"{prefix}success! (md5: {md5status} | sha256: {sha256status})");
                    }
                }
            }
        }
        private static (string wfilename, long start, long length, long rlength, string[] checksums, int decryptsize) DecryptItem(XElement item, int pagesize)
        {
            string sha256sum = "";
            string md5sum = "";
            string wfilename = "";
            long start = -1;
            long rlength = 0;
            int decryptsize = 0x40000;

            if (item.Attribute("Path") != null)
            {
                wfilename = item.Attribute("Path").Value;
            }
            else if (item.Attribute("filename") != null)
            {
                wfilename = item.Attribute("filename").Value;
            }

            if (item.Attribute("sha256") != null)
            {
                sha256sum = item.Attribute("sha256").Value;
            }

            if (item.Attribute("md5") != null)
            {
                md5sum = item.Attribute("md5").Value;
            }

            if (item.Attribute("FileOffsetInSrc") != null)
            {
                start = long.Parse(item.Attribute("FileOffsetInSrc").Value) * pagesize;
            }
            else if (item.Attribute("SizeInSectorInSrc") != null)
            {
                start = long.Parse(item.Attribute("SizeInSectorInSrc").Value) * pagesize;
            }

            if (item.Attribute("SizeInByteInSrc") != null)
            {
                rlength = long.Parse(item.Attribute("SizeInByteInSrc").Value);
            }

            long length;
            if (item.Attribute("SizeInSectorInSrc") != null)
            {
                length = long.Parse(item.Attribute("SizeInSectorInSrc").Value) * pagesize;
            }
            else
            {
                length = rlength;
            }

            return (wfilename, start, length, rlength, new string[] { sha256sum, md5sum }, decryptsize);
        }/*
    private static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Oppo MTK QC decrypt tool 1.1 (c) B.Kerler 2020-2022\n");
            Console.WriteLine("Usage: ofp_qc_extract.exe [Filename.ofp] [Directory to extract files to]");
            Environment.Exit(1);
        }
        string filename = args[0];
        string outdir = args[1];
        if (!Directory.Exists(outdir))
        {
            Directory.CreateDirectory(outdir);
        }
        bool pk = false;
        using (FileStream rf = new FileStream(filename, FileMode.Open, FileAccess.Read))
        {
            byte[] buffer = new byte[2];
            rf.Read(buffer, 0, 2);
            if (Encoding.ASCII.GetString(buffer) == "PK")
            {
                pk = true;
            }
        }
        if (pk)
        {
            Console.WriteLine("Zip file detected, trying to decrypt files");
            byte[] zippw = Encoding.UTF8.GetBytes("flash@realme$50E7F7D847732396F1582CD62DD385ED7ABB0897");
            using (ZipArchive archive = ZipFile.OpenRead(filename))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    Console.WriteLine("Extracting " + entry.FullName + " to " + outdir);
                    entry.ExtractToFile(Path.Combine(outdir, entry.FullName), true);
                }
            }
            Console.WriteLine("Files extracted to " + outdir);
            Environment.Exit(0);
        }
        string xml = "";
        var (pagesize, key, iv, data) = GenerateKey2(filename);
        if (pagesize == 0)
        {
            Console.WriteLine("Unknown key. Aborting");
            Environment.Exit(0);
        }
        else
        {
            xml = Encoding.UTF8.GetString(data, 0, data.ToList().LastIndexOf((byte)'>') + 1);
        }

        string path = Path.Combine(Path.GetDirectoryName(filename), outdir);

        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }
        else
        {
            Directory.CreateDirectory(path);
        }

        Console.WriteLine("Saving ProFile.xml");
        File.WriteAllText(Path.Combine(path, "ProFile.xml"), xml);

        XElement root = XElement.Parse(xml);
        foreach (XElement child in root.Elements())
        {
            foreach (XElement item in child.Elements())
            {
                if (item.Attribute("Path") == null && item.Attribute("filename") == null)
                {
                    foreach (XElement subitem in item.Elements())
                    {
                        var (wfilenamei, starti, lengthi, rlengthi, checksumsi, decryptsizei) = DecryptItem(subitem, pagesize);
                        if (string.IsNullOrEmpty(wfilenamei) || starti == -1)
                        {
                            continue;
                        }
                        DecryptFile(key, iv, filename, path, wfilenamei, starti, lengthi, rlengthi, checksumsi, decryptsizei);
                    }
                }
                var (wfilename, start, length, rlength, checksums, decryptsize) = DecryptItem(item, pagesize);
                if (string.IsNullOrEmpty(wfilename) || start == -1)
                {
                    continue;
                }
                if (child.Name.LocalName == "Sahara")
                {
                    decryptsize = (int)rlength;
                }
                if (new[] { "Config", "Provision", "ChainedTableOfDigests", "DigestsToSign", "Firmware" }.Contains(child.Name.LocalName))
                {
                    length = rlength;
                }
                if (new[] { "DigestsToSign", "ChainedTableOfDigests", "Firmware" }.Contains(child.Name.LocalName))
                {
                    Copy(filename, wfilename, path, start, length, checksums);
                }
                else
                {
                    DecryptFile(key, iv, filename, path, wfilename, start, length, rlength, checksums, decryptsize);
                }
            }
        }
        Console.WriteLine("\nDone. Extracted files to " + path);
        Environment.Exit(0);
    }*/
    }
}
