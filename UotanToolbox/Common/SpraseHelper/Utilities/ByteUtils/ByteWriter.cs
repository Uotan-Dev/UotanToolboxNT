using System;
using System.IO;
using System.Text;

namespace UotanToolbox.Common.SpraseHelper.Utilities.ByteUtils
{
    internal class ByteWriter
    {
        public static void WriteByte(byte[] buffer, int offset, byte value)
        {
            buffer[offset] = value;
        }

        public static void WriteByte(byte[] buffer, ref int offset, byte value)
        {
            buffer[offset] = value;
            offset += 1;
        }

        public static void WriteBytes(byte[] buffer, int offset, byte[] bytes)
        {
            WriteBytes(buffer, offset, bytes, bytes.Length);
        }

        public static void WriteBytes(byte[] buffer, ref int offset, byte[] bytes)
        {
            WriteBytes(buffer, offset, bytes);
            offset += bytes.Length;
        }

        public static void WriteBytes(byte[] buffer, int offset, byte[] bytes, int length)
        {
            Array.Copy(bytes, 0, buffer, offset, length);
        }

        public static void WriteBytes(byte[] buffer, ref int offset, byte[] bytes, int length)
        {
            Array.Copy(bytes, 0, buffer, offset, length);
            offset += length;
        }

        public static void WriteAnsiString(byte[] buffer, int offset, string value)
        {
            WriteAnsiString(buffer, offset, value, value.Length);
        }

        public static void WriteAnsiString(byte[] buffer, ref int offset, string value)
        {
            WriteAnsiString(buffer, ref offset, value, value.Length);
        }

        public static void WriteAnsiString(byte[] buffer, int offset, string value, int maximumLength)
        {
            byte[] bytes = ASCIIEncoding.GetEncoding(28591).GetBytes(value);
            Array.Copy(bytes, 0, buffer, offset, Math.Min(value.Length, maximumLength));
        }

        public static void WriteAnsiString(byte[] buffer, ref int offset, string value, int fieldLength)
        {
            WriteAnsiString(buffer, offset, value, fieldLength);
            offset += fieldLength;
        }

        public static void WriteUnicodeString(byte[] buffer, int offset, string value)
        {
            WriteUnicodeString(buffer, offset, value, value.Length);
        }

        public static void WriteUnicodeString(byte[] buffer, ref int offset, string value)
        {
            WriteUnicodeString(buffer, ref offset, value, value.Length);
        }

        public static void WriteUnicodeString(byte[] buffer, int offset, string value, int maximumNumberOfCharacters)
        {
            byte[] bytes = UnicodeEncoding.Unicode.GetBytes(value);
            int maximumNumberOfBytes = Math.Min(value.Length, maximumNumberOfCharacters) * 2;
            Array.Copy(bytes, 0, buffer, offset, maximumNumberOfBytes);
        }

        public static void WriteUnicodeString(byte[] buffer, ref int offset, string value, int numberOfCharacters)
        {
            WriteUnicodeString(buffer, offset, value, numberOfCharacters);
            offset += numberOfCharacters * 2;
        }

        public static void WriteNullTerminatedAnsiString(byte[] buffer, int offset, string value)
        {
            ByteWriter.WriteAnsiString(buffer, offset, value);
            ByteWriter.WriteByte(buffer, offset + value.Length, 0x00);
        }

        public static void WriteNullTerminatedAnsiString(byte[] buffer, ref int offset, string value)
        {
            WriteNullTerminatedAnsiString(buffer, offset, value);
            offset += value.Length + 1;
        }

        public static void WriteNullTerminatedUnicodeString(byte[] buffer, int offset, string value)
        {
            ByteWriter.WriteUnicodeString(buffer, offset, value);
            ByteWriter.WriteBytes(buffer, offset + value.Length * 2, new byte[] { 0x00, 0x00 });
        }

        public static void WriteNullTerminatedUnicodeString(byte[] buffer, ref int offset, string value)
        {
            WriteNullTerminatedUnicodeString(buffer, offset, value);
            offset += value.Length * 2 + 2;
        }

        public static void WriteBytes(Stream stream, byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteAnsiString(Stream stream, string value)
        {
            WriteAnsiString(stream, value, value.Length);
        }

        public static void WriteAnsiString(Stream stream, string value, int fieldLength)
        {
            byte[] bytes = ASCIIEncoding.GetEncoding(28591).GetBytes(value);
            stream.Write(bytes, 0, Math.Min(bytes.Length, fieldLength));
            if (bytes.Length < fieldLength)
            {
                byte[] zeroFill = new byte[fieldLength - bytes.Length];
                stream.Write(zeroFill, 0, zeroFill.Length);
            }
        }

        public static void WriteUnicodeString(Stream stream, string value)
        {
            byte[] bytes = UnicodeEncoding.Unicode.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
