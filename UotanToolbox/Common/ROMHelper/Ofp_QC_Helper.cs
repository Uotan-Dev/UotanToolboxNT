using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using System.IO.Compression;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using SharpCompress.Common;

namespace UotanToolbox.Common.ROMHelper
{
    internal class Ofp_QC_Helper
    {
        public static byte Swap(byte ch)
        {
            return (byte)((ch & 0x0F) << 4 | (ch & 0xF0) >> 4);
        }

        public static byte[] KeyShuffle(byte[] key, byte[] hkey)
        {
            for (int i = 0; i < 0x10; i += 4)
            {
                key[i] = Swap((byte)(hkey[i] ^ key[i]));
                key[i + 1] = Swap((byte)(hkey[i + 1] ^ key[i + 1]));
                key[i + 2] = Swap((byte)(hkey[i + 2] ^ key[i + 2]));
                key[i + 3] = Swap((byte)(hkey[i + 3] ^ key[i + 3]));
            }
            return key;
        }
        public static (byte[], byte[]) GenerateKey1()
        {
            string key1Str = "42F2D5399137E2B2813CD8ECDF2F4D72";
            string key2Str = "F6C50203515A2CE7D8C3E1F938B7E94C";
            string key3Str = "67657963787565E837D226B69A495D21";

            byte[] key1 = Enumerable.Range(0, key1Str.Length / 2).Select(x => Convert.ToByte(key1Str.Substring(x * 2, 2), 16)).ToArray();
            byte[] key2 = Enumerable.Range(0, key2Str.Length / 2).Select(x => Convert.ToByte(key2Str.Substring(x * 2, 2), 16)).ToArray();
            byte[] key3 = Enumerable.Range(0, key3Str.Length / 2).Select(x => Convert.ToByte(key3Str.Substring(x * 2, 2), 16)).ToArray();

            key2 = KeyShuffle(key2, key3);
            byte[] aesKey = Encoding.UTF8.GetBytes(BitConverter.ToString(MD5.HashData(key2)).Replace("-", "")[..16]);

            key1 = KeyShuffle(key1, key3);
            byte[] iv = Encoding.UTF8.GetBytes(BitConverter.ToString(MD5.HashData(key1)).Replace("-", "")[..16]);

            return (aesKey, iv);
        }

        public static byte ROL(byte x, int n, int bits = 8)
        {
            n = bits - n;
            byte mask = (byte)((1 << n) - 1);
            byte maskBits = (byte)(x & mask);
            return (byte)(x >> n | maskBits << bits - n);
        }

        public static byte[] Deobfuscate(byte[] data, byte[] mask)
        {
            byte[] ret = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                byte v = ROL((byte)(data[i] ^ mask[i]), 4, 8);
                ret[i] = v;
            }
            return ret;
        }

        public static (int, byte[], byte[], byte[]) GenerateKey2(string filename)
        {
            var keys = new List<string[]>
        {
            new string[] { "V1.4.17/1.4.27", "27827963787265EF89D126B69A495A21", "82C50203285A2CE7D8C3E198383CE94C", "422DD5399181E223813CD8ECDF2E4D72" },
            new string[] { "V1.6.17", "E11AA7BB558A436A8375FD15DDD4651F", "77DDF6A0696841F6B74782C097835169", "A739742384A44E8BA45207AD5C3700EA" },
            new string[] { "V1.5.13", "67657963787565E837D226B69A495D21", "F6C50203515A2CE7D8C3E1F938B7E94C", "42F2D5399137E2B2813CD8ECDF2F4D72" },
            new string[] { "V1.6.6/1.6.9/1.6.17/1.6.24/1.6.26/1.7.6", "3C2D518D9BF2E4279DC758CD535147C3", "87C74A29709AC1BF2382276C4E8DF232", "598D92E967265E9BCABE2469FE4A915E" },
            new string[] { "V1.7.2", "8FB8FB261930260BE945B841AEFA9FD4", "E529E82B28F5A2F8831D860AE39E425D", "8A09DA60ED36F125D64709973372C1CF" },
            new string[] { "V2.0.3", "E8AE288C0192C54BF10C5707E9C4705B", "D64FC385DCD52A3C9B5FBA8650F92EDA", "79051FD8D8B6297E2E4559E997F63B7F" }
        };

            foreach (var dkey in keys)
            {
                byte[] mc = Enumerable.Range(0, dkey[1].Length / 2).Select(x => Convert.ToByte(dkey[1].Substring(x * 2, 2), 16)).ToArray();
                byte[] userKey = Enumerable.Range(0, dkey[2].Length / 2).Select(x => Convert.ToByte(dkey[2].Substring(x * 2, 2), 16)).ToArray();
                byte[] ivec = Enumerable.Range(0, dkey[3].Length / 2).Select(x => Convert.ToByte(dkey[3].Substring(x * 2, 2), 16)).ToArray();
                byte[] key = Encoding.UTF8.GetBytes(BitConverter.ToString(MD5.HashData(Deobfuscate(userKey, mc))).Replace("-", "").ToLower()[..16]);
                byte[] iv = Encoding.UTF8.GetBytes(BitConverter.ToString(MD5.HashData(Deobfuscate(ivec, mc))).Replace("-", "").ToLower()[..16]);
                Console.WriteLine(Convert.ToHexString(key));
                Console.WriteLine(Convert.ToHexString(iv));
                var (pageSize, data) = ExtractXml(filename, key, iv);

                if (pageSize != 0)
                {
                    return (pageSize, key, iv, data);
                }
            }
            return (0, [], [], []);
        }

        public static (int pagesize, byte[] data) ExtractXml(string filename, byte[] key, byte[] iv)
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
            //Console.WriteLine("data: "+Convert.ToHexString(data).ToLower());
            byte[] dec = AesCfbDecrypt(data, key, iv);
            Console.WriteLine("dec: " + Convert.ToHexString(dec).ToLower());
            if (Encoding.UTF8.GetString(dec).Contains("<?xml"))
            {
                return (pagesize, dec);
            }
            else
            {
                return (0, []);
            }
        }
        static byte[] AesCfbDecrypt(byte[] data, byte[] key, byte[] iv)
        {
            //什么？你问我为什么不用NET内置的AES解密库？那自然是它算出来的有问题啊！这问题浪费了我足足3天的时间   --Zi_Cai
            AesEngine engine = new();
            CfbBlockCipher cfb = new(engine, 128);
            BufferedBlockCipher cipher = new(cfb);
            cipher.Init(false, new ParametersWithIV(new KeyParameter(key), iv));
            byte[] decrypted = new byte[cipher.GetOutputSize(data.Length)];
            int len = cipher.ProcessBytes(data, 0, data.Length, decrypted, 0);
            cipher.DoFinal(decrypted, len);
            return decrypted;
        }
        public static long CopySub(FileStream rf, FileStream wf, long start, long length)
        {
            rf.Seek(start, SeekOrigin.Begin);
            long rlen = 0;
            byte[] buffer = new byte[0x100000]; // 1 MB buffer

            while (length > 0)
            {
                int size = length < 0x100000 ? (int)length : 0x100000;
                int bytesRead = rf.Read(buffer, 0, size);
                wf.Write(buffer, 0, bytesRead);
                rlen += bytesRead;
                length -= bytesRead;
            }

            return rlen;
        }
        public static void Copy(string filename, string wfilename, string path, long start, long length, string[] checksums)
        {
            Console.WriteLine($"\nExtracting {wfilename}");
            string outputPath = Path.Combine(path, wfilename);

            using (FileStream rf = new(filename, FileMode.Open, FileAccess.Read))
            {
                using FileStream wf = new(outputPath, FileMode.Create, FileAccess.Write);
                rf.Seek(start, SeekOrigin.Begin);
                byte[] data = new byte[length];
                rf.Read(data, 0, (int)length);
                wf.Write(data, 0, data.Length);
            }

            CheckHashFile(outputPath, checksums, true);
        }
        public static void DecryptFile(byte[] key, byte[] iv, string filename, string path, string wfilename, long start, long length, long rlength, string[] checksums, int decryptsize = 0x40000)
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

            using (FileStream rf = new(filename, FileMode.Open, FileAccess.Read))
            {
                using FileStream wf = new(outputPath, FileMode.Create, FileAccess.Write);
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

            CheckHashFile(outputPath, checksums, false);
        }
        public static void CheckHashFile(string wfilename, string[] checksums, bool isCopy)
        {
            string sha256sum = checksums[0];
            string md5sum = checksums[1];
            string prefix = isCopy ? "Copy: " : "Decrypt: ";

            using FileStream rf = new(wfilename, FileMode.Open, FileAccess.Read);
            long size = new FileInfo(wfilename).Length;
            rf.Seek(0, SeekOrigin.Begin);
            byte[] buffer = new byte[0x40000];
            rf.Read(buffer, 0, buffer.Length);
            byte[] md5Hash = MD5.HashData(buffer);
            string md5Hex = BitConverter.ToString(md5Hash).Replace("-", "").ToLower();

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
                            sha256.TransformBlock(buffer, 0, buffer.Length, buffer, 0);
                            sha256.TransformFinalBlock([], 0, 0);
                        }
                        else if (x == size)
                        {
                            int bytesRead;
                            while ((bytesRead = rf.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                sha256.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                            }
                            sha256.TransformFinalBlock([], 0, 0);
                        }

                        string sha256Hex = BitConverter.ToString(sha256.Hash).Replace("-", "").ToLower();
                        if (sha256sum != sha256Hex)
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
                if (md5sum != md5Hex)
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
        public static (string, long, long, long, string[], int) DecryptItem(XElement item, int pageSize)
        {
            string sha256sum = "";
            string md5sum = "";
            string wfilename = "";
            long start = -1;
            long rlength = 0;
            int decryptSize = 0x40000;

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
                start = long.Parse(item.Attribute("FileOffsetInSrc").Value) * pageSize;
            }
            else if (item.Attribute("SizeInSectorInSrc") != null)
            {
                start = long.Parse(item.Attribute("SizeInSectorInSrc").Value) * pageSize;
            }

            if (item.Attribute("SizeInByteInSrc") != null)
            {
                rlength = long.Parse(item.Attribute("SizeInByteInSrc").Value);
            }

            long length;
            if (item.Attribute("SizeInSectorInSrc") != null)
            {
                length = long.Parse(item.Attribute("SizeInSectorInSrc").Value) * pageSize;
            }
            else
            {
                length = rlength;
            }

            return (wfilename, start, length, rlength, new string[] { sha256sum, md5sum }, decryptSize);
        } } }
        /*
        public static void Extract(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Oppo MTK QC decrypt Logic 1.1 (c) B.Kerler 2020-2022\n");
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
                if (buffer[0] == 'P' && buffer[1] == 'K')
                {
                    pk = true;
                }
            }

            if (pk)
            {
                string zippw = "flash@realme$50E7F7D847732396F1582CD62DD385ED7ABB0897";
                Console.WriteLine("Zip file detected, trying to decrypt files");

                using (var archive = ZipArchive.Open(filename, new ReaderOptions { Password = zippw }))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.IsDirectory)
                        {
                            Console.WriteLine($"Extracting {entry.Key} to {outdir}");
                            entry.WriteToDirectory(outdir, new ExtractionOptions
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                }

                Console.WriteLine($"Files extracted to {outdir}");
                Environment.Exit(0);
            }
            var (pageSize, key, iv, data) = GenerateKey2(filename);
            if (pageSize == 0)
            {
                Console.WriteLine("Unknown key. Aborting");
                Environment.Exit(0);
            }
            else
            {
                string xml = Encoding.UTF8.GetString(data, 0, data.ToList().LastIndexOf((byte)'>') + 1);
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
                                var (wfilenamei, starti, lengthi, rlengthi, checksumsi, decryptSizei) = DecryptItem(subitem, pageSize);
                                if (wfilenamei == "" || starti == -1)
                                {
                                    continue;
                                }
                                DecryptFile(key, iv, filename, path, wfilenamei, starti, lengthi, rlengthi, checksumsi, decryptSizei);
                            }
                        }
                        var (wfilename, start, length, rlength, checksums, decryptSize) = DecryptItem(item, pageSize);
                        if (wfilename == "" || start == -1)
                        {
                            continue;
                        }
                        if (child.Name.LocalName == "Sahara")
                        {
                            decryptSize = (int)rlength;
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
                            DecryptFile(key, iv, filename, path, wfilename, start, length, rlength, checksums, decryptSize);
                        }
                    }
                }
                Console.WriteLine($"\nDone. Extracted files to {path}");
                Environment.Exit(0);
            }
        }
    }
}
        */