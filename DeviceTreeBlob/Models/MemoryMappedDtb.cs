using DeviceTreeNode.Core;
using DeviceTreeNode.Nodes;
using System.Text;
using System.Text.RegularExpressions;

namespace DeviceTreeNode.Models
{
    /// <summary>
    /// 内存映射DTB文件，支持查找、读取和修改DTB内容
    /// </summary>
    public class MemoryMappedDtb : IDisposable
    {
        private const uint FDT_MAGIC = 0xd00dfeed;
        private readonly MemoryMappedFile _file;
        private readonly string _filePath;
        private readonly bool _isReadOnly;

        /// <summary>
        /// 打开只读DTB文件
        /// </summary>
        public static MemoryMappedDtb OpenRead(string path)
        {
            return new MemoryMappedDtb(path, false);
        }

        /// <summary>
        /// 打开可读写DTB文件
        /// </summary>
        public static MemoryMappedDtb OpenReadWrite(string path)
        {
            return new MemoryMappedDtb(path, true);
        }

        private MemoryMappedDtb(string path, bool readWrite)
        {
            _filePath = path ?? throw new ArgumentNullException(nameof(path));
            _isReadOnly = !readWrite;
            _file = readWrite ?
                MemoryMappedFile.OpenReadWrite(path) :
                MemoryMappedFile.OpenRead(path);
        }

        /// <summary>
        /// 查找文件中的所有DTB，并对每个DTB执行操作
        /// </summary>
        public void ForEachDtb(Action<int, Fdt> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            byte[] fileData = _file.ToArray();
            int position = 0;
            int dtbNum = 0;

            while (position < fileData.Length)
            {
                // 查找DTB魔数
                int magicPos = FindDtbMagic(fileData, position);
                if (magicPos == -1 || magicPos + 40 > fileData.Length)
                    break;

                position = magicPos;

                try
                {
                    // 获取该位置开始的DTB数据
                    byte[] dtbData = new byte[fileData.Length - position];
                    Array.Copy(fileData, position, dtbData, 0, dtbData.Length);

                    // 解析DTB
                    Fdt fdt = new Fdt(dtbData);

                    // 执行操作
                    action(dtbNum, fdt);

                    // 移动到下一个DTB位置
                    position += (int)fdt.Header.TotalSize;
                    dtbNum++;
                }
                catch (Exception)
                {
                    // 解析错误，尝试继续查找下一个DTB
                    position += 4;
                }
            }
        }

        /// <summary>
        /// 获取文件中第n个DTB
        /// </summary>
        public Fdt GetDtb(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            Fdt result = null;
            int count = 0;

            ForEachDtb((i, fdt) =>
            {
                if (i == index)
                    result = fdt;
                count = i + 1;
            });

            if (result == null)
                throw new ArgumentOutOfRangeException(nameof(index), $"DTB index {index} not found, max index is {count - 1}");

            return result;
        }

        /// <summary>
        /// 修改字节值
        /// </summary>
        public void ModifyBytes(long offset, byte[] newData)
        {
            if (_isReadOnly)
                throw new InvalidOperationException("Cannot modify read-only DTB");

            if (offset < 0 || offset + newData.Length > _file.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            _file.WriteBytes(offset, newData);
        }

        /// <summary>
        /// 替换字符串
        /// </summary>
        public bool ReplaceString(long startOffset, string oldValue, string newValue)
        {
            if (_isReadOnly)
                throw new InvalidOperationException("Cannot modify read-only DTB");

            byte[] oldBytes = Encoding.UTF8.GetBytes(oldValue);
            byte[] newBytes = Encoding.UTF8.GetBytes(newValue);

            if (newBytes.Length > oldBytes.Length)
                throw new ArgumentException("Replacement string must not be longer than original");

            byte[] currentBytes = _file.ReadBytes(startOffset, oldBytes.Length);
            if (!ByteArrayEquals(currentBytes, oldBytes))
                return false;

            // 创建新的数据，填充剩余部分为0
            byte[] fillData = new byte[oldBytes.Length];
            Array.Copy(newBytes, fillData, newBytes.Length);

            _file.WriteBytes(startOffset, fillData);
            return true;
        }

        /// <summary>
        /// 修改节点属性
        /// </summary>
        public bool ModifyNodeProperty(Fdt dtb, int dtbOffset, FdtNode node, string propertyName, string newValue)
        {
            if (_isReadOnly)
                throw new InvalidOperationException("Cannot modify read-only DTB");

            // 找到节点属性
            var property = node.GetProperty(propertyName);
            if (property == null)
                return false;

            // 计算属性值在文件中的偏移
            int propValueOffset = -1;
            byte[] fileBytes = _file.ToArray();

            // 在文件中查找属性值
            for (int i = dtbOffset; i < fileBytes.Length - property.Value.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < property.Value.Length; j++)
                {
                    if (fileBytes[i + j] != property.Value[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    propValueOffset = i;
                    break;
                }
            }

            if (propValueOffset == -1)
                return false;

            // 创建新的数据
            byte[] newValueBytes = Encoding.UTF8.GetBytes(newValue);
            if (newValueBytes.Length > property.Value.Length)
                throw new ArgumentException("New value is too long for the property");

            // 填充剩余部分为0
            byte[] fillData = new byte[property.Value.Length];
            Array.Copy(newValueBytes, fillData, newValueBytes.Length);

            // 确保结束为null字节
            fillData[newValueBytes.Length] = 0;

            _file.WriteBytes(propValueOffset, fillData);
            return true;
        }

        /// <summary>
        /// 查找并修补fstab节点中的verity标志
        /// </summary>
        public bool PatchVerity(bool keepVerity = false)
        {
            if (_isReadOnly)
                throw new InvalidOperationException("Cannot modify read-only DTB");

            bool patched = false;

            ForEachDtb((dtbNum, fdt) =>
            {
                // 首先尝试修补bootargs中的skip_initramfs -> want_initramfs
                var chosenNodes = new List<FdtNode>();
                foreach (var node in fdt.AllNodes())
                {
                    if (node.Name == "chosen")
                        chosenNodes.Add(node);
                }

                // 计算DTB在文件中的偏移
                int dtbOffset = FindDtbOffset(dtbNum);

                foreach (var chosen in chosenNodes)
                {
                    var bootargs = chosen.GetProperty("bootargs");
                    if (bootargs != null)
                    {
                        string args = bootargs.AsString();
                        if (args.Contains("skip_initramfs"))
                        {
                            // 在整个文件中查找并替换字符串
                            if (ReplaceStringInFile(dtbOffset, "skip_initramfs", "want_initramfs"))
                                patched = true;
                        }
                    }
                }

                // 如果不保留verity，则修补fstab中的标志
                if (!keepVerity)
                {
                    // 查找fstab节点
                    var fstabNode = fdt.AllNodes().FirstOrDefault(n => n.Name == "fstab");
                    if (fstabNode != null)
                    {
                        foreach (var child in fstabNode.Children())
                        {
                            var flags = child.GetProperty("fsmgr_flags");
                            if (flags != null)
                            {
                                string flagsValue = flags.AsString();
                                string newFlags = PatchVerityFlags(flagsValue);

                                if (newFlags != flagsValue)
                                {
                                    // 定位并修改标志值
                                    if (ModifyNodeProperty(fdt, dtbOffset, child, "fsmgr_flags", newFlags))
                                        patched = true;
                                }
                            }
                        }
                    }
                }
            });

            return patched;
        }

        /// <summary>
        /// 测试fstab节点是否包含/system_root挂载点
        /// </summary>
        public bool TestFstabHasSystemRoot()
        {
            bool hasSystemRoot = false;

            ForEachDtb((_, fdt) =>
            {
                var fstabNode = fdt.AllNodes().FirstOrDefault(n => n.Name == "fstab");
                if (fstabNode != null)
                {
                    foreach (var child in fstabNode.Children())
                    {
                        if (child.Name != "system")
                            continue;

                        var mountPoint = child.GetProperty("mnt_point");
                        if (mountPoint != null && mountPoint.AsString() == "/system_root")
                        {
                            hasSystemRoot = true;
                            break;
                        }
                    }
                }
            });

            return hasSystemRoot;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _file?.Dispose();
        }

        #region 私有辅助方法

        /// <summary>
        /// 查找DTB魔数在文件中的位置
        /// </summary>
        private int FindDtbMagic(byte[] data, int startPos)
        {
            for (int i = startPos; i <= data.Length - 4; i++)
            {
                if (data[i] == 0xd0 && data[i + 1] == 0x0d &&
                    data[i + 2] == 0xfe && data[i + 3] == 0xed)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 在文件中查找并替换字符串
        /// </summary>
        private bool ReplaceStringInFile(int dtbOffset, string oldStr, string newStr)
        {
            byte[] fileData = _file.ToArray();
            byte[] oldBytes = Encoding.UTF8.GetBytes(oldStr);
            byte[] newBytes = Encoding.UTF8.GetBytes(newStr);

            if (newBytes.Length > oldBytes.Length)
                throw new ArgumentException("Replacement string cannot be longer than original");

            bool replaced = false;

            for (int i = dtbOffset; i <= fileData.Length - oldBytes.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < oldBytes.Length; j++)
                {
                    if (fileData[i + j] != oldBytes[j])
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    byte[] paddedBytes = new byte[oldBytes.Length];
                    Array.Copy(newBytes, paddedBytes, newBytes.Length);
                    _file.WriteBytes(i, paddedBytes);
                    replaced = true;
                }
            }

            return replaced;
        }

        /// <summary>
        /// 修补verity标志
        /// </summary>
        private string PatchVerityFlags(string flags)
        {
            string[] verityFlags = ["verify", "avb", "verity"];
            string result = flags;

            foreach (var flag in verityFlags)
            {
                // 移除标志（考虑不同的前缀和后缀情况）
                result = Regex.Replace(result, $"(^|,){flag}(,|$)", "$1$2");
                result = Regex.Replace(result, ",,", ",");
            }

            // 清理头尾的逗号
            result = result.Trim(',');
            return result;
        }

        /// <summary>
        /// 查找第n个DTB在文件中的偏移
        /// </summary>
        private int FindDtbOffset(int index)
        {
            byte[] fileData = _file.ToArray();
            int position = 0;
            int currentIndex = 0;

            while (position < fileData.Length && currentIndex <= index)
            {
                int magicPos = FindDtbMagic(fileData, position);
                if (magicPos == -1 || magicPos + 40 > fileData.Length)
                    return -1;

                position = magicPos;

                try
                {
                    byte[] dtbData = new byte[fileData.Length - position];
                    Array.Copy(fileData, position, dtbData, 0, dtbData.Length);

                    Fdt fdt = new Fdt(dtbData);

                    if (currentIndex == index)
                        return position;

                    position += (int)fdt.Header.TotalSize;
                    currentIndex++;
                }
                catch
                {
                    position += 4;
                }
            }

            return -1;
        }

        /// <summary>
        /// 比较两个字节数组是否相等
        /// </summary>
        private bool ByteArrayEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }

        #endregion
    }
}
