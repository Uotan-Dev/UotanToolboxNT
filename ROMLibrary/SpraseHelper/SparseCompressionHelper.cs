using ROMLibrary.SpraseHelper.Utilities.ByteUtils;

namespace ROMLibrary.SpraseHelper
{
    internal class SparseCompressionHelper
    {
        public const int BlockSize = 4096;

        /// <param name="sparseIndex">one-based index</param>
        /// <returns>True if the write was complete (i.e. this was the last sparse)</returns>
        public static bool WriteCompressedSparse(Stream input, Stream output, long maxSparseSize)
        {
            // We will write the header later
            output.Seek(SparseHeader.Length, SeekOrigin.Begin);
            long currentSparseSize = SparseHeader.Length;

            uint chunkCount = 0;
            if (input.Position != 0)
            {
                WriteDontCareChunk(output, (uint)(input.Position / BlockSize));
                chunkCount++;
            }

            MemoryStream rawChunk = new MemoryStream();
            byte[] fill = null;
            int fillCount = 0;

            // We keep reading as long as we have a room for an additional chunk
            // (the largest chunk is a raw chunk)
            while (currentSparseSize + ChunkHeader.Length + BlockSize <= maxSparseSize &&
                   input.Position < input.Length)
            {
                byte[] block = new byte[BlockSize];
                input.ReadExactly(block, 0, BlockSize);
                byte[] currentFill = TryToCompressBlock(block);

                if (fill != null)
                {
                    if (currentFill != null)
                    {
                        if (FileName.AreByteArraysEqual(fill, currentFill))
                        {
                            fillCount++;
                        }
                        else
                        {
                            WriteFillChunk(output, fill, (uint)fillCount);
                            chunkCount++;

                            fillCount = 1;
                            fill = currentFill;
                            currentSparseSize += ChunkHeader.Length + 4;
                        }
                    }
                    else
                    {
                        WriteFillChunk(output, fill, (uint)fillCount);
                        chunkCount++;

                        ByteWriter.WriteBytes(rawChunk, block);
                        fill = null;
                        fillCount = 0;
                        currentSparseSize += ChunkHeader.Length + BlockSize;
                    }
                }
                else // fill == null
                {
                    if (currentFill != null)
                    {
                        WriteRawChunk(output, rawChunk);
                        chunkCount++;

                        rawChunk = new MemoryStream();
                        fillCount = 1;
                        fill = currentFill;
                        currentSparseSize += ChunkHeader.Length + 4;
                    }
                    else
                    {
                        ByteWriter.WriteBytes(rawChunk, block);
                        currentSparseSize += BlockSize;
                    }
                }
            }

            if (rawChunk.Length > 0)
            {
                WriteRawChunk(output, rawChunk);
                chunkCount++;
            }
            else
            {
                WriteFillChunk(output, fill, (uint)fillCount);
                chunkCount++;
            }

            bool complete = input.Position == input.Length;
            if (!complete)
            {
                WriteDontCareChunk(output, (uint)((input.Length - input.Position) / BlockSize));
                chunkCount++;
            }

            output.Seek(0, SeekOrigin.Begin);
            SparseHeader sparseHeader = new SparseHeader();
            sparseHeader.BlockSize = BlockSize;
            sparseHeader.TotalBlocks = (uint)(input.Length / BlockSize);
            sparseHeader.TotalChunks = chunkCount;
            sparseHeader.WriteBytes(output);
            output.Close();

            return complete;
        }

        public static byte[] TryToCompressBlock(byte[] block)
        {
            if (block.Length % 4 > 0)
            {
                throw new ArgumentException("Block size must be a multiple of 4 bytes");
            }
            byte[] fill = ByteReader.ReadBytes(block, 0, 4);
            for (int offset = 4; offset < block.Length; offset += 4)
            {
                if (fill[0] != block[offset + 0] ||
                    fill[1] != block[offset + 1] ||
                    fill[2] != block[offset + 2] ||
                    fill[3] != block[offset + 3])
                {
                    return null;
                }
            }

            return fill;
        }

        public static void WriteFillChunk(Stream output, byte[] fill, uint blockCount)
        {
            ChunkHeader chunkHeader = new ChunkHeader();
            chunkHeader.ChunkType = ChunkType.Fill;
            chunkHeader.ChunkSize = blockCount;
            chunkHeader.TotalSize = ChunkHeader.Length + 4;
            chunkHeader.WriteBytes(output);

            ByteWriter.WriteBytes(output, fill);
        }

        public static void WriteRawChunk(Stream output, Stream rawChunk)
        {
            ChunkHeader chunkHeader = new ChunkHeader();
            chunkHeader.ChunkType = ChunkType.Raw;
            chunkHeader.ChunkSize = (uint)(rawChunk.Length / BlockSize);
            chunkHeader.TotalSize = ChunkHeader.Length + (uint)rawChunk.Length;
            chunkHeader.WriteBytes(output);

            rawChunk.Seek(0, SeekOrigin.Begin);
            long blockCount = rawChunk.Length / BlockSize;
            for (long blockIndex = 0; blockIndex < blockCount; blockIndex++)
            {
                byte[] block = new byte[BlockSize];
                rawChunk.ReadExactly(block, 0, BlockSize);
                ByteWriter.WriteBytes(output, block);
            }
        }

        public static void WriteDontCareChunk(Stream output, uint blockLength)
        {
            ChunkHeader chunkHeader = new ChunkHeader();
            chunkHeader.ChunkType = ChunkType.DontCare;
            chunkHeader.ChunkSize = blockLength;
            chunkHeader.TotalSize = ChunkHeader.Length;
            chunkHeader.WriteBytes(output);
        }
    }
}
