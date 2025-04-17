using DeviceTreeNode.Nodes;
using System.Text;
using System.Text.RegularExpressions;

namespace DeviceTreeNode.Core
{
    /// <summary>
    /// 提供DTB编辑和修补功能
    /// </summary>
    public class FdtEditor
    {
        private readonly Fdt _originalFdt;
        private Fdt _modifiedFdt;
        private bool _modified = false;

        public FdtEditor(Fdt fdt)
        {
            _originalFdt = fdt ?? throw new ArgumentNullException(nameof(fdt));
            // 深度克隆FDT结构 - 实际实现中需要一个完整的克隆方法
            // 这里简化处理，只引用原始FDT
            _modifiedFdt = fdt;
        }

        /// <summary>
        /// 修补chosen节点中的skip_initramfs为want_initramfs
        /// </summary>
        /// <returns>是否修改了DTB</returns>
        public bool PatchInitRamfs()
        {
            bool patched = false;
            var chosen = _modifiedFdt.FindNode("/chosen");
            if (chosen != null)
            {
                var bootargs = chosen.GetProperty("bootargs");
                if (bootargs != null)
                {
                    string argsStr = bootargs.AsString();
                    if (argsStr.Contains("skip_initramfs"))
                    {
                        string newArgsStr = argsStr.Replace("skip_initramfs", "want_initramfs");
                        if (newArgsStr != argsStr)
                        {
                            // 使用新值替换属性
                            ReplaceProperty(chosen, "bootargs", StringToBytes(newArgsStr));
                            patched = true;
                            _modified = true;
                        }
                    }
                }
            }
            return patched;
        }

        /// <summary>
        /// 修补fstab节点中的verity/avb标志
        /// </summary>
        /// <returns>是否修改了DTB</returns>
        public bool PatchVerity(bool keepVerity)
        {
            if (keepVerity)
                return false;

            bool patched = false;
            var fstab = FindFstab();
            if (fstab != null)
            {
                foreach (var child in fstab.Children())
                {
                    var flags = child.GetProperty("fsmgr_flags");
                    if (flags != null)
                    {
                        string flagsStr = flags.AsString();
                        string newFlagsStr = RemoveVerityFlags(flagsStr);

                        if (newFlagsStr != flagsStr)
                        {
                            // 替换属性值
                            ReplaceProperty(child, "fsmgr_flags", StringToBytes(newFlagsStr));
                            patched = true;
                            _modified = true;
                        }
                    }
                }
            }
            return patched;
        }

        /// <summary>
        /// 获取修改后的DTB二进制数据
        /// </summary>
        public byte[] GetModifiedDtb()
        {
            if (!_modified)
                return null;

            var serializer = new FdtSerializer(_modifiedFdt);
            return serializer.Serialize();
        }

        /// <summary>
        /// 查找fstab节点
        /// </summary>
        private FdtNode FindFstab()
        {
            return _modifiedFdt.AllNodes().FirstOrDefault(n => n.Name == "fstab");
        }

        /// <summary>
        /// 从标志字符串中移除verity相关标志
        /// </summary>
        private string RemoveVerityFlags(string flags)
        {
            // 移除与verity相关的标志
            string result = flags;
            string[] verityFlags = ["verify", "avb", "verity"];

            foreach (var flag in verityFlags)
            {
                // 移除标志，同时考虑标志可能有前缀或后缀
                result = Regex.Replace(result, $"(^|,){flag}(,|$)", "$1$2");
                result = result.Replace(",,", ",");
                result = result.Trim(',');
            }

            return result;
        }

        /// <summary>
        /// 替换节点的属性值
        /// </summary>
        private void ReplaceProperty(FdtNode node, string propName, byte[] newValue)
        {
            // 这需要NodeProperty和FdtNode的修改支持
            // 在下面的实现改进中会添加这些功能
            // 这里只是一个占位符
            ((MutableFdtNode)node).ReplaceProperty(propName, newValue);
        }

        /// <summary>
        /// 将字符串转换为带NULL终止符的UTF-8字节数组
        /// </summary>
        private byte[] StringToBytes(string str)
        {
            byte[] strBytes = Encoding.UTF8.GetBytes(str);
            byte[] result = new byte[strBytes.Length + 1];
            Array.Copy(strBytes, result, strBytes.Length);
            result[strBytes.Length] = 0;
            return result;
        }
    }
}
