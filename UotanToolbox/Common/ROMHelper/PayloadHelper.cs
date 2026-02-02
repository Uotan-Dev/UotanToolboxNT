using ChromeosUpdateEngine;
using SharpCompress.Compressors.BZip2;
using SharpCompress.Compressors.Xz;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UotanToolbox.Common.ROMHelper
{
    internal class PayloadParser
    {
        private const string ZipMagic = "PK";
        private const string PayloadMagic = "CrAU";
        private const ulong BrilloMajorPayloadVersion = 2;
        private const string PayloadFilename = "payload.bin";

        /// <summary>
        /// 从 payload 文件中解压分区（可按分区名过滤）。
        /// </summary>
        public static void Extracet(string filepath, string[] extractFiles = null)
        {
            if (extractFiles == null)
            {
                extractFiles = Array.Empty<string>();
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

        /// <summary>
        /// 异步从 payload 文件中解压分区（可按分区名过滤）。
        /// </summary>
        public static async Task ExtracetAsync(string filepath, string[] extractFiles = null)
        {
            if (extractFiles == null)
            {
                extractFiles = Array.Empty<string>();
            }
            FileStream f = File.OpenRead(filepath);
            {
                if (IsZip(f))
                {
                    f.Close();

                    using (ZipArchive zr = ZipFile.OpenRead(filepath))
                    {
                        ZipArchiveEntry zf = FindPayload(zr);
                        if (zf == null)
                        {
                            throw new Exception($"{PayloadFilename} not found in the zip file");
                        }

                        using (FileStream outFile = File.Create(PayloadFilename))
                        using (Stream zfStream = zf.Open())
                        {
                            await zfStream.CopyToAsync(outFile);
                        }
                    }

                    f = File.OpenRead(PayloadFilename);
                }
                PayloadParser parser = new PayloadParser();
                await parser.ParsePayloadAsync(f, extractFiles);

            }
        }

        /// <summary>
        /// 通过魔数判断流是否为 zip 文件。
        /// </summary>
        private static bool IsZip(FileStream f)
        {
            byte[] header = new byte[ZipMagic.Length];
            int bytesRead = f.Read(header, 0, header.Length);
            f.Seek(0, SeekOrigin.Begin);
            return bytesRead == header.Length && Encoding.ASCII.GetString(header) == ZipMagic;
        }

        /// <summary>
        /// 在 zip 包中查找 payload 条目。
        /// </summary>
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

        public class PayloadMetadata
        {
            public uint BlockSize { get; set; }
            public List<string> PartitionNames { get; } = new List<string>();
            public ulong ManifestLength { get; set; }
            public uint MetadataSignatureLength { get; set; }
            public ulong BaseOffset { get; set; }
            public DeltaArchiveManifest Manifest { get; set; }
        }

        /// <summary>
        /// 不解压分区数据，读取 payload 元信息与分区列表。
        /// </summary>
        public static PayloadMetadata GetPayloadMetadata(string filepath)
        {
            PayloadParser parser = new PayloadParser();
            using (FileStream f = File.OpenRead(filepath))
            {
                if (IsZip(f))
                {
                    using (ZipArchive zr = ZipFile.OpenRead(filepath))
                    {
                        ZipArchiveEntry zf = FindPayload(zr);
                        if (zf == null)
                        {
                            throw new FileNotFoundException($"{PayloadFilename} not found in the zip file");
                        }

                        using (Stream zfStream = zf.Open())
                        {
                            return BuildMetadata(parser.ReadManifest(zfStream));
                        }
                    }
                }

                f.Seek(0, SeekOrigin.Begin);
                return BuildMetadata(parser.ReadManifest(f));
            }
        }

        /// <summary>
        /// 异步不解压分区数据，读取 payload 元信息与分区列表。
        /// </summary>
        public static async Task<PayloadMetadata> GetPayloadMetadataAsync(string filepath)
        {
            PayloadParser parser = new PayloadParser();
            using (FileStream f = File.OpenRead(filepath))
            {
                if (IsZip(f))
                {
                    using (ZipArchive zr = ZipFile.OpenRead(filepath))
                    {
                        ZipArchiveEntry zf = FindPayload(zr);
                        if (zf == null)
                        {
                            throw new FileNotFoundException($"{PayloadFilename} not found in the zip file");
                        }

                        using (Stream zfStream = zf.Open())
                        {
                            return BuildMetadata(await parser.ReadManifestAsync(zfStream));
                        }
                    }
                }

                f.Seek(0, SeekOrigin.Begin);
                return BuildMetadata(await parser.ReadManifestAsync(f));
            }
        }

        /// <summary>
        /// 仅解压指定分区。
        /// </summary>
        public static void ExtractSelectedPartitions(string filepath, string[] partitionNames = null)
        {
            partitionNames ??= Array.Empty<string>();

            string payloadPath = filepath;
            string tempPath = null;

            using (FileStream fcheck = File.OpenRead(filepath))
            {
                if (IsZip(fcheck))
                {
                    using (ZipArchive zr = ZipFile.OpenRead(filepath))
                    {
                        ZipArchiveEntry zf = FindPayload(zr);
                        if (zf == null)
                        {
                            throw new FileNotFoundException($"{PayloadFilename} not found in the zip file");
                        }

                        tempPath = Path.GetTempFileName();
                        using (FileStream outFile = File.Create(tempPath))
                        using (Stream zfStream = zf.Open())
                        {
                            zfStream.CopyTo(outFile);
                        }
                        payloadPath = tempPath;
                    }
                }
            }

            try
            {
                using (FileStream stream = File.OpenRead(payloadPath))
                {
                    PayloadParser parser = new PayloadParser();
                    var (manifest, manifestLen, metadataSigLen, baseOffset) = parser.ReadManifest(stream);

                    IEnumerable<PartitionUpdate> partitions = manifest.Partitions.Where(p => p.PartitionName != null);
                    if (partitionNames.Length > 0)
                    {
                        partitions = partitions.Where(p => partitionNames.Contains(p.PartitionName));
                    }

                    foreach (var partition in partitions)
                    {
                        string outFilename = $"{partition.PartitionName}.img";
                        parser.ExtractPartition(partition, outFilename, stream, baseOffset, manifest.BlockSize);
                    }
                }
            }
            finally
            {
                if (tempPath != null)
                {
                    try { File.Delete(tempPath); } catch { }
                }
            }
        }

        /// <summary>
        /// 异步仅解压指定分区。
        /// </summary>
        public static async Task ExtractSelectedPartitionsAsync(string filepath, string[] partitionNames = null)
        {
            partitionNames ??= Array.Empty<string>();

            string payloadPath = filepath;
            string tempPath = null;

            using (FileStream fcheck = File.OpenRead(filepath))
            {
                if (IsZip(fcheck))
                {
                    using (ZipArchive zr = ZipFile.OpenRead(filepath))
                    {
                        ZipArchiveEntry zf = FindPayload(zr);
                        if (zf == null)
                        {
                            throw new FileNotFoundException($"{PayloadFilename} not found in the zip file");
                        }

                        tempPath = Path.GetTempFileName();
                        using (FileStream outFile = File.Create(tempPath))
                        using (Stream zfStream = zf.Open())
                        {
                            await zfStream.CopyToAsync(outFile);
                        }
                        payloadPath = tempPath;
                    }
                }
            }

            try
            {
                using (FileStream stream = File.OpenRead(payloadPath))
                {
                    PayloadParser parser = new PayloadParser();
                    var (manifest, manifestLen, metadataSigLen, baseOffset) = await parser.ReadManifestAsync(stream);

                    IEnumerable<PartitionUpdate> partitions = manifest.Partitions.Where(p => p.PartitionName != null);
                    if (partitionNames.Length > 0)
                    {
                        partitions = partitions.Where(p => partitionNames.Contains(p.PartitionName));
                    }

                    var tasks = new List<Task>();
                    foreach (var partition in partitions)
                    {
                        string outFilename = $"{partition.PartitionName}.img";
                        tasks.Add(Task.Run(() =>
                        {
                            using (FileStream readStream = File.OpenRead(payloadPath))
                            {
                                parser.ExtractPartition(partition, outFilename, readStream, baseOffset, manifest.BlockSize);
                            }
                        }));
                    }
                    await Task.WhenAll(tasks);
                }
            }
            finally
            {
                if (tempPath != null)
                {
                    try { File.Delete(tempPath); } catch { }
                }
            }
        }

        /// <summary>
        /// 由解析后的 manifest 元组构建元信息对象。
        /// </summary>
        private static PayloadMetadata BuildMetadata((DeltaArchiveManifest manifest, ulong manifestLen, uint metadataSigLen, ulong baseOffset) data)
        {
            var (manifest, manifestLen, metadataSigLen, baseOffset) = data;
            var meta = new PayloadMetadata
            {
                BlockSize = manifest.BlockSize,
                ManifestLength = manifestLen,
                MetadataSignatureLength = metadataSigLen,
                BaseOffset = baseOffset,
                Manifest = manifest
            };

            foreach (var p in manifest.Partitions)
            {
                if (!string.IsNullOrEmpty(p.PartitionName))
                {
                    meta.PartitionNames.Add(p.PartitionName);
                }
            }

            return meta;
        }

        /// <summary>
        /// 从 payload 流读取并解析 manifest。
        /// </summary>
        private (DeltaArchiveManifest manifest, ulong manifestLen, uint metadataSigLen, ulong baseOffset) ReadManifest(Stream stream)
        {
            byte[] magic = new byte[PayloadMagic.Length];
            int bytesRead = stream.Read(magic, 0, magic.Length);
            if (bytesRead != magic.Length || Encoding.ASCII.GetString(magic) != PayloadMagic)
            {
                throw new InvalidOperationException($"Incorrect magic ({Encoding.ASCII.GetString(magic)})");
            }

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

            ulong baseOffset = 24 + manifestLen + metadataSigLen;
            return (manifest, manifestLen, metadataSigLen, baseOffset);
        }

        /// <summary>
        /// 异步从 payload 流读取并解析 manifest。
        /// </summary>
        private async Task<(DeltaArchiveManifest manifest, ulong manifestLen, uint metadataSigLen, ulong baseOffset)> ReadManifestAsync(Stream stream)
        {
            byte[] magic = new byte[PayloadMagic.Length];
            int bytesRead = await stream.ReadAsync(magic, 0, magic.Length);
            if (bytesRead != magic.Length || Encoding.ASCII.GetString(magic) != PayloadMagic)
            {
                throw new InvalidOperationException($"Incorrect magic ({Encoding.ASCII.GetString(magic)})");
            }

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

            ulong baseOffset = 24 + manifestLen + metadataSigLen;
            return (manifest, manifestLen, metadataSigLen, baseOffset);
        }

        /// <summary>
        /// 解析 payload 并解压分区为镜像文件。
        /// </summary>
        private void ParsePayload(Stream stream, string[] extractFiles)
        {
            //"Parsing payload..."

            // Magic
            byte[] magic = new byte[PayloadMagic.Length];
            int bytesRead = stream.Read(magic, 0, magic.Length);
            if (bytesRead != magic.Length || Encoding.ASCII.GetString(magic) != PayloadMagic)
            {
                throw new InvalidOperationException($"Incorrect magic ({Encoding.ASCII.GetString(magic)})");
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

        /// <summary>
        /// 从流读取大端 UInt64。
        /// </summary>
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

        /// <summary>
        /// 从流读取大端 UInt32。
        /// </summary>
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
        /// <summary>
        /// 解压 manifest 中描述的分区。
        /// </summary>
        private void ExtractPartitions(DeltaArchiveManifest manifest, Stream stream, ulong baseOffset, string[] extractFiles)
        {
            foreach (var partition in manifest.Partitions)
            {
                if (partition.PartitionName == null || extractFiles.Length > 0 && !extractFiles.Contains(partition.PartitionName))
                {
                    continue;
                }
                //$"Extracting {partition.PartitionName} ({partition.Operations.Count} ops) ..."
                string outFilename = $"{partition.PartitionName}.img";
                ExtractPartition(partition, outFilename, stream, baseOffset, manifest.BlockSize);
            }
        }
        /// <summary>
        /// 异步解析 payload 并解压分区为镜像文件。
        /// </summary>
        public async Task ParsePayloadAsync(Stream stream, string[] extractFiles)
        {
            //"Parsing payload..."

            // Magic
            byte[] magic = new byte[PayloadMagic.Length];
            int bytesRead = await stream.ReadAsync(magic, 0, magic.Length);
            if (bytesRead != magic.Length || Encoding.ASCII.GetString(magic) != PayloadMagic)
            {
                throw new InvalidOperationException($"Incorrect magic ({Encoding.ASCII.GetString(magic)})");
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

        /// <summary>
        /// 异步从流读取大端 UInt64。
        /// </summary>
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

        /// <summary>
        /// 异步从流读取大端 UInt32。
        /// </summary>
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
        /// <summary>
        /// 异步解压 manifest 中描述的分区。
        /// </summary>
        public async Task ExtractPartitionsAsync(DeltaArchiveManifest manifest, Stream stream, ulong baseOffset, string[] extractFiles)
        {
            var tasks = new List<Task>();

            foreach (var partition in manifest.Partitions)
            {
                if (partition.PartitionName == null || extractFiles.Length > 0 && !extractFiles.Contains(partition.PartitionName))
                {
                    continue;
                }

                //$"Extracting {partition.PartitionName} ({partition.Operations.Count} ops) ..."
                string outFilename = $"{partition.PartitionName}.img";
                tasks.Add(Task.Run(() => ExtractPartition(partition, outFilename, stream, baseOffset, manifest.BlockSize)));
            }
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 解压单个分区到镜像文件。
        /// </summary>
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
