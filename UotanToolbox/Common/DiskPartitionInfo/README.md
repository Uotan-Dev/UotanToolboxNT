该路径下代码来自于https://github.com/Uotan-Dev/DiskPartitionInfo-.NET8.0/
原项目地址：https://github.com/f1x3d/DiskPartitionInfo
主要添加了CRC32的修复算法和分区表的写入算法，用于实现EDL内的读取分区表后后续的分区表可视化更改与固件工具箱的分区表写入功能。
以下为该库的使用范例：
```
using DiskPartitionInfo.FluentApi;
using DiskPartitionInfo.Gpt;
using DiskPartitionInfo.Mbr;
using System.Xml.Linq;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("请提供要解析的.bin文件路径。");
            return;
        }
        string filePath = args[0];
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"文件 {filePath} 不存在。");
            return;
        }
        try
        {
            IMbrReader mbrReader = DiskPartitionInfo.DiskPartitionInfo.ReadMbr();
            MasterBootRecord mbr = mbrReader.FromPath(filePath);
            Console.WriteLine("MBR信息：");
            Console.WriteLine($"IsProtectiveMbr: {mbr.IsProtectiveMbr}");
            Console.WriteLine($"IsBootSignatureValid: {mbr.IsBootSignatureValid}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"读取MBR失败: {ex.Message}");
        }

        try
        {
            IGptReader gptReader = DiskPartitionInfo.DiskPartitionInfo.ReadGpt().Primary();
            GuidPartitionTable gpt = gptReader.FromPath(filePath);
            Console.WriteLine("GPT信息：");
            Console.WriteLine($"HasValidSignature: {gpt.HasValidSignature()}");
            Console.WriteLine($"PrimaryHeaderLocation: {gpt.PrimaryHeaderLocation}");
            Console.WriteLine($"SecondaryHeaderLocation: {gpt.SecondaryHeaderLocation}");
            Console.WriteLine($"FirstUsableLba: {gpt.FirstUsableLba}");
            Console.WriteLine($"LastUsableLba: {gpt.LastUsableLba}");
            Console.WriteLine($"DiskGuid: {gpt.DiskGuid}");
            Console.WriteLine("Partitions:");
            XElement root = new("data");
            foreach (var partition in gpt.Partitions)
            {
                if (partition.Guid.ToString() == "00000000-0000-0000-0000-000000000000")
                {
                    continue;
                }
                Console.WriteLine($"  - Type: {partition.Type}");
                Console.WriteLine($"    Guid: {partition.Guid}");
                Console.WriteLine($"    FirstLba: {partition.FirstLba}");
                Console.WriteLine($"    LastLba: {partition.LastLba}");
                Console.WriteLine($"    Name: {partition.Name}");
                Console.WriteLine($"    IsRequired: {partition.IsRequired}");
                Console.WriteLine($"    ShouldNotHaveDriveLetterAssigned: {partition.ShouldNotHaveDriveLetterAssigned}");
                Console.WriteLine($"    IsHidden: {partition.IsHidden}");
                Console.WriteLine($"    IsShadowCopy: {partition.IsShadowCopy}");
                Console.WriteLine($"    IsReadOnly: {partition.IsReadOnly}");
                XElement partitionElement = new XElement("Partition",
                                            new XAttribute("SECTOR_SIZE_IN_BYTES", "4096"),
                                            new XAttribute("file_sector_offset", "0"),
                                            new XAttribute("filename", ""),
                                            new XAttribute("label", partition.Name),
                                            new XAttribute("num_partition_sectors", (partition.LastLba - partition.FirstLba + 1).ToString()),
                                            new XAttribute("physical_partition_number", ""),
                                            new XAttribute("size_in_KB", ((partition.LastLba - partition.FirstLba + 1) * 4096 / 1024.0).ToString("F1")),
                                            new XAttribute("sparse", "false"),
                                            new XAttribute("start_byte_hex", $"{(partition.FirstLba) * 4096}"),
                                            new XAttribute("start_sector", $"{partition.FirstLba}")
                                            ); 
                root.Add(partitionElement);
            }
            XDocument doc = new(new XDeclaration("1.0", "utf-8", "yes"), root);
            doc.Save("Partitions.xml");
            Console.WriteLine("分区信息已保存到 Partitions.xml 文件中。");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"读取GPT失败: {ex.Message}");
        }
    }
}

```csharp
