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
    }
}
