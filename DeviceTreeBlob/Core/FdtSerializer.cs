using DeviceTreeNode.Models;
using DeviceTreeNode.Nodes;
using System.Text;

namespace DeviceTreeNode.Core
{
    /// <summary>
    /// 提供将FDT对象序列化为DTB二进制数据的功能
    /// </summary>
    public class FdtSerializer
    {
        private readonly Fdt _fdt;
        private List<MemoryReservation> _memoryReservations;
        private Dictionary<string, int> _stringOffsets;
        private MemoryStream _structsStream;
        private MemoryStream _stringsStream;

        // DTB格式常量
        private const int FDT_HEADER_SIZE = 40;
        private const uint FDT_MAGIC = 0xd00dfeed;
        private const uint FDT_VERSION = 17;
        private const uint FDT_LAST_COMP_VERSION = 16;

        public FdtSerializer(Fdt fdt)
        {
            _fdt = fdt ?? throw new ArgumentNullException(nameof(fdt));
            _memoryReservations = new List<MemoryReservation>();
            _stringOffsets = new Dictionary<string, int>();
            _structsStream = new MemoryStream();
            _stringsStream = new MemoryStream();
        }

        /// <summary>
        /// 序列化当前FDT对象为DTB二进制数据
        /// </summary>
        public byte[] Serialize()
        {
            // 收集内存保留块
            _memoryReservations = _fdt.MemoryReservations.ToList();

            // 序列化结构和字符串
            SerializeRoot();

            // 计算各部分大小和偏移
            int memReserveSize = CalculateMemoryReservationBlockSize();
            int structsSize = (int)_structsStream.Length;
            int stringsSize = (int)_stringsStream.Length;

            // 确保4字节对齐
            structsSize = AlignTo4(structsSize);
            stringsSize = AlignTo4(stringsSize);

            // 计算总大小和偏移
            int totalSize = FDT_HEADER_SIZE + memReserveSize + structsSize + stringsSize;
            int memReserveOffset = FDT_HEADER_SIZE;
            int structsOffset = memReserveOffset + memReserveSize;
            int stringsOffset = structsOffset + structsSize;

            // 创建输出流
            MemoryStream output = new MemoryStream(totalSize);
            BinaryWriter writer = new BinaryWriter(output);

            // 写入头部
            WriteHeader(writer, totalSize, memReserveOffset, structsOffset, stringsOffset,
                        structsSize, stringsSize);

            // 写入内存保留块
            WriteMemoryReservations(writer);

            // 写入结构块
            _structsStream.Position = 0;
            _structsStream.CopyTo(output);

            // 对齐到4字节边界
            AlignStream(output);

            // 写入字符串块
            _stringsStream.Position = 0;
            _stringsStream.CopyTo(output);

            // 对齐到4字节边界
            AlignStream(output);

            return output.ToArray();
        }

        /// <summary>
        /// 序列化根节点及其所有子节点
        /// </summary>
        private void SerializeRoot()
        {
            var root = _fdt.Root.Node;
            if (root == null)
                throw new InvalidOperationException("FDT does not contain a valid root node");

            BinaryWriter structWriter = new BinaryWriter(_structsStream);
            BinaryWriter stringWriter = new BinaryWriter(_stringsStream);

            // 确保空字符串在偏移0
            AddString("", stringWriter);

            // 序列化根节点
            WriteNode(root, structWriter, stringWriter);

            // 写入结束标记
            WriteU32(structWriter, FdtConstants.FDT_END);
        }

        /// <summary>
        /// 递归序列化节点
        /// </summary>
        private void WriteNode(FdtNode node, BinaryWriter structWriter, BinaryWriter stringWriter)
        {
            // 写入节点开始标记
            WriteU32(structWriter, FdtConstants.FDT_BEGIN_NODE);

            // 写入节点名称（包括NULL终止符和补齐）
            WriteNodeName(structWriter, node.Name);

            // 写入所有属性
            foreach (var prop in node.Properties())
            {
                WriteProperty(prop, structWriter, stringWriter);
            }

            // 写入所有子节点
            foreach (var child in node.Children())
            {
                WriteNode(child, structWriter, stringWriter);
            }

            // 写入节点结束标记
            WriteU32(structWriter, FdtConstants.FDT_END_NODE);
        }

        /// <summary>
        /// 序列化属性
        /// </summary>
        private void WriteProperty(NodeProperty prop, BinaryWriter structWriter, BinaryWriter stringWriter)
        {
            // 写入属性标记
            WriteU32(structWriter, FdtConstants.FDT_PROP);

            // 写入值长度
            WriteU32(structWriter, (uint)prop.Value.Length);

            // 写入名称偏移量
            int nameOffset = AddString(prop.Name, stringWriter);
            WriteU32(structWriter, (uint)nameOffset);

            // 写入属性值
            structWriter.Write(prop.Value);

            // 对齐到4字节边界
            AlignWriter(structWriter);
        }

        /// <summary>
        /// 写入节点名称，确保以NULL结尾并对齐到4字节边界
        /// </summary>
        private void WriteNodeName(BinaryWriter writer, string name)
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(name);
            writer.Write(nameBytes);
            writer.Write((byte)0); // NULL终止符

            // 对齐到4字节边界
            int padding = 4 - ((nameBytes.Length + 1) % 4);
            if (padding < 4)
            {
                for (int i = 0; i < padding; i++)
                {
                    writer.Write((byte)0);
                }
            }
        }

        /// <summary>
        /// 将字符串添加到字符串块并返回其偏移量
        /// </summary>
        private int AddString(string str, BinaryWriter stringWriter)
        {
            if (_stringOffsets.TryGetValue(str, out int offset))
            {
                return offset;
            }

            int newOffset = (int)_stringsStream.Length;
            _stringOffsets[str] = newOffset;

            byte[] strBytes = Encoding.UTF8.GetBytes(str);
            stringWriter.Write(strBytes);
            stringWriter.Write((byte)0); // NULL终止符

            return newOffset;
        }

        /// <summary>
        /// 写入DTB头部
        /// </summary>
        private void WriteHeader(BinaryWriter writer, int totalSize, int memReserveOffset,
                                int structsOffset, int stringsOffset, int structsSize, int stringsSize)
        {
            WriteU32(writer, FDT_MAGIC); // 魔数
            WriteU32(writer, (uint)totalSize); // 总大小
            WriteU32(writer, (uint)structsOffset); // 结构块偏移
            WriteU32(writer, (uint)stringsOffset); // 字符串块偏移
            WriteU32(writer, (uint)memReserveOffset); // 内存保留块偏移
            WriteU32(writer, FDT_VERSION); // 版本
            WriteU32(writer, FDT_LAST_COMP_VERSION); // 最后兼容版本
            WriteU32(writer, 0); // 启动CPU ID
            WriteU32(writer, (uint)stringsSize); // 字符串块大小
            WriteU32(writer, (uint)structsSize); // 结构块大小
        }

        /// <summary>
        /// 写入内存保留块
        /// </summary>
        private void WriteMemoryReservations(BinaryWriter writer)
        {
            foreach (var reservation in _memoryReservations)
            {
                WriteU64(writer, (ulong)reservation.Address.ToInt64());
                WriteU64(writer, reservation.Size);
            }

            // 写入终止项 (两个0)
            WriteU64(writer, 0);
            WriteU64(writer, 0);
        }

        /// <summary>
        /// 计算内存保留块的大小
        /// </summary>
        private int CalculateMemoryReservationBlockSize()
        {
            // 每个条目16字节（地址+大小），加上一个终止条目
            return (_memoryReservations.Count + 1) * 16;
        }

        /// <summary>
        /// 以大端序写入32位无符号整数
        /// </summary>
        private void WriteU32(BinaryWriter writer, uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            writer.Write(bytes);
        }

        /// <summary>
        /// 以大端序写入64位无符号整数
        /// </summary>
        private void WriteU64(BinaryWriter writer, ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            writer.Write(bytes);
        }

        /// <summary>
        /// 将流对齐到4字节边界
        /// </summary>
        private void AlignStream(Stream stream)
        {
            int padding = 4 - ((int)stream.Length % 4);
            if (padding < 4)
            {
                for (int i = 0; i < padding; i++)
                {
                    stream.WriteByte(0);
                }
            }
        }

        /// <summary>
        /// 将写入器对齐到4字节边界
        /// </summary>
        private void AlignWriter(BinaryWriter writer)
        {
            int padding = 4 - ((int)writer.BaseStream.Position % 4);
            if (padding < 4)
            {
                for (int i = 0; i < padding; i++)
                {
                    writer.Write((byte)0);
                }
            }
        }

        /// <summary>
        /// 将值对齐到4字节边界
        /// </summary>
        private int AlignTo4(int value)
        {
            return (value + 3) & ~3;
        }
    }
}
