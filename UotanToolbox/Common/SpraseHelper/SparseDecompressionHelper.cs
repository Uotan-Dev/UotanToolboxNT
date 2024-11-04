using System;
using System.IO;
using UotanToolbox.Common.SpraseHelper.Utilities.ByteUtils;

namespace UotanToolbox.Common.SpraseHelper
{
    internal class SparseDecompressionHelper
    {
        public static void DecompressSparse(Stream input, Stream output)
        {
            SparseHeader sparseHeader = SparseHeader.Read(input);
            if (sparseHeader == null)
            {
                throw new ArgumentException("Invalid Sparse Image Format");
            }

            output.Seek(0, SeekOrigin.Begin);
            for (uint index = 0; index < sparseHeader.TotalChunks; index++)
            {
                ChunkHeader chunkHeader = ChunkHeader.Read(input);
                long dataLength = (long)chunkHeader.ChunkSize * sparseHeader.BlockSize;
                switch (chunkHeader.ChunkType)
                {
                    case ChunkType.Raw:
                        {
                            byte[] data = ReadBytes(input, (int)dataLength);
                            ByteWriter.WriteBytes(output, data);
                            break;
                        }
                    case ChunkType.Fill:
                        {
                            byte[] fillBytes = ReadBytes(input, 4);
                            byte[] data = new byte[dataLength];
                            for (int offset = 0; offset < data.Length; offset += 4)
                            {
                                ByteWriter.WriteBytes(data, offset, fillBytes);
                            }
                            ByteWriter.WriteBytes(output, data);
                            break;
                        }
                    case ChunkType.DontCare:
                        {
                            output.Seek(dataLength, SeekOrigin.Current);
                            break;
                        }
                    case ChunkType.CRC:
                        {
                            byte[] crcBytes = ReadBytes(input, 4);
                            break;
                        }
                    default:
                        {
                            throw new ArgumentException("Error: Invalid Chunk Type");
                        }
                }
            }
            input.Close();
        }

        public static byte[] ReadBytes(Stream stream, int length)
        {
            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return buffer;
        }
    }
}
