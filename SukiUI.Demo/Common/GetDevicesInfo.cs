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
            string vndkversion = "--";
            string cpucode = "--";
            string powerontime = "--";
            string devicebrand = "--";
            string devicemodel = "--";
            string androidsdk = "--";
            string cpuabi = "--";
            string displayhw = "--";
            string density = "--";
            string boardid = "--";
            string compileversion =  "--";
            string platform = "--";
            string kernel = "--";
            string selinux = "--";
            string batterylevel = "0";
            string batteryinfo = "--";
            string memlevel = "--";
            string usemem = "--";
            string diskinfo = "--";
            string progressdisk = "--";
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
                if (status == "系统")
                {
                    string android = await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.build.version.release");
                    string sdk = await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.build.version.sdk");
                    androidsdk = String.Format($"Android {StringHelper.RemoveLineFeed(android)}({StringHelper.RemoveLineFeed(sdk)})");
                    displayhw = StringHelper.ColonSplit(StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell wm size")));
                    density = StringHelper.Density(await CallExternalProgram.ADB($"-s {devicename} shell wm density"));
                }
                else if (status == "Recovery" || status == "Sideload")
                {
                    androidsdk = "Recovery";
                    displayhw = "--";
                    density = "--";
                }
                string bid = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell cat /sys/devices/soc0/serial_number"));
                if (bid.IndexOf("No such file") != -1 || bid.IndexOf("Permission denied") != -1)
                {
                    boardid = "--";
                }
                else
                {
                    boardid = bid;
                }
                vndkversion = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.vndk.version"));
                cpucode = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.board.platform"));
                devicebrand = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.product.brand"));
                devicemodel = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.product.model"));
                cpuabi = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.product.cpu.abi"));
                codename = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.product.board"));
                blstatus = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.secureboot.lockstate"));
                selinux = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell getenforce"));
                compileversion = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.system.build.version.incremental"));
                platform = StringHelper.ColonSplit(StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell cat /proc/cpuinfo | grep Hardware")));
                kernel = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell uname -r"));
                try
                {
                    string ptime = await CallExternalProgram.ADB($"-s {devicename} shell uptime -s");
                    DateTime givenDateTime = DateTime.Parse(ptime);
                    TimeSpan timeDifference = DateTime.Now - givenDateTime;
                    powerontime = $"{timeDifference.Days}天{timeDifference.Hours}时{timeDifference.Minutes}分{timeDifference.Seconds}秒";
                }
                catch
                {
                    powerontime = "--";
                }
                try
                {
                    string[] battery = StringHelper.Battery(await CallExternalProgram.ADB($"-s {devicename} shell dumpsys battery"));
                    batterylevel = battery[0];
                    batteryinfo = String.Format($"{Double.Parse(battery[1]) / 1000.0}V {Double.Parse(battery[2]) / 10.0}℃");
                }
                catch
                {
                    batterylevel = "0";
                    batteryinfo = "--";
                }
                try
                {
                    string[] mem = StringHelper.Mem(await CallExternalProgram.ADB($"-s {devicename} shell cat /proc/meminfo | grep Mem"));
                    memlevel = Math.Round(Math.Round(Double.Parse(mem[1]) * 1.024 / 1000000, 1) / Math.Round(Double.Parse(mem[0]) * 1.024 / 1000000) * 100).ToString();
                    usemem = String.Format($"{Math.Round(Double.Parse(mem[1]) * 1.024 / 1000000, 1)}GB/{Math.Round(Double.Parse(mem[0]) * 1.024 / 1000000)}GB");
                }
                catch
                {
                    memlevel = "0";
                    usemem = "--";
                }
                try
                {
                    string diskinfos1 = await CallExternalProgram.ADB($"-s {devicename} shell df /storage/emulated");
                    string diskinfos2 = await CallExternalProgram.ADB($"-s {devicename} shell df /data");
                    if (diskinfos1.IndexOf("/storage/emulated") != -1)
                    {
                        string[] columns = StringHelper.DiskInfo(diskinfos1, "/storage/emulated");
                        progressdisk = columns[4].TrimEnd('%');
                        diskinfo = String.Format($"{double.Parse(columns[2]) / 1024 / 1024:0.00}GB/{double.Parse(columns[1]) / 1024 / 1024:0.00}GB");
                    }
                    else
                    {
                        string[] columns = StringHelper.DiskInfo(diskinfos2, "/data");
                        progressdisk = columns[4].TrimEnd('%');
                        diskinfo = String.Format($"{double.Parse(columns[2]) / 1024 / 1024:0.00}GB/{double.Parse(columns[1]) / 1024 / 1024:0.00}GB");
                    }
                }
                catch
                {
                    progressdisk = "0";
                    diskinfo = "--";
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
            devices.Add("VNDKVersion", vndkversion);
            devices.Add("CPUCode", cpucode);
            devices.Add("PowerOnTime", powerontime);
            devices.Add("DeviceBrand", devicebrand);
            devices.Add("DeviceModel", devicemodel);
            devices.Add("AndroidSDK", androidsdk);
            devices.Add("CPUABI", cpuabi);
            devices.Add("DisplayHW", displayhw);
            devices.Add("SELinux", selinux);
            devices.Add("Density", density);
            devices.Add("BoardID", boardid);
            devices.Add("Platform", platform);
            devices.Add("Compile", compileversion);
            devices.Add("Kernel", kernel);
            devices.Add("BatteryLevel", batterylevel);
            devices.Add("BatteryInfo", batteryinfo);
            devices.Add("MemLevel", memlevel);
            devices.Add("UseMem", usemem);
            devices.Add("DiskInfo", diskinfo);
            devices.Add("ProgressDisk", progressdisk);
            return devices;
        }

        public static async Task<Dictionary<string, string>> DevicesInfoLittle(string devicename)
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
