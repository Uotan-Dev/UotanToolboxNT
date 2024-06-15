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
                    File.Copy(file, aimPath + Path.GetFileName(file), true);
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
        /// <summary>
        /// 删除指定目录及其所有内容。
        /// </summary>
        /// <param name="directoryPath">要删除的目录路径。</param>
        /// <param name="recursive">是否递归删除子目录，默认为true。</param>
        public static bool DeleteDirectory(string directoryPath, bool recursive = true)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, recursive);
                    return true;
                }
                else
                {
                    return true;
                }
            }
            catch (IOException ex)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"清理Temp时发生错误: {ex.Message}"), allowBackgroundClose: true);
                return false;
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
    }
}

