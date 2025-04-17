using System.Text;

namespace DeviceTreeNode.Core
{
    public class CString
    {
        private readonly byte[] _data;

        private CString(byte[] data)
        {
            _data = data;
        }

        public static CString FromBytes(byte[] data)
        {
            if (data == null)
                return null;

            // 查找null终止符
            int length = 0;
            while (length < data.Length && data[length] != 0)
                length++;

            // 必须包含null终止符
            if (length >= data.Length)
                return null;

            byte[] copy = new byte[length + 1];
            Array.Copy(data, copy, length + 1);

            return new CString(copy);
        }

        // 不包含null终止符的长度
        public int Length => _data.Length > 0 ? _data.Length - 1 : 0;

        // 转换为UTF-8字符串
        public string AsString()
        {
            if (_data == null || _data.Length <= 1)
                return string.Empty;

            return Encoding.UTF8.GetString(_data, 0, _data.Length - 1);
        }

        public override string ToString() => AsString();
    }
}
