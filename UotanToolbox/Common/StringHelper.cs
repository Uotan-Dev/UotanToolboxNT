using SukiUI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                // 使用正则表达式匹配MAGISK_VER行，并提取版本号
                Match match = Regex.Match(content, regex);
                if (match.Success)
                {
                    return match.Groups[i].Value;
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog($"Unable to find MAGISK_VER in the file: {filePath}"), allowBackgroundClose: true);
                    return null;
                }
            }
            catch (FileNotFoundException)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"File not found: {filePath}"), allowBackgroundClose: true);
                return null;
            }
            catch (Exception ex)
            {
                SukiHost.ShowDialog(new ConnectionDialog($"An error occurred while reading the file: {ex.Message}"), allowBackgroundClose: true);
                return null;
            }
        }
        public static bool Magisk_Validation(string MD5,string MAGISK_VER,string MAGISK_VER_CODE)
        {
            Dictionary<string, PatchPlan> patchPlans = new Dictionary<string, PatchPlan>
        {
                
            {"cf9e4aa382b3e63d89197fdc68830622", new PatchPlan { MAGISK_VER = "26.3",MAGISK_VER_CODE = "26300", IsVivoSuuPatch = false }},
            {"3b324a47607ae17ac0376c19043bb7b1", new PatchPlan { MAGISK_VER = "26.3",MAGISK_VER_CODE = "26300", IsVivoSuuPatch = false }},
            {"aef5b749e978c6ea5ebd0f3df910ae6c", new PatchPlan { MAGISK_VER = "26.3",MAGISK_VER_CODE = "26300", IsVivoSuuPatch = false }},
            {"c840c6803c68ec0f91ca6e2cec21ed27", new PatchPlan { MAGISK_VER = "26.3",MAGISK_VER_CODE = "26300", IsVivoSuuPatch = true }},
            {"10870a74acf93ba4f87af22c19ab1677", new PatchPlan { MAGISK_VER = "26.3",MAGISK_VER_CODE = "26300", IsVivoSuuPatch = true }},
            {"daf3cffe200d4e492edd0ca3c676f07f", new PatchPlan { MAGISK_VER = "26.2",MAGISK_VER_CODE = "26200", IsVivoSuuPatch = false }},
            {"16cbb54272b01c13bdb860e3207284b8", new PatchPlan { MAGISK_VER = "26.2",MAGISK_VER_CODE = "26200", IsVivoSuuPatch = true }},
            {"ccf5647834aeefbd61ce6c2594dd43e4", new PatchPlan { MAGISK_VER = "26.0",MAGISK_VER_CODE = "26000", IsVivoSuuPatch = false }},
            {"0e8255080363ee0f895105cdc3dfa419", new PatchPlan { MAGISK_VER = "26.0",MAGISK_VER_CODE = "26000", IsVivoSuuPatch = false }},
            {"3d2c5bcc43373eb17939f0592b2b40f9", new PatchPlan { MAGISK_VER = "26.0",MAGISK_VER_CODE = "26000", IsVivoSuuPatch = false }},
            {"bf6ef4d02c48875ae3929d26899a868d", new PatchPlan { MAGISK_VER = "25.2",MAGISK_VER_CODE = "25200", IsVivoSuuPatch = false }},
            {"c48a22c8ed43cd20fe406acccc600308", new PatchPlan { MAGISK_VER = "25.2",MAGISK_VER_CODE = "25200", IsVivoSuuPatch = false }},
            {"b4a4a2be5fa2a38db5149f3c752a1104", new PatchPlan { MAGISK_VER = "25.2",MAGISK_VER_CODE = "25200", IsVivoSuuPatch = true }},
            {"7b40f9efd587b59bade9b9ec892e875e", new PatchPlan { MAGISK_VER = "25.0", MAGISK_VER_CODE = "25000", IsVivoSuuPatch = false }},
            {"0fb168d5339faf37c1c86ace16fe0953", new PatchPlan { MAGISK_VER = "25.0", MAGISK_VER_CODE = "25000", IsVivoSuuPatch = false }},
            {"55285c3ad04cdf72e6e2be9d7ba4a333", new PatchPlan { MAGISK_VER = "23.0", MAGISK_VER_CODE = "23000", IsVivoSuuPatch = false }},
            {"49452bcb3ea3362392ab05b7fe7ec128", new PatchPlan { MAGISK_VER = "23.0", MAGISK_VER_CODE = "23000", IsVivoSuuPatch = false }},
            {"c2e189a0a37d789dd233d19ad9236bdc", new PatchPlan { MAGISK_VER = "21.4", MAGISK_VER_CODE = "21400", IsVivoSuuPatch = false }},
            {"b8256416216461c247c2b82d60e8dca0", new PatchPlan { MAGISK_VER = "21.2", MAGISK_VER_CODE = "21200", IsVivoSuuPatch = false }},
            {"ac3d1448b7481d7e70d2558d4c733fee", new PatchPlan { MAGISK_VER = "21.2", MAGISK_VER_CODE = "21200", IsVivoSuuPatch = false }},
            {"69ebab4d9513484988a48a38560c6032", new PatchPlan { MAGISK_VER = "21.2", MAGISK_VER_CODE = "21200", IsVivoSuuPatch = false }},
            {"232aaecb0fae34baa5a13211fccde93c", new PatchPlan { MAGISK_VER = "21.2", MAGISK_VER_CODE = "21200", IsVivoSuuPatch = false }},
            {"cafa4ed2bfe5e45c85864a9ccf52502f", new PatchPlan { MAGISK_VER = "21.2", MAGISK_VER_CODE = "21200", IsVivoSuuPatch = false }},
            {"8595503b132d7154385a043b66e65d5d", new PatchPlan { MAGISK_VER = "19.4", MAGISK_VER_CODE = "19400", IsVivoSuuPatch = false }},
            {"05455b21ce3ea71c7d7b5c041023d392", new PatchPlan { MAGISK_VER = "19.4", MAGISK_VER_CODE = "19400", IsVivoSuuPatch = false }},
            {"2816b613afbca2288b753cad592299cf", new PatchPlan { MAGISK_VER = "19.0", MAGISK_VER_CODE = "19000", IsVivoSuuPatch = false }},
            {"11dc7caa2e7e734e11cc92e226b18bb2", new PatchPlan { MAGISK_VER = "18.1", MAGISK_VER_CODE = "18100", IsVivoSuuPatch = false }},
            {"7aacf5e27d35d6675a35969a74970172", new PatchPlan { MAGISK_VER = "18.1", MAGISK_VER_CODE = "18100", IsVivoSuuPatch = false }},
            {"e6040a2cac1af04dc0b41560dd0a8bc8", new PatchPlan { MAGISK_VER = "17.2", MAGISK_VER_CODE = "17200", IsVivoSuuPatch = false }}
        };
            if (patchPlans.TryGetValue(MD5, out PatchPlan outputplan))
            {
                if ((outputplan.MAGISK_VER_CODE == MAGISK_VER_CODE) & (outputplan.MAGISK_VER==MAGISK_VER))
                {
                    SukiHost.ShowDialog(new ConnectionDialog("检测到有效的"+MAGISK_VER+"面具安装包"), allowBackgroundClose: true);
                    return true;
                }
                Console.WriteLine("未检测到有效的面具安装包，继续修补存在风险");
                return false;
            }
            else
            {
                Console.WriteLine("未检测到有效的面具安装包，继续修补存在风险");
                return false;
            }

        }
    }
}
