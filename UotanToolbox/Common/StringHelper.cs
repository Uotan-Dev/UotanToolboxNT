using SukiUI.Controls;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Common
{


    internal class StringHelper
    {
        internal static readonly char[] separator = ['\r', '\n'];

        public static string[] ADBDevices(string ADBInfo)
        {
            string[] devices = new string[20];
            string[] Lines = ADBInfo.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i].Contains('\t'))
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
            string[] Lines = FastbootInfo.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i].Contains('\t'))
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
                string[] Lines = COMInfo.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < Lines.Length; i++)
                {
                    if (Lines[i].Contains("QDLoader") || Lines[i].Contains("900E (") || Lines[i].Contains("901D (") || Lines[i].Contains("9091 ("))
                    {
                        string[] deviceParts = Lines[i].Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                        if (deviceParts.Length > 1)
                            devices[i] = deviceParts[1];
                    }
                }
                devices = devices.Where(s => !String.IsNullOrEmpty(s)).ToArray();
                return devices;
            }
            else
            {
                int j = 0;
                string[] devices = new string[100];
                string[] Lines = COMInfo.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < Lines.Length; i++)
                {
                    if (Lines[i].Contains(":9008"))
                    {
                        devices[i] = $"/dev/ttyUSB{j}";
                        j++;
                    }
                    else if (Lines[i].Contains(":900e") || Lines[i].Contains(":901d") || Lines[i].Contains(":9091"))
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
                string[] infos = info.Split(separator, StringSplitOptions.RemoveEmptyEntries);
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
            if (string.IsNullOrEmpty(result) || result.Contains("not found") || result.Contains("dialog on your device") || result.Contains("device offline") || result.Contains("closed"))
                return "--";
            return result;
        }

        public static string ColonSplit(string info)
        {
            var parts = info.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts.Last() : "--";
        }

        public static string Density(string info)
        {
            if (string.IsNullOrEmpty(info) || info.Contains("not found") || info.Contains("dialog on your device") || info.Contains("device offline") || info.Contains("closed"))
                return "--";
            else
            {
                string[] Lines = info.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                return Lines.Length == 2 ? ColonSplit(Lines[1]) : ColonSplit(Lines[0]);
            }
        }

        public static string[] Battery(string info)
        {
            string[] infos = new string[100];
            string[] Lines = info.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < Lines.Length; i++)
            {
                if (!Lines[i].Contains("Max charging voltage") && !Lines[i].Contains("Charger voltage"))
                {
                    if (Lines[i].Contains("level") || Lines[i].Contains("voltage") || Lines[i].Contains("temperature"))
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
            string[] Lines = info.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i].Contains("MemTotal") || Lines[i].Contains("MemAvailable"))
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
                columns = targetLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            columns = columns.Where(s => !String.IsNullOrEmpty(s)).ToArray();
            return columns;
        }

        public static string FastbootVar(string info, string find)
        {
            string[] infos = info.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string targetInfo = infos.FirstOrDefault(info => info.Contains(find));
            if (targetInfo != null)
                return ColonSplit(RemoveLineFeed(targetInfo));
            return "--";
        }

        public static string FilePath(string path)
        {
            if (path.Contains("file:///"))
            {
                int startIndex = Global.System == "Windows" ? 8 : 7;
                return path.Substring(startIndex);
            }
            return path;
        }

        public static int TextBoxLine(string info)
        {
            string[] Lines = info.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return Lines.Length;
        }

        public static int Onlynum(string text)//只保留数字
        {
            string[] size = text.Split('.');
            string num = Regex.Replace(size[0], @"[^0-9]+", "");
            int numint = int.Parse(num);
            return numint;
        }

        public static float OnlynumFloat(string text)//只保留数字(含小数）
        {
            string pattern = @"[-+]?\d*\.\d+|[-+]?\d+";
            Match match = Regex.Match(text, pattern);
            if (float.TryParse(match.Value, out float result))
                return result;
            else
                return 0;
        }

        public static int GetDP(string wm, string dpi)
        {
            string[] wh = wm.Split("x");
            int dp = Onlynum(wh[0]) * 160 / Onlynum(dpi);
            return dp;
        }

        public static int GetDPI(string wm, string dp)
        {
            string[] wh = wm.Split("x");
            int dpi = Onlynum(wh[0]) * 160 / Onlynum(dp);
            return dpi;
        }

        public static string Partno(string parttable, string findpart)//分区号
        {
            char[] charSeparators = [' '];
            string[] parts = parttable.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 6; i < parts.Length; i++)
            {
                string partneed = parts[i];
                if (partneed.Contains(findpart))
                {
                    string[] partno = partneed.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                    int lastPartIndex = partno.Length == 5 ? 4 : 5;
                    if (partno[lastPartIndex] == findpart)
                        return partno[0];
                }
            }
            return null;
        }

        public static string DiskSize(string PartTable)
        {
            string[] Lines = PartTable.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] NeedLine = Lines[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string size = NeedLine[NeedLine.Length - 1];
            return size;
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
        public static string FileRegex(string filePath, string regex, int i)
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
                    SukiHost.ShowDialog(new ConnectionDialog($"Unable to find {regex} in the file: {filePath}"));
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
        public static string RandomString(int length, string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789")
        {
            Random random = new Random();
            StringBuilder result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }
            return result.ToString();
        }
        /// <summary>
        /// 从源字节数组的指定索引开始，每隔一个字节读取数据并保存到新的字节数组。
        /// </summary>
        /// <param name="source">原始字节数组。</param>
        /// <param name="startIndex">开始读取的索引位置，从0开始计数。</param>
        /// <returns>包含按指定规则读取的数据的新字节数组。</returns>
        public static byte[] ReadBytesWithInterval(byte[] source, int startIndex)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (startIndex < 0 || startIndex >= source.Length) throw new ArgumentOutOfRangeException(nameof(startIndex));

            var result = new byte[(source.Length - startIndex + 1) / 2]; // 计算目标数组的最大可能长度

            for (int i = startIndex, j = 0; i < source.Length && j < result.Length; i += 2, j++)
            {
                result[j] = source[i];
            }
            return result;
        }
        public static string ByteToHex(byte comByte)
        {
            return comByte.ToString("X2") + " ";
        }
        public static string ByteToHex(byte[] comByte, int len)
        {
            string returnStr = "";
            if (comByte != null)
            {
                for (int i = 0; i < len; i++)
                {
                    returnStr += comByte[i].ToString("X2") + " ";
                }
            }
            return returnStr;
        }
        public static byte[] HexToByte(string msg)
        {
            msg = msg.Replace(" ", "");

            byte[] comBuffer = new byte[msg.Length / 2];
            for (int i = 0; i < msg.Length; i += 2)
            {
                comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
            }

            return comBuffer;
        }
        public static string HEXToASCII(string data)
        {
            data = data.Replace(" ", "");
            byte[] comBuffer = new byte[data.Length / 2];
            for (int i = 0; i < data.Length; i += 2)
            {
                comBuffer[i / 2] = (byte)Convert.ToByte(data.Substring(i, 2), 16);
            }
            string result = Encoding.Default.GetString(comBuffer);
            return result;
        }
        public static string ASCIIToHEX(string data)
        {
            StringBuilder result = new StringBuilder(data.Length * 2);
            for (int i = 0; i < data.Length; i++)
            {
                result.Append(((int)data[i]).ToString("X2") + " ");
            }
            return Convert.ToString(result);
        }
    }
}
