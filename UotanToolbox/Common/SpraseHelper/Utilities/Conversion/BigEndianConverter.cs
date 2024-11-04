using System;

namespace UotanToolbox.Common.SpraseHelper.Utilities.Conversion
{
    internal class BigEndianConverter
    {
        public static ushort ToUInt16(byte[] buffer, int offset)
        {
            return (ushort)((buffer[offset + 0] << 8) | (buffer[offset + 1] << 0));
        }

        public static short ToInt16(byte[] buffer, int offset)
        {
            return (short)ToUInt16(buffer, offset);
        }

        public static uint ToUInt32(byte[] buffer, int offset)
        {
            return (uint)((buffer[offset + 0] << 24) | (buffer[offset + 1] << 16)
                | (buffer[offset + 2] << 8) | (buffer[offset + 3] << 0));
        }

        public static int ToInt32(byte[] buffer, int offset)
        {
            return (int)ToUInt32(buffer, offset);
        }

        public static ulong ToUInt64(byte[] buffer, int offset)
        {
            return (((ulong)ToUInt32(buffer, offset + 0)) << 32) | ToUInt32(buffer, offset + 4);
        }

        public static long ToInt64(byte[] buffer, int offset)
        {
            return (long)ToUInt64(buffer, offset);
        }

        public static Guid ToGuid(byte[] buffer, int offset)
        {
            return new Guid(
                ToUInt32(buffer, offset + 0),
                ToUInt16(buffer, offset + 4),
                ToUInt16(buffer, offset + 6),
                buffer[offset + 8],
                buffer[offset + 9],
                buffer[offset + 10],
                buffer[offset + 11],
                buffer[offset + 12],
                buffer[offset + 13],
                buffer[offset + 14],
                buffer[offset + 15]);
        }

        public static byte[] GetBytes(ushort value)
        {
            byte[] result = new byte[2];
            result[0] = (byte)((value >> 8) & 0xFF);
            result[1] = (byte)((value >> 0) & 0xFF);
            return result;
        }

        public static byte[] GetBytes(short value)
        {
            return GetBytes((ushort)value);
        }

        public static byte[] GetBytes(uint value)
        {
            byte[] result = new byte[4];
            result[0] = (byte)((value >> 24) & 0xFF);
            result[1] = (byte)((value >> 16) & 0xFF);
            result[2] = (byte)((value >> 8) & 0xFF);
            result[3] = (byte)((value >> 0) & 0xFF);

            return result;
        }

        public static byte[] GetBytes(int value)
        {
            return GetBytes((uint)value);
        }

        public static byte[] GetBytes(ulong value)
        {
            byte[] result = new byte[8];
            Array.Copy(GetBytes((uint)(value >> 32)), 0, result, 0, 4);
            Array.Copy(GetBytes((uint)(value & 0xFFFFFFFF)), 0, result, 4, 4);

            return result;
        }

        public static byte[] GetBytes(long value)
        {
            return GetBytes((ulong)value);
        }

        public static byte[] GetBytes(Guid value)
        {
            byte[] result = value.ToByteArray();
            if (BitConverter.IsLittleEndian)
            {
                // reverse first 4 bytes
                byte temp = result[0];
                result[0] = result[3];
                result[3] = temp;

                temp = result[1];
                result[1] = result[2];
                result[2] = temp;

                // reverse next 2 bytes
                temp = result[4];
                result[4] = result[5];
                result[5] = temp;

                // reverse next 2 bytes
                temp = result[6];
                result[6] = result[7];
                result[7] = temp;
            }
            return result;
        }
    }
}
