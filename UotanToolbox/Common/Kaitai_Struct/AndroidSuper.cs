// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using System.Collections.Generic;

namespace Kaitai
{

    /// <summary>
    /// The metadata stored by Android at the beginning of a &quot;super&quot; partition, which
    /// is what it calls a disk partition that holds one or more Dynamic Partitions.
    /// Dynamic Partitions do more or less the same thing that LVM does on Linux,
    /// allowing Android to map ranges of non-contiguous extents to a single logical
    /// device. This metadata holds that mapping.
    /// </summary>
    /// <remarks>
    /// Reference: <a href="https://source.android.com/docs/core/ota/dynamic_partitions">Source</a>
    /// </remarks>
    /// <remarks>
    /// Reference: <a href="https://android.googlesource.com/platform/system/core/+/refs/tags/android-11.0.0_r8/fs_mgr/liblp/include/liblp/metadata_format.h">Source</a>
    /// </remarks>
    public partial class AndroidSuper : KaitaiStruct
    {
        public static AndroidSuper FromFile(string fileName)
        {
            return new AndroidSuper(new KaitaiStream(fileName));
        }

        public AndroidSuper(KaitaiStream p__io, KaitaiStruct p__parent = null, AndroidSuper p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            f_root = false;
            _read();
        }
        private void _read()
        {
        }
        public partial class Root : KaitaiStruct
        {
            public static Root FromFile(string fileName)
            {
                return new Root(new KaitaiStream(fileName));
            }

            public Root(KaitaiStream p__io, AndroidSuper p__parent = null, AndroidSuper p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                __raw_primaryGeometry = m_io.ReadBytes(4096);
                var io___raw_primaryGeometry = new KaitaiStream(__raw_primaryGeometry);
                _primaryGeometry = new Geometry(io___raw_primaryGeometry, this, m_root);
                __raw_backupGeometry = m_io.ReadBytes(4096);
                var io___raw_backupGeometry = new KaitaiStream(__raw_backupGeometry);
                _backupGeometry = new Geometry(io___raw_backupGeometry, this, m_root);
                __raw_primaryMetadata = new List<byte[]>();
                _primaryMetadata = new List<Metadata>();
                for (var i = 0; i < PrimaryGeometry.MetadataSlotCount; i++)
                {
                    __raw_primaryMetadata.Add(m_io.ReadBytes(PrimaryGeometry.MetadataMaxSize));
                    var io___raw_primaryMetadata = new KaitaiStream(__raw_primaryMetadata[__raw_primaryMetadata.Count - 1]);
                    _primaryMetadata.Add(new Metadata(io___raw_primaryMetadata, this, m_root));
                }
                __raw_backupMetadata = new List<byte[]>();
                _backupMetadata = new List<Metadata>();
                for (var i = 0; i < PrimaryGeometry.MetadataSlotCount; i++)
                {
                    __raw_backupMetadata.Add(m_io.ReadBytes(PrimaryGeometry.MetadataMaxSize));
                    var io___raw_backupMetadata = new KaitaiStream(__raw_backupMetadata[__raw_backupMetadata.Count - 1]);
                    _backupMetadata.Add(new Metadata(io___raw_backupMetadata, this, m_root));
                }
            }
            private Geometry _primaryGeometry;
            private Geometry _backupGeometry;
            private List<Metadata> _primaryMetadata;
            private List<Metadata> _backupMetadata;
            private AndroidSuper m_root;
            private AndroidSuper m_parent;
            private byte[] __raw_primaryGeometry;
            private byte[] __raw_backupGeometry;
            private List<byte[]> __raw_primaryMetadata;
            private List<byte[]> __raw_backupMetadata;
            public Geometry PrimaryGeometry { get { return _primaryGeometry; } }
            public Geometry BackupGeometry { get { return _backupGeometry; } }
            public List<Metadata> PrimaryMetadata { get { return _primaryMetadata; } }
            public List<Metadata> BackupMetadata { get { return _backupMetadata; } }
            public AndroidSuper M_Root { get { return m_root; } }
            public AndroidSuper M_Parent { get { return m_parent; } }
            public byte[] M_RawPrimaryGeometry { get { return __raw_primaryGeometry; } }
            public byte[] M_RawBackupGeometry { get { return __raw_backupGeometry; } }
            public List<byte[]> M_RawPrimaryMetadata { get { return __raw_primaryMetadata; } }
            public List<byte[]> M_RawBackupMetadata { get { return __raw_backupMetadata; } }
        }
        public partial class Geometry : KaitaiStruct
        {
            public static Geometry FromFile(string fileName)
            {
                return new Geometry(new KaitaiStream(fileName));
            }

            public Geometry(KaitaiStream p__io, AndroidSuper.Root p__parent = null, AndroidSuper p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _magic = m_io.ReadBytes(4);
                if (!((KaitaiStream.ByteArrayCompare(Magic, new byte[] { 103, 68, 108, 97 }) == 0)))
                {
                    throw new ValidationNotEqualError(new byte[] { 103, 68, 108, 97 }, Magic, M_Io, "/types/geometry/seq/0");
                }
                _structSize = m_io.ReadU4le();
                _checksum = m_io.ReadBytes(32);
                _metadataMaxSize = m_io.ReadU4le();
                _metadataSlotCount = m_io.ReadU4le();
                _logicalBlockSize = m_io.ReadU4le();
            }
            private byte[] _magic;
            private uint _structSize;
            private byte[] _checksum;
            private uint _metadataMaxSize;
            private uint _metadataSlotCount;
            private uint _logicalBlockSize;
            private AndroidSuper m_root;
            private AndroidSuper.Root m_parent;
            public byte[] Magic { get { return _magic; } }
            public uint StructSize { get { return _structSize; } }

            /// <summary>
            /// SHA-256 hash of struct_size bytes from beginning of geometry,
            /// calculated as if checksum were zeroed out
            /// </summary>
            public byte[] Checksum { get { return _checksum; } }
            public uint MetadataMaxSize { get { return _metadataMaxSize; } }
            public uint MetadataSlotCount { get { return _metadataSlotCount; } }
            public uint LogicalBlockSize { get { return _logicalBlockSize; } }
            public AndroidSuper M_Root { get { return m_root; } }
            public AndroidSuper.Root M_Parent { get { return m_parent; } }
        }
        public partial class Metadata : KaitaiStruct
        {
            public static Metadata FromFile(string fileName)
            {
                return new Metadata(new KaitaiStream(fileName));
            }


            public enum TableKind
            {
                Partitions = 0,
                Extents = 1,
                Groups = 2,
                BlockDevices = 3,
            }
            public Metadata(KaitaiStream p__io, AndroidSuper.Root p__parent = null, AndroidSuper p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _magic = m_io.ReadBytes(4);
                if (!((KaitaiStream.ByteArrayCompare(Magic, new byte[] { 48, 80, 76, 65 }) == 0)))
                {
                    throw new ValidationNotEqualError(new byte[] { 48, 80, 76, 65 }, Magic, M_Io, "/types/metadata/seq/0");
                }
                _majorVersion = m_io.ReadU2le();
                _minorVersion = m_io.ReadU2le();
                _headerSize = m_io.ReadU4le();
                _headerChecksum = m_io.ReadBytes(32);
                _tablesSize = m_io.ReadU4le();
                _tablesChecksum = m_io.ReadBytes(32);
                _partitions = new TableDescriptor(TableKind.Partitions, m_io, this, m_root);
                _extents = new TableDescriptor(TableKind.Extents, m_io, this, m_root);
                _groups = new TableDescriptor(TableKind.Groups, m_io, this, m_root);
                _blockDevices = new TableDescriptor(TableKind.BlockDevices, m_io, this, m_root);
            }
            public partial class BlockDevice : KaitaiStruct
            {
                public static BlockDevice FromFile(string fileName)
                {
                    return new BlockDevice(new KaitaiStream(fileName));
                }

                public BlockDevice(KaitaiStream p__io, AndroidSuper.Metadata.TableDescriptor p__parent = null, AndroidSuper p__root = null) : base(p__io)
                {
                    m_parent = p__parent;
                    m_root = p__root;
                    _read();
                }
                private void _read()
                {
                    _firstLogicalSector = m_io.ReadU8le();
                    _alignment = m_io.ReadU4le();
                    _alignmentOffset = m_io.ReadU4le();
                    _size = m_io.ReadU8le();
                    _partitionName = System.Text.Encoding.GetEncoding("UTF-8").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(36), 0, false));
                    _flagSlotSuffixed = m_io.ReadBitsIntLe(1) != 0;
                    _flagsReserved = m_io.ReadBitsIntLe(31);
                }
                private ulong _firstLogicalSector;
                private uint _alignment;
                private uint _alignmentOffset;
                private ulong _size;
                private string _partitionName;
                private bool _flagSlotSuffixed;
                private ulong _flagsReserved;
                private AndroidSuper m_root;
                private AndroidSuper.Metadata.TableDescriptor m_parent;
                public ulong FirstLogicalSector { get { return _firstLogicalSector; } }
                public uint Alignment { get { return _alignment; } }
                public uint AlignmentOffset { get { return _alignmentOffset; } }
                public ulong Size { get { return _size; } }
                public string PartitionName { get { return _partitionName; } }
                public bool FlagSlotSuffixed { get { return _flagSlotSuffixed; } }
                public ulong FlagsReserved { get { return _flagsReserved; } }
                public AndroidSuper M_Root { get { return m_root; } }
                public AndroidSuper.Metadata.TableDescriptor M_Parent { get { return m_parent; } }
            }
            public partial class Extent : KaitaiStruct
            {
                public static Extent FromFile(string fileName)
                {
                    return new Extent(new KaitaiStream(fileName));
                }


                public enum TargetType
                {
                    Linear = 0,
                    Zero = 1,
                }
                public Extent(KaitaiStream p__io, AndroidSuper.Metadata.TableDescriptor p__parent = null, AndroidSuper p__root = null) : base(p__io)
                {
                    m_parent = p__parent;
                    m_root = p__root;
                    _read();
                }
                private void _read()
                {
                    _numSectors = m_io.ReadU8le();
                    _targetType = ((TargetType)m_io.ReadU4le());
                    _targetData = m_io.ReadU8le();
                    _targetSource = m_io.ReadU4le();
                }
                private ulong _numSectors;
                private TargetType _targetType;
                private ulong _targetData;
                private uint _targetSource;
                private AndroidSuper m_root;
                private AndroidSuper.Metadata.TableDescriptor m_parent;
                public ulong NumSectors { get { return _numSectors; } }
                public TargetType Targettype { get { return _targetType; } }
                public ulong TargetData { get { return _targetData; } }
                public uint TargetSource { get { return _targetSource; } }
                public AndroidSuper M_Root { get { return m_root; } }
                public AndroidSuper.Metadata.TableDescriptor M_Parent { get { return m_parent; } }
            }
            public partial class TableDescriptor : KaitaiStruct
            {
                public TableDescriptor(TableKind p_kind, KaitaiStream p__io, AndroidSuper.Metadata p__parent = null, AndroidSuper p__root = null) : base(p__io)
                {
                    m_parent = p__parent;
                    m_root = p__root;
                    _kind = p_kind;
                    f_table = false;
                    _read();
                }
                private void _read()
                {
                    _offset = m_io.ReadU4le();
                    _numEntries = m_io.ReadU4le();
                    _entrySize = m_io.ReadU4le();
                }
                private bool f_table;
                private List<object> _table;
                public List<object> Table
                {
                    get
                    {
                        if (f_table)
                            return _table;
                        long _pos = m_io.Pos;
                        m_io.Seek((M_Parent.HeaderSize + Offset));
                        __raw_table = new List<byte[]>();
                        _table = new List<object>();
                        for (var i = 0; i < NumEntries; i++)
                        {
                            switch (Kind)
                            {
                                case AndroidSuper.Metadata.TableKind.Partitions:
                                    {
                                        __raw_table.Add(m_io.ReadBytes(EntrySize));
                                        var io___raw_table = new KaitaiStream(__raw_table[__raw_table.Count - 1]);
                                        _table.Add(new Partition(io___raw_table, this, m_root));
                                        break;
                                    }
                                case AndroidSuper.Metadata.TableKind.Extents:
                                    {
                                        __raw_table.Add(m_io.ReadBytes(EntrySize));
                                        var io___raw_table = new KaitaiStream(__raw_table[__raw_table.Count - 1]);
                                        _table.Add(new Extent(io___raw_table, this, m_root));
                                        break;
                                    }
                                case AndroidSuper.Metadata.TableKind.Groups:
                                    {
                                        __raw_table.Add(m_io.ReadBytes(EntrySize));
                                        var io___raw_table = new KaitaiStream(__raw_table[__raw_table.Count - 1]);
                                        _table.Add(new Group(io___raw_table, this, m_root));
                                        break;
                                    }
                                case AndroidSuper.Metadata.TableKind.BlockDevices:
                                    {
                                        __raw_table.Add(m_io.ReadBytes(EntrySize));
                                        var io___raw_table = new KaitaiStream(__raw_table[__raw_table.Count - 1]);
                                        _table.Add(new BlockDevice(io___raw_table, this, m_root));
                                        break;
                                    }
                                default:
                                    {
                                        _table.Add(m_io.ReadBytes(EntrySize));
                                        break;
                                    }
                            }
                        }
                        m_io.Seek(_pos);
                        f_table = true;
                        return _table;
                    }
                }
                private uint _offset;
                private uint _numEntries;
                private uint _entrySize;
                private TableKind _kind;
                private AndroidSuper m_root;
                private AndroidSuper.Metadata m_parent;
                private List<byte[]> __raw_table;
                public uint Offset { get { return _offset; } }
                public uint NumEntries { get { return _numEntries; } }
                public uint EntrySize { get { return _entrySize; } }
                public TableKind Kind { get { return _kind; } }
                public AndroidSuper M_Root { get { return m_root; } }
                public AndroidSuper.Metadata M_Parent { get { return m_parent; } }
                public List<byte[]> M_RawTable { get { return __raw_table; } }
            }
            public partial class Partition : KaitaiStruct
            {
                public static Partition FromFile(string fileName)
                {
                    return new Partition(new KaitaiStream(fileName));
                }

                public Partition(KaitaiStream p__io, AndroidSuper.Metadata.TableDescriptor p__parent = null, AndroidSuper p__root = null) : base(p__io)
                {
                    m_parent = p__parent;
                    m_root = p__root;
                    _read();
                }
                private void _read()
                {
                    _name = System.Text.Encoding.GetEncoding("UTF-8").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(36), 0, false));
                    _attrReadonly = m_io.ReadBitsIntLe(1) != 0;
                    _attrSlotSuffixed = m_io.ReadBitsIntLe(1) != 0;
                    _attrUpdated = m_io.ReadBitsIntLe(1) != 0;
                    _attrDisabled = m_io.ReadBitsIntLe(1) != 0;
                    _attrsReserved = m_io.ReadBitsIntLe(28);
                    m_io.AlignToByte();
                    _firstExtentIndex = m_io.ReadU4le();
                    _numExtents = m_io.ReadU4le();
                    _groupIndex = m_io.ReadU4le();
                }
                private string _name;
                private bool _attrReadonly;
                private bool _attrSlotSuffixed;
                private bool _attrUpdated;
                private bool _attrDisabled;
                private ulong _attrsReserved;
                private uint _firstExtentIndex;
                private uint _numExtents;
                private uint _groupIndex;
                private AndroidSuper m_root;
                private AndroidSuper.Metadata.TableDescriptor m_parent;
                public string Name { get { return _name; } }
                public bool AttrReadonly { get { return _attrReadonly; } }
                public bool AttrSlotSuffixed { get { return _attrSlotSuffixed; } }
                public bool AttrUpdated { get { return _attrUpdated; } }
                public bool AttrDisabled { get { return _attrDisabled; } }
                public ulong AttrsReserved { get { return _attrsReserved; } }
                public uint FirstExtentIndex { get { return _firstExtentIndex; } }
                public uint NumExtents { get { return _numExtents; } }
                public uint GroupIndex { get { return _groupIndex; } }
                public AndroidSuper M_Root { get { return m_root; } }
                public AndroidSuper.Metadata.TableDescriptor M_Parent { get { return m_parent; } }
            }
            public partial class Group : KaitaiStruct
            {
                public static Group FromFile(string fileName)
                {
                    return new Group(new KaitaiStream(fileName));
                }

                public Group(KaitaiStream p__io, AndroidSuper.Metadata.TableDescriptor p__parent = null, AndroidSuper p__root = null) : base(p__io)
                {
                    m_parent = p__parent;
                    m_root = p__root;
                    _read();
                }
                private void _read()
                {
                    _name = System.Text.Encoding.GetEncoding("UTF-8").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(36), 0, false));
                    _flagSlotSuffixed = m_io.ReadBitsIntLe(1) != 0;
                    _flagsReserved = m_io.ReadBitsIntLe(31);
                    m_io.AlignToByte();
                    _maximumSize = m_io.ReadU8le();
                }
                private string _name;
                private bool _flagSlotSuffixed;
                private ulong _flagsReserved;
                private ulong _maximumSize;
                private AndroidSuper m_root;
                private AndroidSuper.Metadata.TableDescriptor m_parent;
                public string Name { get { return _name; } }
                public bool FlagSlotSuffixed { get { return _flagSlotSuffixed; } }
                public ulong FlagsReserved { get { return _flagsReserved; } }
                public ulong MaximumSize { get { return _maximumSize; } }
                public AndroidSuper M_Root { get { return m_root; } }
                public AndroidSuper.Metadata.TableDescriptor M_Parent { get { return m_parent; } }
            }
            private byte[] _magic;
            private ushort _majorVersion;
            private ushort _minorVersion;
            private uint _headerSize;
            private byte[] _headerChecksum;
            private uint _tablesSize;
            private byte[] _tablesChecksum;
            private TableDescriptor _partitions;
            private TableDescriptor _extents;
            private TableDescriptor _groups;
            private TableDescriptor _blockDevices;
            private AndroidSuper m_root;
            private AndroidSuper.Root m_parent;
            public byte[] Magic { get { return _magic; } }
            public ushort MajorVersion { get { return _majorVersion; } }
            public ushort MinorVersion { get { return _minorVersion; } }
            public uint HeaderSize { get { return _headerSize; } }

            /// <summary>
            /// SHA-256 hash of header_size bytes from beginning of metadata,
            /// calculated as if header_checksum were zeroed out
            /// </summary>
            public byte[] HeaderChecksum { get { return _headerChecksum; } }
            public uint TablesSize { get { return _tablesSize; } }

            /// <summary>
            /// SHA-256 hash of tables_size bytes from end of header
            /// </summary>
            public byte[] TablesChecksum { get { return _tablesChecksum; } }
            public TableDescriptor Partitions { get { return _partitions; } }
            public TableDescriptor Extents { get { return _extents; } }
            public TableDescriptor Groups { get { return _groups; } }
            public TableDescriptor BlockDevices { get { return _blockDevices; } }
            public AndroidSuper M_Root { get { return m_root; } }
            public AndroidSuper.Root M_Parent { get { return m_parent; } }
        }
        private bool f_root;
        private Root _root;
        public Root root
        {
            get
            {
                if (f_root)
                    return _root;
                long _pos = m_io.Pos;
                m_io.Seek(4096);
                _root = new Root(m_io, this, m_root);
                m_io.Seek(_pos);
                f_root = true;
                return _root;
            }
        }
        private AndroidSuper m_root;
        private KaitaiStruct m_parent;
        public AndroidSuper M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
