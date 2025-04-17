namespace DeviceTreeNode.Core
{
    // 32位大端序值封装
    public struct BigEndianU32 : IEquatable<BigEndianU32>, IComparable<BigEndianU32>
    {
        private readonly uint _value;

        public BigEndianU32(uint value)
        {
            _value = value;
        }

        public uint Value => _value;

        public static BigEndianU32? FromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 4)
                return null;

            byte[] valueBytes = new byte[4];
            Array.Copy(bytes, valueBytes, 4);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(valueBytes);

            return new BigEndianU32(BitConverter.ToUInt32(valueBytes, 0));
        }

        // 运算符重载
        public static bool operator ==(BigEndianU32 left, BigEndianU32 right) => left._value == right._value;
        public static bool operator !=(BigEndianU32 left, BigEndianU32 right) => left._value != right._value;
        public static bool operator <(BigEndianU32 left, BigEndianU32 right) => left._value < right._value;
        public static bool operator <=(BigEndianU32 left, BigEndianU32 right) => left._value <= right._value;
        public static bool operator >(BigEndianU32 left, BigEndianU32 right) => left._value > right._value;
        public static bool operator >=(BigEndianU32 left, BigEndianU32 right) => left._value >= right._value;

        // 与其他数值类型的比较运算符
        public static bool operator ==(BigEndianU32 left, uint right) => left._value == right;
        public static bool operator !=(BigEndianU32 left, uint right) => left._value != right;
        public static bool operator <(BigEndianU32 left, uint right) => left._value < right;
        public static bool operator <=(BigEndianU32 left, uint right) => left._value <= right;
        public static bool operator >(BigEndianU32 left, uint right) => left._value > right;
        public static bool operator >=(BigEndianU32 left, uint right) => left._value >= right;

        public static bool operator ==(uint left, BigEndianU32 right) => left == right._value;
        public static bool operator !=(uint left, BigEndianU32 right) => left != right._value;
        public static bool operator <(uint left, BigEndianU32 right) => left < right._value;
        public static bool operator <=(uint left, BigEndianU32 right) => left <= right._value;
        public static bool operator >(uint left, BigEndianU32 right) => left > right._value;
        public static bool operator >=(uint left, BigEndianU32 right) => left >= right._value;

        // 与int类型的比较运算符
        public static bool operator ==(BigEndianU32 left, int right) => left._value == (uint)right;
        public static bool operator !=(BigEndianU32 left, int right) => left._value != (uint)right;
        public static bool operator <(BigEndianU32 left, int right) => (int)left._value < right;
        public static bool operator <=(BigEndianU32 left, int right) => (int)left._value <= right;
        public static bool operator >(BigEndianU32 left, int right) => (int)left._value > right;
        public static bool operator >=(BigEndianU32 left, int right) => (int)left._value >= right;

        public static bool operator ==(int left, BigEndianU32 right) => (uint)left == right._value;
        public static bool operator !=(int left, BigEndianU32 right) => (uint)left != right._value;
        public static bool operator <(int left, BigEndianU32 right) => left < (int)right._value;
        public static bool operator <=(int left, BigEndianU32 right) => left <= (int)right._value;
        public static bool operator >(int left, BigEndianU32 right) => left > (int)right._value;
        public static bool operator >=(int left, BigEndianU32 right) => left >= (int)right._value;

        // 隐式转换
        public static implicit operator uint(BigEndianU32 value) => value._value;
        public static implicit operator BigEndianU32(uint value) => new BigEndianU32(value);

        // 显式转换
        public static explicit operator int(BigEndianU32 value) => (int)value._value;

        // 实现IEquatable<T>
        public bool Equals(BigEndianU32 other) => _value == other._value;

        // 重写Object方法
        public override bool Equals(object obj) =>
            obj is BigEndianU32 other && Equals(other);

        public override int GetHashCode() => _value.GetHashCode();

        // 实现IComparable<T>
        public int CompareTo(BigEndianU32 other) => _value.CompareTo(other._value);

        public override string ToString() => $"0x{_value:X8}";
    }

    // 64位大端序值封装
    public struct BigEndianU64 : IEquatable<BigEndianU64>, IComparable<BigEndianU64>
    {
        private readonly ulong _value;

        public BigEndianU64(ulong value)
        {
            _value = value;
        }

        public ulong Value => _value;

        public static BigEndianU64? FromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 8)
                return null;

            byte[] valueBytes = new byte[8];
            Array.Copy(bytes, valueBytes, 8);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(valueBytes);

            return new BigEndianU64(BitConverter.ToUInt64(valueBytes, 0));
        }

        // 运算符重载
        public static bool operator ==(BigEndianU64 left, BigEndianU64 right) => left._value == right._value;
        public static bool operator !=(BigEndianU64 left, BigEndianU64 right) => left._value != right._value;
        public static bool operator <(BigEndianU64 left, BigEndianU64 right) => left._value < right._value;
        public static bool operator <=(BigEndianU64 left, BigEndianU64 right) => left._value <= right._value;
        public static bool operator >(BigEndianU64 left, BigEndianU64 right) => left._value > right._value;
        public static bool operator >=(BigEndianU64 left, BigEndianU64 right) => left._value >= right._value;

        // 与其他数值类型的比较运算符
        public static bool operator ==(BigEndianU64 left, ulong right) => left._value == right;
        public static bool operator !=(BigEndianU64 left, ulong right) => left._value != right;
        public static bool operator <(BigEndianU64 left, ulong right) => left._value < right;
        public static bool operator <=(BigEndianU64 left, ulong right) => left._value <= right;
        public static bool operator >(BigEndianU64 left, ulong right) => left._value > right;
        public static bool operator >=(BigEndianU64 left, ulong right) => left._value >= right;

        public static bool operator ==(ulong left, BigEndianU64 right) => left == right._value;
        public static bool operator !=(ulong left, BigEndianU64 right) => left != right._value;
        public static bool operator <(ulong left, BigEndianU64 right) => left < right._value;
        public static bool operator <=(ulong left, BigEndianU64 right) => left <= right._value;
        public static bool operator >(ulong left, BigEndianU64 right) => left > right._value;
        public static bool operator >=(ulong left, BigEndianU64 right) => left >= right._value;

        // 与int类型的比较运算符
        public static bool operator ==(BigEndianU64 left, int right) => left._value == (ulong)right;
        public static bool operator !=(BigEndianU64 left, int right) => left._value != (ulong)right;
        public static bool operator >(BigEndianU64 left, int right) => right >= 0 && left._value > (ulong)right;
        public static bool operator >=(BigEndianU64 left, int right) => right >= 0 && left._value >= (ulong)right;
        public static bool operator <(BigEndianU64 left, int right) => right > 0 && left._value < (ulong)right;
        public static bool operator <=(BigEndianU64 left, int right) => right >= 0 && left._value <= (ulong)right;

        public static bool operator ==(int left, BigEndianU64 right) => left >= 0 && (ulong)left == right._value;
        public static bool operator !=(int left, BigEndianU64 right) => left < 0 || (ulong)left != right._value;
        public static bool operator <(int left, BigEndianU64 right) => left < 0 || (ulong)left < right._value;
        public static bool operator <=(int left, BigEndianU64 right) => left < 0 || (ulong)left <= right._value;
        public static bool operator >(int left, BigEndianU64 right) => left >= 0 && (ulong)left > right._value;
        public static bool operator >=(int left, BigEndianU64 right) => left >= 0 && (ulong)left >= right._value;

        // 隐式转换
        public static implicit operator ulong(BigEndianU64 value) => value._value;
        public static implicit operator BigEndianU64(ulong value) => new BigEndianU64(value);

        // 显式转换
        public static explicit operator long(BigEndianU64 value) => (long)value._value;
        public static explicit operator int(BigEndianU64 value) => (int)value._value;

        // 实现IEquatable<T>
        public bool Equals(BigEndianU64 other) => _value == other._value;

        // 重写Object方法
        public override bool Equals(object obj) =>
            obj is BigEndianU64 other && Equals(other);

        public override int GetHashCode() => _value.GetHashCode();

        // 实现IComparable<T>
        public int CompareTo(BigEndianU64 other) => _value.CompareTo(other._value);

        public override string ToString() => $"0x{_value:X16}";
    }
}
