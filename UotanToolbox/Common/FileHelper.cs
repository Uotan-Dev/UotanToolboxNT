using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using SkiaSharp;
using SukiUI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
        /*
        /// <summary>
        /// 删除指定目录及其所有内容。
        /// </summary>
        /// <param name="directoryPath">要删除的目录路径。</param>
        /// <param name="recursive">是否递归删除子目录，默认为false。</param>
        public static bool DeleteDirectory(string directoryPath, bool recursive = false)
        {
            try
            {
                foreach (string filePath in Directory.GetFiles(directoryPath))
                {
                    // 设置文件属性为正常，以忽略只读属性
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.Delete(filePath);
                }

                if (recursive)
                {
                    foreach (string subDirPath in Directory.GetDirectories(directoryPath))
                    {
                        DeleteDirectory(subDirPath, recursive); // 递归删除子目录下的文件
                    }
                    return true;
                }
                return true;
            }
            catch (IOException ex)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"清理Temp时发生错误: {ex.Message}"), allowBackgroundClose: true);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                if (ex.Message.Contains("is denied")) 
                {
                    SukiHost.ShowDialog(new ConnectionDialog($"{ex.Message}"), allowBackgroundClose: false);
                    //string path = StringHelper.StringRegex(ex.Message, @"(?i)(?:file|path):[\s]*([\w\\/:.]+)",1);
                    //bool result =FileHelper.WipeFile(path);
                    //if (!result)
                    //{
                    //    SukiHost.ShowDialog(new ConnectionDialog($"删除文件时发生错误: {path}"), allowBackgroundClose: true);
                    //}
                    //return result;
                }

                //SukiHost.ShowDialog(new ConnectionDialog($"没有足够的权限删除Temp: {ex.Message}"), allowBackgroundClose: true);
                return false;
            }
            catch (Exception ex)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"未知错误: {ex.Message}"), allowBackgroundClose: true);
                return false;
            }
        }*/
        /// <summary>
        /// 删除指定目录及其所有内容。
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
                SukiHost.ShowDialog(new ConnectionDialog($"没有足够的权限删除Temp: {ex.Message}"), allowBackgroundClose: true);
                return false;
            }
            catch (Exception ex)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"未知错误: {ex.Message}"), allowBackgroundClose: true);
                return false;
            }
        }

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
        public static bool WipeFile(string filename)
        {
            try
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
            catch (Exception ex)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"未知错误: {ex.Message}"), allowBackgroundClose: true);
                return false;
            }
        }
    }
}

