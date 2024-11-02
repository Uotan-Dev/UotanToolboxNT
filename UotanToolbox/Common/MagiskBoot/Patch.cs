using System;
using System.IO;
using System.Linq;
using System.Text;

namespace UotanToolbox.Common.MagiskBoot
{
    internal class Patch
    {
        public static int PatchVerity(byte[] buf)
        {
            return RemovePattern(buf, MatchVerityPattern);
        }

        public static int PatchEncryption(byte[] buf)
        {
            return RemovePattern(buf, MatchEncryptionPattern);
        }

        public static bool HexPatch(string filePath, string fromHex, string toHex)
        {
            try
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                byte[] fromBytes = Hex2Byte(fromHex);
                byte[] toBytes = Hex2Byte(toHex);

                bool patched = false;
                for (int i = 0; i <= fileBytes.Length - fromBytes.Length; i++)
                {
                    if (fileBytes.Skip(i).Take(fromBytes.Length).SequenceEqual(fromBytes))
                    {
                        Array.Copy(toBytes, 0, fileBytes, i, toBytes.Length);
                        patched = true;
                    }
                }
                if (patched)
                {
                    File.WriteAllBytes(filePath, fileBytes);
                }
                return patched;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to patch file: {ex.Message}");
            }
        }

        private static int RemovePattern(byte[] buf, Func<byte[], int, int?> patternMatcher)
        {
            int write = 0;
            int read = 0;
            int sz = buf.Length;

            while (read < buf.Length)
            {
                int? len = patternMatcher(buf, read);
                if (len.HasValue)
                {
                    string skipped = Encoding.ASCII.GetString(buf, read, len.Value);
                    sz -= len.Value;
                    read += len.Value;
                }
                else
                {
                    buf[write] = buf[read];
                    write++;
                    read++;
                }
            }
            Array.Clear(buf, write, buf.Length - write);
            return sz;
        }

        private static int? MatchVerityPattern(byte[] buf, int offset)
        {
            return MatchPatterns(buf, offset, "verifyatboot", "verify", "avb_keys", "avb", "support_scfs", "fsverity");
        }

        private static int? MatchEncryptionPattern(byte[] buf, int offset)
        {
            return MatchPatterns(buf, offset, "forceencrypt", "forcefdeorfbe", "fileencryption");
        }

        private static int? MatchPatterns(byte[] buf, int offset, params string[] patterns)
        {
            foreach (string pattern in patterns)
            {
                byte[] patternBytes = Encoding.ASCII.GetBytes(pattern);
                if (buf.Skip(offset).Take(patternBytes.Length).SequenceEqual(patternBytes))
                {
                    int len = patternBytes.Length;
                    if (buf.Length > offset + len && buf[offset + len] == (byte)'=')
                    {
                        len++;
                        while (buf.Length > offset + len && !new byte[] { (byte)' ', (byte)'\n', (byte)'\0' }.Contains(buf[offset + len]))
                        {
                            len++;
                        }
                    }
                    return len;
                }
            }
            return null;
        }

        private static byte[] Hex2Byte(string hex)
        {
            int len = hex.Length;
            byte[] result = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
            {
                result[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return result;
        }
    }
}
