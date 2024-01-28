using Avalonia.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SukiUI.Demo.Common
{
    internal class GetDevicesInfo
    {
        public static async Task<string[]> DevicesList()
        {
            string adb = await CallExternalProgram.ADB("devices");
            string fastboot = await CallExternalProgram.Fastboot("devices");
            string devcon = await CallExternalProgram.Devcon("find usb*");
            string[] adbdevices = StringHelper.ADBDevices(adb);
            string[] fbdevices = StringHelper.FastbootDevices(fastboot);
            string[] comdevices = StringHelper.COMDevices(devcon);
            string[] devices = new string[adbdevices.Length + fbdevices.Length + comdevices.Length];
            Array.Copy(adbdevices, 0, devices, 0, adbdevices.Length);
            Array.Copy(fbdevices, 0, devices, adbdevices.Length, fbdevices.Length);
            Array.Copy(comdevices, 0, devices, adbdevices.Length + fbdevices.Length, comdevices.Length);
            return devices;
        }

        public static async Task<Dictionary<string, string>> DevicesInfo(string devicename)
        {
            Dictionary<string, string> devices = new Dictionary<string, string>();
            string status = "--";
            string blstatus = "--";
            string codename = "--";
            string vabstatus = "--";
            string adb = await CallExternalProgram.ADB("devices");
            string fastboot = await CallExternalProgram.Fastboot("devices");
            string devcon = await CallExternalProgram.Devcon("find usb*");
            if (fastboot.IndexOf(devicename) != -1)
            {
                status = "Fastboot";
                string blinfo = await CallExternalProgram.Fastboot($"-s {devicename} getvar unlocked");
                int unlocked = blinfo.IndexOf("yes");
                if (unlocked != -1)
                {
                    blstatus = "已解锁";
                }
                int locked = blinfo.IndexOf("no");
                if (locked != -1)
                {
                    blstatus = "未解锁";
                }
                string productinfos = await CallExternalProgram.Fastboot($"-s {devicename} getvar product");
                string product = StringHelper.GetProductID(productinfos);
                if (product != null)
                {
                    codename = product;
                }
                string active = await CallExternalProgram.Fastboot($"-s {devicename} getvar current-slot");
                if (active.IndexOf("current-slot: a") != -1)
                {
                    vabstatus = "A槽位";
                }
                else if (active.IndexOf("current-slot: b") != -1)
                {
                    vabstatus = "B槽位";
                }
                else if (active.IndexOf("FAILED") != -1)
                {
                    vabstatus = "A-Only设备";
                }
            }
            if (adb.IndexOf(devicename) != -1)
            {
                string thisdevice = "";
                string[] Lines = adb.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < Lines.Length; i++)
                {
                    if (Lines[i].IndexOf(devicename) != -1)
                    {
                        thisdevice = Lines[i];
                        break;
                    }
                }
                if (thisdevice.IndexOf("recovery") != -1)
                {
                    status = "Recovery";
                }
                else if (thisdevice.IndexOf("sideload") != -1)
                {
                    status = "Sideload";
                }
                else if (thisdevice.IndexOf("	device") != -1)
                {
                    status = "系统";
                }
                string active = await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.boot.slot_suffix");
                if (active.IndexOf("_a") != -1)
                {
                    vabstatus = "A槽位";
                }
                else if (active.IndexOf("_b") != -1)
                {
                    vabstatus = "B槽位";
                }
                else
                {
                    vabstatus = "A-Only设备";
                }
            }
            if (devcon.IndexOf(devicename) != -1)
            {
                string thisdevice = "";
                string[] Lines = adb.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < Lines.Length; i++)
                {
                    if (Lines[i].IndexOf(devicename) != -1)
                    {
                        thisdevice = Lines[i];
                        break;
                    }
                }
                if (thisdevice.IndexOf("QDLoader") != -1)
                {
                    status = "9008";
                }
                else if (thisdevice.IndexOf("900E (") != -1)
                {
                    status = "900E";
                }
                else if (thisdevice.IndexOf("901D (") != -1)
                {
                    status = "901D";
                }
                else if (thisdevice.IndexOf("9091 (") != -1)
                {
                    status = "9091";
                }
            }
            devices.Add("Status", status);
            devices.Add("BLStatus", blstatus);
            devices.Add("CodeName", codename);
            devices.Add("VABStatus", vabstatus);
            return devices;
        }
    }
}
