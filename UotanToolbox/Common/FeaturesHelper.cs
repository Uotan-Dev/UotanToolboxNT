using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UotanToolbox.Common
{
    internal class FeaturesHelper
    {
        public static async void PushMakefs(string device)
        {
            await CallExternalProgram.ADB($"-s {device} push bin/linux/mkfs.f2fs /tmp/");
            await CallExternalProgram.ADB($"-s {device} shell chmod +x /tmp/mkfs.f2fs");
            await CallExternalProgram.ADB($"-s {device} push bin/linux/mkntfs /tmp/");
            await CallExternalProgram.ADB($"-s {device} shell chmod +x /tmp/mkntfs");
        }

        public static async void GetPartTable(string device)
        {
            await CallExternalProgram.ADB($"-s {device} push bin/linux/parted /tmp/");
            await CallExternalProgram.ADB($"-s {device} shell chmod +x /tmp/parted");
            Global.sdatable = await CallExternalProgram.ADB($"-s {device} shell /tmp/parted /dev/block/sda print");
            Global.sdbtable = await CallExternalProgram.ADB($"-s {device} shell /tmp/parted /dev/block/sdb print");
            Global.sdctable = await CallExternalProgram.ADB($"-s {device} shell /tmp/parted /dev/block/sdc print");
            Global.sddtable = await CallExternalProgram.ADB($"-s {device} shell /tmp/parted /dev/block/sdd print");
            Global.sdetable = await CallExternalProgram.ADB($"-s {device} shell /tmp/parted /dev/block/sde print");
            Global.sdftable = await CallExternalProgram.ADB($"-s {device} shell /tmp/parted /dev/block/sdf print");
            Global.emmcrom = await CallExternalProgram.ADB($"-s {device} shell /tmp/parted /dev/block/mmcblk0 print");
        }

        public static async void GetPartTableSystem(string device)
        {
            await CallExternalProgram.ADB($"-s {device} push bin/linux/parted /data/local/tmp/");
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
