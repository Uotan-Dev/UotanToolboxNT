using SukiUI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Common
{
    public class PatchPlan
    {
        public string? MAGISK_VER { get; set; }
        public string? MAGISK_VER_CODE { get; set; }
        public bool IsVivoSuuPatch { get; set; }
    }

    internal class StringHelper
    {
        public static string[] ADBDevices(string ADBInfo)
        {
            string[] devices = new string[20];
            string[] Lines = ADBInfo.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i].IndexOf('\t') != -1)
                {
                    string[] device = Lines[i].Split('\t', StringSplitOptions.RemoveEmptyEntries);
                    devices[i] = device[0];
                }
            }
            devices = devices.Where(s => !String.IsNullOrEmpty(s)).ToArray();
            return devices;
        }

        public static string[] FastbootDevices(string FastbootInfo)
        {
            string[] devices = new string[20];
            string[] Lines = FastbootInfo.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i].IndexOf('\t') != -1)
                {
                    string[] device = Lines[i].Split('\t', StringSplitOptions.RemoveEmptyEntries);
                    devices[i] = device[0];
                }
            }
            devices = devices.Where(s => !String.IsNullOrEmpty(s)).ToArray();
            return devices;
        }

        public static string[] COMDevices(string COMInfo)
        {
            if (Global.System == "Windows")
            {
                string[] devices = new string[100];
                string[] Lines = COMInfo.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < Lines.Length; i++)
                {
                    int Find9008 = Lines[i].IndexOf("QDLoader");
                    int Find900E = Lines[i].IndexOf("900E (");
                    int Find901D = Lines[i].IndexOf("901D (");
                    int Find9091 = Lines[i].IndexOf("9091 (");
                    if (Find9008 != -1 || Find900E != -1 || Find901D != -1 || Find9091 != -1)
                    {
                        string[] device = Lines[i].Split(new char[2] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                        devices[i] = device[1];
                    }
                }
                devices = devices.Where(s => !String.IsNullOrEmpty(s)).ToArray();
                return devices;
            }
            else
            {
                int j = 0;
                string[] devices = new string[100];
                string[] Lines = COMInfo.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < Lines.Length; i++)
                {
                    int Find9008 = Lines[i].IndexOf(":9008");
                    if (Find9008 != -1)
                    {
                        devices[i] = String.Format($"/dev/ttyUSB{j}");
                        j++;
                    }
                    int Find900E = Lines[i].IndexOf(":900e");
                    int Find901D = Lines[i].IndexOf(":901d");
                    int Find9091 = Lines[i].IndexOf(":9091");
                    if (Find900E != -1 || Find901D != -1 || Find9091 != -1)
                    {
                        devices[i] = "Unknown device";
                    }
                }
                devices = devices.Where(s => !String.IsNullOrEmpty(s)).ToArray();
                return devices;
            }
        }

        public static string GetProductID(string info)
        {
            if (info.IndexOf("FAILED") == -1)
            {
                string[] infos = info.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                string[] product = infos[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return product[1];
            }
            else
            {
                return "--";
            }
        }

        public static string RemoveLineFeed(string str)
        {
            string[] lines = str.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string result = string.Concat(lines);
            if (result == "" || result.IndexOf("not found") != -1 || result.IndexOf("dialog on your device") != -1 || result.IndexOf("device offline") != -1 || result.IndexOf("closed") != -1)
            {
                return "--";
            }
            return result;
        }

        public static string ColonSplit(string info)
        {
            if (info.IndexOf(':') != -1)
            {
                string[] text = info.Split(':', StringSplitOptions.RemoveEmptyEntries);
                return text[text.Length - 1];
            }
            else
            {
                return "--";
            }
        }

        public static string Density(string info)
        {
            if (info == "" || info.IndexOf("not found") != -1 || info.IndexOf("dialog on your device") != -1 || info.IndexOf("device offline") != -1 || info.IndexOf("closed") != -1)
            {
                return "--";
            }
            else
            {
                string[] Lines = info.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (Lines.Length == 2)
                {
                    return ColonSplit(Lines[1]);
                }
                else
                {
                    return ColonSplit(Lines[0]);
                }
            }
        }

        public static string[] Battery(string info)
        {
            string[] infos = new string[100];
            string[] Lines = info.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i].IndexOf("Max charging voltage") == -1 && Lines[i].IndexOf("Charger voltage") == -1)
                {
                    if (Lines[i].IndexOf("level") != -1 || Lines[i].IndexOf("voltage") != -1 || Lines[i].IndexOf("temperature") != -1)
                    {
                        string[] device = Lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        infos[i] = device[device.Length - 1];
                    }
                }
            }
            infos = infos.Where(s => !String.IsNullOrEmpty(s)).ToArray();
            return infos;
        }

        public static string[] Mem(string info)
        {
            string[] infos = new string[20];
            string[] Lines = info.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i].IndexOf("MemTotal") != -1 || Lines[i].IndexOf("MemAvailable") != -1)
                {
                    string[] device = Lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    infos[i] = device[device.Length - 2];
                }
            }
            infos = infos.Where(s => !String.IsNullOrEmpty(s)).ToArray();
            return infos;
        }

        public static string[] DiskInfo(string info, string find)
        {
            string[] columns = new string[20];
            string[] lines = info.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string targetLine = lines.FirstOrDefault(line => line.Contains(find));
            if (targetLine != null)
            {
                columns = targetLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            columns = columns.Where(s => !String.IsNullOrEmpty(s)).ToArray();
            return columns;
        }

        public static string FastbootVar(string info, string find)
        {
            string[] infos = info.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < infos.Length; i++)
            {
                if (infos[i].IndexOf(find) != -1)
                {
                    return ColonSplit(RemoveLineFeed(infos[i]));
                }
            }
            return "--";
        }

        public static string FilePath(string path)
        {
            if (path.IndexOf("file:///") != -1)
            {
                if (Global.System == "Windows")
                {
                    return path.Substring(8, path.Length - 8);
                }
                else
                {
                    return path.Substring(7, path.Length - 7);
                }
            }
            return path;
        }

        public static int TextBoxLine(string info)
        {
            string[] Lines = info.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return Lines.Length;
        }

        public static int Onlynum(string text)//只保留数字
        {
            string[] size = text.Split('.');
            string num = Regex.Replace(size[0], @"[^0-9]+", "");
            int numint = int.Parse(num);
            return numint;
        }

        public static string Partno(string parttable, string findpart)//分区号
        {
            char[] charSeparators = new char[] { ' ' };
            string[] parts = parttable.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string partneed = "";
            string[] partno = null;
            for (int i = 6; i < parts.Length; i++)
            {
                partneed = parts[i];
                int find = partneed.IndexOf(findpart);
                if (find != -1)
                {
                    partno = partneed.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                    if (partno.Length == 5)
                    {
                        if (partno[4] == findpart)
                            return partno[0];
                    }
                    else
                    {
                        if (partno[4] == findpart || partno[5] == findpart)
                            return partno[0];
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 根据提供的正则表达式，提取指定指定路径文本文件中的内容。
        /// </summary>
        /// <param name="filePath">要检查的文件路径。</param>
        /// <param name="regex">用于匹配的正则表达式</param>
        /// <param name="i">索引号</param>
        /// <returns>匹配到的字符串信息</returns>
        /// <exception cref="FileNotFoundException">当指定的文件路径不存在时抛出。</exception>
        /// <exception cref="Exception">读取文件出错时抛出</exception>
        public static string FileRegex(string filePath,string regex,int i)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                // 使用正则表达式匹配并提取
                Match match = Regex.Match(content, regex);
                if (match.Success)
                {
                    return match.Groups[i].Value;
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog($"Unable to find MAGISK_VER in the file: {filePath}"));
                    return null;
                }
            }
            catch (FileNotFoundException)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"File not found: {filePath}"));
                return null;
            }
            catch (Exception ex)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"An error occurred while reading the file: {ex.Message}"));
                return null;
            }
        }
        public static string StringRegex(string content, string regex, int i)
        {
            try
            {
                Match match = Regex.Match(content, regex);
                if (match.Success)
                {
                    return match.Groups[i].Value;
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog($"Unable to find {regex} in the string"));
                    return null;
                }
            }
            catch (FileNotFoundException)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"String not found"));
                return null;
            }
            catch (Exception ex)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"An error occurred while reading the string: {ex.Message}"));
                return null;
            }
        }
        public static string RandomString(int length,string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789")
        {
            Random random = new Random();
            StringBuilder result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }
            return result.ToString();
        }
        public static bool Magisk_Validation(string MD5_in,string MAGISK_VER)
        {
            string MD5_out = null;
            string MD5;
            Dictionary<string, string> patchPlans = new Dictionary<string, string>
        {
            {"27.0" , "3b324a47607ae17ac0376c19043bb7b1"},
            {"26.4" , "3b324a47607ae17ac0376c19043bb7b1"},
            {"26.3" , "3b324a47607ae17ac0376c19043bb7b1"},
            {"26.2" , "daf3cffe200d4e492edd0ca3c676f07f"},
            {"26.1" , "0e8255080363ee0f895105cdc3dfa419"},
            {"26.0" , "3d2c5bcc43373eb17939f0592b2b40f9"},
            {"25.2" , "bf6ef4d02c48875ae3929d26899a868d"},
            {"25.1" , "c48a22c8ed43cd20fe406acccc600308"},
            {"25.0" , "7b40f9efd587b59bade9b9ec892e875e"},
            {"22.1" , "55285c3ad04cdf72e6e2be9d7ba4a333"}
        };
            if (patchPlans.TryGetValue(MAGISK_VER, out MD5_out))
            {
                if (MD5_out == MD5_in)
                {
                    SukiHost.ShowDialog(new ConnectionDialog("检测到有效的"+MAGISK_VER+"面具安装包"));
                    return true;
                }
                SukiHost.ShowDialog(new ConnectionDialog("面具安装包可能失效，继续修补存在风险"));
                return false;
            }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("面具安装包不被支持"));
                return false;
            }
        }
    }
}
