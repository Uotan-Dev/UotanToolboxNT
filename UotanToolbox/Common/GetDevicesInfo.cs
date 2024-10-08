﻿using Avalonia.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UotanToolbox.Common
{
    internal class GetDevicesInfo
    {
        private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
        public static async Task<string[]> DevicesList()
        {
            string adb = await CallExternalProgram.ADB("devices");
            string fastboot = await CallExternalProgram.Fastboot("devices");
            string devcon;
            if (Global.System == "Windows")
            {
                devcon = await CallExternalProgram.Devcon("find usb*");
            }
            else
            {
                devcon = await CallExternalProgram.LsUSB();
            }
            string[] adbdevices = StringHelper.ADBDevices(adb);
            string[] fbdevices = StringHelper.FastbootDevices(fastboot);
            string[] comdevices = StringHelper.COMDevices(devcon);
            string[] devices = new string[adbdevices.Length + fbdevices.Length + comdevices.Length];
            Array.Copy(adbdevices, 0, devices, 0, adbdevices.Length);
            Array.Copy(fbdevices, 0, devices, adbdevices.Length, fbdevices.Length);
            Array.Copy(comdevices, 0, devices, adbdevices.Length + fbdevices.Length, comdevices.Length);
            return devices;
        }

        public static async Task<bool> SetDevicesInfoLittle()
        {
            string[] devices = await GetDevicesInfo.DevicesList();
            if (devices.Length != 0)
            {
                Global.deviceslist = new AvaloniaList<string>(devices);
                if (Global.thisdevice == null || !string.Join("", Global.deviceslist).Contains(Global.thisdevice))
                {
                    Global.thisdevice = Global.deviceslist.First();
                }
                if (Global.thisdevice != null && Global.deviceslist.Contains(Global.thisdevice))
                {
                    MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
                    Dictionary<string, string> DevicesInfoLittle = await GetDevicesInfo.DevicesInfoLittle(Global.thisdevice);
                    sukiViewModel.Status = DevicesInfoLittle["Status"];
                    sukiViewModel.BLStatus = DevicesInfoLittle["BLStatus"];
                    sukiViewModel.VABStatus = DevicesInfoLittle["VABStatus"];
                    sukiViewModel.CodeName = DevicesInfoLittle["CodeName"];
                }
                return true;
            }
            else
            {
                Global.deviceslist = null;
                MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
                sukiViewModel.Status = "--";
                sukiViewModel.BLStatus = "--";
                sukiViewModel.VABStatus = "--";
                sukiViewModel.CodeName = "--";
                return false;
            }
        }

        public static async Task<Dictionary<string, string>> DevicesInfo(string devicename)
        {
            Dictionary<string, string> devices = [];
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
            string compileversion = "--";
            string platform = "--";
            string kernel = "--";
            string disktype = "--";
            string batterylevel = "0";
            string batteryinfo = "--";
            string memlevel = "0";
            string usemem = "--";
            string diskinfo = "--";
            string progressdisk = "0";
            string adb = await CallExternalProgram.ADB("devices");
            string fastboot = await CallExternalProgram.Fastboot("devices");
            string devcon;
            if (Global.System == "Windows")
            {
                devcon = await CallExternalProgram.Devcon("find usb*");
            }
            else
            {
                devcon = await CallExternalProgram.LsUSB();
            }
            if (fastboot.Contains(devicename))
            {
                string isuserspace = await CallExternalProgram.Fastboot($"-s {devicename} getvar is-userspace");
                if (isuserspace.Contains("yes"))
                {
                    status = GetTranslation("Home_Fastbootd");
                    vndkversion = StringHelper.FastbootVar(await CallExternalProgram.Fastboot($"-s {devicename} getvar version-vndk"), "version-vndk");
                }
                else
                {
                    status = GetTranslation("Home_Fastboot");
                    string type = await CallExternalProgram.Fastboot($"-s {devicename} getvar variant");
                    if (type.Contains("UFS"))
                    {
                        disktype = "UFS";
                    }
                    else if (type.Contains("EMMC"))
                    {
                        disktype = "EMMC";
                    }
                    else
                    {
                        disktype = "--";
                    }
                }
                string blinfo = await CallExternalProgram.Fastboot($"-s {devicename} getvar unlocked");
                if (blinfo.Contains("yes"))
                {
                    blstatus = GetTranslation("Info_BLstatusUnlocked");
                }
                else
                {
                    blstatus = GetTranslation("Info_BLstatusLocked");
                }
                string productinfos = await CallExternalProgram.Fastboot($"-s {devicename} getvar product");
                string product = StringHelper.GetProductID(productinfos);
                if (product != null)
                {
                    codename = product;
                }
                string active = await CallExternalProgram.Fastboot($"-s {devicename} getvar current-slot");
                if (active.Contains("current-slot: a"))
                {
                    vabstatus = GetTranslation("Info_ASlot");
                }
                else if (active.Contains("current-slot: b"))
                {
                    vabstatus = GetTranslation("Info_BSlot");
                }
                else if (active.Contains("FAILED"))
                {
                    vabstatus = GetTranslation("Info_AOnly");
                }
            }
            if (adb.Contains(devicename))
            {
                string thisdevice = "";
                string[] Lines = adb.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < Lines.Length; i++)
                {
                    if (Lines[i].Contains(devicename))
                    {
                        thisdevice = Lines[i];
                        break;
                    }
                }
                if (thisdevice.Contains("recovery"))
                {
                    status = GetTranslation("Home_Recovery");
                }
                else if (thisdevice.Contains("sideload"))
                {
                    status = GetTranslation("Home_Sideload");
                }
                else if (thisdevice.Contains("	device"))
                {
                    status = GetTranslation("Home_System");
                }
                else if (thisdevice.Contains("unauthorized"))
                {
                    status = GetTranslation("Info_UnauthorizedDevice");
                }
                string active = await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.boot.slot_suffix");
                if (active.Contains("_a"))
                {
                    vabstatus = GetTranslation("Info_ASlot");
                }
                else if (active.Contains("_b"))
                {
                    vabstatus = GetTranslation("Info_BSlot");
                }
                else if (status == GetTranslation("Info_UnauthorizedDevice") || status == GetTranslation("Home_Sideload"))
                {
                    vabstatus = "--";
                }
                else
                {
                    vabstatus = GetTranslation("Info_AOnly");
                }
                if (status == GetTranslation("Home_System"))
                {
                    string android = await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.build.version.release");
                    string sdk = await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.build.version.sdk");
                    androidsdk = String.Format($"Android {StringHelper.RemoveLineFeed(android)}({StringHelper.RemoveLineFeed(sdk)})");
                    displayhw = StringHelper.ColonSplit(StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell wm size")));
                    density = StringHelper.Density(await CallExternalProgram.ADB($"-s {devicename} shell wm density"));
                }
                else if (status == GetTranslation("Home_Recovery") || status == GetTranslation("Home_Sideload"))
                {
                    androidsdk = GetTranslation("Home_Recovery");
                    displayhw = "--";
                    density = "--";
                }
                string bid = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell cat /sys/devices/soc0/serial_number"));
                if (bid.Contains("No such file") || bid.Contains("Permission denied"))
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
                codename = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.product.device"));
                blstatus = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.secureboot.lockstate"));
                if (blstatus == "--")
                {
                    blstatus = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.boot.vbmeta.device_state"));
                }
                compileversion = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.system.build.version.incremental"));
                platform = StringHelper.ColonSplit(StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell cat /proc/cpuinfo | grep Hardware")));
                kernel = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell uname -r"));
                try
                {
                    string ptime = await CallExternalProgram.ADB($"-s {devicename} shell cat /proc/uptime");
                    int intptime = int.Parse(ptime.Split('.')[0].Trim());
                    TimeSpan timeSpan = TimeSpan.FromSeconds(intptime);
                    powerontime = $"{timeSpan.Days}{GetTranslation("Info_Day")}{timeSpan.Hours}{GetTranslation("Info_Hour")}{timeSpan.Minutes}{GetTranslation("Info_Minute")}{timeSpan.Seconds}{GetTranslation("Info_Second")}";
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
                    if (diskinfos1.Contains("/storage/emulated"))
                    {
                        string[] columns = StringHelper.DiskInfo(diskinfos1, "/storage/emulated");
                        progressdisk = columns[4].TrimEnd('%');
                        diskinfo = String.Format($"{double.Parse(columns[2]) / 1024 / 1024:0.00}GB/{double.Parse(columns[1]) / 1024 / 1024:0.00}GB");
                    }
                    else if (diskinfos2.Contains("/sdcard"))
                    {
                        string[] columns = StringHelper.DiskInfo(diskinfos2, "/sdcard");
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
            var deviceLines = devcon.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (devcon.Contains(devicename))
            {
                string thisdevice = deviceLines.FirstOrDefault(line => line.Contains(devicename));
                if (thisdevice != null)
                {
                    if (thisdevice.Contains("QDLoader"))
                    {
                        status = "9008";
                    }
                    else if (thisdevice.Contains("900E ("))
                    {
                        status = "900E";
                    }
                    else if (thisdevice.Contains("901D ("))
                    {
                        status = "901D";
                    }
                    else if (thisdevice.Contains("9091 ("))
                    {
                        status = "9091";
                    }
                }
            }
            if (devicename.Contains("ttyUSB"))
            {
                status = "9008";
            }
            if (devicename == "Unknown device")
            {
                string statusPattern = deviceLines.FirstOrDefault(line => line.EndsWith(":900e") || line.EndsWith(":901d") || line.EndsWith(":9091"));
                if (statusPattern != null)
                {
                    status = statusPattern.Substring(statusPattern.LastIndexOf(':') + 1);
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
            devices.Add("DiskType", disktype);
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

        internal static readonly char[] separator = ['\r', '\n'];

        public static async Task<Dictionary<string, string>> DevicesInfoLittle(string devicename)
        {
            Dictionary<string, string> devices = [];
            string status = "--";
            string blstatus = "--";
            string codename = "--";
            string vabstatus = "--";
            string adb = await CallExternalProgram.ADB("devices");
            string fastboot = await CallExternalProgram.Fastboot("devices");
            string devcon;
            if (Global.System == "Windows")
            {
                devcon = await CallExternalProgram.Devcon("find usb*");
            }
            else
            {
                devcon = await CallExternalProgram.LsUSB();
            }
            if (fastboot.Contains(devicename))
            {
                string isuserspace = await CallExternalProgram.Fastboot($"-s {devicename} getvar is-userspace");
                if (isuserspace.Contains("yes"))
                {
                    status = GetTranslation("Home_Fastbootd");
                }
                else
                {
                    status = GetTranslation("Home_Fastboot");
                }
                string blinfo = await CallExternalProgram.Fastboot($"-s {devicename} getvar unlocked");
                if (blinfo.Contains("yes"))
                {
                    blstatus = GetTranslation("Info_BLstatusUnlocked");
                }
                else
                {
                    blstatus = GetTranslation("Info_BLstatusLocked");
                }
                string productinfos = await CallExternalProgram.Fastboot($"-s {devicename} getvar product");
                string product = StringHelper.GetProductID(productinfos);
                if (product != null)
                {
                    codename = product;
                }
                string active = await CallExternalProgram.Fastboot($"-s {devicename} getvar current-slot");
                if (active.Contains("current-slot: a"))
                {
                    vabstatus = GetTranslation("Info_ASlot");
                }
                else if (active.Contains("current-slot: b"))
                {
                    vabstatus = GetTranslation("Info_BSlot");
                }
                else if (active.Contains("FAILED"))
                {
                    vabstatus = GetTranslation("Info_AOnly");
                }
            }
            if (adb.Contains(devicename))
            {
                string thisdevice = "";
                string[] Lines = adb.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < Lines.Length; i++)
                {
                    if (Lines[i].Contains(devicename))
                    {
                        thisdevice = Lines[i];
                        break;
                    }
                }
                if (thisdevice.Contains("recovery"))
                {
                    status = GetTranslation("Home_Recovery");
                }
                else if (thisdevice.Contains("sideload"))
                {
                    status = GetTranslation("Home_Sideload");
                }
                else if (thisdevice.Contains("	device"))
                {
                    status = GetTranslation("Home_System");
                }
                else if (thisdevice.Contains("unauthorized"))
                {
                    status = GetTranslation("Info_UnauthorizedDevice");
                }
                string active = await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.boot.slot_suffix");
                if (active.Contains("_a"))
                {
                    vabstatus = GetTranslation("Info_ASlot");
                }
                else if (active.Contains("_b"))
                {
                    vabstatus = GetTranslation("Info_BSlot");
                }
                else if (status == GetTranslation("Info_UnauthorizedDevice") || status == GetTranslation("Home_Sideload"))
                {
                    vabstatus = "--";
                }
                else
                {
                    vabstatus = GetTranslation("Info_AOnly");
                }
                codename = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.product.device"));
                blstatus = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.secureboot.lockstate"));
                if (blstatus == "--")
                {
                    blstatus = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.boot.vbmeta.device_state"));
                }
            }
            var deviceLines = devcon.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (devcon.Contains(devicename))
            {
                string thisdevice = deviceLines.FirstOrDefault(line => line.Contains(devicename));
                if (thisdevice != null)
                {
                    if (thisdevice.Contains("QDLoader"))
                    {
                        status = "9008";
                    }
                    else if (thisdevice.Contains("900E ("))
                    {
                        status = "900E";
                    }
                    else if (thisdevice.Contains("901D ("))
                    {
                        status = "901D";
                    }
                    else if (thisdevice.Contains("9091 ("))
                    {
                        status = "9091";
                    }
                }
            }
            if (devicename.Contains("ttyUSB"))
            {
                status = "9008";
            }
            if (devicename == "Unknown device")
            {
                string statusPattern = deviceLines.FirstOrDefault(line => line.EndsWith(":900e") || line.EndsWith(":901d") || line.EndsWith(":9091"));
                if (statusPattern != null)
                {
                    status = statusPattern.Substring(statusPattern.LastIndexOf(':') + 1);
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
