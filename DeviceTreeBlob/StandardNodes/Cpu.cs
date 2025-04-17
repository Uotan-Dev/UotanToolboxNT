using DeviceTreeNode.Nodes;

namespace DeviceTreeNode.StandardNodes
{
    public class Cpu
    {
        private readonly FdtNode _node;

        public Cpu(FdtNode node)
        {
            _node = node ?? throw new ArgumentNullException(nameof(node));
        }

        // 获取CPU ID(s)
        public uint[] Ids
        {
            get
            {
                var regProp = _node.GetProperty("reg");
                return regProp?.AsUInt32Array() ?? Array.Empty<uint>();
            }
        }

        // 获取CPU时钟频率
        public ulong? ClockFrequency
        {
            get
            {
                var clockProp = _node.GetProperty("clock-frequency");
                if (clockProp?.Value.Length >= 8)
                    return clockProp.AsUInt64();
                else if (clockProp?.Value.Length >= 4)
                    return clockProp.AsUInt32();
                return null;
            }
        }

        // 获取CPU兼容性列表
        public string[] Compatible =>
            _node.GetProperty("compatible")?.AsStringList() ?? Array.Empty<string>();

        // 获取设备树节点
        public FdtNode Node => _node;
    }
}
