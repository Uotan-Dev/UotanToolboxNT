using System.Text;

namespace UotanToolbox.Common.MagiskBoot
{
    internal class FormatChecker
    {
        public enum Format
        {
            UNKNOWN,
            CHROMEOS,
            AOSP,
            AOSP_VENDOR,
            GZIP,
            LZOP,
            XZ,
            LZMA,
            BZIP2,
            LZ4,
            LZ4_LEGACY,
            MTK,
            DTB,
            DHTB,
            BLOB,
            ZIMAGE
        }

        private static readonly byte[] CHROMEOS_MAGIC = Encoding.ASCII.GetBytes("CHROMEOS_MAGIC");
        private static readonly byte[] BOOT_MAGIC = Encoding.ASCII.GetBytes("BOOT_MAGIC");
        private static readonly byte[] VENDOR_BOOT_MAGIC = Encoding.ASCII.GetBytes("VENDOR_BOOT_MAGIC");
        private static readonly byte[] GZIP1_MAGIC = new byte[] { 0x1F, 0x8B };
        private static readonly byte[] GZIP2_MAGIC = new byte[] { 0x1F, 0x9D };
        private static readonly byte[] LZOP_MAGIC = Encoding.ASCII.GetBytes("LZOP_MAGIC");
        private static readonly byte[] XZ_MAGIC = new byte[] { 0xFD, 0x37, 0x7A, 0x58, 0x5A, 0x00 };
        private static readonly byte[] BZIP_MAGIC = new byte[] { 0x42, 0x5A, 0x68 };
        private static readonly byte[] LZ41_MAGIC = new byte[] { 0x04, 0x22, 0x4D, 0x18 };
        private static readonly byte[] LZ42_MAGIC = new byte[] { 0x04, 0x22, 0x4D, 0x22 };
        private static readonly byte[] LZ4_LEG_MAGIC = new byte[] { 0x02, 0x21, 0x4C, 0x18 };
        private static readonly byte[] MTK_MAGIC = Encoding.ASCII.GetBytes("MTK_MAGIC");
        private static readonly byte[] DTB_MAGIC = new byte[] { 0xD0, 0x0D, 0xFE, 0xED };
        private static readonly byte[] DHTB_MAGIC = Encoding.ASCII.GetBytes("DHTB_MAGIC");
        private static readonly byte[] TEGRABLOB_MAGIC = Encoding.ASCII.GetBytes("TEGRABLOB_MAGIC");
        private static readonly byte[] ZIMAGE_MAGIC = Encoding.ASCII.GetBytes("ZIMAGE_MAGIC");

        public static Format CheckFormat(byte[] buf)
        {
            int len = buf.Length;

            if (CheckedMatch(buf, CHROMEOS_MAGIC))
            {
                return Format.CHROMEOS;
            }
            else if (CheckedMatch(buf, BOOT_MAGIC))
            {
                return Format.AOSP;
            }
            else if (CheckedMatch(buf, VENDOR_BOOT_MAGIC))
            {
                return Format.AOSP_VENDOR;
            }
            else if (CheckedMatch(buf, GZIP1_MAGIC) || CheckedMatch(buf, GZIP2_MAGIC))
            {
                return Format.GZIP;
            }
            else if (CheckedMatch(buf, LZOP_MAGIC))
            {
                return Format.LZOP;
            }
            else if (CheckedMatch(buf, XZ_MAGIC))
            {
                return Format.XZ;
            }
            else if (len >= 13 && buf[0] == 0x5D && buf[1] == 0x00 && buf[2] == 0x00 &&
                     (buf[12] == 0xFF || buf[12] == 0x00))
            {
                return Format.LZMA;
            }
            else if (CheckedMatch(buf, BZIP_MAGIC))
            {
                return Format.BZIP2;
            }
            else if (CheckedMatch(buf, LZ41_MAGIC) || CheckedMatch(buf, LZ42_MAGIC))
            {
                return Format.LZ4;
            }
            else if (CheckedMatch(buf, LZ4_LEG_MAGIC))
            {
                return Format.LZ4_LEGACY;
            }
            else if (CheckedMatch(buf, MTK_MAGIC))
            {
                return Format.MTK;
            }
            else if (CheckedMatch(buf, DTB_MAGIC))
            {
                return Format.DTB;
            }
            else if (CheckedMatch(buf, DHTB_MAGIC))
            {
                return Format.DHTB;
            }
            else if (CheckedMatch(buf, TEGRABLOB_MAGIC))
            {
                return Format.BLOB;
            }
            else if (len >= 0x28 && buf[0x24] == ZIMAGE_MAGIC[0] && buf[0x25] == ZIMAGE_MAGIC[1] &&
                     buf[0x26] == ZIMAGE_MAGIC[2] && buf[0x27] == ZIMAGE_MAGIC[3])
            {
                return Format.ZIMAGE;
            }
            else
            {
                return Format.UNKNOWN;
            }
        }

        private static bool CheckedMatch(byte[] buf, byte[] magic)
        {
            if (buf.Length < magic.Length)
            {
                return false;
            }
            for (int i = 0; i < magic.Length; i++)
            {
                if (buf[i] != magic[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}

