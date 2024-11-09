using System;
using UotanToolbox.Common.ROMHelper.SpraseHelper.Utilities.Conversion;

namespace UotanToolbox.Common.ROMHelper.SpraseHelper.Utilities.ByteUtils
{
    internal class LittleEndianReader
    {
        public static ushort ReadUInt16(byte[] buffer, ref int offset)
        {
            offset += 2;
            return LittleEndianConverter.ToUInt16(buffer, offset - 2);
        }

        public static uint ReadUInt32(byte[] buffer, ref int offset)
        {
            offset += 4;
            return LittleEndianConverter.ToUInt32(buffer, offset - 4);
        }

        public static ulong ReadUInt64(byte[] buffer, ref int offset)
        {
            offset += 8;
            return LittleEndianConverter.ToUInt64(buffer, offset - 8);
        }

        public static Guid ReadGuid(byte[] buffer, ref int offset)
        {
            offset += 16;
            return LittleEndianConverter.ToGuid(buffer, offset - 16);
        }
    }
}
