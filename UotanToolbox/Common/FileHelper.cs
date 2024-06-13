using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
        /// 使用file命令判断文件的类型和指令集。暂不支持FAT Binary多架构检测
        /// </summary>
        /// <param name="filePath">要检查的文件路径。</param>
        /// <returns>file命令的输出结果，包含文件类型和指令集信息。</returns>
        /// <exception cref="FileNotFoundException">当指定的文件路径不存在时抛出。</exception>
        /// <summary>
        /// 调用file命令读取文件信息。
        /// </summary>
        /// <param name="filePath">要分析的文件路径。</param>
        /// <returns>file命令的输出，包含文件的类型和相关信息。</returns>
        public static string ExtractFileInfo(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));

            string command;
            string arguments;

            // 判断操作系统以确定命令和参数
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // 假设file.exe位于程序同目录或PATH环境变量中
                command = "bin\\Windows\\File\\file.exe";
                arguments = filePath;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)| (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)))
            {
                command = "file";
                arguments = $"\"{filePath}\"";
            }
            else
            {
                throw new PlatformNotSupportedException("This function only supports Windows,macOS and Linux.");
            }

            // 使用Process执行命令
            using (var process = new Process())
            {
                process.StartInfo.FileName = command;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                try
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        throw new InvalidOperationException($"Command '{command} {arguments}' failed with exit code {process.ExitCode}.");
                    }

                    return output.Trim();
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to execute '{command} {arguments}'.", ex);
                }
            }
        }
    }
}

