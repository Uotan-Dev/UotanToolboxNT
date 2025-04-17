using DeviceTreeNode.Models;

namespace DeviceTreeNode.Core
{
    public class FdtData
    {
        private readonly byte[] _data;
        private int _position;

        public FdtData(byte[] data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _position = 0;
        }

        public BigEndianU32? ReadUInt32()
        {
            if (_position + 4 > _data.Length)
                return null;

            var result = BigEndianU32.FromBytes(_data.Skip(_position).Take(4).ToArray());
            if (result.HasValue)
                _position += 4;

            return result;
        }

        public BigEndianU64? ReadUInt64()
        {
            if (_position + 8 > _data.Length)
                return null;

            var result = BigEndianU64.FromBytes(_data.Skip(_position).Take(8).ToArray());
            if (result.HasValue)
                _position += 8;

            return result;
        }

        public BigEndianU32? PeekUInt32()
        {
            int savedPosition = _position;
            var result = ReadUInt32();
            _position = savedPosition;
            return result;
        }

        public void Skip(int bytes)
        {
            if (bytes <= 0)
                return;

            _position = Math.Min(_position + bytes, _data.Length);
        }

        public byte[] Take(int bytes)
        {
            if (bytes <= 0 || _position + bytes > _data.Length)
                return Array.Empty<byte>();

            byte[] result = new byte[bytes];
            Array.Copy(_data, _position, result, 0, bytes);
            _position += bytes;
            return result;
        }

        public bool IsEmpty() => _position >= _data.Length;

        public byte[] Remaining()
        {
            if (_position >= _data.Length)
                return Array.Empty<byte>();

            byte[] result = new byte[_data.Length - _position];
            Array.Copy(_data, _position, result, 0, result.Length);
            return result;
        }

        public void SkipNops()
        {
            while (true)
            {
                var token = PeekUInt32();
                // 注意这里使用Value属性而不是Get()方法
                if (token.HasValue && token.Value.Value == FdtConstants.FDT_NOP)
                {
                    Skip(4); // 跳过NOP标记
                }
                else
                {
                    break;
                }
            }
        }
    }
}
