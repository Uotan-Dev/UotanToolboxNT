using System;
using System.Globalization;
using System.Resources;
using System.Threading.Tasks;

namespace UotanToolbox.Common
{
    internal class FeaturesHelper
    {
        private static readonly ResourceManager resMgr = new ResourceManager("UotanToolbox.Assets.Resources", typeof(App).Assembly);
        public static string GetTranslation(string key)
        {
            CultureInfo CurCulture;
            if (Settings.Default.Language != null && Settings.Default.Language != "") CurCulture = new CultureInfo(Settings.Default.Language, false);
            else CurCulture = CultureInfo.CurrentCulture;
            string res = resMgr.GetString(key, CurCulture) ?? "?????";
            return res;
        }

            public static async void PushMakefs(string device)
        {
            await CallExternalProgram.ADB($"-s {device} push {Global.runpath}/Push/mkfs.f2fs /tmp/");
            await CallExternalProgram.ADB($"-s {device} shell chmod +x /tmp/mkfs.f2fs");
            await CallExternalProgram.ADB($"-s {device} push {Global.runpath}/Push/mkntfs /tmp/");
            await CallExternalProgram.ADB($"-s {device} shell chmod +x /tmp/mkntfs");
        }

        public static async Task GetPartTable(string device)
        {
            await CallExternalProgram.ADB($"-s {device} push {Global.runpath}/Push/parted /tmp/");
            await CallExternalProgram.ADB($"-s {device} shell chmod +x /tmp/parted");
            Global.sdatable = await CallExternalProgram.ADB($"-s {device} shell /tmp/parted /dev/block/sda print");
            Global.sdbtable = await CallExternalProgram.ADB($"-s {device} shell /tmp/parted /dev/block/sdb print");
            Global.sdctable = await CallExternalProgram.ADB($"-s {device} shell /tmp/parted /dev/block/sdc print");
            Global.sddtable = await CallExternalProgram.ADB($"-s {device} shell /tmp/parted /dev/block/sdd print");
            Global.sdetable = await CallExternalProgram.ADB($"-s {device} shell /tmp/parted /dev/block/sde print");
            Global.sdftable = await CallExternalProgram.ADB($"-s {device} shell /tmp/parted /dev/block/sdf print");
            Global.emmcrom = await CallExternalProgram.ADB($"-s {device} shell /tmp/parted /dev/block/mmcblk0 print");
        }

        public static async Task GetPartTableSystem(string device)
        {
            await CallExternalProgram.ADB($"-s {device} push {Global.runpath}/Push/parted /data/local/tmp/");
            await CallExternalProgram.ADB($"-s {device} shell su -c \"chmod +x /data/local/tmp/parted\"");
            Global.sdatable = await CallExternalProgram.ADB($"-s {device} shell su -c \"/data/local/tmp/parted /dev/block/sda print\"");
            Global.sdbtable = await CallExternalProgram.ADB($"-s {device} shell su -c \"/data/local/tmp/parted /dev/block/sdb print\"");
            Global.sdctable = await CallExternalProgram.ADB($"-s {device} shell su -c \"/data/local/tmp/parted /dev/block/sdc print\"");
            Global.sddtable = await CallExternalProgram.ADB($"-s {device} shell su -c \"/data/local/tmp/parted /dev/block/sdd print\"");
            Global.sdetable = await CallExternalProgram.ADB($"-s {device} shell su -c \"/data/local/tmp/parted /dev/block/sde print\"");
            Global.sdftable = await CallExternalProgram.ADB($"-s {device} shell su -c \"/data/local/tmp/parted /dev/block/sdf print\"");
            Global.emmcrom = await CallExternalProgram.ADB($"-s {device} shell su -c \"/data/local/tmp/parted /dev/block/mmcblk0 print\"");
        }

        public static string FindDisk(string Partname)
        {
            string sdxdisk = "";
            string[] diskTables = { Global.sdatable, Global.sdetable, Global.sdbtable, Global.sdctable, Global.sddtable, Global.sdftable, Global.emmcrom };
            string[] diskNames = { "sda", "sde", "sdb", "sdc", "sdd", "sdf", "mmcblk0p" };
            for (int i = 0; i < diskTables.Length; i++)
            {
                if (diskTables[i].Contains(Partname))
                {
                    if (StringHelper.Partno(diskTables[i], Partname) != null)
                    {
                        sdxdisk = diskNames[i];
                        break;
                    }
                }
            }
            return sdxdisk;
        }

        public static string FindPart(string Partname)
        {
            string sdxdisk = "";
            string[] diskTables = { Global.sdatable, Global.sdetable, Global.sdbtable, Global.sdctable, Global.sddtable, Global.sdftable, Global.emmcrom };
            foreach (string diskTable in diskTables)
            {
                if (diskTable.IndexOf(Partname) != -1)
                {
                    sdxdisk = diskTable;
                    break;
                }
            }
            return sdxdisk;
        }

        public static async Task<string> ActiveApp(string output)
        {
            string adb_output;
            if (output.Contains("moe.shizuku.privileged.api"))
            {
                adb_output = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell sh /storage/emulated/0/Android/data/moe.shizuku.privileged.api/start.sh");
                if (adb_output.Contains("info: shizuku_starter exit with 0"))
                {
                    return "Shizuku" + GetTranslation("Appmgr_ActiveSucc");
                }
                else
                {
                    return "Shizuku" + GetTranslation("Appmgr_ActiveFail");
                }
            }
            else if (output.Contains("com.oasisfeng.greenify"))
            {
                int a = 0;
                adb_output = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm grant com.oasisfeng.greenify android.permission.WRITE_SECURE_SETTINGS");
                if (!String.IsNullOrEmpty(adb_output))
                {
                    a = a + 1;
                }
                adb_output = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm grant com.oasisfeng.greenify android.permission.DUMP");
                if (!String.IsNullOrEmpty(adb_output))
                {
                    a = a + 1;
                }
                adb_output = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm grant com.oasisfeng.greenify android.permission.READ_LOGS");
                if (!String.IsNullOrEmpty(adb_output))
                {
                    a = a + 1;
                }
                adb_output = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell am force-stop com.oasisfeng.greenify");
                if (!String.IsNullOrEmpty(adb_output))
                {
                    a = a + 1;
                }
                if (a == 0)
                {
                    return GetTranslation("Appmgr_Greenify") + GetTranslation("Appmgr_ActiveSucc");
                }
                else
                {
                    return GetTranslation("Appmgr_Greenify") + GetTranslation("Appmgr_ActiveFail");
                }
            }
            else if (output.Contains("com.rosan.dhizuku"))
            {
                adb_output = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell dpm set-device-owner com.rosan.dhizuku/.server.DhizukuDAReceiver");
                if (adb_output.Contains("Success: Device owner set to package"))
                {
                    return "Dhizuku" + GetTranslation("Appmgr_ActiveSucc");
                }
                else
                {
                    return "Dhizuku" + GetTranslation("Appmgr_ActiveFail");
                }
            }
            else if (output.Contains("com.oasisfeng.island"))
            {
                adb_output = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell pm grant com.oasisfeng.island android.permission.INTERACT_ACROSS_USERS");
                if (String.IsNullOrEmpty(adb_output))
                {
                    return "Island" + GetTranslation("Appmgr_ActiveSucc");
                }
                else
                {
                    return "Island" + GetTranslation("Appmgr_ActiveFail");
                }
            }
            else if (output.Contains("me.piebridge.brevent"))
            {
                adb_output = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell sh /data/data/me.piebridge.brevent/brevent.sh");
                if (adb_output.Contains("..success"))
                {
                    return "Brevent" + GetTranslation("Appmgr_ActiveSucc");
                }
                else
                {
                    return "Brevent" + GetTranslation("Appmgr_ActiveFail");
                }
            }
            else if (output.Contains("com.catchingnow.icebox"))
            {
                adb_output = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell sh /sdcard/Android/data/com.catchingnow.icebox/files/start.sh");
                if (adb_output.Contains("success"))
                {
                    return "IceBox" + GetTranslation("Appmgr_ActiveSucc");
                }
                else
                {
                    return "IceBox" + GetTranslation("Appmgr_ActiveFail");
                }
            }
            else if (output.Contains("web1n.stopapp"))
            {
                adb_output = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell sh /storage/emulated/0/Android/data/web1n.stopapp/files/starter.sh");
                if (adb_output.Contains("success to register app changed listener."))
                {
                    return GetTranslation("Appmgr_Stopapp") + GetTranslation("Appmgr_ActiveSucc");
                }
                else
                {
                    return GetTranslation("Appmgr_Stopapp") + GetTranslation("Appmgr_ActiveFail");
                }
            }
            else if (output.Contains("com.web1n.permissiondog"))
            {
                adb_output = await CallExternalProgram.ADB($"-s {Global.thisdevice} shell sh /storage/emulated/0/Android/data/com.web1n.permissiondog/files/starter.sh");
                if (adb_output.Contains("success to register app changed listener."))
                {
                    return GetTranslation("Appmgr_PermissionDog") + GetTranslation("Appmgr_ActiveSucc");
                }
                else
                {
                    return GetTranslation("Appmgr_PermissionDog") + GetTranslation("Appmgr_ActiveFail");
                }
            }
            return "当前主界面应用不被支持！";

        }
    }
}
