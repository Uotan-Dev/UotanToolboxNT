using ROMLibrary.SpraseHelper.Utilities.Conversion;

namespace ROMLibrary.SpraseHelper.Utilities.ByteUtils
{
    internal class BigEndianReader
    {
        public static ushort ReadUInt16(byte[] buffer, ref int offset)
        {
            offset += 2;
            return BigEndianConverter.ToUInt16(buffer, offset - 2);
        }

        public static uint ReadUInt32(byte[] buffer, ref int offset)
        {
            offset += 4;
            return BigEndianConverter.ToUInt32(buffer, offset - 4);
        }

        public static long ReadInt64(byte[] buffer, ref int offset)
        {
            offset += 8;
            return BigEndianConverter.ToInt64(buffer, offset - 8);
        }

        public static ulong ReadUInt64(byte[] buffer, ref int offset)
        {
            offset += 8;
            return BigEndianConverter.ToUInt64(buffer, offset - 8);
        }

        public static Guid ReadGuidBytes(byte[] buffer, ref int offset)
        {
            offset += 16;
            return BigEndianConverter.ToGuid(buffer, offset - 16);
        }
    }
}
