using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UotanToolbox.Common.PatchHelper
{
    internal static class KsuAssetExtractor
    {
        public static string ExtractKoFromApk(string apkPath, string outputDir, string arch, string kernelVersion)
        {
            string archSubfolder = arch switch
            {
                "aarch64" => "arm64-v8a",
                //"X86-64" => "x86_64",
                _ => throw new ArgumentException($"Unsupported architecture: {arch}")
            };

            using (ZipArchive archive = ZipFile.OpenRead(apkPath))
            {
                var ksudEntry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith($"lib/{archSubfolder}/libksud.so"));
                if (ksudEntry == null)
                {
                    ksudEntry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith("libksud.so") && e.FullName.Contains(archSubfolder));
                }

                if (ksudEntry == null)
                {
                    throw new Exception($"Could not find libksud.so for architecture {archSubfolder} in APK.");
                }

                using (Stream s = ksudEntry.Open())
                using (MemoryStream ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    byte[] data = ms.ToArray();
                    var extractedFiles = ProcessData(data, outputDir);
                    
                    if (!string.IsNullOrEmpty(kernelVersion))
                    {
                        var match = extractedFiles.Keys.FirstOrDefault(k => k.EndsWith(".ko") && k.Contains(kernelVersion));
                        if (match != null)
                        {
                            return Path.Combine(outputDir, match);
                        }
                        
                        throw new Exception($"Cannot find a matching kernelsu.ko for kernel version {kernelVersion} in this APK.");
                    }

                    var koFile = extractedFiles.Keys.FirstOrDefault(k => k.EndsWith(".ko"));
                    if (koFile != null)
                    {
                        return Path.Combine(outputDir, koFile);
                    }
                }
            }

            return null;
        }

        private static Dictionary<string, byte[]> ProcessData(byte[] data, string outputDir)
        {
            var results = new Dictionary<string, byte[]>();

            List<string> recordedNames = new List<string>();
            string rawString = Encoding.Default.GetString(data.Select(b => (b >= 32 && b <= 126) ? b : (byte)'.').ToArray());
            var nameRegex = new Regex(@"android\d+-\d+\.\d+_kernelsu\.ko|bootctl|busybox|resetprop|ksud");
            foreach (Match m in nameRegex.Matches(rawString))
            {
                if (!recordedNames.Contains(m.Value)) recordedNames.Add(m.Value);
            }

            for (int i = 0; i < data.Length - 100; i++)
            {
                var content = TryDecompressRaw(data, i, out int consumed);
                if (content != null && IsElf(content))
                {
                    string finalName = IdentifyAsset(content, recordedNames, i);
                    
                    string uniqueName = finalName;
                    int counter = 1;
                    while (results.ContainsKey(uniqueName))
                    {
                        var ext = Path.GetExtension(finalName);
                        var baseName = Path.GetFileNameWithoutExtension(finalName);
                        uniqueName = $"{baseName}_{counter}{ext}";
                        counter++;
                    }

                    results[uniqueName] = content;
                    if (!string.IsNullOrEmpty(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                        File.WriteAllBytes(Path.Combine(outputDir, uniqueName), content);
                    }

                    i += Math.Max(1, consumed - 1);
                }
            }

            return results;
        }

        private static string IdentifyAsset(byte[] content, List<string> recordedNames, int offset)
        {
            string vermagic = GetVermagic(content);
            if (!string.IsNullOrEmpty(vermagic))
            {
                var koMatch = recordedNames.FirstOrDefault(k => k.EndsWith(".ko") && 
                    Regex.IsMatch(k, @"-(\d+\.\d+)_") && 
                    vermagic.Contains(Regex.Match(k, @"-(\d+\.\d+)_").Groups[1].Value));

                if (koMatch != null) return koMatch;
                return $"kernel_{vermagic.Replace(' ', '_')}.ko";
            }

            var toolMatch = recordedNames.FirstOrDefault(t => !t.EndsWith(".ko") && ContainsBytes(content, Encoding.ASCII.GetBytes(t)));
            if (toolMatch != null) return toolMatch;

            if (ContainsBytes(content, Encoding.ASCII.GetBytes("BusyBox"))) return "busybox";
            if (ContainsBytes(content, Encoding.ASCII.GetBytes("resetprop"))) return "resetprop";
            if (ContainsBytes(content, Encoding.ASCII.GetBytes("ksu"))) return "ksud";

            return $"asset_0x{offset:x}.elf";
        }

        private static byte[] TryDecompressRaw(byte[] data, int offset, out int consumed)
        {
            consumed = 0;
            try
            {
                using (var ms = new MemoryStream(data, offset, data.Length - offset))
                using (var tracker = new PositionTrackerStream(ms))
                using (var ds = new DeflateStream(tracker, CompressionMode.Decompress))
                using (var outMs = new MemoryStream())
                {
                    byte[] head = new byte[4];
                    int r = ds.Read(head, 0, 4);
                    if (r == 4 && head[0] == 0x7f && head[1] == 0x45 && head[2] == 0x4c && head[3] == 0x46)
                    {
                        outMs.Write(head, 0, 4);
                        ds.CopyTo(outMs);
                        consumed = (int)tracker.Position;
                        return outMs.ToArray();
                    }
                }
            }
            catch { }
            return null;
        }

        private static bool IsElf(byte[] data) => data.Length > 4 && data[0] == 0x7f && data[1] == 0x45 && data[2] == 0x4c && data[3] == 0x46;

        private static string GetVermagic(byte[] data)
        {
            byte[] pattern = Encoding.ASCII.GetBytes("vermagic=");
            int idx = FindPattern(data, pattern);
            if (idx == -1) return null;
            var bytes = data.Skip(idx + pattern.Length).Take(64).TakeWhile(b => b != 0).ToArray();
            return Encoding.ASCII.GetString(bytes);
        }

        private static int FindPattern(byte[] source, byte[] pattern)
        {
            for (int i = 0; i < source.Length - pattern.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < pattern.Length; j++) if (source[i + j] != pattern[j]) { found = false; break; }
                if (found) return i;
            }
            return -1;
        }

        private static bool ContainsBytes(byte[] source, byte[] pattern) => FindPattern(source, pattern) != -1;

        private class PositionTrackerStream : Stream
        {
            private readonly Stream _inner;
            public PositionTrackerStream(Stream inner) => _inner = inner;
            public override long Position { get => _inner.Position; set => _inner.Position = value; }
            public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
            public override bool CanRead => _inner.CanRead;
            public override bool CanSeek => _inner.CanSeek;
            public override bool CanWrite => false;
            public override long Length => _inner.Length;
            public override void Flush() => _inner.Flush();
            public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
            public override void SetLength(long value) => _inner.SetLength(value);
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }
    }
}
