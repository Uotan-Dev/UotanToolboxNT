using System;
using System.IO;
using UotanToolbox.Common.SpraseHelper.Utilities.Conversion;

namespace UotanToolbox.Common.SpraseHelper.Utilities.ByteUtils
{
    internal class BigEndianWriter
    {
        public static void WriteUInt16(byte[] buffer, int offset, ushort value)
        {
            byte[] bytes = BigEndianConverter.GetBytes(value);
            Array.Copy(bytes, 0, buffer, offset, bytes.Length);
        }

        public static void WriteUInt16(byte[] buffer, ref int offset, ushort value)
        {
            WriteUInt16(buffer, offset, value);
            offset += 2;
        }

        public static void WriteInt16(byte[] buffer, int offset, short value)
        {
            byte[] bytes = BigEndianConverter.GetBytes(value);
            Array.Copy(bytes, 0, buffer, offset, bytes.Length);
        }

        public static void WriteUInt32(byte[] buffer, int offset, uint value)
        {
            byte[] bytes = BigEndianConverter.GetBytes(value);
            Array.Copy(bytes, 0, buffer, offset, bytes.Length);
        }

        public static void WriteUInt32(byte[] buffer, ref int offset, uint value)
        {
            WriteUInt32(buffer, offset, value);
            offset += 4;
        }

        public static void WriteInt32(byte[] buffer, int offset, int value)
        {
            byte[] bytes = BigEndianConverter.GetBytes(value);
            Array.Copy(bytes, 0, buffer, offset, bytes.Length);
        }

        public static void WriteUInt64(byte[] buffer, int offset, ulong value)
        {
            byte[] bytes = BigEndianConverter.GetBytes(value);
            Array.Copy(bytes, 0, buffer, offset, bytes.Length);
        }

        public static void WriteUInt64(byte[] buffer, ref int offset, ulong value)
        {
            WriteUInt64(buffer, offset, value);
            offset += 8;
        }

        public static void WriteInt64(byte[] buffer, int offset, long value)
        {
            byte[] bytes = BigEndianConverter.GetBytes(value);
            Array.Copy(bytes, 0, buffer, offset, bytes.Length);
        }

        public static void WriteGuidBytes(byte[] buffer, int offset, Guid value)
        {
            byte[] bytes = BigEndianConverter.GetBytes(value);
            Array.Copy(bytes, 0, buffer, offset, bytes.Length);
        }

        public static void WriteGuidBytes(byte[] buffer, ref int offset, Guid value)
        {
            WriteGuidBytes(buffer, offset, value);
            offset += 16;
        }

        public static void WriteUInt16(Stream stream, ushort value)
        {
            byte[] bytes = BigEndianConverter.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteInt32(Stream stream, int value)
        {
            byte[] bytes = BigEndianConverter.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteUInt32(Stream stream, uint value)
        {
            byte[] bytes = BigEndianConverter.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
