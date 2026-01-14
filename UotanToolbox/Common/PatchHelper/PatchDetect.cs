using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.IO;
using System.Threading.Tasks;

namespace UotanToolbox.Common.PatchHelper
{
    internal class PatchDetect
    {
        private static string GetTranslation(string key)
        {
            return FeaturesHelper.GetTranslation(key);
        }

        public static async Task<PatchInfo> Patch_Detect(string path)
        {
            PatchInfo Patchinfo = new PatchInfo("", "", false, PatchMode.None)
            {
                Path = path,
                TempPath = Path.Combine(Global.tmp_path, "Patch-" + StringHelper.RandomString(8))
            };
            bool istempclean = FileHelper.ClearFolder(Patchinfo.TempPath);
            if (!istempclean)
            {
                throw new Exception(GetTranslation("Basicflash_FatalError"));
            }
            string file_magic = await CallExternalProgram.File(path);
            if (file_magic.Contains("ELF 64-bit LSB relocatable"))
            {
                File.Copy(path, Path.Combine(Patchinfo.TempPath, "kernelsu.ko"), true);
                Patchinfo.Mode = PatchMode.LKM;
                Patchinfo.IsUseful = true;
            }
            if (file_magic.Contains("archive") || file_magic.Contains("APK"))
            {
                using IArchive archive = ArchiveFactory.Open(path);
                foreach (IArchiveEntry entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                    {
                        entry.WriteToDirectory(Patchinfo.TempPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                    }
                }
                bool isMagisk = Directory.Exists(Path.Combine(Patchinfo.TempPath, "assets")) && File.Exists(Path.Combine(Patchinfo.TempPath, "assets", "util_functions.sh"));
                if (!isMagisk)
                {
                    string[] utilFiles = Directory.GetFiles(Patchinfo.TempPath, "util_functions.sh", SearchOption.AllDirectories);
                    isMagisk = utilFiles.Length > 0;
                }
                bool isGki = File.Exists(Path.Combine(Patchinfo.TempPath, "Image"));
                if (!isGki)
                {
                    string[] imageFiles = Directory.GetFiles(Patchinfo.TempPath, "Image", SearchOption.AllDirectories);
                    isGki = imageFiles.Length > 0;
                }
                if (isMagisk)
                {
                    Patchinfo.Mode = PatchMode.Magisk;
                    Patchinfo.IsUseful = true;
                }
                else if (isGki)
                {
                    Patchinfo.Mode = PatchMode.GKI;
                    Patchinfo.IsUseful = true;
                }
                else
                {
                    throw new Exception(GetTranslation("Basicflash_ZipError") + file_magic);
                }
            }
            return Patchinfo;
        }
    }
}
