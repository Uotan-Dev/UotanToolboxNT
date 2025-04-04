using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace WuXingLibrary.code.Utility;

public class FileSearcher
{
    public static string[] SearchFiles(string destinationDic, string pattern)
    {
        List<string> list = new List<string>();
        FileInfo[] files = new DirectoryInfo(destinationDic).GetFiles();
        foreach (FileInfo fileInfo in files)
        {
            if (fileInfo.Name.IndexOf("DA") > 0)
            {
                Log.W("vboytest " + fileInfo.Name);
            }
            Match match = new Regex(pattern).Match(fileInfo.Name);
            if (match.Groups.Count > 0 && match.Groups[0].Value == fileInfo.Name)
            {
                list.Add(fileInfo.FullName);
            }
        }
        return list.ToArray();
    }
}
