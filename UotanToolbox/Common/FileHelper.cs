using Avalonia.Controls.Notifications;
using SukiUI.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace UotanToolbox.Common
{
    internal class FileHelper
    {
        private static string GetTranslation(string key)
        {
            return FeaturesHelper.GetTranslation(key);
        }
        private ISukiDialogManager dialogManager;
        public static void CopyDirectory(string srcPath, string aimPath)
        {
            try
            {
                string[] fileList = Directory.GetFiles(srcPath);
                foreach (string file in fileList)
                {
                    File.Copy(file, aimPath + System.IO.Path.GetFileName(file), true);
                }
            }
            catch
            {
                throw;
            }
        }
        /// <summary>
        /// 从文件路径读取内容为字节数组。
        /// </summary>
        /// <param name="filePath">文件的完整路径。</param>
        /// <returns>文件内容对应的字节数组。</returns>
        public static byte[] ReadFileToByteArray(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("FilePath cannot be null or empty.", nameof(filePath));
            }

            try
            {
                using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                int fileSize = (int)fileStream.Length;
                byte[] byteArray = new byte[fileSize];

                _ = fileStream.Read(byteArray, 0, fileSize);
                return byteArray;
            }
            catch (Exception ex)
            {
                throw new IOException($"An error occurred while reading the file: {ex.Message}", ex);
            }
        }
        public static async Task<string> SHA1HashAsync(string filePath)
        {
            using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using SHA1 sha1 = SHA1.Create();
            byte[] hash = await sha1.ComputeHashAsync(fileStream);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
            {
                _ = sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
        public static void Write(string file, string text)//写入到txt文件
        {
            FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(text);
            sw.Flush();
            sw.Close();
            fs.Close();
        }

        public static string Readtxt(string path)//读取txt文档
        {
            StreamReader sr = new StreamReader(path);
            string line = sr.ReadToEnd();
            sr.Close();
            return line;
        }
        /// <summary>
        /// 计算并返回指定文件的SHA1哈希值。
        /// </summary>
        /// <param name="filePath">要计算哈希值的文件路径。</param>
        /// <returns>文件的SHA1哈希值，表示为32位小写字母和数字的字符串。</returns>
        public string SHA1Hash(string filePath)
        {
            try
            {
                using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using SHA1 sha1 = SHA1.Create();
                byte[] hashBytes = sha1.ComputeHash(fileStream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
            catch (FileNotFoundException)
            {
                dialogManager.CreateDialog().OfType(NotificationType.Error).WithTitle(GetTranslation("Common_Error")).WithActionButton(GetTranslation("Common_Know"), _ => { }, true).WithContent($"The file '{filePath}' was not found.").TryShow();
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                dialogManager.CreateDialog().OfType(NotificationType.Error).WithTitle(GetTranslation("Common_Error")).WithActionButton(GetTranslation("Common_Know"), _ => { }, true).WithContent($"Access to the file '{filePath}' is denied.").TryShow();
                throw;
            }
            catch (Exception ex)
            {
                dialogManager.CreateDialog().OfType(NotificationType.Error).WithTitle(GetTranslation("Common_Error")).WithActionButton(GetTranslation("Common_Know"), _ => { }, true).WithContent($"An unexpected error occurred while computing the SHA1 hash of '{filePath}': {ex.Message}").TryShow();
                return null;
            }
        }
        /// <summary>
        /// 计算给定文件的MD5特征码
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>给定文件的MD5特征码（十六进制小写字符串）</returns>
        public static string Md5Hash(string filePath)
        {
            using MD5 md5 = MD5.Create();
            using FileStream stream = File.OpenRead(filePath);
            byte[] hashBytes = md5.ComputeHash(stream);
            // 将字节数组转换为十六进制字符串表示形式，方便匹配字典
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                _ = sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
        /// <summary>
        /// 传入一个字符串数组，检查指定目录下是否存在给出文件
        /// </summary>
        /// <param name="directoryPath">检查目录</param>
        /// <param name="fileNames">需要检查的文件名组成的字符串数组</param>
        /// <returns>返回一个字典，key是传入的文件名，value是文件否存在</returns>
        /// <exception cref="DirectoryNotFoundException">给定目录不存在时抛出</exception>
        public static Dictionary<string, bool> CheckFilesExistInDirectory(string directoryPath, params string[] fileNames)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"The directory {directoryPath} does not exist.");
            }
            Dictionary<string, bool> existenceResults = [];
            foreach (string fileName in fileNames)
            {
                bool exists = File.Exists(System.IO.Path.Combine(directoryPath, fileName));
                existenceResults.Add(fileName, exists);
            }
            return existenceResults;
        }

        /// <summary>
        /// 删除指定目录及其所有内容,若目录不存在则新建目录。
        /// </summary>
        /// <param name="folderPath">要删除的目录路径。</param>
        /// <returns>删除目录是否成功</returns>
        public static bool ClearFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                _ = Directory.CreateDirectory(folderPath);
                return true;
            }
            string[] subDirs = Directory.GetDirectories(folderPath);
            foreach (string subDirPath in subDirs)
            {
                _ = ClearFolder(subDirPath);
            }
            string[] files = Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);
            foreach (string filePath in files)
            {
                FileAttributes attr = File.GetAttributes(filePath);
                if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(filePath, attr & ~FileAttributes.ReadOnly);
                }
                File.Delete(filePath);
            }
            subDirs = Directory.GetDirectories(folderPath);
            foreach (string subDirPath in subDirs)
            {
                Directory.Delete(subDirPath, true);
            }
            return true;

        }
        /// <summary>
        /// 跨平台打开指定文件夹。
        /// </summary>
        /// <param name="folderPath">需要打开的文件路径</param>
        public static void OpenFolder(string folderPath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                startInfo.FileName = "explorer.exe";
                startInfo.Arguments = folderPath;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                startInfo.FileName = "xdg-open";
                startInfo.Arguments = folderPath;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                startInfo.FileName = "open";
                startInfo.Arguments = folderPath;
            }
            _ = Process.Start(startInfo);
        }
        /// <summary>
        /// 通过向文件路径写入随机数据流并且取消文件只读属性来删除文件。
        /// </summary>
        /// <param name="filename">需要删除的文件绝对路径</param>
        /// <returns>是否删除成功</returns>
        public static bool WipeFile(string filename)
        {
            if (File.Exists(filename))
            {
                File.SetAttributes(filename, FileAttributes.Normal);
                double sectors = Math.Ceiling(new FileInfo(filename).Length / 512.0);
                byte[] dummyBuffer = new byte[512];
                RandomNumberGenerator rng = RandomNumberGenerator.Create();
                FileStream inputStream = new FileStream(filename, FileMode.Open, FileAccess.Write, FileShare.ReadWrite)
                {
                    Position = 0
                };
                for (int sectorsWritten = 0; sectorsWritten < sectors; sectorsWritten++)
                {
                    rng.GetBytes(dummyBuffer);
                    inputStream.Write(dummyBuffer, 0, dummyBuffer.Length);
                }
                inputStream.SetLength(0);
                inputStream.Close();
                DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
                File.SetCreationTime(filename, dt);
                File.SetLastAccessTime(filename, dt);
                File.SetLastWriteTime(filename, dt);
                File.Delete(filename);
            }
            return true;
        }
        /// <summary>
        /// 检索目录下大小在10KB到200KB为后缀为gz的文件
        /// </summary>
        /// <param name="directoryPath">需要检索的目录</param>
        /// <returns>符合条件的文件名组成的字符串列表</returns>
        /// <exception cref="DirectoryNotFoundException">指定的目录不存在</exception>
        public static string[] FindConfigGzFiles(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"The directory {directoryPath} does not exist.");
            }
            string[] allGzFiles = Directory.GetFiles(directoryPath, "*.gz", SearchOption.TopDirectoryOnly);
            IEnumerable<string> filteredFiles = allGzFiles.Where(path =>
            {
                FileInfo fileInfo = new FileInfo(path);
                return fileInfo.Length is >= (10 * 1024) and <= (200 * 1024);
            });
            string[] fileNames = filteredFiles.Select(Path.GetFileName).ToArray();
            return fileNames;
        }
        /*
        /// <summary>
        /// 从给定的压缩文件名数组中解压出文件
        /// </summary>
        /// <param name="gzFileNames">给定的压缩文件名数组</param>
        /// <returns>解压成功的文件名数组（无后缀）</returns>
        public static async Task<string[]> DecompressConfigGzFiles(string[] gzFileNames)
        {
            string decompressedFileName;
            string[] decompressedFileNames = new string[gzFileNames.Length];
            int a = 0;
            for (int i = 0; i < gzFileNames.Length; i++)
            {
                //string gzFilePath = Path.Combine(BootInfo.tmp_path, "kernel-component", gzFileNames[i]);
                //string output = await CallExternalProgram.SevenZip($"e -o{Path.Combine(BootInfo.tmp_path, "kernel-component")} {gzFilePath} -y");
                //if (output.Contains("Everything is Ok"))
                //{
                //    decompressedFileNames[a] = Path.GetFileNameWithoutExtension(gzFilePath);
                //    a = a + 1;
                //}
            }
            decompressedFileNames = decompressedFileNames.Where(str => str != null).ToArray();
            return decompressedFileNames;
        }
        /// <summary>
        /// 根据config文件中的配置选项推断内核是否支持gki2.0（方法完善性不佳，仅作拒绝筛选）
        /// </summary>
        /// <param name="filePaths"></param>
        /// <returns>是否可能为gki2.0内核</returns>
        public static bool CheckGkiConfig(string[] filePaths)
        {
            foreach (string filePath in filePaths)
            {
                //string fullPath = Path.Combine(BootInfo.tmp_path, "kernel-component", filePath);
                //string content = File.ReadAllText(fullPath);
                //if (content.Contains("CONFIG_MODVERSIONS=y") & content.Contains("CONFIG_MODULES=y") & content.Contains("CONFIG_MODULE_UNLOAD=y") & content.Contains("CONFIG_MODVERSIONS=y"))
                //{
                //    return true;
                //}
            }
            return false;
        }*/
        /// <summary>
        /// 通过写入，读取，删除文件来判断程序是否拥有指定目录的写入权限
        /// </summary>
        /// <param name="directoryPath">给定程序目录</param>
        /// <returns>是否拥有权限</returns>
        public static bool TestPermission(string directoryPath)
        {
            string tempFileName = Path.Combine(directoryPath, Guid.NewGuid().ToString() + ".tmp");
            try
            {
                byte[] content = { 0x48, 0x65, 0x6C, 0x6C, 0x6F };//Hello
                using (FileStream fs = File.Create(tempFileName))
                {
                    fs.Write(content, 0, content.Length);
                }
                byte[] readContent = File.ReadAllBytes(tempFileName);
                if (!content.SequenceEqual(readContent))
                {
                    return false;
                }
                File.Delete(tempFileName);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 走kernel文件中提取编译签名信息
        /// </summary>
        /// <param name="filePath">内核文件路径</param>
        /// <returns>内核编译签名信息</returns>
        public static string ReadKernelVersion(string filePath)
        {
            byte[] Signature = new byte[] { 0x69, 0x6e, 0x69, 0x74, 0x63, 0x61, 0x6c, 0x6c, 0x5f, 0x64, 0x65, 0x62, 0x75, 0x67, 0x00 };
            using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using BinaryReader br = new BinaryReader(fs);
            long signaturePosition = FindSignaturePosition(br, Signature);
            if (signaturePosition == -1)
            {
                return "";
            }
            _ = fs.Seek(signaturePosition + Signature.Length, SeekOrigin.Begin);
            return ReadUntilTerminator(br);
        }
        private static long FindSignaturePosition(BinaryReader reader, byte[] signature)
        {
            byte[] buffer = new byte[signature.Length];
            long position = 0;
            while (position + signature.Length <= reader.BaseStream.Length)
            {
                _ = reader.BaseStream.Seek(position, SeekOrigin.Begin);
                _ = reader.Read(buffer, 0, signature.Length);
                if (buffer.SequenceEqual(signature))
                {
                    return position;
                }
                position++;
            }
            return -1;
        }
        private static string ReadUntilTerminator(BinaryReader reader)
        {
            StringBuilder sb = new StringBuilder();
            int b;
            while ((b = reader.ReadByte()) != 0x00)
            {
                _ = sb.Append((char)b);
            }
            while ((b = reader.ReadByte()) != 0x00)
            {
                _ = sb.Append((char)b);
            }
            return sb.ToString();
        }
    }
}

