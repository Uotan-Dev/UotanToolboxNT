// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using Kaitai;
using System.Collections.Generic;

namespace UotanToolbox.Common.ROMHelper.Kaitai_Struct
{

    /// <summary>
    /// Format for Android DTB/DTBO partitions. It's kind of archive with
    /// dtb/dtbo files. Used only when there is a separate unique partition
    /// (dtb, dtbo) on an android device to organize device tree files.
    /// The format consists of a header with info about size and number
    /// of device tree entries and the entries themselves. This format
    /// description could be used to extract device tree entries from a
    /// partition images and decompile them with dtc (device tree compiler).
    /// </summary>
    /// <remarks>
    /// Reference: <a href="https://source.android.com/docs/core/architecture/dto/partitions">Source</a>
    /// </remarks>
    /// <remarks>
    /// Reference: <a href="https://android.googlesource.com/platform/system/libufdt/+/refs/tags/android-10.0.0_r47">Source</a>
    /// </remarks>
    public partial class AndroidDto : KaitaiStruct
    {
        public static AndroidDto FromFile(string fileName)
        {
            return new AndroidDto(new KaitaiStream(fileName));
        }

        public AndroidDto(KaitaiStream p__io, KaitaiStruct p__parent = null, AndroidDto p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            _read();
        }
        private void _read()
        {
            _header = new DtTableHeader(m_io, this, m_root);
            _entries = new List<DtTableEntry>();
            for (var i = 0; i < Header.DtEntryCount; i++)
            {
                _entries.Add(new DtTableEntry(m_io, this, m_root));
            }
        }
        public partial class DtTableHeader : KaitaiStruct
        {
            public static DtTableHeader FromFile(string fileName)
            {
                return new DtTableHeader(new KaitaiStream(fileName));
            }

            public DtTableHeader(KaitaiStream p__io, AndroidDto p__parent = null, AndroidDto p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _magic = m_io.ReadBytes(4);
                if (!(KaitaiStream.ByteArrayCompare(Magic, new byte[] { 215, 183, 171, 30 }) == 0))
                {
                    throw new ValidationNotEqualError(new byte[] { 215, 183, 171, 30 }, Magic, M_Io, "/types/dt_table_header/seq/0");
                }
                _totalSize = m_io.ReadU4be();
                _headerSize = m_io.ReadU4be();
                _dtEntrySize = m_io.ReadU4be();
                _dtEntryCount = m_io.ReadU4be();
                _dtEntriesOffset = m_io.ReadU4be();
                _pageSize = m_io.ReadU4be();
                _version = m_io.ReadU4be();
            }
            private byte[] _magic;
            private uint _totalSize;
            private uint _headerSize;
            private uint _dtEntrySize;
            private uint _dtEntryCount;
            private uint _dtEntriesOffset;
            private uint _pageSize;
            private uint _version;
            private AndroidDto m_root;
            private AndroidDto m_parent;
            public byte[] Magic { get { return _magic; } }

            /// <summary>
            /// includes dt_table_header + all dt_table_entry and all dtb/dtbo
            /// </summary>
            public uint TotalSize { get { return _totalSize; } }

            /// <summary>
            /// sizeof(dt_table_header)
            /// </summary>
            public uint HeaderSize { get { return _headerSize; } }

            /// <summary>
            /// sizeof(dt_table_entry)
            /// </summary>
            public uint DtEntrySize { get { return _dtEntrySize; } }

            /// <summary>
            /// number of dt_table_entry
            /// </summary>
            public uint DtEntryCount { get { return _dtEntryCount; } }

            /// <summary>
            /// offset to the first dt_table_entry from head of dt_table_header
            /// </summary>
            public uint DtEntriesOffset { get { return _dtEntriesOffset; } }

            /// <summary>
            /// flash page size
            /// </summary>
            public uint PageSize { get { return _pageSize; } }

            /// <summary>
            /// DTBO image version
            /// </summary>
            public uint Version { get { return _version; } }
            public AndroidDto M_Root { get { return m_root; } }
            public AndroidDto M_Parent { get { return m_parent; } }
        }
        public partial class DtTableEntry : KaitaiStruct
        {
            public static DtTableEntry FromFile(string fileName)
            {
                return new DtTableEntry(new KaitaiStream(fileName));
            }

            public DtTableEntry(KaitaiStream p__io, AndroidDto p__parent = null, AndroidDto p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_body = false;
                _read();
            }
            private void _read()
            {
                _dtSize = m_io.ReadU4be();
                _dtOffset = m_io.ReadU4be();
                _id = m_io.ReadU4be();
                _rev = m_io.ReadU4be();
                _custom = new List<uint>();
                for (var i = 0; i < 4; i++)
                {
                    _custom.Add(m_io.ReadU4be());
                }
            }
            private bool f_body;
            private byte[] _body;

            /// <summary>
            /// DTB/DTBO file
            /// </summary>
            public byte[] Body
            {
                get
                {
                    if (f_body)
                        return _body;
                    KaitaiStream io = M_Root.M_Io;
                    long _pos = io.Pos;
                    io.Seek(DtOffset);
                    _body = io.ReadBytes(DtSize);
                    io.Seek(_pos);
                    f_body = true;
                    return _body;
                }
            }
            private uint _dtSize;
            private uint _dtOffset;
            private uint _id;
            private uint _rev;
            private List<uint> _custom;
            private AndroidDto m_root;
            private AndroidDto m_parent;

            /// <summary>
            /// size of this entry
            /// </summary>
            public uint DtSize { get { return _dtSize; } }

            /// <summary>
            /// offset from head of dt_table_header
            /// </summary>
            public uint DtOffset { get { return _dtOffset; } }

            /// <summary>
            /// optional, must be zero if unused
            /// </summary>
            public uint Id { get { return _id; } }

            /// <summary>
            /// optional, must be zero if unused
            /// </summary>
            public uint Rev { get { return _rev; } }

            /// <summary>
            /// optional, must be zero if unused
            /// </summary>
            public List<uint> Custom { get { return _custom; } }
            public AndroidDto M_Root { get { return m_root; } }
            public AndroidDto M_Parent { get { return m_parent; } }
        }
        private DtTableHeader _header;
        private List<DtTableEntry> _entries;
        private AndroidDto m_root;
        private KaitaiStruct m_parent;
        public DtTableHeader Header { get { return _header; } }
        public List<DtTableEntry> Entries { get { return _entries; } }
        public AndroidDto M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
