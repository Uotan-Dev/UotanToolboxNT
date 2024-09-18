using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UotanToolbox.Common
{
    internal class CryptoHelper
    {
        static string SHA256Hash(string input)
        {
            using var sha256 = SHA256.Create();
            return BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", "");
        }

        public static byte[] AESEncrypt(byte[] plainText, byte[] key, byte[] iv)
        {
            using var aesAlg = Aes.Create();
            aesAlg.Key = key;
            aesAlg.IV = iv;

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            csEncrypt.Write(plainText, 0, plainText.Length);
            csEncrypt.FlushFinalBlock();
            return msEncrypt.ToArray();
        }

        public static byte[] AESDecrypt(byte[] cipherText, byte[] key, byte[] iv)
        {
            using var aesAlg = Aes.Create();
            aesAlg.Key = key;
            aesAlg.IV = iv;

            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using var msDecrypt = new MemoryStream(cipherText);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            var decryptedData = new byte[cipherText.Length];
            var decryptedByteCount = csDecrypt.Read(decryptedData, 0, decryptedData.Length);
            Array.Resize(ref decryptedData, decryptedByteCount);
            return decryptedData;
        }

        public static byte[] AESCBC(byte[] data, byte[] key, byte[] iv, bool decrypt)
        {
            using var aesAlg = Aes.Create();
            aesAlg.Key = key;
            aesAlg.IV = iv;

            var encryptorOrDecryptor = decrypt ? aesAlg.CreateDecryptor() : aesAlg.CreateEncryptor();

            using var ms = new();
            using var cs = new(ms, encryptorOrDecryptor, CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();
        }

        public static byte[] Unhexlify(string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
            {
                throw new ArgumentException("hexString cannot be null or empty.", nameof(hexString));
            }

            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("The length of the hexString must be even.", nameof(hexString));
            }

            var bytes = new byte[hexString.Length / 2];

            for (int i = 0; i < hexString.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            

            return bytes;
        }
    }
}