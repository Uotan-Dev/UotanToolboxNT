using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ChromeosUpdateEngine;
using SharpCompress.Compressors.BZip2;
using SharpCompress.Compressors.Xz;

namespace UotanToolbox.Common
{
    internal class PayloadParser
    {
        private const string ZipMagic = "PK";
        private const string PayloadMagic = "CrAU";
        private const ulong BrilloMajorPayloadVersion = 2;
        private const string PayloadFilename = "payload.bin";
        public static void Extracet(string filepath, string[] extractFiles = null)
        {
            if (extractFiles == null)
            {
                extractFiles = [];
            }
            FileStream f = File.OpenRead(filepath);
            {
                if (IsZip(f))
                {
                    // Extract payload.bin from the zip first
                    f.Close();
                    //$"Input is a zip file, searching for {PayloadFilename} ..."

                    using (ZipArchive zr = ZipFile.OpenRead(filepath))
                    {
                        ZipArchiveEntry zf = FindPayload(zr);
                        if (zf == null)
                        {
                            throw new Exception($"{PayloadFilename} not found in the zip file");
                        }

                        //$"Extracting {PayloadFilename} ..."
                        using (FileStream outFile = File.Create(PayloadFilename))
                        using (Stream zfStream = zf.Open())
                        {
                            zfStream.CopyTo(outFile);
                        }
                    }

                    f = File.OpenRead(PayloadFilename);
                }
                PayloadParser parser = new PayloadParser();
                parser.ParsePayload(f, extractFiles);
            }
        }

        public static async Task ExtracetAsync(string filepath, string[] extractFiles = null)
        {
            if (extractFiles == null)
            {
                extractFiles = [];
            }
            FileStream f = File.OpenRead(filepath);
            {
                if (IsZip(f))
                {
                    // Extract payload.bin from the zip first
                    f.Close();
                    //$"Input is a zip file, searching for {PayloadFilename} ..."

                    using (ZipArchive zr = ZipFile.OpenRead(filepath))
                    {
                        ZipArchiveEntry zf = FindPayload(zr);
                        if (zf == null)
                        {
                            throw new Exception($"{PayloadFilename} not found in the zip file");
                        }

                        //$"Extracting {PayloadFilename} ..."
                        using (FileStream outFile = File.Create(PayloadFilename))
                        using (Stream zfStream = zf.Open())
                        {
                            zfStream.CopyTo(outFile);
                        }
                    }

                    f = File.OpenRead(PayloadFilename);
                }
                PayloadParser parser = new PayloadParser();
                await parser.ParsePayloadAsync(f, extractFiles);

            }
        }

        private static bool IsZip(FileStream f)
        {
            byte[] header = new byte[ZipMagic.Length];
            int bytesRead = f.Read(header, 0, header.Length);
            f.Seek(0, SeekOrigin.Begin);
            return bytesRead == header.Length && System.Text.Encoding.ASCII.GetString(header) == ZipMagic;
        }

        private static ZipArchiveEntry FindPayload(ZipArchive zr)
        {
            foreach (ZipArchiveEntry entry in zr.Entries)
            {
                if (entry.Name == PayloadFilename)
                {
                    return entry;
                }
            }
            return null;
        }
        private void ParsePayload(Stream stream, string[] extractFiles)
        {
            //"Parsing payload..."

            // Magic
            byte[] magic = new byte[PayloadMagic.Length];
            int bytesRead = stream.Read(magic, 0, magic.Length);
            if (bytesRead != magic.Length || System.Text.Encoding.ASCII.GetString(magic) != PayloadMagic)
            {
                throw new InvalidOperationException($"Incorrect magic ({System.Text.Encoding.ASCII.GetString(magic)})");
            }

            // Version & lengths
            ulong version = ReadUInt64BigEndian(stream);
            if (version != BrilloMajorPayloadVersion)
            {
                throw new InvalidOperationException($"Unsupported payload version ({version}). This tool only supports version {BrilloMajorPayloadVersion}");
            }

            ulong manifestLen = ReadUInt64BigEndian(stream);
            if (manifestLen <= 0)
            {
                throw new InvalidOperationException($"Incorrect manifest length ({manifestLen})");
            }

            uint metadataSigLen = ReadUInt32BigEndian(stream);
            if (metadataSigLen <= 0)
            {
                throw new InvalidOperationException($"Incorrect metadata signature length ({metadataSigLen})");
            }

            // Manifest
            byte[] manifestRaw = new byte[manifestLen];
            bytesRead = stream.Read(manifestRaw, 0, manifestRaw.Length);
            if ((ulong)bytesRead != manifestLen)
            {
                throw new InvalidOperationException($"Failed to read the manifest ({manifestLen})");
            }

            DeltaArchiveManifest manifest = DeltaArchiveManifest.Parser.ParseFrom(manifestRaw);
            if (manifest.MinorVersion != 0)
            {
                throw new InvalidOperationException("Delta payloads are not supported, please use a full payload file");
            }

            // Print manifest info
            //$"Block size: {manifest.BlockSize}, Partition count: {manifest.Partitions.Count}"

            // Extract partitions
            ExtractPartitions(manifest, stream, 24 + manifestLen + metadataSigLen, extractFiles);

            // Done
            //"Done!"
        }

        private ulong ReadUInt64BigEndian(Stream stream)
        {
            byte[] buffer = new byte[8];
            if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new EndOfStreamException();
            }
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
            return BitConverter.ToUInt64(buffer, 0);
        }

        private uint ReadUInt32BigEndian(Stream stream)
        {
            byte[] buffer = new byte[4];
            if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new EndOfStreamException();
            }
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
            return BitConverter.ToUInt32(buffer, 0);
        }
        private void ExtractPartitions(DeltaArchiveManifest manifest, Stream stream, ulong baseOffset, string[] extractFiles)
        {
            foreach (var partition in manifest.Partitions)
            {
                if (partition.PartitionName == null || (extractFiles.Length > 0 && !extractFiles.Contains(partition.PartitionName)))
                {
                    continue;
                }
                //$"Extracting {partition.PartitionName} ({partition.Operations.Count} ops) ..."
                string outFilename = $"{partition.PartitionName}.img";
                ExtractPartition(partition, outFilename, stream, baseOffset, manifest.BlockSize);
            }
        }
        public async Task ParsePayloadAsync(Stream stream, string[] extractFiles)
        {
            //"Parsing payload..."

            // Magic
            byte[] magic = new byte[PayloadMagic.Length];
            int bytesRead = await stream.ReadAsync(magic, 0, magic.Length);
            if (bytesRead != magic.Length || System.Text.Encoding.ASCII.GetString(magic) != PayloadMagic)
            {
                throw new InvalidOperationException($"Incorrect magic ({System.Text.Encoding.ASCII.GetString(magic)})");
            }

            // Version & lengths
            ulong version = await ReadUInt64BigEndianAsync(stream);
            if (version != BrilloMajorPayloadVersion)
            {
                throw new InvalidOperationException($"Unsupported payload version ({version}). This tool only supports version {BrilloMajorPayloadVersion}");
            }

            ulong manifestLen = await ReadUInt64BigEndianAsync(stream);
            if (manifestLen <= 0)
            {
                throw new InvalidOperationException($"Incorrect manifest length ({manifestLen})");
            }

            uint metadataSigLen = await ReadUInt32BigEndianAsync(stream);
            if (metadataSigLen <= 0)
            {
                throw new InvalidOperationException($"Incorrect metadata signature length ({metadataSigLen})");
            }

            // Manifest
            byte[] manifestRaw = new byte[manifestLen];
            bytesRead = await stream.ReadAsync(manifestRaw, 0, manifestRaw.Length);
            if ((ulong)bytesRead != manifestLen)
            {
                throw new InvalidOperationException($"Failed to read the manifest ({manifestLen})");
            }

            DeltaArchiveManifest manifest = DeltaArchiveManifest.Parser.ParseFrom(manifestRaw);
            if (manifest.MinorVersion != 0)
            {
                throw new InvalidOperationException("Delta payloads are not supported, please use a full payload file");
            }

            // Print manifest info
            //$"Block size: {manifest.BlockSize}, Partition count: {manifest.Partitions.Count}"

            // Extract partitions
            await ExtractPartitionsAsync(manifest, stream, 24 + manifestLen + metadataSigLen, extractFiles);

            // Done
        }

        private async Task<ulong> ReadUInt64BigEndianAsync(Stream stream)
        {
            byte[] buffer = new byte[8];
            if (await stream.ReadAsync(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new EndOfStreamException();
            }
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
            return BitConverter.ToUInt64(buffer, 0);
        }

        private async Task<uint> ReadUInt32BigEndianAsync(Stream stream)
        {
            byte[] buffer = new byte[4];
            if (await stream.ReadAsync(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new EndOfStreamException();
            }
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
            return BitConverter.ToUInt32(buffer, 0);
        }
        public async Task ExtractPartitionsAsync(DeltaArchiveManifest manifest, Stream stream, ulong baseOffset, string[] extractFiles)
        {
            var tasks = new List<Task>();

            foreach (var partition in manifest.Partitions)
            {
                if (partition.PartitionName == null || (extractFiles.Length > 0 && !extractFiles.Contains(partition.PartitionName)))
                {
                    continue;
                }

                //$"Extracting {partition.PartitionName} ({partition.Operations.Count} ops) ..."
                string outFilename = $"{partition.PartitionName}.img";
                tasks.Add(Task.Run(() => ExtractPartition(partition, outFilename, stream, baseOffset, manifest.BlockSize)));
            }
            await Task.WhenAll(tasks);
        }

        private void ExtractPartition(PartitionUpdate partition, string outFilename, Stream stream, ulong baseOffset, uint blockSize)
        {
            using (var outFile = new FileStream(outFilename, FileMode.Create, FileAccess.Write))
            {
                foreach (var op in partition.Operations)
                {
                    byte[] data = new byte[op.DataLength];
                    long dataPos = (long)(baseOffset + op.DataOffset);

                    stream.Seek(dataPos, SeekOrigin.Begin);
                    int bytesRead = stream.Read(data, 0, data.Length);
                    if (bytesRead != data.Length)
                    {
                        throw new InvalidOperationException($"Failed to read enough data from partition {outFilename}");
                    }

                    long outSeekPos = (long)(op.DstExtents[0].StartBlock * blockSize);
                    outFile.Seek(outSeekPos, SeekOrigin.Begin);

                    switch (op.Type)
                    {
                        case InstallOperation.Types.Type.Replace:
                            outFile.Write(data, 0, data.Length);
                            break;

                        case InstallOperation.Types.Type.ReplaceBz:
                            using (var bzr = new BZip2Stream(new MemoryStream(data), SharpCompress.Compressors.CompressionMode.Decompress, false))
                            {
                                bzr.CopyTo(outFile);
                            }
                            break;

                        case InstallOperation.Types.Type.ReplaceXz:
                            using (var xzr = new XZStream(new MemoryStream(data)))
                            {
                                xzr.CopyTo(outFile);
                            }
                            break;

                        case InstallOperation.Types.Type.Zero:
                            foreach (var ext in op.DstExtents)
                            {
                                outSeekPos = (long)(ext.StartBlock * blockSize);
                                outFile.Seek(outSeekPos, SeekOrigin.Begin);
                                byte[] zeros = new byte[ext.NumBlocks * blockSize];
                                outFile.Write(zeros, 0, zeros.Length);
                            }
                            break;

                        default:
                            throw new InvalidOperationException($"Unsupported operation type: {op.Type}");
                    }
                }
            }
        }
    }
}
