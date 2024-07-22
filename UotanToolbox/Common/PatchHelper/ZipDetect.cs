using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UotanToolbox.Common.PatchHelper
{
    internal class ZipDetect
    {
        public static async Task<ZipInfo> Zip_Detect(string path)
        {
            ZipInfo Zipinfo = new ZipInfo("", "", "", "", "", false, PatchMode.None, "");
            Zipinfo.Path = path;
            Zipinfo.SHA1 = await FileHelper.SHA1HashAsync(Zipinfo.Path);
            Zipinfo.TempPath = Path.Combine(Global.tmp_path, "Zip-" + StringHelper.RandomString(8));
            bool istempclean = FileHelper.ClearFolder(Zipinfo.TempPath);
            if (!istempclean)
            {
                throw new Exception("fatal error!");
            }

            await Task.Run(() =>
            {
                using (IArchive archive = ArchiveFactory.Open(path))
                {
                    foreach (IArchiveEntry entry in archive.Entries)
                    {
                        if (!entry.IsDirectory)
                        {
                            entry.WriteToDirectory(Zipinfo.TempPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                        }
                    }
                }
            });

            if (File.Exists(Path.Combine(Zipinfo.TempPath, "assets", "util_functions.sh")))
            {
                Zipinfo.Mode = PatchMode.Magisk;
            }
            else if (File.Exists(Path.Combine(Zipinfo.TempPath, "Image")))
            {
                Zipinfo.Mode = PatchMode.KernelSU;
            }
            else
            {
                throw new Exception("error when prasing zip file format");
            }
            switch (Zipinfo.Mode)
            {
                case PatchMode.Magisk:
                    Zipinfo.Version = Magisk_Ver(Zipinfo.SHA1);
                    await Magisk_Pre(Zipinfo.TempPath);
                    await Magisk_Compress(Zipinfo.TempPath);
                    Zipinfo.IsUseful = true;
                    break;
                case PatchMode.KernelSU:
                    (Zipinfo.SubSHA1, Zipinfo.IsUseful, Zipinfo.Version, Zipinfo.KMI) = await KernelSU_Valid(Zipinfo.TempPath);
                    break;
            }
            return Zipinfo;
        }
        private static string Magisk_Ver(string SHA1)
        {
            string magisk_ver;
            Dictionary<string, string> version_dict = new Dictionary<string, string>
            {
            {"872199f3781706f51b84d8a89c1d148d26bcdbad" , "27000"},
            {"dc7db76b5fb895d34b7274abb6ca59b56590a784" , "26400"},
            {"d052b0e1c1a83cb25739eb87471ba6d8791f4b5a" , "26300"}
             //其他的支持还没写，你要是看到这段文字可以考虑一下帮我写写然后PR到仓库。 -zicai
            };
            if (version_dict.TryGetValue(SHA1, out magisk_ver))
            {
                return magisk_ver;
            }
            else
            {
                throw new Exception("wrong magisk package");
            }
        }
        private static async Task Magisk_Pre(string temp_path)
        {
            List<(string SourcePath, string DestinationPath)> filesToCopy = new List<(string, string)>
            {
                (Path.Combine(temp_path, "lib", "armeabi-v7a", "libmagisk32.so"), Path.Combine(temp_path, "lib", "armeabi-v7a", "magisk32")),
                (Path.Combine(temp_path, "lib", "armeabi-v7a", "libmagiskinit.so"), Path.Combine(temp_path, "lib", "armeabi-v7a", "init")),
                (Path.Combine(temp_path, "lib", "armeabi-v7a", "libmagisk32.so"), Path.Combine(temp_path, "lib", "arm64-v8a", "magisk32")),
                (Path.Combine(temp_path, "lib", "arm64-v8a", "libmagisk64.so"), Path.Combine(temp_path, "lib", "arm64-v8a", "magisk64")),
                (Path.Combine(temp_path, "lib", "arm64-v8a", "libmagiskinit.so"), Path.Combine(temp_path, "lib", "arm64-v8a", "init")),
                (Path.Combine(temp_path, "lib", "x86", "libmagisk32.so"), Path.Combine(temp_path, "lib", "x86", "magisk32")),
                (Path.Combine(temp_path, "lib", "x86", "libmagiskinit.so"), Path.Combine(temp_path, "lib", "x86", "init")),
                (Path.Combine(temp_path, "lib", "x86", "libmagisk32.so"), Path.Combine(temp_path, "lib", "x86_64", "magisk32")),
                (Path.Combine(temp_path, "lib", "x86_64", "libmagisk64.so"), Path.Combine(temp_path, "lib", "x86_64", "magisk64")),
                (Path.Combine(temp_path, "lib", "x86_64", "libmagiskinit.so"), Path.Combine(temp_path, "lib", "x86_64", "init"))
            };
            await Task.WhenAll(filesToCopy.Select(async file =>
            {
                try
                {
                    using var sourceStream = new FileStream(file.SourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var destStream = new FileStream(file.DestinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    int bufferSize = Math.Min(4096, (int)(sourceStream.Length / 1024));
                    byte[] buffer = new byte[bufferSize];
                    int bytesRead;
                    while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, bufferSize)) > 0)
                    {
                        await destStream.WriteAsync(buffer, 0, bytesRead);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error copying file from {file.SourcePath} to {file.DestinationPath}: {ex.Message}");
                }
            }));
        }
        private static async Task Magisk_Compress(string temp_path)
        {
            List<Task<(string Output, int ExitCode)>> tasks = new List<Task<(string Output, int ExitCode)>>();
            tasks.Add(CallExternalProgram.MagiskBoot($"compress=xz magisk32 magisk32.xz", Path.Combine(temp_path, "lib", "armeabi-v7a")));
            tasks.Add(CallExternalProgram.MagiskBoot($"compress=xz magisk32 magisk32.xz", Path.Combine(temp_path, "lib", "arm64-v8a")));
            tasks.Add(CallExternalProgram.MagiskBoot($"compress=xz magisk32 magisk32.xz", Path.Combine(temp_path, "lib", "x86")));
            tasks.Add(CallExternalProgram.MagiskBoot($"compress=xz magisk32 magisk32.xz", Path.Combine(temp_path, "lib", "x86_64")));
            tasks.Add(CallExternalProgram.MagiskBoot($"compress=xz magisk64 magisk64.xz", Path.Combine(temp_path, "lib", "arm64-v8a")));
            tasks.Add(CallExternalProgram.MagiskBoot($"compress=xz magisk64 magisk64.xz", Path.Combine(temp_path, "lib", "x86_64")));
            tasks.Add(CallExternalProgram.MagiskBoot($"compress=xz stub.apk stub.xz", Path.Combine(temp_path, "assets")));
            await Task.WhenAll(tasks);
            foreach (var task in tasks)
            {
                (string result, int exitcode) = await task;
                if (exitcode != 0)
                {
                    throw new Exception("magiskboot error " + result);
                }
            }
        }
        private static async Task<(string, bool, string, string)> KernelSU_Valid(string temp_path)
        {
            string SubSHA1 = await FileHelper.SHA1HashAsync(Path.Combine(temp_path, "Image"));
            string KMI = null;
            string version = null;
            bool useful = false;
            Dictionary<string, string> version_dict = new Dictionary<string, string>
            {
                {"453111373ba9ed3ac487b128f07a28b73dd89ee9" , "1.0.0"},
                {"85e6564ebe4e394e4e31fdd20004672bfec58413" , "1.0.0"},
                {"0f1f4a79fcc5f17c989e7c89141060506c2ba467" , "1.0.0"},
                {"046cae58d6260767215d49abf22298681e032578" , "1.0.0"},
                {"8746c63c501365fac1bfc5e4e9f4ec66dbda39bd" , "1.0.0"},
                {"c3c25886bfb49f188bc66a342638e2b01eb30f8d" , "1.0.0"},
                {"b9711a989d0b887559999376a1bc107acc67b91f" , "1.0.0"},
                {"4f5b2afdc73ca3aa16296e2fa1728cfed78333c0" , "1.0.0"},
                {"c29857e0968ccc23f8d8e3812188146a98679146" , "1.0.0"},
                {"ba4ff1cf79230e09633681c9d245534bcc5ae22a" , "1.0.0"},
                {"428c20454f4e16f01432849242bfeb008403d690" , "1.0.0"},
                {"4d620304e6d36950967aa48d214c9f836e853d2e" , "1.0.0"},
                {"d1b45316531d2ddebd173eb30e73f0debbaeda4f" , "1.0.0"},
                {"19d1475da4c376cc69e39c7a887327cbbb555a88" , "1.0.0"},
                {"6f4384f69c8a71021546f761fe064464fd00f8ee" , "1.0.0"},
                {"2cc6588e5917fa248b6864bd63c532f79be2096c" , "1.0.0"},
                {"d1575ad4699d107baa240dc40599e5086957233b" , "1.0.0"},
                {"a3bc027c5dea15ecfa9c209d7fc5b8c79f2d7165" , "1.0.0"},
                {"98f6d7ef98e345ff4aea856db0868f54a53039ef" , "1.0.0"},
                {"48c9b20d906e606cd267f60faf3943c11e98f6d0" , "1.0.0"}
             //其他的支持还没写，你要是看到这段文字可以考虑一下帮我写写然后PR到仓库。 -zicai
            };
            if (!version_dict.TryGetValue(SubSHA1, out version))
            {
                throw new Exception("wrong KernelSU package");
            }
            Task<string> KernelVersionTask = Task.Run(() => FileHelper.ReadKernelVersion(Path.Combine(temp_path, "Image")));
            string KernelVersion = await KernelVersionTask;
            Task<string> kmiTask = Task.Run(() => StringHelper.ExtractKMI(KernelVersion));
            KMI = await kmiTask;
            useful = true;
            return (SubSHA1, useful, version, KMI);
        }
    }
}
