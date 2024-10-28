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
        private static string GetTranslation(string key)
        {
            return FeaturesHelper.GetTranslation(key);
        }

        public static async Task<ZipInfo> Zip_Detect(string path)
        {
            string kmi;
            ZipInfo Zipinfo = new ZipInfo("", "", "", "", "", false, PatchMode.None, "")
            {
                Path = path
            };
            Zipinfo.SHA1 = await FileHelper.SHA1HashAsync(Zipinfo.Path);
            Zipinfo.TempPath = Path.Combine(Global.tmp_path, "Zip-" + StringHelper.RandomString(8));
            bool istempclean = FileHelper.ClearFolder(Zipinfo.TempPath);
            if (!istempclean)
            {
                throw new Exception(GetTranslation("Basicflash_FatalError"));
            }
            Dictionary<string, string> lkm_dict = new Dictionary<string, string>
            {
                {"01d17cd7027c752add00f10e65952f9ba766050d" , "1.0.0"},
                {"fc071efd0b0d89b589f60c0819104d8a3ad6683f" , "1.0.0"},
                {"575b9baf2ecc7dc94fbccfef2a381962b40efdbd" , "1.0.0"},
                {"0fc7d297139587c0b4a3bea6384f9a23e24b89da" , "1.0.0"},
                {"0cf6d72ef11db286dcb719f67e9cb3335ed1a397" , "1.0.0"},
                {"c271a5c2dfdbb0070c49c8276d691ce13a3e5938" , "1.0.1"},
                {"4a816aadf741dbfeb84d31c09cefa4f19b748e2f" , "1.0.1"},
                {"0022acdfd7e827ca2828c4803f9fc38efc05446e" , "1.0.1"},
                {"4f8542a4c3fc76613b75b13d34c8baa6a3954bdf" , "1.0.1"},
                {"2c2c51cb82d80d022687c287f1539aecdd93e8f4" , "1.0.1"}
            };
            if (lkm_dict.TryGetValue(Zipinfo.SHA1, out string lkm_version))
            {
                File.Copy(Zipinfo.Path, Path.Combine(Zipinfo.TempPath, "kernelsu.ko"), true);
                Zipinfo.Mode = PatchMode.LKM;

            }
            else
            {
                await Task.Run(() =>
                {
                    using IArchive archive = ArchiveFactory.Open(path);
                    foreach (IArchiveEntry entry in archive.Entries)
                    {
                        if (!entry.IsDirectory)
                        {
                            entry.WriteToDirectory(Zipinfo.TempPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                        }
                    }
                });

                Zipinfo.Mode = File.Exists(Path.Combine(Zipinfo.TempPath, "assets", "util_functions.sh"))
                    ? PatchMode.Magisk
                    : File.Exists(Path.Combine(Zipinfo.TempPath, "Image"))
                        ? PatchMode.GKI
                        : throw new Exception(GetTranslation("Basicflash_ZipError"));
            }
            switch (Zipinfo.Mode)
            {
                case PatchMode.Magisk:
                    Zipinfo.SubSHA1 = await FileHelper.SHA1HashAsync(Path.Combine(Zipinfo.TempPath, "assets", "util_functions.sh"));
                    Zipinfo.Version = Magisk_Ver(Zipinfo.SHA1);
                    await Magisk_Pre(Zipinfo.TempPath);
                    await Magisk_Compress(Zipinfo.TempPath);
                    Zipinfo.IsUseful = true;
                    break;
                case PatchMode.GKI:
                    (Zipinfo.SubSHA1, Zipinfo.IsUseful, Zipinfo.Version, Zipinfo.KMI) = await GKI_Valid(Zipinfo.TempPath);
                    break;
                case PatchMode.LKM:
                    Dictionary<string, string> kmi_dict = new Dictionary<string, string>
                    {
                        {"01d17cd7027c752add00f10e65952f9ba766050d" , "android12-5.10"},
                        {"fc071efd0b0d89b589f60c0819104d8a3ad6683f" , "android13-5.10"},
                        {"575b9baf2ecc7dc94fbccfef2a381962b40efdbd" , "android13-5.15"},
                        {"0fc7d297139587c0b4a3bea6384f9a23e24b89da" , "android14-5.15"},
                        {"0cf6d72ef11db286dcb719f67e9cb3335ed1a397" , "android14-6.1"},
                        {"c271a5c2dfdbb0070c49c8276d691ce13a3e5938" , "android12-5.10"},
                        {"4a816aadf741dbfeb84d31c09cefa4f19b748e2f" , "android13-5.10"},
                        {"0022acdfd7e827ca2828c4803f9fc38efc05446e" , "android13-5.15"},
                        {"4f8542a4c3fc76613b75b13d34c8baa6a3954bdf" , "android14-5.15"},
                        {"2c2c51cb82d80d022687c287f1539aecdd93e8f4" , "android14-6.1"}
                    };
                    Zipinfo.Version = lkm_version;
                    Zipinfo.IsUseful = true;
                    _ = kmi_dict.TryGetValue(Zipinfo.SHA1, out kmi);
                    Zipinfo.KMI = kmi;
                    break;
            }
            return Zipinfo;
        }
        private static string Magisk_Ver(string SHA1)
        {
            Dictionary<string, string> version_dict = new Dictionary<string, string>
            {
                //{"84c3cdea6f4b10d0e2abeb24bdfead502a348a63" , "28000"},
                {"872199f3781706f51b84d8a89c1d148d26bcdbad" , "27000"},
                {"dc7db76b5fb895d34b7274abb6ca59b56590a784" , "26400"},
                {"d052b0e1c1a83cb25739eb87471ba6d8791f4b5a" , "26300"}
             //其他的支持还没写，你要是看到这段文字可以考虑一下帮我写写然后PR到仓库。 -zicai
            };
            return version_dict.TryGetValue(SHA1, out string magisk_ver) ? magisk_ver : throw new Exception(GetTranslation("Basicflash_MagsikError"));
        }
        private static async Task Magisk_Pre(string temp_path)
        {
            List<(string SourcePath, string DestinationPath)> filesToCopy;
            if (File.Exists(Path.Combine(temp_path, "lib", "armeabi-v7a", "libmagisk32.so")))
            {

                filesToCopy =
                [
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
                ];
            }
            else
            {
                filesToCopy =
                [
                (Path.Combine(temp_path, "lib", "armeabi-v7a", "libmagisk.so"), Path.Combine(temp_path, "lib", "armeabi-v7a", "magisk32")),
                (Path.Combine(temp_path, "lib", "armeabi-v7a", "libmagiskinit.so"), Path.Combine(temp_path, "lib", "armeabi-v7a", "init")),
                (Path.Combine(temp_path, "lib", "armeabi-v7a", "libmagisk.so"), Path.Combine(temp_path, "lib", "arm64-v8a", "magisk32")),
                (Path.Combine(temp_path, "lib", "arm64-v8a", "libmagisk.so"), Path.Combine(temp_path, "lib", "arm64-v8a", "magisk64")),
                (Path.Combine(temp_path, "lib", "arm64-v8a", "libmagiskinit.so"), Path.Combine(temp_path, "lib", "arm64-v8a", "init")),
                (Path.Combine(temp_path, "lib", "x86", "libmagisk.so"), Path.Combine(temp_path, "lib", "x86", "magisk32")),
                (Path.Combine(temp_path, "lib", "x86", "libmagiskinit.so"), Path.Combine(temp_path, "lib", "x86", "init")),
                (Path.Combine(temp_path, "lib", "x86", "libmagisk.so"), Path.Combine(temp_path, "lib", "x86_64", "magisk32")),
                (Path.Combine(temp_path, "lib", "x86_64", "libmagisk.so"), Path.Combine(temp_path, "lib", "x86_64", "magisk64")),
                (Path.Combine(temp_path, "lib", "x86_64", "libmagiskinit.so"), Path.Combine(temp_path, "lib", "x86_64", "init"))
                ];
            }
            await Task.WhenAll(filesToCopy.Select(async file =>
            {
                try
                {
                    using FileStream sourceStream = new FileStream(file.SourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using FileStream destStream = new FileStream(file.DestinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
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
                    throw new Exception($"Error copying file from {file.SourcePath} to {file.DestinationPath}: {ex.Message}");
                }
            }));
        }
        private static async Task Magisk_Compress(string temp_path)
        {
            List<Task<(string Output, int ExitCode)>> tasks =
            [
                CallExternalProgram.MagiskBoot($"compress=xz magisk32 magisk32.xz", Path.Combine(temp_path, "lib", "armeabi-v7a")),
                CallExternalProgram.MagiskBoot($"compress=xz magisk32 magisk32.xz", Path.Combine(temp_path, "lib", "arm64-v8a")),
                CallExternalProgram.MagiskBoot($"compress=xz magisk32 magisk32.xz", Path.Combine(temp_path, "lib", "x86")),
                CallExternalProgram.MagiskBoot($"compress=xz magisk32 magisk32.xz", Path.Combine(temp_path, "lib", "x86_64")),
                CallExternalProgram.MagiskBoot($"compress=xz magisk64 magisk64.xz", Path.Combine(temp_path, "lib", "arm64-v8a")),
                CallExternalProgram.MagiskBoot($"compress=xz magisk64 magisk64.xz", Path.Combine(temp_path, "lib", "x86_64")),
                CallExternalProgram.MagiskBoot($"compress=xz stub.apk stub.xz", Path.Combine(temp_path, "assets")),
            ];
            _ = await Task.WhenAll(tasks);
            foreach (Task<(string Output, int ExitCode)> task in tasks)
            {
                (string result, int exitcode) = await task;
                if (exitcode != 0)
                {
                    throw new Exception("magiskboot error " + result);
                }
            }
        }
        private static async Task<(string, bool, string, string)> GKI_Valid(string temp_path)
        {
            string SubSHA1 = await FileHelper.SHA1HashAsync(Path.Combine(temp_path, "Image"));
            string KMI = null;
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
                {"48c9b20d906e606cd267f60faf3943c11e98f6d0" , "1.0.0"},
                {"341994cc66dae16dceec59847650ef021d08f3dc" , "1.0.1"},
                {"1cdb61d25a962c898cd4bc5c9f74e81570dcb59b" , "1.0.1"},
                {"0e54c270826cbcbd3cec3f07ac03cc83dae761f1" , "1.0.1"},
                {"ae1f4b43230f2a8f75e96578f2eb8217902944c5" , "1.0.1"},
                {"8de5af00110aac33fc43b262a1a7af69ac58a5a8" , "1.0.1"},
                {"7e941308cf49f55b43fc9a20a583a71f3fcaea8a" , "1.0.1"},
                {"0fdf3896d26621b523ecd7e53527cba2b06650f1" , "1.0.1"},
                {"484c9f384d0d78c2f00a05d38da4479514e03a46" , "1.0.1"},
                {"7269ef78da7811011efdccaf00d270dbed682b6e" , "1.0.1"},
                {"81f2ebc60e93641e31b30eb0a0e5bc0fe1f09f6c" , "1.0.1"},
                {"8abbea3884932d6215728ed6e582bbeba3167b46" , "1.0.1"},
                {"02aa27e41e039eb9dde3272d86eb23557a2e8554" , "1.0.1"},
                {"7cbd2b9aeec389e5a579c14becb53000389a540b" , "1.0.1"},
                {"10526738a49fdd2c229e30e72dd54f7776efc399" , "1.0.1"},
                {"acb23baffb0978cfe8050b0331adeffdc66aa2b3" , "1.0.1"},
                {"b89c87f254d3b30c17a1835028f041f7affddf8d" , "1.0.1"},
                {"87019a29475b2b0eb10e2b20ba3ea2aa6d78eee2" , "1.0.1"},
                {"403ffa26c02727f48c9c7cf3306bff2539ad9a8d" , "1.0.1"},
                {"0d015e9f9a48868b11fffd1412d1d59dcadcf838" , "1.0.1"},
                {"7d591f577a96b7acc401a2718bfc71127486a46c" , "1.0.1"}
            };
            if (!version_dict.TryGetValue(SubSHA1, out string version))
            {
                throw new Exception(GetTranslation("Basicflash_KSUError"));
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
