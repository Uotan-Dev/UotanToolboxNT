using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SukiUI.Dialogs;

namespace UotanToolbox.Common
{
    internal class StringHelper
    {
        internal static readonly char[] separator = ['\r', '\n'];

        public static string[] ADBDevices(string ADBInfo)
        {
            var devices = new string[20];
            var Lines = ADBInfo.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i].Contains('\t'))
                {
                    var device = Lines[i].Split('\t', StringSplitOptions.RemoveEmptyEntries);
                    devices[i] = device[0];
                }
            }

            devices = devices.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            return devices;
        }

        ISukiDialogManager dialogManager;
        public static string[] FastbootDevices(string FastbootInfo)
        {
            var devices = new string[20];
            var Lines = FastbootInfo.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i].Contains('\t'))
                {
                    var device = Lines[i].Split('\t', StringSplitOptions.RemoveEmptyEntries);
                    devices[i] = device[0];
                }
            }

            devices = devices.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            return devices;
        }

        public static string[] COMDevices(string COMInfo)
        {
            if (Global.System == "Windows")
            {
                var devices = new string[100];
                var Lines = COMInfo.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < Lines.Length; i++)
                {
                    if (Lines[i].Contains("QDLoader") || Lines[i].Contains("900E (") || Lines[i].Contains("901D (") || Lines[i].Contains("9091 ("))
                    {
                        var deviceParts = Lines[i].Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);

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
                var j = 0;
                var devices = new string[100];
                var Lines = COMInfo.Split(separator, StringSplitOptions.RemoveEmptyEntries);

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
                var infos = info.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                var product = infos[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return product[1];
            }
            else
            {
                return "--";
            }
        }

        public static string RemoveLineFeed(string str)
        {
            var lines = str.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var result = string.Concat(lines);

            return string.IsNullOrEmpty(result) || result.Contains("not found") || result.Contains("dialog on your device") || result.Contains("device offline") || result.Contains("closed")
                ? "--"
                : result;
        }

        public static string ColonSplit(string info)
        {
            var parts = info.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts.Last() : "--";
        }

        public static string Density(string info)
        {
            if (string.IsNullOrEmpty(info) || info.Contains("not found") || info.Contains("dialog on your device") || info.Contains("device offline") || info.Contains("closed"))
            {
                return "--";
            }
            else
            {
                var Lines = info.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                return Lines.Length == 2 ? ColonSplit(Lines[1]) : ColonSplit(Lines[0]);
            }
        }

        public static string[] Battery(string info)
        {
            var infos = new string[100];
            var Lines = info.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < Lines.Length; i++)
            {
                if (!Lines[i].Contains("Max charging voltage") && !Lines[i].Contains("Charger voltage"))
                {
                    if (Lines[i].Contains("level") || Lines[i].Contains("voltage") || Lines[i].Contains("temperature"))
                    {
                        var device = Lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        infos[i] = device[^1];
                    }
                }
            }

            infos = infos.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            return infos;
        }

        public static string[] Mem(string info)
        {
            var infos = new string[20];
            var Lines = info.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i].Contains("MemTotal") || Lines[i].Contains("MemAvailable"))
                {
                    var device = Lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    infos[i] = device[^2];
                }
            }

            infos = infos.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            return infos;
        }

        public static string[] DiskInfo(string info, string find)
        {
            var columns = new string[20];
            var lines = info.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var targetLine = lines.FirstOrDefault(line => line.Contains(find));

            if (targetLine != null)
            {
                columns = targetLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }

            columns = columns.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            return columns;
        }

        public static string FastbootVar(string info, string find)
        {
            var infos = info.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var targetInfo = infos.FirstOrDefault(info => info.Contains(find));
            return targetInfo != null ? ColonSplit(RemoveLineFeed(targetInfo)) : "--";
        }

        public static string FilePath(string path)
        {
            if (path.Contains("file:///"))
            {
                var startIndex = Global.System == "Windows" ? 8 : 7;
                return path[startIndex..];
            }

            return Uri.UnescapeDataString(path);
        }

        public static int TextBoxLine(string info)
        {
            var Lines = info.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return Lines.Length;
        }

        public static int Onlynum(string text)//只保留数字
        {
            var size = text.Split('.');
            var num = Regex.Replace(size[0], @"[^0-9]+", "");
            var numint = int.Parse(num);
            return numint;
        }

        public static float OnlynumFloat(string text)//只保留数字(含小数）
        {
            var pattern = @"[-+]?\d*\.\d+|[-+]?\d+";
            var match = Regex.Match(text, pattern);
            return float.TryParse(match.Value, out float result) ? result : 0;
        }

        public static int GetDP(string wm, string dpi)
        {
            var wh = wm.Split("x");
            var dp = Onlynum(wh[0]) * 160 / Onlynum(dpi);
            return dp;
        }

        public static int GetDPI(string wm, string dp)
        {
            var wh = wm.Split("x");
            var dpi = Onlynum(wh[0]) * 160 / Onlynum(dp);
            return dpi;
        }

        public static async Task<string> GetBinVersion()
        {
            string sevenzip_version, adb_info, fb_info, file_info;

            try
            {
                var sevenzip_info = await CallExternalProgram.SevenZip("i");
                var lines = sevenzip_info.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var nonEmptyLines = lines.Where(line => !string.IsNullOrWhiteSpace(line));
                var firstThreeLines = nonEmptyLines.Take(1);
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
            var parts = parttable.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 6; i < parts.Length; i++)
            {
                var partneed = parts[i];

                if (partneed.Contains(findpart))
                {
                    var partno = partneed.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                    var lastPartIndex = partno.Length == 5 ? 4 : 5;

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
            var Lines = PartTable.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var NeedLine = Lines[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var size = NeedLine[^1];
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
                var content = File.ReadAllText(filePath);
                // 使用正则表达式匹配并提取
                var match = Regex.Match(content, regex);

                if (match.Success)
                {
                    return match.Groups[i].Value;
                }
                else
                {
                    _ = dialogManager.CreateDialog().WithTitle("Error").WithActionButton("知道了", _ => { }, true).WithContent($"Unable to find {regex} in the file: {filePath}").TryShow();
                    return null;
                }
            }
            catch (FileNotFoundException)
            {
                _ = dialogManager.CreateDialog().WithTitle("Error").WithActionButton("知道了", _ => { }, true).WithContent($"File not found: {filePath}").TryShow();
                return null;
            }
            catch (Exception ex)
            {
                _ = dialogManager.CreateDialog().WithTitle("Error").WithActionButton("知道了", _ => { }, true).WithContent($"An error occurred while reading the file: {ex.Message}").TryShow();
                return null;
            }
        }

        public static string RandomString(int length, string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789")
        {
            var random = new Random();
            var result = new StringBuilder(length);

            for (int i = 0; i < length; i++)
                _ = result.Append(chars[random.Next(chars.Length)]);
            

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

            var result = new byte[(source.Length - startIndex + 1) / 2]; // 计算目标数组的最大可能长度

            for (int i = startIndex, j = 0; i < source.Length && j < result.Length; i += 2, j++)
                result[j] = source[i];
            

            return result;
        }

        /// <summary>
        /// 从给定version字符串中提取KMI版本号
        /// </summary>
        /// <param name="version">内核version签名</param>
        /// <returns>KMI版本号</returns>
        public static string ExtractKMI(string version)
        {
            var pattern = @"(.* )?(\d+\.\d+)(\S+)?(android\d+)(.*)";
            var match = Regex.Match(version, pattern);

            if (!match.Success)
            {
                return "";
            }

            var androidVersion = match.Groups[4].Value;
            var kernelVersion = match.Groups[2].Value;
            return $"{androidVersion}-{kernelVersion}";
        }

        public static string ByteToHex(byte comByte) => comByte.ToString("X2") + " ";

        public static string ByteToHex(byte[] comByte, int len)
        {
            var returnStr = "";

            if (comByte != null)
            {
                for (int i = 0; i < len; i++)
                    returnStr += comByte[i].ToString("X2") + " ";
                
            }

            return returnStr;
        }

        public static byte[] HexToByte(string msg)
        {
            msg = msg.Replace(" ", "");

            var comBuffer = new byte[msg.Length / 2];

            for (int i = 0; i < msg.Length; i += 2)
                comBuffer[i / 2] = Convert.ToByte(msg.Substring(i, 2), 16);
            

            return comBuffer;
        }

        public static string HEXToASCII(string data)
        {
            data = data.Replace(" ", "");
            var comBuffer = new byte[data.Length / 2];

            for (int i = 0; i < data.Length; i += 2)
                comBuffer[i / 2] = Convert.ToByte(data.Substring(i, 2), 16);
            

            var result = Encoding.Default.GetString(comBuffer);
            return result;
        }

        public static string ASCIIToHEX(string data)
        {
            var result = new StringBuilder(data.Length * 2);

            for (int i = 0; i < data.Length; i++)
                _ = result.Append(((int)data[i]).ToString("X2") + " ");
            

            return Convert.ToString(result);
        }
    }
}