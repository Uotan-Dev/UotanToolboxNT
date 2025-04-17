namespace DeviceTreeNode.Nodes
{
    /// <summary>
    /// 支持修改的FDT节点
    /// </summary>
    public class MutableFdtNode : FdtNode
    {
        private Dictionary<string, NodeProperty> _properties;
        private List<MutableFdtNode> _children;

        public MutableFdtNode(string name, Fdt owner, byte[] props, byte[] parentProps = null)
            : base(name, owner, props, parentProps)
        {
            // 预加载所有属性和子节点以便修改
            _properties = base.Properties().ToDictionary(p => p.Name, p => p);
            _children = base.Children().Cast<MutableFdtNode>().ToList();
        }

        /// <summary>
        /// 重写父类方法，返回可修改的属性集合
        /// </summary>
        public override IEnumerable<NodeProperty> Properties()
        {
            return _properties.Values;
        }

        /// <summary>
        /// 重写父类方法，返回可修改的子节点集合
        /// </summary>
        public override IEnumerable<FdtNode> Children()
        {
            return _children;
        }

        /// <summary>
        /// 替换属性值
        /// </summary>
        public void ReplaceProperty(string name, byte[] newValue)
        {
            if (_properties.TryGetValue(name, out var prop))
            {
                _properties[name] = new NodeProperty(name, newValue, Owner);
            }
            else
            {
                _properties.Add(name, new NodeProperty(name, newValue, Owner));
            }
        }

        /// <summary>
        /// 移除属性
        /// </summary>
        public void RemoveProperty(string name)
        {
            _properties.Remove(name);
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        public void AddChild(MutableFdtNode child)
        {
            _children.Add(child);
        }

        /// <summary>
        /// 移除子节点
        /// </summary>
        public void RemoveChild(MutableFdtNode child)
        {
            _children.Remove(child);
        }

        /// <summary>
        /// 创建节点的可修改克隆
        /// </summary>
        public static MutableFdtNode Clone(FdtNode node)
        {
            if (node is MutableFdtNode mutableNode)
                return mutableNode;

            var clone = new MutableFdtNode(node.Name, node.Owner, Array.Empty<byte>());

            // 复制属性
            foreach (var prop in node.Properties())
            {
                byte[] valueCopy = new byte[prop.Value.Length];
                Array.Copy(prop.Value, valueCopy, prop.Value.Length);
                clone.ReplaceProperty(prop.Name, valueCopy);
            }

            // 递归复制子节点
            foreach (var child in node.Children())
            {
                clone.AddChild(Clone(child));
            }

            return clone;
        }
    }
}
