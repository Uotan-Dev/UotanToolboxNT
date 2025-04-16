using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiskPartition.Extensions
{
    /// <summary>
    /// 提供分区类型相关的辅助方法
    /// </summary>
    public class PartitionTypeHelper
    {
        /// <summary>
        /// 根据GUID获取分区类型名称
        /// </summary>
        /// <param name="guid">分区类型GUID字符串</param>
        /// <returns>分区类型名称，如果未找到则返回"Unknown"</returns>
        public static string GetPartitionTypeName(string guid)
        {
            var typeInfo = PartitionType.GetByGuid(guid);
            return typeInfo?.Name ?? "Unknown";
        }

        /// <summary>
        /// 根据MBR类型代码获取分区类型名称
        /// </summary>
        /// <param name="mbrType">MBR分区类型代码</param>
        /// <returns>分区类型名称，如果未找到则返回"Unknown"</returns>
        public static string GetPartitionTypeName(ushort mbrType)
        {
            var typeInfo = PartitionType.GetByMbrType(mbrType);
            return typeInfo?.Name ?? "Unknown";
        }

        /// <summary>
        /// 根据GUID获取相应的MBR类型代码
        /// </summary>
        /// <param name="guid">分区类型GUID字符串</param>
        /// <returns>MBR类型代码，如果未找到则返回0xFFFF</returns>
        public static ushort GetMbrType(string guid)
        {
            var typeInfo = PartitionType.GetByGuid(guid);
            return typeInfo?.MbrType ?? 0xFFFF;
        }

        /// <summary>
        /// 根据MBR类型代码获取相应的GUID
        /// </summary>
        /// <param name="mbrType">MBR分区类型代码</param>
        /// <returns>分区类型GUID字符串，如果未找到则返回空字符串</returns>
        public static string GetGuidType(ushort mbrType)
        {
            var typeInfo = PartitionType.GetByMbrType(mbrType);
            return typeInfo?.GuidType ?? string.Empty;
        }

        /// <summary>
        /// 显示所有可用的分区类型
        /// </summary>
        /// <param name="searchString">可选的搜索字符串，用于过滤结果</param>
        /// <returns>格式化后的分区类型列表字符串</returns>
        public static string FormatPartitionTypes(string searchString = "")
        {
            var types = PartitionType.SearchByName(searchString);
            StringBuilder sb = new StringBuilder();
            int colWidth = 40;
            
            // 创建两列格式的输出
            IEnumerable<PartitionTypeInfo> typesList = types.ToList();
            for (int i = 0; i < typesList.Count(); i += 2)
            {
                sb.Append($"{typesList.ElementAt(i).MbrType:X4} {typesList.ElementAt(i).Name}");
                
                // 填充空格，使得第一列对齐
                int padding = colWidth - typesList.ElementAt(i).Name.Length - 5;
                sb.Append(new string(' ', padding > 0 ? padding : 1));
                
                // 如果还有第二列内容
                if (i + 1 < typesList.Count())
                {
                    sb.AppendLine($"{typesList.ElementAt(i+1).MbrType:X4} {typesList.ElementAt(i+1).Name}");
                }
                else
                {
                    sb.AppendLine();
                }
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// 检查给定的MBR类型代码是否是常见分区类型
        /// </summary>
        /// <param name="mbrType">MBR分区类型代码</param>
        /// <returns>如果是常见类型则返回true，否则返回false</returns>
        public static bool IsCommonPartitionType(ushort mbrType)
        {
            // 常见分区类型的MBR代码
            ushort[] commonTypes = new ushort[] {
                0x0700, // Microsoft basic data (NTFS)
                0x8200, // Linux swap
                0x8300, // Linux filesystem
                0xef00, // EFI system partition
                0xfd00, // Linux RAID
                0x8e00, // Linux LVM
                0xaf0a  // Apple APFS
            };
            
            return commonTypes.Contains(mbrType);
        }

        /// <summary>
        /// 获取分区类型的简短描述
        /// </summary>
        /// <param name="guid">分区类型的GUID字符串</param>
        /// <returns>分区类型的简短描述</returns>
        public static string GetPartitionTypeDescription(string guid)
        {
            var typeInfo = PartitionType.GetByGuid(guid);
            if (typeInfo == null)
                return "未知分区类型";
                
            switch (typeInfo.MbrType)
            {
                case 0x0700:
                    return "Windows NTFS分区";
                case 0x0c01:
                    return "Microsoft保留分区";
                case 0x2700:
                    return "Windows恢复环境";
                case 0x8200:
                    return "Linux交换分区";
                case 0x8300:
                    return "Linux文件系统分区";
                case 0x8302:
                    return "Linux /home分区";
                case 0x8e00:
                    return "Linux LVM分区";
                case 0xef00:
                    return "EFI系统分区";
                case 0xef02:
                    return "BIOS启动分区";
                case 0xfd00:
                    return "Linux RAID分区";
                case 0xaf0a:
                    return "Apple APFS分区";
                default:
                    return typeInfo.Name;
            }
        }
    }
}