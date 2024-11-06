// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using System.Collections.Generic;

namespace Kaitai
{

    /// <summary>
    /// A variable-length unsigned integer using base128 encoding. 1-byte groups
    /// consist of 1-bit flag of continuation and 7-bit value chunk, and are ordered
    /// &quot;least significant group first&quot;, i.e. in &quot;little-endian&quot; manner.
    /// 
    /// This particular encoding is specified and used in:
    /// 
    /// * DWARF debug file format, where it's dubbed &quot;unsigned LEB128&quot; or &quot;ULEB128&quot;.
    ///   http://dwarfstd.org/doc/dwarf-2.0.0.pdf - page 139
    /// * Google Protocol Buffers, where it's called &quot;Base 128 Varints&quot;.
    ///   https://developers.google.com/protocol-buffers/docs/encoding?csw=1#varints
    /// * Apache Lucene, where it's called &quot;VInt&quot;
    ///   http://lucene.apache.org/core/3_5_0/fileformats.html#VInt
    /// * Apache Avro uses this as a basis for integer encoding, adding ZigZag on
    ///   top of it for signed ints
    ///   http://avro.apache.org/docs/current/spec.html#binary_encode_primitive
    /// 
    /// More information on this encoding is available at https://en.wikipedia.org/wiki/LEB128
    /// 
    /// This particular implementation supports serialized values to up 8 bytes long.
    /// </summary>
    public partial class VlqBase128Le : KaitaiStruct
    {
        public static VlqBase128Le FromFile(string fileName)
        {
            return new VlqBase128Le(new KaitaiStream(fileName));
        }

        public VlqBase128Le(KaitaiStream p__io, KaitaiStruct p__parent = null, VlqBase128Le p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            f_len = false;
            f_value = false;
            _read();
        }
        private void _read()
        {
            _groups = new List<Group>();
            {
                var i = 0;
                Group M_;
                do {
                    M_ = new Group(m_io, this, m_root);
                    _groups.Add(M_);
                    i++;
                } while (!(!(M_.HasNext)));
            }
        }

        /// <summary>
        /// One byte group, clearly divided into 7-bit &quot;value&quot; chunk and 1-bit &quot;continuation&quot; flag.
        /// </summary>
        public partial class Group : KaitaiStruct
        {
            public static Group FromFile(string fileName)
            {
                return new Group(new KaitaiStream(fileName));
            }

            public Group(KaitaiStream p__io, VlqBase128Le p__parent = null, VlqBase128Le p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_hasNext = false;
                f_value = false;
                _read();
            }
            private void _read()
            {
                _b = m_io.ReadU1();
            }
            private bool f_hasNext;
            private bool _hasNext;

            /// <summary>
            /// If true, then we have more bytes to read
            /// </summary>
            public bool HasNext
            {
                get
                {
                    if (f_hasNext)
                        return _hasNext;
                    _hasNext = (bool) ((B & 128) != 0);
                    f_hasNext = true;
                    return _hasNext;
                }
            }
            private bool f_value;
            private int _value;

            /// <summary>
            /// The 7-bit (base128) numeric value chunk of this group
            /// </summary>
            public int Value
            {
                get
                {
                    if (f_value)
                        return _value;
                    _value = (int) ((B & 127));
                    f_value = true;
                    return _value;
                }
            }
            private byte _b;
            private VlqBase128Le m_root;
            private VlqBase128Le m_parent;
            public byte B { get { return _b; } }
            public VlqBase128Le M_Root { get { return m_root; } }
            public VlqBase128Le M_Parent { get { return m_parent; } }
        }
        private bool f_len;
        private int _len;
        public int Len
        {
            get
            {
                if (f_len)
                    return _len;
                _len = (int) (Groups.Count);
                f_len = true;
                return _len;
            }
        }
        private bool f_value;
        private int _value;

        /// <summary>
        /// Resulting value as normal integer
        /// </summary>
        public int Value
        {
            get
            {
                if (f_value)
                    return _value;
                _value = (int) ((((((((Groups[0].Value + (Len >= 2 ? (Groups[1].Value << 7) : 0)) + (Len >= 3 ? (Groups[2].Value << 14) : 0)) + (Len >= 4 ? (Groups[3].Value << 21) : 0)) + (Len >= 5 ? (Groups[4].Value << 28) : 0)) + (Len >= 6 ? (Groups[5].Value << 35) : 0)) + (Len >= 7 ? (Groups[6].Value << 42) : 0)) + (Len >= 8 ? (Groups[7].Value << 49) : 0)));
                f_value = true;
                return _value;
            }
        }
        private List<Group> _groups;
        private VlqBase128Le m_root;
        private KaitaiStruct m_parent;
        public List<Group> Groups { get { return _groups; } }
        public VlqBase128Le M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
