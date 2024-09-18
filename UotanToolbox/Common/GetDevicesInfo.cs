using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Collections;

namespace UotanToolbox.Common
{
    internal class GetDevicesInfo
    {
        static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);

        public static async Task<string[]> DevicesList()
        {
            var adb = await CallExternalProgram.ADB("devices");
            var fastboot = await CallExternalProgram.Fastboot("devices");
            var devcon = Global.System == "Windows" ? await CallExternalProgram.Devcon("find usb*") : await CallExternalProgram.LsUSB();
            var adbdevices = StringHelper.ADBDevices(adb);
            var fbdevices = StringHelper.FastbootDevices(fastboot);
            var comdevices = StringHelper.COMDevices(devcon);
            var devices = new string[adbdevices.Length + fbdevices.Length + comdevices.Length];
            Array.Copy(adbdevices, 0, devices, 0, adbdevices.Length);
            Array.Copy(fbdevices, 0, devices, adbdevices.Length, fbdevices.Length);
            Array.Copy(comdevices, 0, devices, adbdevices.Length + fbdevices.Length, comdevices.Length);
            return devices;
        }

        public static async Task<bool> SetDevicesInfoLittle()
        {
            var devices = await GetDevicesInfo.DevicesList();

            if (devices.Length != 0)
            {
                Global.deviceslist = new AvaloniaList<string>(devices);

                if (Global.thisdevice == null || !string.Join("", Global.deviceslist).Contains(Global.thisdevice))
                {
                    Global.thisdevice = Global.deviceslist.First();
                }

                if (Global.thisdevice != null && Global.deviceslist.Contains(Global.thisdevice))
                {
                    var sukiViewModel = GlobalData.MainViewModelInstance;
                    var DevicesInfoLittle = await GetDevicesInfo.DevicesInfoLittle(Global.thisdevice);
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
                var sukiViewModel = GlobalData.MainViewModelInstance;
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
            var status = "--";
            var blstatus = "--";
            var codename = "--";
            var vabstatus = "--";
            var vndkversion = "--";
            var cpucode = "--";
            var powerontime = "--";
            var devicebrand = "--";
            var devicemodel = "--";
            var androidsdk = "--";
            var cpuabi = "--";
            var displayhw = "--";
            var density = "--";
            var boardid = "--";
            var compileversion = "--";
            var platform = "--";
            var kernel = "--";
            var disktype = "--";
            var batterylevel = "0";
            var batteryinfo = "--";
            var memlevel = "0";
            var usemem = "--";
            var diskinfo = "--";
            var progressdisk = "0";
            var adb = await CallExternalProgram.ADB("devices");
            var fastboot = await CallExternalProgram.Fastboot("devices");
            var devcon = Global.System == "Windows" ? await CallExternalProgram.Devcon("find usb*") : await CallExternalProgram.LsUSB();

            if (fastboot.Contains(devicename))
            {
                var isuserspace = await CallExternalProgram.Fastboot($"-s {devicename} getvar is-userspace");

                if (isuserspace.Contains("yes"))
                {
                    status = GetTranslation("Home_Fastbootd");
                    vndkversion = StringHelper.FastbootVar(await CallExternalProgram.Fastboot($"-s {devicename} getvar version-vndk"), "version-vndk");
                }
                else
                {
                    status = GetTranslation("Home_Fastboot");
                    var type = await CallExternalProgram.Fastboot($"-s {devicename} getvar variant");
                    disktype = type.Contains("UFS") ? "UFS" : type.Contains("EMMC") ? "EMMC" : "--";
                }

                var blinfo = await CallExternalProgram.Fastboot($"-s {devicename} getvar unlocked");
                blstatus = blinfo.Contains("yes") ? GetTranslation("Info_BLstatusUnlocked") : GetTranslation("Info_BLstatusLocked");
                var productinfos = await CallExternalProgram.Fastboot($"-s {devicename} getvar product");
                var product = StringHelper.GetProductID(productinfos);

                if (product != null)
                {
                    codename = product;
                }

                var active = await CallExternalProgram.Fastboot($"-s {devicename} getvar current-slot");

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
                var thisdevice = "";
                var Lines = adb.Split(separator, StringSplitOptions.RemoveEmptyEntries);

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

                var active = await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.boot.slot_suffix");

                vabstatus = active.Contains("_a")
                    ? GetTranslation("Info_ASlot")
                    : active.Contains("_b")
                        ? GetTranslation("Info_BSlot")
                        : status == GetTranslation("Info_UnauthorizedDevice") || status == GetTranslation("Home_Sideload")
                                            ? "--"
                                            : GetTranslation("Info_AOnly");

                if (status == GetTranslation("Home_System"))
                {
                    var android = await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.build.version.release");
                    var sdk = await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.build.version.sdk");
                    androidsdk = string.Format($"Android {StringHelper.RemoveLineFeed(android)}({StringHelper.RemoveLineFeed(sdk)})");
                    displayhw = StringHelper.ColonSplit(StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell wm size")));
                    density = StringHelper.Density(await CallExternalProgram.ADB($"-s {devicename} shell wm density"));
                }
                else if (status == GetTranslation("Home_Recovery") || status == GetTranslation("Home_Sideload"))
                {
                    androidsdk = GetTranslation("Home_Recovery");
                    displayhw = "--";
                    density = "--";
                }

                var bid = StringHelper.RemoveLineFeed(await CallExternalProgram.ADB($"-s {devicename} shell cat /sys/devices/soc0/serial_number"));
                boardid = bid.Contains("No such file") || bid.Contains("Permission denied") ? "--" : bid;
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
                    var ptime = await CallExternalProgram.ADB($"-s {devicename} shell cat /proc/uptime");
                    var intptime = int.Parse(ptime.Split('.')[0].Trim());
                    var timeSpan = TimeSpan.FromSeconds(intptime);
                    powerontime = $"{timeSpan.Days}{GetTranslation("Info_Day")}{timeSpan.Hours}{GetTranslation("Info_Hour")}{timeSpan.Minutes}{GetTranslation("Info_Minute")}{timeSpan.Seconds}{GetTranslation("Info_Second")}";
                }
                catch
                {
                    powerontime = "--";
                }

                try
                {
                    var battery = StringHelper.Battery(await CallExternalProgram.ADB($"-s {devicename} shell dumpsys battery"));
                    batterylevel = battery[0];
                    batteryinfo = string.Format($"{double.Parse(battery[1]) / 1000.0}V {double.Parse(battery[2]) / 10.0}℃");
                }
                catch
                {
                    batterylevel = "0";
                    batteryinfo = "--";
                }

                try
                {
                    var mem = StringHelper.Mem(await CallExternalProgram.ADB($"-s {devicename} shell cat /proc/meminfo | grep Mem"));
                    memlevel = Math.Round(Math.Round(double.Parse(mem[1]) * 1.024 / 1000000, 1) / Math.Round(double.Parse(mem[0]) * 1.024 / 1000000) * 100).ToString();
                    usemem = string.Format($"{Math.Round(double.Parse(mem[1]) * 1.024 / 1000000, 1)}GB/{Math.Round(double.Parse(mem[0]) * 1.024 / 1000000)}GB");
                }
                catch
                {
                    memlevel = "0";
                    usemem = "--";
                }

                try
                {
                    var diskinfos1 = await CallExternalProgram.ADB($"-s {devicename} shell df /storage/emulated");
                    var diskinfos2 = await CallExternalProgram.ADB($"-s {devicename} shell df /data");

                    if (diskinfos1.Contains("/storage/emulated"))
                    {
                        var columns = StringHelper.DiskInfo(diskinfos1, "/storage/emulated");
                        progressdisk = columns[4].TrimEnd('%');
                        diskinfo = string.Format($"{double.Parse(columns[2]) / 1024 / 1024:0.00}GB/{double.Parse(columns[1]) / 1024 / 1024:0.00}GB");
                    }
                    else if (diskinfos2.Contains("/sdcard"))
                    {
                        var columns = StringHelper.DiskInfo(diskinfos2, "/sdcard");
                        progressdisk = columns[4].TrimEnd('%');
                        diskinfo = string.Format($"{double.Parse(columns[2]) / 1024 / 1024:0.00}GB/{double.Parse(columns[1]) / 1024 / 1024:0.00}GB");
                    }
                    else
                    {
                        var columns = StringHelper.DiskInfo(diskinfos2, "/data");
                        progressdisk = columns[4].TrimEnd('%');
                        diskinfo = string.Format($"{double.Parse(columns[2]) / 1024 / 1024:0.00}GB/{double.Parse(columns[1]) / 1024 / 1024:0.00}GB");
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
                var thisdevice = deviceLines.FirstOrDefault(line => line.Contains(devicename));

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
                var statusPattern = deviceLines.FirstOrDefault(line => line.EndsWith(":900e") || line.EndsWith(":901d") || line.EndsWith(":9091"));

                if (statusPattern != null)
                {
                    status = statusPattern[(statusPattern.LastIndexOf(':') + 1)..];
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
            var status = "--";
            var blstatus = "--";
            var codename = "--";
            var vabstatus = "--";
            var adb = await CallExternalProgram.ADB("devices");
            var fastboot = await CallExternalProgram.Fastboot("devices");
            var devcon = Global.System == "Windows" ? await CallExternalProgram.Devcon("find usb*") : await CallExternalProgram.LsUSB();

            if (fastboot.Contains(devicename))
            {
                var isuserspace = await CallExternalProgram.Fastboot($"-s {devicename} getvar is-userspace");
                status = isuserspace.Contains("yes") ? GetTranslation("Home_Fastbootd") : GetTranslation("Home_Fastboot");
                var blinfo = await CallExternalProgram.Fastboot($"-s {devicename} getvar unlocked");
                blstatus = blinfo.Contains("yes") ? GetTranslation("Info_BLstatusUnlocked") : GetTranslation("Info_BLstatusLocked");
                var productinfos = await CallExternalProgram.Fastboot($"-s {devicename} getvar product");
                var product = StringHelper.GetProductID(productinfos);

                if (product != null)
                {
                    codename = product;
                }

                var active = await CallExternalProgram.Fastboot($"-s {devicename} getvar current-slot");

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
                var thisdevice = "";
                var Lines = adb.Split(separator, StringSplitOptions.RemoveEmptyEntries);

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

                var active = await CallExternalProgram.ADB($"-s {devicename} shell getprop ro.boot.slot_suffix");

                vabstatus = active.Contains("_a")
                    ? GetTranslation("Info_ASlot")
                    : active.Contains("_b")
                        ? GetTranslation("Info_BSlot")
                        : status == GetTranslation("Info_UnauthorizedDevice") || status == GetTranslation("Home_Sideload")
                                            ? "--"
                                            : GetTranslation("Info_AOnly");

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
                var thisdevice = deviceLines.FirstOrDefault(line => line.Contains(devicename));

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
                var statusPattern = deviceLines.FirstOrDefault(line => line.EndsWith(":900e") || line.EndsWith(":901d") || line.EndsWith(":9091"));

                if (statusPattern != null)
                {
                    status = statusPattern[(statusPattern.LastIndexOf(':') + 1)..];
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