using System;
using System.Collections.Generic;
using System.IO;

namespace UotanToolbox.Common
{
    internal class SimgInfo
    {
        public uint Magic { get; set; }
        public ushort MajorVersion { get; set; }
        public ushort MinorVersion { get; set; }
        public uint FileHdrSz { get; set; }
        public uint ChunkHdrSz { get; set; }
        public uint BlkSz { get; set; }
        public uint TotalBlks { get; set; }
        public uint TotalChunks { get; set; }
        public uint ImageChecksum { get; set; }
        public List<ChunkInfo> Chunks { get; set; } = new List<ChunkInfo>();
    }

    internal class ChunkInfo
    {
        public int ChunkType { get; set; }
        public int ChunkSz { get; set; }
        public int TotalSz { get; set; }
        public long CurPos { get; set; }
        public int DataSz { get; set; }
        public string Type { get; set; }
    }

    internal class SimgHelper
    {
        public static SimgInfo Parse(string path)
        {
            SimgInfo simgInfo = new SimgInfo();

            using (FileStream FH = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new BinaryReader(FH))
            {
                simgInfo.Magic = reader.ReadUInt32();
                simgInfo.MajorVersion = reader.ReadUInt16();
                simgInfo.MinorVersion = reader.ReadUInt16();
                simgInfo.FileHdrSz = reader.ReadUInt16();
                simgInfo.ChunkHdrSz = reader.ReadUInt16();
                simgInfo.BlkSz = reader.ReadUInt32();
                simgInfo.TotalBlks = reader.ReadUInt32();
                simgInfo.TotalChunks = reader.ReadUInt32();
                simgInfo.ImageChecksum = reader.ReadUInt32();
                if (simgInfo.Magic != 0xED26FF3A || simgInfo.FileHdrSz != 28 || simgInfo.ChunkHdrSz != 12)
                {
                    throw new Exception($"file is not a valid simg {simgInfo.Magic},{simgInfo.FileHdrSz},{simgInfo.ChunkHdrSz}");
                }
                if (simgInfo.ImageChecksum != 0)
                {
                    Console.WriteLine($"checksum=0x{simgInfo.ImageChecksum:X8}");
                }
                long offset = 0;
                for (int i = 1; i <= simgInfo.TotalChunks; i++)
                {
                    int chunkType = reader.ReadInt16();
                    reader.BaseStream.Seek(2, SeekOrigin.Current); // Skip 2 bytes
                    int chunkSz = reader.ReadInt32();
                    int totalSz = reader.ReadInt32();
                    long curPos = FH.Position;
                    int dataSz = totalSz - 12;
                    string type = "";
                    switch (chunkType)
                    {
                        case -13631: // Raw data
                            type = "Raw data";
                            reader.BaseStream.Seek(dataSz, SeekOrigin.Current);
                            break;
                        case -13630: // Fill chunk
                            if (dataSz != 4)
                            {
                                throw new Exception($"Fill chunk should have 4 bytes of fill, but this has {dataSz}");
                            }
                            int fillValue = reader.ReadInt32();
                            type = $"Fill with 0x{fillValue:X8}";
                            break;
                        case -13629: // Don't care chunk
                            if (dataSz != 0)
                            {
                                throw new Exception($"Don't care chunk input size is non-zero ({dataSz})");
                            }
                            type = "Don't care";
                            break;
                        case -13628: // CRC32 chunk
                            if (dataSz != 4)
                            {
                                throw new Exception($"CRC32 chunk should have 4 bytes of CRC, but this has {dataSz}");
                            }
                            int crcValue = reader.ReadInt32();
                            type = $"Unverified CRC32 0x{crcValue:X8}";
                            break;
                    }
                    FH.Position = curPos + dataSz;
                    offset += chunkSz;
                    simgInfo.Chunks.Add(new ChunkInfo
                    {
                        ChunkType = chunkType,
                        ChunkSz = chunkSz,
                        TotalSz = totalSz,
                        CurPos = curPos,
                        DataSz = dataSz,
                        Type = type
                    });
                }
            }
            return simgInfo;
        }
    }
}
