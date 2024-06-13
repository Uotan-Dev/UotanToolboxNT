using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace UotanToolbox.Common
{
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
    }
}
