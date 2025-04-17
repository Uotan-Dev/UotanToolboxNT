using DeviceTreeNode.Core;
using DeviceTreeNode.Models;
using DeviceTreeNode.Nodes;
using DeviceTreeNode.StandardNodes;
using System.Text;

namespace DeviceTreeNode;

public class Fdt
{
    private readonly byte[] _data;
    private readonly FdtHeader _header;

    /// <summary>
    /// 从二进制数据创建新的FDT实例
    /// </summary>
    public Fdt(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        _data = data;
        var dataReader = new FdtData(data);

        // 解析头部
        _header = FdtHeader.FromBytes(dataReader) ?? throw new FormatException("Invalid FDT header");

        // 验证魔数和大小
        if (!_header.ValidMagic)
            throw new FormatException("Invalid FDT magic value");

        if (data.Length < _header.TotalSize)
            throw new FormatException("Buffer too small for FDT");
    }

    /// <summary>
    /// 获取设备树头部信息
    /// </summary>
    public FdtHeader Header => _header;

    /// <summary>
    /// 查找指定路径的节点
    /// </summary>
    public FdtNode FindNode(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        // 确保路径以/开头
        if (!path.StartsWith("/"))
            path = "/" + path;

        string[] parts = path.Split(['/'], StringSplitOptions.RemoveEmptyEntries);

        // 从根节点开始查找
        FdtNode current = Root.Node;
        if (parts.Length == 0)
            return current;

        foreach (string part in parts)
        {
            bool found = false;
            foreach (var child in current.Children())
            {
                if (child.Name == part)
                {
                    current = child;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                // 尝试查找别名
                var aliases = Aliases;
                if (aliases != null)
                {
                    var aliasNode = aliases.ResolveAlias(path.TrimStart('/'));
                    if (aliasNode != null)
                        return aliasNode;
                }

                return null;
            }
        }

        return current;
    }

    /// <summary>
    /// 获取所有节点的扁平列表
    /// </summary>
    public IEnumerable<FdtNode> AllNodes()
    {
        var rootNode = Root.Node;
        return EnumerateNodes(rootNode);
    }

    private IEnumerable<FdtNode> EnumerateNodes(FdtNode node)
    {
        yield return node;

        foreach (var child in node.Children())
        {
            foreach (var descendant in EnumerateNodes(child))
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// 获取根节点
    /// </summary>
    public Root Root => new Root(ParseRoot());

    /// <summary>
    /// 获取chosen节点
    /// </summary>
    public Chosen Chosen
    {
        get
        {
            var node = FindNode("/chosen");
            return node != null ? new Chosen(node) : null;
        }
    }

    /// <summary>
    /// 获取memory节点
    /// </summary>
    public Memory Memory
    {
        get
        {
            var node = FindNode("/memory");
            return node != null ? new Memory(node) : null;
        }
    }

    /// <summary>
    /// 获取所有CPU节点
    /// </summary>
    public IEnumerable<Cpu> Cpus
    {
        get
        {
            var cpusNode = FindNode("/cpus");
            if (cpusNode == null)
                return Enumerable.Empty<Cpu>();

            return cpusNode.Children()
                .Where(node => node.Name.StartsWith("cpu@") ||
                              node.GetProperty("device_type")?.AsString() == "cpu")
                .Select(node => new Cpu(node));
        }
    }

    /// <summary>
    /// 获取别名节点
    /// </summary>
    public Aliases Aliases
    {
        get
        {
            var node = FindNode("/aliases");
            return node != null ? new Aliases(node, this) : null;
        }
    }

    /// <summary>
    /// 获取所有内存保留区域
    /// </summary>
    public IEnumerable<MemoryReservation> MemoryReservations
    {
        get
        {
            if (_header.MemoryReservationOffset >= _data.Length)
                yield break;

            var stream = new FdtData(_data.Skip((int)_header.MemoryReservationOffset).ToArray());

            while (true)
            {
                var address = stream.ReadUInt64();
                var size = stream.ReadUInt64();

                if (!address.HasValue || !size.HasValue || address.Value == 0 && size.Value == 0)
                    break;

                yield return new MemoryReservation(new IntPtr((long)address.Value), size.Value);
            }
        }
    }

    /// <summary>
    /// 从字符串块中获取指定偏移量的字符串
    /// </summary>
    internal string GetStringAtOffset(int offset)
    {
        int startOffset = (int)_header.StringsOffset + offset;
        if (startOffset >= _data.Length)
            return null;

        int endOffset = startOffset;
        while (endOffset < _data.Length && _data[endOffset] != 0)
            endOffset++;

        if (endOffset >= _data.Length)
            return null;

        int length = endOffset - startOffset;
        return Encoding.UTF8.GetString(_data, startOffset, length);
    }

    /// <summary>
    /// 获取所有字符串表中的字符串（调试用）
    /// </summary>
    public IEnumerable<string> Strings
    {
        get
        {
            if (_header.StringsOffset >= _data.Length || _header.StringsSize == 0)
                yield break;

            int offset = 0;
            byte[] stringsBlock = _data.Skip((int)_header.StringsOffset)
                                     .Take((int)_header.StringsSize)
                                     .ToArray();

            while (offset < stringsBlock.Length)
            {
                int startOffset = offset;

                while (offset < stringsBlock.Length && stringsBlock[offset] != 0)
                    offset++;

                if (offset > startOffset)
                {
                    string str = Encoding.UTF8.GetString(stringsBlock, startOffset, offset - startOffset);
                    yield return str;
                }

                offset++; // 跳过null终止符
            }
        }
    }

    /// <summary>
    /// 获取结构块的字节数据
    /// </summary>
    private byte[] GetStructsBlock()
    {
        return _data.Skip((int)_header.StructOffset)
                   .Take((int)_header.StructSize)
                   .ToArray();
    }

    /// <summary>
    /// 解析根节点
    /// </summary>
    private FdtNode ParseRoot()
    {
        var stream = new FdtData(GetStructsBlock());

        // 跳过NOP标记
        stream.SkipNops();

        // 检查是否为FDT_BEGIN_NODE
        if (stream.PeekUInt32()?.Value != FdtConstants.FDT_BEGIN_NODE)
            throw new FormatException("Invalid FDT structure: expected FDT_BEGIN_NODE");

        // 跳过标记
        stream.Skip(4);

        // 读取名称（应为空字符串）
        StringBuilder nameBuilder = new StringBuilder();
        byte[] nameBytes = stream.Remaining();
        int i = 0;

        while (i < nameBytes.Length && nameBytes[i] != 0)
        {
            nameBuilder.Append((char)nameBytes[i]);
            i++;
        }

        // 对齐到4字节边界
        int paddingLen = i + 1; // +1 是null终止符
        int alignedLen = (paddingLen + 3) & ~3;
        stream.Skip(alignedLen);

        // 解析属性和子节点
        var propsStart = stream.Remaining();
        byte[] nodeProps = ParseNodeProps(ref stream);

        return new FdtNode("", this, nodeProps);
    }

    /// <summary>
    /// 解析节点的属性部分
    /// </summary>
    private byte[] ParseNodeProps(ref FdtData stream)
    {
        var start = stream.Remaining();
        int propSectionLen = 0;

        // 跳过所有属性和子节点，直到找到FDT_END_NODE
        while (!stream.IsEmpty())
        {
            stream.SkipNops();
            uint? token = stream.PeekUInt32()?.Value;

            if (token == FdtConstants.FDT_END_NODE)
            {
                propSectionLen = start.Length - stream.Remaining().Length;
                stream.Skip(4); // 跳过FDT_END_NODE
                break;
            }
            else if (token == FdtConstants.FDT_PROP)
            {
                stream.Skip(4); // 跳过FDT_PROP

                uint? len = stream.ReadUInt32()?.Value;
                uint? nameOffset = stream.ReadUInt32()?.Value;

                if (!len.HasValue || !nameOffset.HasValue)
                    throw new FormatException("Invalid FDT property structure");

                int alignedLen = ((int)len.Value + 3) & ~3;
                stream.Skip(alignedLen);
            }
            else if (token == FdtConstants.FDT_BEGIN_NODE)
            {
                // 跳过整个子节点
                stream.Skip(4); // 跳过FDT_BEGIN_NODE

                // 跳过名称和对齐
                int i = 0;
                byte[] remaining = stream.Remaining();
                while (i < remaining.Length && remaining[i] != 0)
                    i++;

                int paddingLen = i + 1;
                int alignedLen = (paddingLen + 3) & ~3;
                stream.Skip(alignedLen);

                // 递归跳过子节点的内容
                ParseNodeProps(ref stream);
            }
            else if (token == FdtConstants.FDT_END)
            {
                break;
            }
            else
            {
                throw new FormatException($"Unknown FDT token: 0x{token:X8}");
            }
        }

        // 提取属性部分的数据
        if (propSectionLen > 0)
        {
            byte[] props = new byte[propSectionLen];
            Array.Copy(start, props, propSectionLen);
            return props;
        }

        return Array.Empty<byte>();
    }
}
