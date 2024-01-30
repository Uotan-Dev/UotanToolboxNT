using Avalonia.Collections;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SukiUI.Demo.Common
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
            string[] devices = new string[20];
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
                    devices[i] = device[0];
                }
            }
            devices = devices.Where(s => !String.IsNullOrEmpty(s)).ToArray();
            return devices;
        }

        public static string GetProductID(string info)
        {
            if (info.IndexOf("FAILED") == -1)
            {
                char[] charSeparators = new char[] { ' ' };
                string[] infos = info.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                string[] product = infos[0].Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                return product[1];
            }
            else
            {
                return "--";
            }
        }

        public static string RemoveLineFeed(string str)
        {
            string output;
            if (str.IndexOf('\r') != -1 || str.IndexOf('\n') != -1)
            {
                output = str.Substring(0, str.Length - 2);
                if (output == "")
                {
                    return "--";
                }
                else
                {
                    return output;
                }
            }
            else
            {
                return "--";
            }
        }

        public static string ColonSplit(string info)
        {
            if (info.IndexOf(':') !=  -1)
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

        public static string[] Battery(string info)
        {
            string[] infos = new string[20];
            string[] Lines = info.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i].IndexOf("Max charging voltage") == -1)
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

        public static string AllDevices(AvaloniaList<string> devices)
        {
            string list = "";
            for (int i = 0; i < devices.Count; i++)
            {
                list += devices[i];
            }
            return list;
        }
    }
}
