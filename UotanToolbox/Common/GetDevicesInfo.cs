using Avalonia.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UotanToolbox.Common
{
    internal class GetDevicesInfo
    {
        private static string GetTranslation(string key)
        {
            return FeaturesHelper.GetTranslation(key);
        }

        private static async Task<string> RunAdb(string deviceId, string cmd)
        {
            if (Global.DeviceManager != null)
            {
                var dev = Global.DeviceManager.Devices.FirstOrDefault(d => d.Id == deviceId && d.Transport == Devices.TransportType.Adb);
                if (dev != null)
                {
                    return await Global.DeviceManager.ExecuteAsync(dev, cmd);
                }
            }
            return await CallExternalProgram.ADB($"-s {deviceId} {cmd}");
        }

        private static async Task<string> RunFastboot(string deviceId, string cmd)
        {
            if (Global.DeviceManager != null)
            {
                var dev = Global.DeviceManager.Devices.FirstOrDefault(d => d.Id == deviceId && d.Transport == Devices.TransportType.Fastboot);
                if (dev != null)
                {
                    return await Global.DeviceManager.ExecuteAsync(dev, cmd);
                }
            }
            return await CallExternalProgram.Fastboot($"-s {deviceId} {cmd}");
        }

        private static async Task<string> RunHdc(string deviceId, string cmd)
        {
            if (Global.DeviceManager != null)
            {
                var dev = Global.DeviceManager.Devices.FirstOrDefault(d => d.Id == deviceId && d.Transport == Devices.TransportType.Hdc);
                if (dev != null)
                {
                    return await Global.DeviceManager.ExecuteAsync(dev, cmd);
                }
            }
            return await CallExternalProgram.HDC($"-t {deviceId} {cmd}");
        }

        public static async Task<string[]> DevicesList()
        {
            // 使用统一的设备管理器进行扫描
            if (Global.DeviceManager == null)
            {
                return Array.Empty<string>();
            }

            await Global.DeviceManager.ScanAsync();
            return Global.DeviceManager.Devices.Select(d => d.Id).ToArray();
        }

        public static async Task<bool> SetDevicesInfoLittle()
        {
            if (Global.DeviceManager == null)
            {
                Global.deviceslist = null;
                return false;
            }

            // 确保扫描过一次
            await Global.DeviceManager.ScanAsync();

            var list = Global.DeviceManager.Devices.Select(d => d.Id).ToArray();
            if (list.Length != 0)
            {
                Global.deviceslist = new AvaloniaList<string>(list);
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
            string systemsdk = "--";
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
            string adb = await FeaturesHelper.AdbCmd("", "devices");
            string fastboot = await FeaturesHelper.FastbootCmd("", "devices");
            string devcon = Global.System == "Windows" ? await CallExternalProgram.Devcon("find usb*") : await CallExternalProgram.LsUSB();
            string hdc = await FeaturesHelper.HdcCmd("", "list targets");
            if (fastboot.Contains(devicename))
            {
                string isuserspace = await RunFastboot(devicename, "getvar is-userspace");
                if (isuserspace.Contains("yes"))
                {
                    status = GetTranslation("Home_Fastbootd");
                    vndkversion = StringHelper.FastbootVar(await RunFastboot(devicename, "getvar version-vndk"), "version-vndk");
                }
                else
                {
                    status = GetTranslation("Home_Fastboot");
                    string type = await RunFastboot(devicename, "getvar variant");
                    disktype = type.Contains("UFS") ? "UFS" : type.Contains("EMMC") ? "EMMC" : "--";
                }
                string blinfo = await RunFastboot(devicename, "getvar unlocked");
                blstatus = blinfo.Contains("yes") ? GetTranslation("Info_BLstatusUnlocked") : GetTranslation("Info_BLstatusLocked");
                string productinfos = await RunFastboot(devicename, "getvar product");
                string product = StringHelper.GetProductID(productinfos);
                if (product != null)
                {
                    codename = product;
                }
                string active = await RunFastboot(devicename, "getvar current-slot");
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
                    status = GetTranslation("Home_Android");
                }
                else if (thisdevice.Contains("unauthorized"))
                {
                    status = GetTranslation("Info_UnauthorizedDevice");
                }
                string active = await RunAdb(devicename, "shell getprop ro.boot.slot_suffix");
                vabstatus = active.Contains("_a")
                    ? GetTranslation("Info_ASlot")
                    : active.Contains("_b")
                        ? GetTranslation("Info_BSlot")
                        : status == GetTranslation("Info_UnauthorizedDevice") || status == GetTranslation("Home_Sideload")
                                            ? "--"
                                            : GetTranslation("Info_AOnly");
                if (status == GetTranslation("Home_Android"))
                {
                    string android = await RunAdb(devicename, "shell getprop ro.build.version.release");
                    string sdk = await RunAdb(devicename, "shell getprop ro.build.version.sdk");
                    systemsdk = String.Format($"Android {StringHelper.RemoveLineFeed(android)}({StringHelper.RemoveLineFeed(sdk)})");
                    displayhw = StringHelper.ColonSplit(StringHelper.RemoveLineFeed(await RunAdb(devicename, "shell wm size")));
                    density = StringHelper.Density(await RunAdb(devicename, "shell wm density"));
                }
                else if (status == GetTranslation("Home_Recovery") || status == GetTranslation("Home_Sideload"))
                {
                    systemsdk = GetTranslation("Home_Recovery");
                    displayhw = "--";
                    density = "--";
                }
                string bid = StringHelper.RemoveLineFeed(await RunAdb(devicename, "shell cat /sys/devices/soc0/serial_number"));
                boardid = bid.Contains("No such file") || bid.Contains("Permission denied") ? "--" : bid;
                vndkversion = StringHelper.RemoveLineFeed(await RunAdb(devicename, "shell getprop ro.vndk.version"));
                cpucode = StringHelper.RemoveLineFeed(await RunAdb(devicename, "shell getprop ro.board.platform"));
                devicebrand = StringHelper.RemoveLineFeed(await RunAdb(devicename, "shell getprop ro.product.brand"));
                devicemodel = StringHelper.RemoveLineFeed(await RunAdb(devicename, "shell getprop ro.product.model"));
                cpuabi = StringHelper.RemoveLineFeed(await RunAdb(devicename, "shell getprop ro.product.cpu.abi"));
                codename = StringHelper.RemoveLineFeed(await RunAdb(devicename, "shell getprop ro.product.device"));
                blstatus = StringHelper.RemoveLineFeed(await RunAdb(devicename, "shell getprop ro.secureboot.lockstate"));
                if (blstatus == "--")
                {
                    blstatus = StringHelper.RemoveLineFeed(await RunAdb(devicename, "shell getprop ro.boot.vbmeta.device_state"));
                }
                compileversion = StringHelper.RemoveLineFeed(await RunAdb(devicename, "shell getprop ro.system.build.version.incremental"));
                platform = StringHelper.ColonSplit(StringHelper.RemoveLineFeed(await RunAdb(devicename, "shell cat /proc/cpuinfo | grep Hardware")));
                kernel = StringHelper.RemoveLineFeed(await RunAdb(devicename, "shell uname -r"));
                try
                {
                    string ptime = await RunAdb(devicename, "shell cat /proc/uptime");
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
                    string[] battery = StringHelper.Battery(await RunAdb(devicename, "shell dumpsys battery"));
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
                    string[] mem = StringHelper.Mem(await RunAdb(devicename, "shell cat /proc/meminfo | grep Mem"));
                    double use = double.Parse(mem[0]) - double.Parse(mem[1]);
                    memlevel = Math.Round(Math.Round(use * 1.024 / 1000000, 1) / Math.Round(double.Parse(mem[0]) * 1.024 / 1000000) * 100).ToString();
                    usemem = string.Format($"{Math.Round(use * 1.024 / 1000000, 1)}GB/{Math.Round(double.Parse(mem[0]) * 1.024 / 1000000)}GB");
                }
                catch
                {
                    memlevel = "0";
                    usemem = "--";
                }
                try
                {
                    string diskinfos1 = await RunAdb(devicename, "shell df /storage/emulated");
                    string diskinfos2 = await RunAdb(devicename, "shell df /data");
                    if (diskinfos1.Contains("/storage/emulated"))
                    {
                        string[] columns = StringHelper.DiskInfo(diskinfos1, "/storage/emulated");
                        progressdisk = columns[4].TrimEnd('%');
                        diskinfo = string.Format($"{double.Parse(columns[2]) / 1024 / 1024:0.00}GB/{double.Parse(columns[1]) / 1024 / 1024:0.00}GB");
                    }
                    else if (diskinfos2.Contains("/sdcard"))
                    {
                        string[] columns = StringHelper.DiskInfo(diskinfos2, "/sdcard");
                        progressdisk = columns[4].TrimEnd('%');
                        diskinfo = string.Format($"{double.Parse(columns[2]) / 1024 / 1024:0.00}GB/{double.Parse(columns[1]) / 1024 / 1024:0.00}GB");
                    }
                    else
                    {
                        string[] columns = StringHelper.DiskInfo(diskinfos2, "/data");
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
            if (hdc.Contains(devicename))
            {
                string thisdevice = "";
                string[] Lines = hdc.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < Lines.Length; i++)
                {
                    if (Lines[i].Contains(devicename))
                    {
                        thisdevice = Lines[i];
                        break;
                    }
                }
                if (thisdevice.Contains("Unauthorized"))
                {
                    status = GetTranslation("Info_UnauthorizedDevice");
                }
                else
                {
                    status = GetTranslation("Home_OpenHOS");
                }
                blstatus = "--";
                string sdk = StringHelper.RemoveLineFeed(await RunHdc(devicename, "shell param get const.ohos.apiversion"));
                string harmony = StringHelper.OHVersion(await RunHdc(devicename, "shell param get const.product.software.version"));
                if (harmony != "--")
                {
                    systemsdk = String.Format($"OpenHarmony {harmony}({StringHelper.RemoveSpace(sdk)})");
                }
                try
                {
                    string[] deviceinfo = StringHelper.OHDeviceInof(await RunHdc(devicename, "shell SP_daemon -deviceinfo"));
                    //systemsdk = String.Format($"{deviceinfo[1]}({StringHelper.RemoveSpace(sdk)})");
                    cpucode = deviceinfo[0];
                }
                catch
                {
                    cpucode = "--";
                }
                codename = StringHelper.RemoveLineFeed(await RunHdc(devicename, "shell param get const.product.model"));
                devicebrand = StringHelper.RemoveLineFeed(await RunHdc(devicename, "shell param get const.product.brand"));
                if (devicebrand.Contains("default"))
                {
                    devicebrand = StringHelper.RemoveLineFeed(await RunHdc(devicename, "shell param get const.product.manufacturer"));
                }
                devicemodel = StringHelper.RemoveLineFeed(await RunHdc(devicename, "shell param get const.product.name"));
                displayhw = StringHelper.OHColonSplit(await RunHdc(devicename, "shell hidumper -s RenderService -a screen"));
                kernel = StringHelper.OHKernel(await RunHdc(devicename, "shell uname -a"));
                cpuabi = StringHelper.RemoveLineFeed(await RunHdc(devicename, "shell param get const.product.cpu.abilist"));
                try
                {
                    compileversion = StringHelper.OHBuildVersion(await RunHdc(devicename, "shell param get const.build.description"));
                }
                catch
                {
                    compileversion = "--";
                }
                try
                {
                    powerontime = StringHelper.OHPowerOnTime(await RunHdc(devicename, "shell hidumper -s TimeService -a -a"));
                }
                catch
                {
                    powerontime = "--";
                }
                try
                {
                    string[] battery = StringHelper.BatteryOH(await RunHdc(devicename, "shell hidumper -s BatteryService -a -i"));
                    batterylevel = battery[0];
                    batteryinfo = String.Format($"{Double.Parse(battery[1]) / 1000000.0}V {Double.Parse(battery[2]) / 10.0}℃");
                }
                catch
                {
                    batterylevel = "0";
                    batteryinfo = "--";
                }
                try
                {
                    density = StringHelper.OHDensity(await RunHdc(devicename, "shell hidumper -s DisplayManagerService -a -a"));
                }
                catch
                {
                    density = "--";
                }
                try
                {
                    string[] mem = StringHelper.OHMem(await RunHdc(devicename, "shell hidumper -s MemoryManagerService --mem"));
                    double use = double.Parse(mem[0]) - double.Parse(mem[1]);
                    memlevel = Math.Round(Math.Round(use * 1.024 / 1000000, 1) / Math.Round(double.Parse(mem[0]) * 1.024 / 1000000) * 100).ToString();
                    usemem = string.Format($"{Math.Round(use * 1.024 / 1000000, 1)}GB/{Math.Round(double.Parse(mem[0]) * 1.024 / 1000000)}GB");
                }
                catch
                {
                    memlevel = "0";
                    usemem = "--";
                }
                try
                {
                    string diskinfos2 = await RunHdc(devicename, "shell hidumper -s StorageManager --storage");
                    string[] columns = StringHelper.DiskInfo(diskinfos2, "/data", true);
                    progressdisk = columns[4].TrimEnd('%');
                    diskinfo = string.Format($"{double.Parse(columns[2]) / 1024 / 1024:0.00}GB/{double.Parse(columns[1]) / 1024 / 1024:0.00}GB");
                }
                catch
                {
                    progressdisk = "0";
                    diskinfo = "--";
                }
            }
            string[] deviceLines = devcon.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (devcon.Contains(devicename))
            {
                string? thisdevice = deviceLines.FirstOrDefault(line => line.Contains(devicename));
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
                    else if (thisdevice.Contains("SPRD U2S Diag ("))
                    {
                        status = "U2S Diag";
                    }
                }
            }
            if (devicename.Contains("ttyUSB"))
            {
                status = "9008";
            }
            if (devicename == "Unknown device")
            {
                string? statusPattern = deviceLines.FirstOrDefault(line => line.EndsWith(":900e") || line.EndsWith(":901d") || line.EndsWith(":9091") || line.EndsWith("Sprd Gadget Serial "));
                if (statusPattern != null)
                {
                    if (statusPattern.Contains("90"))
                    {
                        status = statusPattern[(statusPattern.LastIndexOf(':') + 1)..];
                    }
                    else if (statusPattern.Contains("Sprd Gadget Serial "))
                    {
                        status = "U2S Diag";
                    }
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
            devices.Add("SystemSDK", systemsdk);
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
            string adb = await FeaturesHelper.AdbCmd("", "devices");
            string fastboot = await FeaturesHelper.FastbootCmd("", "devices");
            string devcon = Global.System == "Windows" ? await CallExternalProgram.Devcon("find usb*") : await CallExternalProgram.LsUSB();
            string hdc = await FeaturesHelper.HdcCmd("", "list targets");
            if (fastboot.Contains(devicename))
            {
                string isuserspace = await RunFastboot(devicename, "getvar is-userspace");
                status = isuserspace.Contains("yes") ? GetTranslation("Home_Fastbootd") : GetTranslation("Home_Fastboot");
                string blinfo = await RunFastboot(devicename, "getvar unlocked");
                blstatus = blinfo.Contains("yes") ? GetTranslation("Info_BLstatusUnlocked") : GetTranslation("Info_BLstatusLocked");
                string productinfos = await RunFastboot(devicename, "getvar product");
                string product = StringHelper.GetProductID(productinfos);
                if (product != null)
                {
                    codename = product;
                }
                string active = await RunFastboot(devicename, "getvar current-slot");
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
                    status = GetTranslation("Home_Android");
                }
                else if (thisdevice.Contains("unauthorized"))
                {
                    status = GetTranslation("Info_UnauthorizedDevice");
                }
                string active = await RunAdb(devicename, "shell getprop ro.boot.slot_suffix");
                vabstatus = active.Contains("_a")
                    ? GetTranslation("Info_ASlot")
                    : active.Contains("_b")
                        ? GetTranslation("Info_BSlot")
                        : status == GetTranslation("Info_UnauthorizedDevice") || status == GetTranslation("Home_Sideload")
                                            ? "--"
                                            : GetTranslation("Info_AOnly");
                codename = StringHelper.RemoveLineFeed(await RunAdb(devicename, "shell getprop ro.product.device"));
                blstatus = StringHelper.RemoveLineFeed(await RunAdb(devicename, "shell getprop ro.secureboot.lockstate"));
                if (blstatus == "--")
                {
                    blstatus = StringHelper.RemoveLineFeed(await RunAdb(devicename, "shell getprop ro.boot.vbmeta.device_state"));
                }
            }
            if (hdc.Contains(devicename))
            {
                string thisdevice = "";
                string[] Lines = hdc.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < Lines.Length; i++)
                {
                    if (Lines[i].Contains(devicename))
                    {
                        thisdevice = Lines[i];
                        break;
                    }
                }
                if (thisdevice.Contains("Unauthorized"))
                {
                    status = GetTranslation("Info_UnauthorizedDevice");
                }
                else
                {
                    status = GetTranslation("Home_OpenHOS");
                }
                codename = StringHelper.RemoveLineFeed(await RunHdc(devicename, "shell param get const.product.model"));
            }
            string[] deviceLines = devcon.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (devcon.Contains(devicename))
            {
                string? thisdevice = deviceLines.FirstOrDefault(line => line.Contains(devicename));
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
                string? statusPattern = deviceLines.FirstOrDefault(line => line.EndsWith(":900e") || line.EndsWith(":901d") || line.EndsWith(":9091"));
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
