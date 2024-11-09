using System;
using System.Text;
using UotanToolbox.Common.ROMHelper.SpraseHelper.Utilities.Conversion;

namespace UotanToolbox.Common.ROMHelper.SpraseHelper.Utilities.ByteUtils
{
    internal class ByteReader
    {
        public static byte ReadByte(byte[] buffer, int offset)
        {
            return buffer[offset];
        }

        public static byte ReadByte(byte[] buffer, ref int offset)
        {
            offset++;
            return buffer[offset - 1];
        }

        public static byte[] ReadBytes(byte[] buffer, int offset, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(buffer, offset, result, 0, length);
            return result;
        }

        public static byte[] ReadBytes(byte[] buffer, ref int offset, int length)
        {
            offset += length;
            return ReadBytes(buffer, offset - length, length);
        }

        /// <summary>
        /// Will return the ANSI string stored in the buffer
        /// </summary>
        public static string ReadAnsiString(byte[] buffer, int offset, int count)
        {
            // ASCIIEncoding.ASCII.GetString will convert some values to '?' (byte value of 63)
            // Any codepage will do, but the only one that Mono supports is 28591.
            return Encoding.GetEncoding(28591).GetString(buffer, offset, count);
        }

        public static string ReadAnsiString(byte[] buffer, ref int offset, int len)
        {
            offset += len;
            return ReadAnsiString(buffer, offset - len, len);
        }

        public static string ReadUnicodeString(byte[] buffer, int offset, int numberOfCharacters)
        {
            int numberOfBytes = numberOfCharacters * 2;
            return Encoding.Unicode.GetString(buffer, offset, numberOfBytes);
        }

        public static string ReadUnicodeString(byte[] buffer, ref int offset, int numberOfCharacters)
        {
            int numberOfBytes = numberOfCharacters * 2;
            offset += numberOfBytes;
            return ReadUnicodeString(buffer, offset - numberOfBytes, numberOfCharacters);
        }

        public static string ReadNullTerminatedAnsiString(byte[] buffer, int offset)
        {
            StringBuilder builder = new StringBuilder();
            char c = (char)ReadByte(buffer, offset);
            while (c != '\0')
            {
                builder.Append(c);
                offset++;
                c = (char)ReadByte(buffer, offset);
            }
            return builder.ToString();
        }

        public static string ReadNullTerminatedUnicodeString(byte[] buffer, int offset)
        {
            StringBuilder builder = new StringBuilder();
            char c = (char)LittleEndianConverter.ToUInt16(buffer, offset);
            while (c != 0)
            {
                builder.Append(c);
                offset += 2;
                c = (char)LittleEndianConverter.ToUInt16(buffer, offset);
            }
            return builder.ToString();
        }

        public static string ReadNullTerminatedAnsiString(byte[] buffer, ref int offset)
        {
            string result = ReadNullTerminatedAnsiString(buffer, offset);
            offset += result.Length + 1;
            return result;
        }

        public static string ReadNullTerminatedUnicodeString(byte[] buffer, ref int offset)
        {
            string result = ReadNullTerminatedUnicodeString(buffer, offset);
            offset += result.Length * 2 + 2;
            return result;
        }
    }
}
