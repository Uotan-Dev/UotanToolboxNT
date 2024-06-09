using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    }
}
