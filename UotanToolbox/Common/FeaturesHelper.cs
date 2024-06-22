using System.Globalization;
using System.Resources;
using System.Threading.Tasks;

namespace UotanToolbox.Common
{
    internal class FeaturesHelper
    {
        private static readonly ResourceManager resMgr = new ResourceManager("UotanToolbox.Assets.Resources", typeof(App).Assembly);
        public static string GetTranslation(string key) => resMgr.GetString(key, CultureInfo.CurrentCulture) ?? "?????";

        public static async void PushMakefs(string device)
        {
            await CallExternalProgram.ADB($"-s {device} push bin/Push/mkfs.f2fs /tmp/");
            await CallExternalProgram.ADB($"-s {device} shell chmod +x /tmp/mkfs.f2fs");
            await CallExternalProgram.ADB($"-s {device} push bin/Push/mkntfs /tmp/");
            await CallExternalProgram.ADB($"-s {device} shell chmod +x /tmp/mkntfs");
        }

        public static async Task GetPartTable(string device)
        {
            await CallExternalProgram.ADB($"-s {device} push bin/Push/parted /tmp/");
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
            await CallExternalProgram.ADB($"-s {device} push bin/Push/parted /data/local/tmp/");
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
    }
}
