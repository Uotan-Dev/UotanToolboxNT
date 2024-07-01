using SukiUI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Common
{
    internal class FileHelper
    {
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
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("FilePath cannot be null or empty.", nameof(filePath));
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var fileSize = (int)fileStream.Length;
                    var byteArray = new byte[fileSize];

                    fileStream.Read(byteArray, 0, fileSize);
                    return byteArray;
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"An error occurred while reading the file: {ex.Message}", ex);
            }
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
        public static string SHA1Hash(string filePath)
        {
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var sha1 = SHA1.Create())
                    {
                        var hashBytes = sha1.ComputeHash(fileStream);
                        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                    }
                }
            }
            catch (FileNotFoundException)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"The file '{filePath}' was not found."));
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"Access to the file '{filePath}' is denied."));
                throw;
            }
            catch (Exception ex)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"An unexpected error occurred while computing the SHA1 hash of '{filePath}': {ex.Message}"));
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
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = md5.ComputeHash(stream);
                    // 将字节数组转换为十六进制字符串表示形式，方便匹配字典
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("x2"));
                    }
                    return sb.ToString();
                }
            }
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
                throw new DirectoryNotFoundException($"指定的目录 '{directoryPath}' 不存在。");
            }
            var existenceResults = new Dictionary<string, bool>();
            foreach (var fileName in fileNames)
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
                Directory.CreateDirectory(folderPath);
                return true;
            }
            try
            {
                string[] subDirs = Directory.GetDirectories(folderPath);
                foreach (string subDirPath in subDirs)
                {
                    ClearFolder(subDirPath);
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
            catch (UnauthorizedAccessException ex)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"没有足够的权限删除Temp: {ex.Message}"));
                return false;
            }
            catch (Exception ex)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"未知错误: {ex.Message}"));
                return false;
            }
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
            Process.Start(startInfo);
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
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                FileStream inputStream = new FileStream(filename, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                inputStream.Position = 0;
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
    }
}

