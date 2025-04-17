using DeviceTreeNode.Nodes;

namespace DeviceTreeNode.StandardNodes
{
    public class Root
    {
        private readonly FdtNode _node;

        public Root(FdtNode node)
        {
            _node = node ?? throw new ArgumentNullException(nameof(node));
        }

        // 获取设备型号
        public string Model =>
            _node.GetProperty("model")?.AsString() ?? "Unknown Model";

        // 获取兼容性列表
        public string[] Compatible =>
            _node.GetProperty("compatible")?.AsStringList() ?? Array.Empty<string>();

        // 获取当前运行的序列号
        public string Serial =>
            _node.GetProperty("serial-number")?.AsString();

        // 获取系统单元大小
        public Models.CellSizes CellSizes => _node.CellSizes;

        // 获取原始节点
        public FdtNode Node => _node;
    }
}
