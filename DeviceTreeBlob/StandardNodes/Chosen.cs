using DeviceTreeNode.Nodes;

namespace DeviceTreeNode.StandardNodes
{
    public class Chosen
    {
        private readonly FdtNode _node;
        private readonly Fdt _fdt;

        public Chosen(FdtNode node)
        {
            _node = node ?? throw new ArgumentNullException(nameof(node));
            _fdt = node.Owner;
        }

        // 获取引导参数
        public string Bootargs => _node.GetProperty("bootargs")?.AsString();

        // 获取初始化RAM磁盘地址
        public IntPtr? InitrdStart
        {
            get
            {
                uint? value = _node.GetProperty("linux,initrd-start")?.AsUInt32();
                return value.HasValue ? new IntPtr(value.Value) : null;
            }
        }

        // 获取初始化RAM磁盘结束地址
        public IntPtr? InitrdEnd
        {
            get
            {
                uint? value = _node.GetProperty("linux,initrd-end")?.AsUInt32();
                return value.HasValue ? new IntPtr(value.Value) : null;
            }
        }

        // 获取初始化RAM磁盘大小
        public long? InitrdSize =>
            InitrdStart.HasValue && InitrdEnd.HasValue ?
            InitrdEnd.Value.ToInt64() - InitrdStart.Value.ToInt64() : null;

        // 获取标准输入设备路径
        public string StdinPath => _node.GetProperty("stdin-path")?.AsString();

        // 获取标准输出设备路径
        public string StdoutPath => _node.GetProperty("stdout-path")?.AsString();

        // 获取标准输入设备节点
        public FdtNode StdinNode => !string.IsNullOrEmpty(StdinPath) ? _fdt.FindNode(StdinPath) : null;

        // 获取标准输出设备节点
        public FdtNode StdoutNode => !string.IsNullOrEmpty(StdoutPath) ? _fdt.FindNode(StdoutPath) : null;
    }
}
