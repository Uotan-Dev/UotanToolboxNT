using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System;
using System.IO;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UotanToolbox.Common
{
    internal class SimgHelper
    {
        public static string Parse(string path)
        {
            using (FileStream FH = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                string Type = "";
                using (BinaryReader reader = new BinaryReader(FH))
                {
                    uint magic = reader.ReadUInt32(); //"I"
                    ushort majorVersion = reader.ReadUInt16(); // "H"
                    ushort minorVersion = reader.ReadUInt16(); // "H"
                    uint fileHdrSz = reader.ReadUInt16(); // "H"
                    uint chunkHdrSz = reader.ReadUInt16(); // "H"
                    uint blkSz = reader.ReadUInt32(); // "I"
                    uint totalBlks = reader.ReadUInt32(); // "I"
                    uint totalChunks = reader.ReadUInt32(); // "I"
                    uint imageChecksum = reader.ReadUInt32(); // "I"

                    if (magic != 0xED26FF3A || fileHdrSz != 28 || chunkHdrSz != 12)
                    {
                        Console.WriteLine($"file is not a simg {magic},{fileHdrSz},{chunkHdrSz}");
                        return "";
                    }
                    Console.WriteLine($"{path}: Total of {totalBlks} {blkSz}-byte output blocks in {totalChunks} input chunks.");
                    if (imageChecksum != 0) Console.WriteLine($"checksum=0x{imageChecksum:X8}");
                    long offset = 0;
                    for (int i = 1; i <= totalChunks; i++)
                    {
                        byte[] chunkHeaderBin = new byte[12];
                        FH.Read(chunkHeaderBin, 0, 12);
                        int chunkType = BitConverter.ToInt16(chunkHeaderBin, 0);
                        int chunkSz = BitConverter.ToInt32(chunkHeaderBin, 4);
                        int totalSz = BitConverter.ToInt32(chunkHeaderBin, 8);
                        long curPos = FH.Position;
                        long a = curPos;
                        int dataSz = totalSz - 12;
                        switch (chunkType)
                        {
                            case -13631: // Raw data
                                Type = "Raw data";
                                byte[] data = new byte[dataSz];
                                FH.Read(data, 0, dataSz);
                                break;
                            case -13630: // Fill chunk
                                if (dataSz != 4)
                                {
                                    Console.WriteLine($"Fill chunk should have 4 bytes of fill, but this has {dataSz}");
                                    return null;
                                }
                                byte[] fillBytes = new byte[4];
                                FH.Read(fillBytes, 0, 4);
                                int fillValue = BitConverter.ToInt32(fillBytes, 0);
                                Type = $"Fill with 0x{fillValue:X8}";
                                byte[] fillData = Enumerable.Repeat(fillValue, (int)(blkSz / 4)).SelectMany(BitConverter.GetBytes).ToArray();
                                break;
                            case -13629: // Don't care chunk
                                if (dataSz != 0)
                                {
                                    Console.WriteLine($"Don't care chunk input size is non-zero ({dataSz})");
                                    return null;
                                }
                                Type = "Don't care";
                                break;
                            case -13628: // CRC32 chunk
                                if (dataSz != 4)
                                {
                                    Console.WriteLine($"CRC32 chunk should have 4 bytes of CRC, but this has {dataSz}");
                                    return null;
                                }
                                byte[] crcBytes = new byte[4];
                                FH.Read(crcBytes, 0, 4);
                                int crcValue = BitConverter.ToInt32(crcBytes, 0);
                                Type = $"Unverified CRC32 0x{crcValue:X8}";
                                break;
                        }
                        FH.Position = a + dataSz;
                        Console.WriteLine($"{i,-4} {curPos,-10} {dataSz,-10} {offset,-7} {chunkSz,-7} " + Type);
                        offset = offset + chunkSz;
                    }
                    return null;
                }
            }
        }
    }
}
