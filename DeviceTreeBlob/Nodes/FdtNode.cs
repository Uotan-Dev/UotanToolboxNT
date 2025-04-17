using DeviceTreeNode.Core;
using DeviceTreeNode.Models;
using System.Text;

namespace DeviceTreeNode.Nodes
{
    /// <summary>
    /// 设备树节点
    /// </summary>
    public class FdtNode
    {
        private readonly Fdt _owner;
        private readonly byte[] _props;
        private readonly byte[] _parentProps;
        private FdtNode _parent;

        public string Name { get; }

        // 添加Owner属性以便外部访问
        public Fdt Owner => _owner;

        public FdtNode(string name, Fdt owner, byte[] props, byte[] parentProps = null, FdtNode parent = null)
        {
            Name = name;
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _props = props ?? Array.Empty<byte>();
            _parentProps = parentProps;
            _parent = parent;
        }

        /// <summary>
        /// 获取所有节点属性
        /// </summary>
        public virtual IEnumerable<NodeProperty> Properties()
        {
            var stream = new FdtData(_props);

            while (!stream.IsEmpty())
            {
                stream.SkipNops();

                var token = stream.PeekUInt32();
                if (token == null || token.Value.Value != FdtConstants.FDT_PROP)
                    break;

                var prop = NodeProperty.Parse(stream, _owner);
                if (prop != null)
                    yield return prop;
            }
        }

        /// <summary>
        /// 获取指定名称的属性
        /// </summary>
        public NodeProperty GetProperty(string name)
        {
            return Properties().FirstOrDefault(p => p.Name == name);
        }

        /// <summary>
        /// 获取所有子节点
        /// </summary>
        public virtual IEnumerable<FdtNode> Children()
        {
            var stream = new FdtData(_props);

            // 跳过所有属性
            while (!stream.IsEmpty())
            {
                var token = stream.PeekUInt32();
                if (token == null)
                    yield break; // 返回空集合

                if (token.Value.Value == FdtConstants.FDT_PROP)
                {
                    // 跳过属性
                    stream.Skip(4); // FDT_PROP标记
                    var length = stream.ReadUInt32();
                    if (length == null)
                        yield break;

                    stream.Skip(4); // 名称偏移

                    // 跳过属性值并对齐到4字节
                    int valueLen = (int)length.Value.Value;
                    int alignedLen = (valueLen + 3) & ~3;
                    stream.Skip(alignedLen);
                }
                else if (token.Value.Value == FdtConstants.FDT_BEGIN_NODE)
                {
                    // 解析子节点
                    yield return ParseChildNode(stream, this);
                }
                else
                {
                    // 未知的标记，结束迭代
                    yield break;
                }
            }

            // 确保即使流为空或遇到未知标记，也能返回空集合
            yield break;
        }

        /// <summary>
        /// 解析子节点
        /// </summary>
        private FdtNode ParseChildNode(FdtData stream, FdtNode parent)
        {
            // 跳过BEGIN_NODE标记
            stream.Skip(4);

            // 读取节点名称
            StringBuilder nameBuilder = new StringBuilder();
            byte[] remaining = stream.Remaining();
            int i = 0;

            while (i < remaining.Length && remaining[i] != 0)
            {
                nameBuilder.Append((char)remaining[i]);
                i++;
            }

            string name = nameBuilder.ToString();

            // 对齐到4字节边界
            int paddingLen = i + 1; // 包括null终止符
            int alignedLen = (paddingLen + 3) & ~3;
            stream.Skip(alignedLen);

            // 标记属性起始位置
            byte[] nodeProps = stream.Remaining();

            // 跳过所有属性和子节点
            int depth = 1;
            while (depth > 0 && !stream.IsEmpty())
            {
                stream.SkipNops();

                var token = stream.ReadUInt32();
                if (token == null)
                    break;

                if (token.Value.Value == FdtConstants.FDT_BEGIN_NODE)
                {
                    depth++;

                    // 跳过节点名称
                    remaining = stream.Remaining();
                    i = 0;
                    while (i < remaining.Length && remaining[i] != 0)
                        i++;

                    paddingLen = i + 1;
                    alignedLen = (paddingLen + 3) & ~3;
                    stream.Skip(alignedLen);
                }
                else if (token.Value.Value == FdtConstants.FDT_END_NODE)
                {
                    depth--;
                }
                else if (token.Value.Value == FdtConstants.FDT_PROP)
                {
                    // 跳过属性
                    var length = stream.ReadUInt32();
                    if (length == null)
                        break;

                    stream.Skip(4); // 名称偏移

                    // 跳过属性值
                    int valueLen = (int)length.Value.Value;
                    int propAlignedLen = (valueLen + 3) & ~3;
                    stream.Skip(propAlignedLen);
                }
            }

            // 计算属性块的长度
            int propsLen = nodeProps.Length - stream.Remaining().Length;
            byte[] props = new byte[propsLen];
            Array.Copy(nodeProps, props, propsLen);

            return new FdtNode(name, _owner, props, _props, parent);
        }

        /// <summary>
        /// 获取当前节点的单元大小
        /// </summary>
        public CellSizes CellSizes
        {
            get
            {
                var addressSize = GetProperty("#address-cells")?.AsUInt32() ?? 2;
                var sizeSize = GetProperty("#size-cells")?.AsUInt32() ?? 1;

                return new CellSizes((int)addressSize, (int)sizeSize);
            }
        }

        /// <summary>
        /// 获取父节点的单元大小 (添加此属性解决错误1)
        /// </summary>
        public CellSizes ParentCellSizes
        {
            get
            {
                if (_parent != null)
                    return _parent.CellSizes;

                // 尝试从父属性区域解析
                if (_parentProps != null && _parentProps.Length > 0)
                {
                    var tempNode = new FdtNode("temp", _owner, _parentProps);
                    return tempNode.CellSizes;
                }

                // 使用默认值
                return CellSizes.Default;
            }
        }

        /// <summary>
        /// 获取内存区域
        /// </summary>
        public IEnumerable<MemoryRegion> Reg()
        {
            var sizes = ParentCellSizes;
            if (sizes.AddressCells > 2 || sizes.SizeCells > 2)
                yield break;

            var regProp = GetProperty("reg");
            if (regProp == null)
                yield break;

            var stream = new FdtData(regProp.Value);

            while (!stream.IsEmpty())
            {
                ulong? address = null;
                switch (sizes.AddressCells)
                {
                    case 1:
                        var addr32 = stream.ReadUInt32();
                        address = addr32?.Value;
                        break;
                    case 2:
                        var addr64 = stream.ReadUInt64();
                        address = addr64?.Value;
                        break;
                    default:
                        yield break;
                }

                if (!address.HasValue)
                    yield break;

                ulong? size = null;
                switch (sizes.SizeCells)
                {
                    case 0:
                        size = null;
                        break;
                    case 1:
                        var size32 = stream.ReadUInt32();
                        size = size32?.Value;
                        break;
                    case 2:
                        var size64 = stream.ReadUInt64();
                        size = size64?.Value;
                        break;
                    default:
                        yield break;
                }

                yield return new MemoryRegion(new IntPtr((long)address.Value), size);
            }
        }

        /// <summary>
        /// 获取兼容性字符串列表
        /// </summary>
        public string[] Compatible =>
            GetProperty("compatible")?.AsStringList() ?? Array.Empty<string>();

        /// <summary>
        /// 检查节点是否与给定字符串兼容
        /// </summary>
        public bool IsCompatible(string compat)
        {
            return Compatible.Contains(compat);
        }
    }
}
