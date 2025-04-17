using DeviceTreeNode.Nodes;

namespace DeviceTreeNode.StandardNodes
{
    public class Aliases
    {
        private readonly FdtNode _node;
        private readonly Fdt _fdt;

        public Aliases(FdtNode node, Fdt fdt)
        {
            _node = node ?? throw new ArgumentNullException(nameof(node));
            _fdt = fdt ?? throw new ArgumentNullException(nameof(fdt));
        }

        /// <summary>
        /// 解析路径别名
        /// </summary>
        public FdtNode ResolveAlias(string alias)
        {
            var prop = _node.GetProperty(alias);
            if (prop == null)
                return null;

            string path = prop.AsString();
            if (string.IsNullOrEmpty(path))
                return null;

            // 确保路径以/开头
            if (!path.StartsWith("/"))
                path = "/" + path;

            return _fdt.FindNode(path);
        }

        /// <summary>
        /// 获取所有别名和对应的路径
        /// </summary>
        public Dictionary<string, string> GetAllAliases()
        {
            var result = new Dictionary<string, string>();

            foreach (var prop in _node.Properties())
            {
                string value = prop.AsString();
                if (!string.IsNullOrEmpty(value))
                    result[prop.Name] = value;
            }

            return result;
        }
    }
}
