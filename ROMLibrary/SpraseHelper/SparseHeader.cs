using ROMLibrary.SpraseHelper.Utilities.ByteUtils;
using ROMLibrary.SpraseHelper.Utilities.Conversion;

namespace ROMLibrary.SpraseHelper
{
    internal class SparseHeader
    {
        public const uint ValidSignature = 0xed26ff3a;
        public const int Length = 28;

        public uint Magic;
        public ushort MajorVersion;
        public ushort MinorVersion;
        public ushort FileHeaderSize;
        public ushort ChunkHeaderSize;
        public uint BlockSize;
        public uint TotalBlocks;
        public uint TotalChunks;
        public uint ImageChecksum;

        public SparseHeader()
        {
            Magic = ValidSignature;
            MajorVersion = 1;
            MinorVersion = 0;
            FileHeaderSize = Length;
            ChunkHeaderSize = ChunkHeader.Length;
        }

        public SparseHeader(byte[] buffer, int offset)
        {
            Magic = LittleEndianConverter.ToUInt32(buffer, offset + 0);
            MajorVersion = LittleEndianConverter.ToUInt16(buffer, offset + 4);
            MinorVersion = LittleEndianConverter.ToUInt16(buffer, offset + 6);
            FileHeaderSize = LittleEndianConverter.ToUInt16(buffer, offset + 8);
            ChunkHeaderSize = LittleEndianConverter.ToUInt16(buffer, offset + 10);
            BlockSize = LittleEndianConverter.ToUInt32(buffer, offset + 12);
            TotalBlocks = LittleEndianConverter.ToUInt32(buffer, offset + 16);
            TotalChunks = LittleEndianConverter.ToUInt32(buffer, offset + 20);
            ImageChecksum = LittleEndianConverter.ToUInt32(buffer, offset + 24);
        }

        public void WriteBytes(byte[] buffer, int offset)
        {
            LittleEndianWriter.WriteUInt32(buffer, offset + 0, Magic);
            LittleEndianWriter.WriteUInt16(buffer, offset + 4, MajorVersion);
            LittleEndianWriter.WriteUInt16(buffer, offset + 6, MinorVersion);
            LittleEndianWriter.WriteUInt16(buffer, offset + 8, FileHeaderSize);
            LittleEndianWriter.WriteUInt16(buffer, offset + 10, ChunkHeaderSize);
            LittleEndianWriter.WriteUInt32(buffer, offset + 12, BlockSize);
            LittleEndianWriter.WriteUInt32(buffer, offset + 16, TotalBlocks);
            LittleEndianWriter.WriteUInt32(buffer, offset + 20, TotalChunks);
            LittleEndianWriter.WriteUInt32(buffer, offset + 24, ImageChecksum);
        }

        public void WriteBytes(Stream stream)
        {
            byte[] buffer = new byte[Length];
            WriteBytes(buffer, 0);
            ByteWriter.WriteBytes(stream, buffer);
        }

        public static SparseHeader Read(byte[] buffer, int offset)
        {
            uint magic = LittleEndianConverter.ToUInt32(buffer, offset + 0);
            if (magic == ValidSignature)
            {
                return new SparseHeader(buffer, offset);
            }
            return null;
        }

        public static SparseHeader Read(Stream stream)
        {
            byte[] buffer = new byte[Length];
            stream.ReadExactly(buffer, 0, Length);
            return Read(buffer, 0);
        }
    }
}
