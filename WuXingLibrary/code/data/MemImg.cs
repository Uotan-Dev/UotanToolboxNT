// Decompiled with JetBrains decompiler
// Type: XiaoMiFlash.code.data.MemImg
// Assembly: XiaoMiFlash, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 61131B6C-96CC-4A81-B2A7-F80681D64A39
// Assembly location: C:\Users\gzw88\Downloads\MicrosoftEdge\MiFlash2020-3-14-0\XiaoMiFlash.exe

using System;
using System.Collections.Generic;
using System.IO;

#nullable disable
namespace WuXingLibrary.code.data
{
    public static class MemImg
    {
        private static readonly object obj_lock = new();
        public static bool isHighSpeed = false;
        public static Dictionary<string, MemoryStream> shareMemTable = [];

        public static long MapImg(string filePath)
        {
            lock (obj_lock)
            {
                try
                {
                    long num;
                    if (!shareMemTable.ContainsKey(filePath))
                    {
                        FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read);
                        int length = (int)fileStream.Length;
                        byte[] buffer = new byte[length];
                        fileStream.Read(buffer, 0, length);
                        MemoryStream memoryStream = new(buffer);
                        shareMemTable[filePath] = memoryStream;
                        num = length;
                    }
                    else
                        num = shareMemTable[filePath].Length;
                    return num;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        public static byte[] GetBytesFromFile(
          string filePath,
          long offset,
          int size,
          out float percent)
        {
            lock (obj_lock)
            {
                MemoryStream memoryStream = shareMemTable[filePath];
                byte[] buffer = new byte[size];
                memoryStream.Position = offset;
                memoryStream.Read(buffer, 0, size);
                percent = offset / (float)memoryStream.Length;
                return buffer;
            }
        }

        public static void Distory()
        {
            if (!isHighSpeed)
                return;
            foreach (KeyValuePair<string, MemoryStream> keyValuePair in shareMemTable)
            {
                keyValuePair.Value.Close();
                keyValuePair.Value.Dispose();
            }
            shareMemTable.Clear();
        }
    }
}
