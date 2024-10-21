using System;
using System.Collections.Generic;
using Avalonia.Controls.Shapes;
using SukiUI.Controls;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using SukiUI.Dialogs;
using UotanToolbox.Features.Appmgr;


namespace UotanToolbox.Common
{
    internal class StringHelper
    {
        private static string GetTranslation(string key)
        {
            return FeaturesHelper.GetTranslation(key);
        }

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
            devices = devices.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            return devices;
        }

        private ISukiDialogManager dialogManager;
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
            devices = devices.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            return devices;
        }

        public static string[] HDCDevices(string HDCInfo)
        {
            string[] devices = HDCInfo.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            devices = devices.Where(s => !String.IsNullOrEmpty(s) && !s.Contains("[Empty]")).ToArray();
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
                        {
                            devices[i] = deviceParts[1];
                        }
                    }
                }
                devices = devices.Where(s => !string.IsNullOrEmpty(s)).ToArray();
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
                devices = devices.Where(s => !string.IsNullOrEmpty(s)).ToArray();
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

        public static List<ApplicationInfo> ParseApplicationInfo(string input)
        {
            var applicationInfos = new List<ApplicationInfo>();
            var lines = input.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 3)
                {
                    applicationInfos.Add(new ApplicationInfo
                    {
                        Name = parts[1],
                        DisplayName = parts[2],
                    });
                }
            }
            return applicationInfos;
        }
        public static string RemoveLineFeed(string str)
        {
            string[] lines = str.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string result = string.Concat(lines);
            return string.IsNullOrEmpty(result) || result.Contains("not found") || result.Contains("dialog on your device") || result.Contains("device offline") || result.Contains("closed")
                ? "--"
                : result;
        }

        public static string RemoveSpace(string str)
        {
            string[] lines = str.Split(new char[] { ' '}, StringSplitOptions.RemoveEmptyEntries);
            string result = string.Concat(lines);
            if (string.IsNullOrEmpty(result))
                return "--";
            return result;
        }

        public static string ColonSplit(string info)
        {
            string[] parts = info.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts.Last() : "--";
        }

        public static string OHColonSplit(string info)
        {
            string[] infos = info.Split(new char[] { '\r', '\n', ','}, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < infos.Length; i++)
            {
                if (infos[i].Contains("physical screen resolution"))
                {
                    string[] device = infos[i].Split(':', StringSplitOptions.RemoveEmptyEntries);
                    return device[1];
                }
            }
            return "--";
        }

        public static string OHKernel(string info)
        {
            string[] infos = info.Split('#', StringSplitOptions.RemoveEmptyEntries);
            return infos[0];
        }

        public static string Density(string info)
        {
            if (string.IsNullOrEmpty(info) || info.Contains("not found") || info.Contains("dialog on your device") || info.Contains("device offline") || info.Contains("closed"))
            {
                return "--";
            }
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
                        infos[i] = device[^1];
                    }
                }
            }
            infos = infos.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            return infos;
        }

        public static string[] BatteryOH(string info)
        {
            string[] infos = new string[100];
            string[] Lines = info.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i].Contains("capacity") || Lines[i].Contains("voltage") || Lines[i].Contains("temperature"))
                {
                    string[] device = Lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    infos[i] = device[device.Length - 1];
                }
            }
            infos = infos.Where(s => !String.IsNullOrEmpty(s)).ToArray();
            return infos;
        }

        public static string OHVersion(string info)
        {
            string[] version = info.Split(new char[] { ' ', '(' }, StringSplitOptions.RemoveEmptyEntries);
            return version[1];
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
                    infos[i] = device[^2];
                }
            }
            infos = infos.Where(s => !string.IsNullOrEmpty(s)).ToArray();
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

            columns = columns.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            return columns;
        }

        public static string FastbootVar(string info, string find)
        {
            string[] infos = info.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string targetInfo = infos.FirstOrDefault(info => info.Contains(find));
            return targetInfo != null ? ColonSplit(RemoveLineFeed(targetInfo)) : "--";
        }

        public static string FilePath(string path)
        {
            if (path.Contains("file:///"))
            {
                int startIndex = Global.System == "Windows" ? 8 : 7;
                return path[startIndex..];
            }
            return Uri.UnescapeDataString(path);
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
            return float.TryParse(match.Value, out float result) ? result : 0;
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
        public static async Task<string> GetBinVersion()
        {
            string sevenzip_version, adb_info, fb_info, file_info;
            try
            {
                string sevenzip_info = await CallExternalProgram.SevenZip("i");
                string[] lines = sevenzip_info.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                System.Collections.Generic.IEnumerable<string> nonEmptyLines = lines.Where(line => !string.IsNullOrWhiteSpace(line));
                System.Collections.Generic.IEnumerable<string> firstThreeLines = nonEmptyLines.Take(1);
                sevenzip_version = string.Join(Environment.NewLine, firstThreeLines) + Environment.NewLine;
            }
            catch
            {
                sevenzip_version = null;
            }

            try
            {
                adb_info = await CallExternalProgram.ADB("version");
            }
            catch
            {
                adb_info = null;
            }
            try
            {
                fb_info = await CallExternalProgram.Fastboot("--version");
            }
            catch
            {
                fb_info = null;
            }
            try
            {
                file_info = await CallExternalProgram.File("-v");
            }
            catch
            {
                file_info = null;
            }

            return "7za: " + sevenzip_version + "ADB" + adb_info + "Fastboot: " + fb_info + "File: " + file_info;
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
                    {
                        return partno[0];
                    }
                }
            }
            return null;
        }

        public static string DiskSize(string PartTable)
        {
            string[] Lines = PartTable.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] NeedLine = Lines[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string size = NeedLine[^1];
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
        public string FileRegex(string filePath, string regex, int i)
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
                    dialogManager.CreateDialog().OfType(NotificationType.Error).WithTitle(GetTranslation("Common_Error")).WithActionButton(GetTranslation("Common_Know"), _ => { }, true).WithContent($"Unable to find {regex} in the file: {filePath}").TryShow();
                    return null;
                }
            }
            catch (FileNotFoundException)
            {
                dialogManager.CreateDialog().OfType(NotificationType.Error).WithTitle(GetTranslation("Common_Error")).WithActionButton(GetTranslation("Common_Know"), _ => { }, true).WithContent($"File not found: {filePath}").TryShow();
                return null;
            }
            catch (Exception ex)
            {
                dialogManager.CreateDialog().OfType(NotificationType.Error).WithTitle(GetTranslation("Common_Error")).WithActionButton(GetTranslation("Common_Know"), _ => { }, true).WithContent($"An error occurred while reading the file: {ex.Message}").TryShow();
                return null;
            }
        }
        public static string RandomString(int length, string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789")
        {
            Random random = new Random();
            StringBuilder result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                _ = result.Append(chars[random.Next(chars.Length)]);
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
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (startIndex < 0 || startIndex >= source.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            byte[] result = new byte[(source.Length - startIndex + 1) / 2]; // 计算目标数组的最大可能长度

            for (int i = startIndex, j = 0; i < source.Length && j < result.Length; i += 2, j++)
            {
                result[j] = source[i];
            }
            return result;
        }
        /// <summary>
        /// 从给定version字符串中提取KMI版本号
        /// </summary>
        /// <param name="version">内核version签名</param>
        /// <returns>KMI版本号</returns>
        public static string ExtractKMI(string version)
        {
            string pattern = @"(.* )?(\d+\.\d+)(\S+)?(android\d+)(.*)";
            Match match = Regex.Match(version, pattern);
            if (!match.Success)
            {
                return "";
            }
            string androidVersion = match.Groups[4].Value;
            string kernelVersion = match.Groups[2].Value;
            return $"{androidVersion}-{kernelVersion}";
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
                comBuffer[i / 2] = Convert.ToByte(msg.Substring(i, 2), 16);
            }

            return comBuffer;
        }
        public static string HEXToASCII(string data)
        {
            data = data.Replace(" ", "");
            byte[] comBuffer = new byte[data.Length / 2];
            for (int i = 0; i < data.Length; i += 2)
            {
                comBuffer[i / 2] = Convert.ToByte(data.Substring(i, 2), 16);
            }
            string result = Encoding.Default.GetString(comBuffer);
            return result;
        }
        public static string ASCIIToHEX(string data)
        {
            StringBuilder result = new StringBuilder(data.Length * 2);
            for (int i = 0; i < data.Length; i++)
            {
                _ = result.Append(((int)data[i]).ToString("X2") + " ");
            }
            return Convert.ToString(result);
        }
    }
}
