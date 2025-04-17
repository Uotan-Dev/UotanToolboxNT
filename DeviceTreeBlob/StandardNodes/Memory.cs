using DeviceTreeNode.Models;
using DeviceTreeNode.Nodes;

namespace DeviceTreeNode.StandardNodes
{
    public class Memory
    {
        private readonly FdtNode _node;

        public Memory(FdtNode node)
        {
            _node = node ?? throw new ArgumentNullException(nameof(node));
        }

        // 获取所有内存区域
        public IEnumerable<MemoryRegion> Regions => _node.Reg();

        // 获取总内存大小
        public ulong TotalSize
        {
            get
            {
                ulong total = 0;
                foreach (var region in Regions)
                {
                    if (region.Size.HasValue)
                        total += region.Size.Value;
                }
                return total;
            }
        }

        // 检查地址是否在内存区域内
        public bool ContainsAddress(IntPtr address)
        {
            foreach (var region in Regions)
            {
                if (region.Contains(address))
                    return true;
            }
            return false;
        }

        // 获取设备树节点
        public FdtNode Node => _node;
    }
}
