using System.Collections.Generic;
using System.Linq;

namespace DiskPartition.Extensions
{
    /// <summary>
    /// 提供分区类型的GUID和名称映射
    /// </summary>
    public class PartitionType
    {
        private static readonly Dictionary<ushort, PartitionTypeInfo> PartitionTypes = new Dictionary<ushort, PartitionTypeInfo>();
        private static readonly Dictionary<string, PartitionTypeInfo> GuidToPartitionType = new Dictionary<string, PartitionTypeInfo>();

        static PartitionType()
        {
            InitializePartitionTypes();
        }

        /// <summary>
        /// 获取指定MBR类型代码对应的分区类型信息
        /// </summary>
        /// <param name="mbrType">MBR分区类型代码</param>
        /// <returns>分区类型信息，如果未找到则返回null</returns>
        public static PartitionTypeInfo? GetByMbrType(ushort mbrType)
        {
            if (PartitionTypes.TryGetValue(mbrType, out PartitionTypeInfo? typeInfo))
                return typeInfo;
            return null;
        }

        /// <summary>
        /// 获取指定GUID对应的分区类型信息
        /// </summary>
        /// <param name="guid">分区类型GUID字符串</param>
        /// <returns>分区类型信息，如果未找到则返回null</returns>
        public static PartitionTypeInfo? GetByGuid(string guid)
        {
            if (GuidToPartitionType.TryGetValue(guid.ToUpper(), out PartitionTypeInfo? typeInfo))
                return typeInfo;
            return null;
        }

        /// <summary>
        /// 获取所有分区类型信息
        /// </summary>
        /// <returns>分区类型信息列表</returns>
        public static IEnumerable<PartitionTypeInfo> GetAllTypes()
        {
            return PartitionTypes.Values;
        }

        /// <summary>
        /// 获取所有用于显示的分区类型信息
        /// </summary>
        /// <returns>用于显示的分区类型信息列表</returns>
        public static IEnumerable<PartitionTypeInfo> GetDisplayTypes()
        {
            return PartitionTypes.Values.Where(t => t.ShowInList);
        }

        /// <summary>
        /// 检查指定的MBR类型代码是否有效
        /// </summary>
        /// <param name="code">MBR分区类型代码</param>
        /// <returns>如果类型代码有效则返回true，否则返回false</returns>
        public static bool IsValidMbrType(ushort code)
        {
            return PartitionTypes.ContainsKey(code);
        }

        /// <summary>
        /// 根据分区类型名称搜索匹配的分区类型
        /// </summary>
        /// <param name="searchString">搜索字符串</param>
        /// <returns>匹配的分区类型信息列表</returns>
        public static IEnumerable<PartitionTypeInfo> SearchByName(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                return GetDisplayTypes();

            string lowerSearch = searchString.ToLower();
            return PartitionTypes.Values
                .Where(t => t.ShowInList && t.Name.ToLower().Contains(lowerSearch));
        }

        private static void InitializePartitionTypes()
        {
            // 未使用的空分区
            AddType(0x0000, "00000000-0000-0000-0000-000000000000", "Unused entry", false);

            // DOS/Windows 分区类型
            AddType(0x0100, "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7", "Microsoft basic data", false); // FAT-12
            AddType(0x0400, "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7", "Microsoft basic data", false); // FAT-16 < 32M
            AddType(0x0600, "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7", "Microsoft basic data", false); // FAT-16
            AddType(0x0700, "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7", "Microsoft basic data", true); // NTFS (or HPFS)
            AddType(0x0701, "558D43C5-A1AC-43C0-AAC8-D1472B2923D1", "Microsoft Storage Replica", true);
            AddType(0x0702, "90B6FF38-B98F-4358-A21F-48F35B4A8AD3", "ArcaOS Type 1", true);
            AddType(0x0b00, "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7", "Microsoft basic data", false); // FAT-32
            AddType(0x0c00, "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7", "Microsoft basic data", false); // FAT-32 LBA
            AddType(0x0c01, "E3C9E316-0B5C-4DB8-817D-F92DF00215AE", "Microsoft reserved", true);
            AddType(0x0e00, "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7", "Microsoft basic data", false); // FAT-16 LBA
            AddType(0x1100, "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7", "Microsoft basic data", false); // Hidden FAT-12
            AddType(0x1400, "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7", "Microsoft basic data", false); // Hidden FAT-16 < 32M
            AddType(0x1600, "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7", "Microsoft basic data", false); // Hidden FAT-16
            AddType(0x1700, "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7", "Microsoft basic data", false); // Hidden NTFS (or HPFS)
            AddType(0x1b00, "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7", "Microsoft basic data", false); // Hidden FAT-32
            AddType(0x1c00, "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7", "Microsoft basic data", false); // Hidden FAT-32 LBA
            AddType(0x1e00, "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7", "Microsoft basic data", false); // Hidden FAT-16 LBA
            AddType(0x2700, "DE94BBA4-06D1-4D40-A16A-BFD50179D6AC", "Windows RE", true);

            // ONIE 特定类型
            AddType(0x3000, "7412F7D5-A156-4B13-81DC-867174929325", "ONIE boot", true);
            AddType(0x3001, "D4E6E2CD-4469-46F3-B5CB-1BFF57AFC149", "ONIE config", true);

            // Plan 9 分区
            AddType(0x3900, "C91818F9-8025-47AF-89D2-F030D7000C2C", "Plan 9", true);

            // PowerPC 引导分区
            AddType(0x4100, "9E1A2D38-C612-4316-AA26-8B49521E5A8B", "PowerPC PReP boot", true);

            // Windows LDM 动态磁盘分区类型
            AddType(0x4200, "AF9B60A0-1431-4F62-BC68-3311714A69AD", "Windows LDM data", true);
            AddType(0x4201, "5808C8AA-7E8F-42E0-85D2-E1E90434CFB3", "Windows LDM metadata", true);
            AddType(0x4202, "E75CAF8F-F680-4CEE-AFA3-B001E56EFC2D", "Windows Storage Spaces", true);

            // IBM 文件系统
            AddType(0x7501, "37AFFC90-EF7D-4E96-91C3-2D7AE055B174", "IBM GPFS", true);

            // ChromeOS 特定分区类型
            AddType(0x7f00, "FE3A2A5D-4F32-41A7-B725-ACCC3285A309", "ChromeOS kernel", true);
            AddType(0x7f01, "3CB8E202-3B7E-47DD-8A3C-7FF2A13CFCEC", "ChromeOS root", true);
            AddType(0x7f02, "2E0A753D-9E48-43B0-8337-B15192CB1B5E", "ChromeOS reserved", true);
            AddType(0x7f03, "CAB6E88E-ABF3-4102-A07A-D4BB9BE3C1D3", "ChromeOS firmware", true);
            AddType(0x7f04, "09845860-705F-4BB5-B16C-8A8A099CAF52", "ChromeOS mini-OS", true);
            AddType(0x7f05, "3F0F8318-F146-4E6B-8222-C28C8F02E0D5", "ChromeOS hibernate", true);

            // Linux 特定分区类型
            AddType(0x8200, "0657FD6D-A4AB-43C4-84E5-0933C84B4F4F", "Linux swap", true);
            AddType(0x8300, "0FC63DAF-8483-4772-8E79-3D69D8477DE4", "Linux filesystem", true);
            AddType(0x8301, "8DA63339-0007-60C0-C436-083AC8230908", "Linux reserved", true);
            AddType(0x8302, "933AC7E1-2EB4-4F13-B844-0E14E2AEF915", "Linux /home", true);
            AddType(0x8303, "44479540-F297-41B2-9AF7-D131D5F0458A", "Linux x86 root (/)", true);
            AddType(0x8304, "4F68BCE3-E8CD-4DB1-96E7-FBCAF984B709", "Linux x86-64 root (/)", true);
            AddType(0x8305, "B921B045-1DF0-41C3-AF44-4C6F280D3FAE", "Linux ARM64 root (/)", true);
            AddType(0x8306, "3B8F8425-20E0-4F3B-907F-1A25A76F98E8", "Linux /srv", true);
            AddType(0x8307, "69DAD710-2CE4-4E3C-B16C-21A1D49ABED3", "Linux ARM32 root (/)", true);
            AddType(0x8308, "7FFEC5C9-2D00-49B7-8941-3EA10A5586B7", "Linux dm-crypt", true);
            AddType(0x8309, "CA7D7CCB-63ED-4C53-861C-1742536059CC", "Linux LUKS", true);
            AddType(0x830A, "993D8D3D-F80E-4225-855A-9DAF8ED7EA97", "Linux IA-64 root (/)", true);

            // 添加更多 Linux 类型...
            AddType(0x830B, "D13C5D3B-B5D1-422A-B29F-9454FDC89D76", "Linux x86 root verity", true);
            AddType(0x830C, "2C7357ED-EBD2-46D9-AEC1-23D437EC2BF5", "Linux x86-64 root verity", true);
            AddType(0x830D, "7386CDF2-203C-47A9-A498-F2ECCE45A2D6", "Linux ARM32 root verity", true);
            AddType(0x830E, "DF3300CE-D69F-4C92-978C-9BFB0F38D820", "Linux ARM64 root verity", true);
            AddType(0x830F, "86ED10D5-B607-45BB-8957-D350F23D0571", "Linux IA-64 root verity", true);
            AddType(0x8310, "4D21B016-B534-45C2-A9FB-5C16E091FD2D", "Linux /var", true);
            AddType(0x8311, "7EC6F557-3BC5-4ACA-B293-16EF5DF639D1", "Linux /var/tmp", true);
            AddType(0x8312, "773F91EF-66D4-49B5-BD83-D683BF40AD16", "Linux user's home", true);

            // Intel 快速启动技术
            AddType(0x8400, "D3BFE2DE-3DAF-11DF-BA40-E3A556D89593", "Intel Rapid Start", true);
            AddType(0x8401, "7C5222BD-8F5D-4087-9C00-BF9843C7B58C", "SPDK block device", true);

            // Container Linux 类型
            AddType(0x8500, "5DFBF5F4-2848-4BAC-AA5E-0D9A20B745A6", "Container Linux /usr", true);
            AddType(0x8501, "3884DD41-8582-4404-B9A8-E9B84F2DF50E", "Container Linux resizable rootfs", true);
            AddType(0x8502, "C95DC21A-DF0E-4340-8D7B-26CBFA9A03E0", "Container Linux /OEM customizations", true);
            AddType(0x8503, "BE9067B9-EA49-4F15-B4F6-F36F8C9E1818", "Container Linux root on RAID", true);

            // 其他 Linux 类型
            AddType(0x8e00, "E6D6D379-F507-44C2-A23C-238F2A3DF928", "Linux LVM", true);

            // Android 分区类型
            AddType(0xa000, "2568845D-2332-4675-BC39-8FA5A4748D15", "Android bootloader", true);
            AddType(0xa001, "114EAFFE-1552-4022-B26E-9B053604CF84", "Android bootloader 2", true);
            AddType(0xa002, "49A4D17F-93A3-45C1-A0DE-F50B2EBE2599", "Android boot 1", true);
            AddType(0xa003, "4177C722-9E92-4AAB-8644-43502BFD5506", "Android recovery 1", true);
            AddType(0xa004, "EF32A33B-A409-486C-9141-9FFB711F6266", "Android misc", true);
            AddType(0xa005, "20AC26BE-20B7-11E3-84C5-6CFDB94711E9", "Android metadata", true);
            AddType(0xa006, "38F428E6-D326-425D-9140-6E0EA133647C", "Android system 1", true);
            AddType(0xa007, "A893EF21-E428-470A-9E55-0668FD91A2D9", "Android cache", true);
            AddType(0xa008, "DC76DDA9-5AC1-491C-AF42-A82591580C0D", "Android data", true);
            AddType(0xa009, "EBC597D0-2053-4B15-8B64-E0AAC75F4DB1", "Android persistent", true);
            AddType(0xa00a, "8F68CC74-C5E5-48DA-BE91-A0C8C15E9C80", "Android factory", true);
            AddType(0xa00b, "767941D0-2085-11E3-AD3B-6CFDB94711E9", "Android fastboot/tertiary", true);
            AddType(0xa00c, "AC6D7924-EB71-4DF8-B48D-E267B27148FF", "Android OEM", true);
            AddType(0xa00d, "C5A0AEEC-13EA-11E5-A1B1-001E67CA0C3C", "Android vendor", true);
            AddType(0xa00e, "BD59408B-4514-490D-BF12-9878D963F378", "Android config", true);

            // FreeBSD 分区类型
            AddType(0xa500, "516E7CB4-6ECF-11D6-8FF8-00022D09712B", "FreeBSD disklabel", true);
            AddType(0xa501, "83BD6B9D-7F41-11DC-BE0B-001560B84F0F", "FreeBSD boot", true);
            AddType(0xa502, "516E7CB5-6ECF-11D6-8FF8-00022D09712B", "FreeBSD swap", true);
            AddType(0xa503, "516E7CB6-6ECF-11D6-8FF8-00022D09712B", "FreeBSD UFS", true);
            AddType(0xa504, "516E7CBA-6ECF-11D6-8FF8-00022D09712B", "FreeBSD ZFS", true);
            AddType(0xa505, "516E7CB8-6ECF-11D6-8FF8-00022D09712B", "FreeBSD Vinum/RAID", true);

            // OpenBSD 分区类型
            AddType(0xa600, "824CC7A0-36A8-11E3-890A-952519AD3F61", "OpenBSD disklabel", true);

            // Mac OS 分区类型
            AddType(0xa800, "55465300-0000-11AA-AA11-00306543ECAC", "Apple UFS", true);
            AddType(0xab00, "426F6F74-0000-11AA-AA11-00306543ECAC", "Recovery HD", true);
            AddType(0xaf00, "48465300-0000-11AA-AA11-00306543ECAC", "Apple HFS/HFS+", true);
            AddType(0xaf01, "52414944-0000-11AA-AA11-00306543ECAC", "Apple RAID", true);
            AddType(0xaf02, "52414944-5F4F-11AA-AA11-00306543ECAC", "Apple RAID offline", true);
            AddType(0xaf03, "4C616265-6C00-11AA-AA11-00306543ECAC", "Apple label", true);
            AddType(0xaf04, "5265636F-7665-11AA-AA11-00306543ECAC", "AppleTV recovery", true);
            AddType(0xaf05, "53746F72-6167-11AA-AA11-00306543ECAC", "Apple Core Storage", true);
            AddType(0xaf0a, "7C3457EF-0000-11AA-AA11-00306543ECAC", "Apple APFS", true);

            // U-Boot 引导加载程序
            AddType(0xb000, "3DE21764-95BD-54BD-A5C3-4ABE786F38A8", "U-Boot boot loader", true);

            // QNX
            AddType(0xb300, "CEF5A9AD-73BC-4601-89F3-CDEEEEE321A1", "QNX6 Power-Safe", true);

            // Barebox 引导加载程序
            AddType(0xbb00, "4778ED65-BF42-45FA-9C5B-287A1DC4AAB1", "Barebox boot loader", true);

            // Acronis 安全区
            AddType(0xbc00, "0311FC50-01CA-4725-AD77-9ADBB20ACE98", "Acronis Secure Zone", true);

            // Solaris 分区类型
            AddType(0xbe00, "6A82CB45-1DD2-11B2-99A6-080020736631", "Solaris boot", true);
            AddType(0xbf00, "6A85CF4D-1DD2-11B2-99A6-080020736631", "Solaris root", true);
            AddType(0xbf01, "6A898CC3-1DD2-11B2-99A6-080020736631", "Solaris /usr & Mac ZFS", true);
            AddType(0xbf02, "6A87C46F-1DD2-11B2-99A6-080020736631", "Solaris swap", true);

            // EFI 系统和相关分区
            AddType(0xef00, "C12A7328-F81F-11D2-BA4B-00A0C93EC93B", "EFI system partition", true);
            AddType(0xef01, "024DEE41-33E7-11D3-9D69-0008C781F39F", "MBR partition scheme", true);
            AddType(0xef02, "21686148-6449-6E6F-744E-656564454649", "BIOS boot partition", true);

            // Veracrypt 加密分区
            AddType(0xe900, "8C8F8EFF-AC95-4770-814A-21994F2DBC8F", "Veracrypt data", true);

            // 其他常用类型
            AddType(0xfd00, "A19D880F-05FC-4D3B-A006-743F0F84911E", "Linux RAID", true);
        }

        private static void AddType(ushort mbrType, string guidType, string name, bool showInList)
        {
            PartitionTypeInfo typeInfo = new PartitionTypeInfo(mbrType, guidType, name, showInList);
            PartitionTypes[mbrType] = typeInfo;
            GuidToPartitionType[guidType.ToUpper()] = typeInfo;
        }
    }

    /// <summary>
    /// 表示一个分区类型信息
    /// </summary>
    public class PartitionTypeInfo
    {
        /// <summary>
        /// MBR分区类型代码
        /// </summary>
        public ushort MbrType { get; }

        /// <summary>
        /// GPT分区类型GUID
        /// </summary>
        public string GuidType { get; }

        /// <summary>
        /// 分区类型名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 是否在列表中显示
        /// </summary>
        public bool ShowInList { get; }

        /// <summary>
        /// 创建一个新的分区类型信息实例
        /// </summary>
        /// <param name="mbrType">MBR分区类型代码</param>
        /// <param name="guidType">GPT分区类型GUID</param>
        /// <param name="name">分区类型名称</param>
        /// <param name="showInList">是否在列表中显示</param>
        public PartitionTypeInfo(ushort mbrType, string guidType, string name, bool showInList)
        {
            MbrType = mbrType;
            GuidType = guidType;
            Name = name;
            ShowInList = showInList;
        }

        /// <summary>
        /// 返回分区类型的字符串表示形式
        /// </summary>
        /// <returns>分区类型的字符串表示形式</returns>
        public override string ToString()
        {
            return $"0x{MbrType:X4} ({Name})";
        }
    }
}