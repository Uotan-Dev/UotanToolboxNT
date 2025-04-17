using DeviceTreeNode.Core;
using System.Text;

namespace DeviceTreeNode.Nodes
{
    public class NodeProperty
    {
        private readonly Fdt _owner;

        public string Name { get; }
        public byte[] Value { get; private set; }

        public NodeProperty(string name, byte[] value, Fdt owner)
        {
            Name = name;
            Value = value ?? Array.Empty<byte>();
            _owner = owner;
        }

        // 修改属性值
        public void SetValue(byte[] newValue)
        {
            Value = newValue ?? Array.Empty<byte>();
        }

        public static NodeProperty Parse(FdtData stream, Fdt owner)
        {
            // 跳过FDT_PROP标记
            stream.Skip(4);

            // 读取值长度和名称偏移
            uint? valueLen = stream.ReadUInt32();
            uint? nameOffset = stream.ReadUInt32();

            if (!valueLen.HasValue || !nameOffset.HasValue)
                return null;

            // 获取属性值
            byte[] value = stream.Take((int)valueLen.Value);
            if (value == null)
                return null;

            // 对齐到4字节边界
            int padding = (4 - (value.Length % 4)) % 4;
            stream.Skip(padding);

            // 从字符串块中获取属性名
            string name = owner.GetStringAtOffset((int)nameOffset.Value);
            if (name == null)
                return null;

            return new NodeProperty(name, value, owner);
        }

        // 将属性值解析为字符串
        public string AsString()
        {
            // 去除末尾的null字节
            int length = Value.Length;
            while (length > 0 && Value[length - 1] == 0)
                length--;

            return Encoding.UTF8.GetString(Value, 0, length);
        }

        // 将属性值解析为32位无符号整数
        public uint? AsUInt32() =>
            Value.Length >= 4 ? BitConverter.ToUInt32(ConvertToHostEndian(Value, 4), 0) : null;

        // 将属性值解析为64位无符号整数
        public ulong? AsUInt64() =>
            Value.Length >= 8 ? BitConverter.ToUInt64(ConvertToHostEndian(Value, 8), 0) : null;

        // 将属性值解析为整数列表
        public uint[] AsUInt32Array()
        {
            if (Value.Length % 4 != 0)
                return Array.Empty<uint>();

            uint[] result = new uint[Value.Length / 4];
            for (int i = 0; i < result.Length; i++)
            {
                byte[] bytes = new byte[4];
                Array.Copy(Value, i * 4, bytes, 0, 4);
                result[i] = BitConverter.ToUInt32(ConvertToHostEndian(bytes, 4), 0);
            }

            return result;
        }

        // 将属性值解析为字符串列表
        public string[] AsStringList()
        {
            if (Value.Length == 0 || Value[Value.Length - 1] != 0)
                return Array.Empty<string>();

            return AsString().Split('\0');
        }

        private byte[] ConvertToHostEndian(byte[] data, int size)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] result = new byte[size];
                for (int i = 0; i < size; i++)
                    result[i] = data[size - 1 - i];
                return result;
            }
            else
            {
                return data;
            }
        }
    }
}
