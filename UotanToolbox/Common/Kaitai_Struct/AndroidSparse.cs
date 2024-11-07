// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using System.Collections.Generic;

namespace Kaitai
{

    /// <summary>
    /// The Android sparse format is a format to more efficiently store files
    /// for for example firmware updates to save on bandwidth. Files in sparse
    /// format first have to be converted back to their original format.
    /// 
    /// A tool to create images for testing can be found in the Android source code tree:
    /// 
    /// &lt;https://android.googlesource.com/platform/system/core/+/e8d02c50d7/libsparse&gt; - `img2simg.c`
    /// 
    /// Note: this is not the same as the Android sparse data image format.
    /// </summary>
    /// <remarks>
    /// Reference: <a href="https://android.googlesource.com/platform/system/core/+/e8d02c50d7/libsparse/sparse_format.h">Source</a>
    /// </remarks>
    /// <remarks>
    /// Reference: <a href="https://web.archive.org/web/20220322054458/https://source.android.com/devices/bootloader/images#sparse-image-format">Source</a>
    /// </remarks>
    public partial class AndroidSparse : KaitaiStruct
    {
        public static AndroidSparse FromFile(string fileName)
        {
            return new AndroidSparse(new KaitaiStream(fileName));
        }


        public enum ChunkTypes
        {
            Raw = 51905,
            Fill = 51906,
            DontCare = 51907,
            Crc32 = 51908,
        }
        public AndroidSparse(KaitaiStream p__io, KaitaiStruct p__parent = null, AndroidSparse p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            _read();
        }
        private void _read()
        {
            _headerPrefix = new FileHeaderPrefix(m_io, this, m_root);
            __raw_header = m_io.ReadBytes((HeaderPrefix.LenHeader - 10));
            var io___raw_header = new KaitaiStream(__raw_header);
            _header = new FileHeader(io___raw_header, this, m_root);
            _chunks = new List<Chunk>();
            for (var i = 0; i < Header.NumChunks; i++)
            {
                _chunks.Add(new Chunk(m_io, this, m_root));
            }
        }
        public partial class FileHeaderPrefix : KaitaiStruct
        {
            public static FileHeaderPrefix FromFile(string fileName)
            {
                return new FileHeaderPrefix(new KaitaiStream(fileName));
            }

            public FileHeaderPrefix(KaitaiStream p__io, AndroidSparse p__parent = null, AndroidSparse p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _magic = m_io.ReadBytes(4);
                if (!((KaitaiStream.ByteArrayCompare(Magic, new byte[] { 58, 255, 38, 237 }) == 0)))
                {
                    throw new ValidationNotEqualError(new byte[] { 58, 255, 38, 237 }, Magic, M_Io, "/types/file_header_prefix/seq/0");
                }
                _version = new Version(m_io, this, m_root);
                _lenHeader = m_io.ReadU2le();
            }
            private byte[] _magic;
            private Version _version;
            private ushort _lenHeader;
            private AndroidSparse m_root;
            private AndroidSparse m_parent;
            public byte[] Magic { get { return _magic; } }

            /// <summary>
            /// internal; access `_root.header.version` instead
            /// </summary>
            public Version Version { get { return _version; } }

            /// <summary>
            /// internal; access `_root.header.len_header` instead
            /// </summary>
            public ushort LenHeader { get { return _lenHeader; } }
            public AndroidSparse M_Root { get { return m_root; } }
            public AndroidSparse M_Parent { get { return m_parent; } }
        }
        public partial class FileHeader : KaitaiStruct
        {
            public static FileHeader FromFile(string fileName)
            {
                return new FileHeader(new KaitaiStream(fileName));
            }

            public FileHeader(KaitaiStream p__io, AndroidSparse p__parent = null, AndroidSparse p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_version = false;
                f_lenHeader = false;
                _read();
            }
            private void _read()
            {
                _lenChunkHeader = m_io.ReadU2le();
                _blockSize = m_io.ReadU4le();
                {
                    uint M_ = BlockSize;
                    if (!(KaitaiStream.Mod(M_, 4) == 0))
                    {
                        throw new ValidationExprError(BlockSize, M_Io, "/types/file_header/seq/1");
                    }
                }
                _numBlocks = m_io.ReadU4le();
                _numChunks = m_io.ReadU4le();
                _checksum = m_io.ReadU4le();
            }
            private bool f_version;
            private Version _version;
            public Version Version
            {
                get
                {
                    if (f_version)
                        return _version;
                    _version = (Version)(M_Root.HeaderPrefix.Version);
                    f_version = true;
                    return _version;
                }
            }
            private bool f_lenHeader;
            private ushort _lenHeader;

            /// <summary>
            /// size of file header, should be 28
            /// </summary>
            public ushort LenHeader
            {
                get
                {
                    if (f_lenHeader)
                        return _lenHeader;
                    _lenHeader = (ushort)(M_Root.HeaderPrefix.LenHeader);
                    f_lenHeader = true;
                    return _lenHeader;
                }
            }
            private ushort _lenChunkHeader;
            private uint _blockSize;
            private uint _numBlocks;
            private uint _numChunks;
            private uint _checksum;
            private AndroidSparse m_root;
            private AndroidSparse m_parent;

            /// <summary>
            /// size of chunk header, should be 12
            /// </summary>
            public ushort LenChunkHeader { get { return _lenChunkHeader; } }

            /// <summary>
            /// block size in bytes, must be a multiple of 4
            /// </summary>
            public uint BlockSize { get { return _blockSize; } }

            /// <summary>
            /// blocks in the original data
            /// </summary>
            public uint NumBlocks { get { return _numBlocks; } }
            public uint NumChunks { get { return _numChunks; } }

            /// <summary>
            /// CRC32 checksum of the original data
            /// 
            /// In practice always 0; if checksum writing is requested, a CRC32 chunk is written
            /// at the end of the file instead. The canonical `libsparse` implementation does this
            /// and other implementations tend to follow it, see
            /// &lt;https://gitlab.com/teskje/android-sparse-rs/-/blob/57c2577/src/write.rs#L112-114&gt;
            /// </summary>
            public uint Checksum { get { return _checksum; } }
            public AndroidSparse M_Root { get { return m_root; } }
            public AndroidSparse M_Parent { get { return m_parent; } }
        }
        public partial class Chunk : KaitaiStruct
        {
            public static Chunk FromFile(string fileName)
            {
                return new Chunk(new KaitaiStream(fileName));
            }

            public Chunk(KaitaiStream p__io, AndroidSparse p__parent = null, AndroidSparse p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                __raw_header = m_io.ReadBytes(M_Root.Header.LenChunkHeader);
                var io___raw_header = new KaitaiStream(__raw_header);
                _header = new ChunkHeader(io___raw_header, this, m_root);
                switch (Header.ChunkType)
                {
                    case AndroidSparse.ChunkTypes.Crc32:
                        {
                            _body = m_io.ReadU4le();
                            break;
                        }
                    default:
                        {
                            _body = m_io.ReadBytes(Header.LenBody);
                            break;
                        }
                }
            }
            public partial class ChunkHeader : KaitaiStruct
            {
                public static ChunkHeader FromFile(string fileName)
                {
                    return new ChunkHeader(new KaitaiStream(fileName));
                }

                public ChunkHeader(KaitaiStream p__io, AndroidSparse.Chunk p__parent = null, AndroidSparse p__root = null) : base(p__io)
                {
                    m_parent = p__parent;
                    m_root = p__root;
                    f_lenBody = false;
                    f_lenBodyExpected = false;
                    _read();
                }
                private void _read()
                {
                    _chunkType = ((AndroidSparse.ChunkTypes)m_io.ReadU2le());
                    _reserved1 = m_io.ReadU2le();
                    _numBodyBlocks = m_io.ReadU4le();
                    _lenChunk = m_io.ReadU4le();
                    if (!(LenChunk == (LenBodyExpected != -1 ? (M_Root.Header.LenChunkHeader + LenBodyExpected) : LenChunk)))
                    {
                        throw new ValidationNotEqualError((LenBodyExpected != -1 ? (M_Root.Header.LenChunkHeader + LenBodyExpected) : LenChunk), LenChunk, M_Io, "/types/chunk/types/chunk_header/seq/3");
                    }
                }
                private bool f_lenBody;
                private int _lenBody;
                public int LenBody
                {
                    get
                    {
                        if (f_lenBody)
                            return _lenBody;
                        _lenBody = (int)((LenChunk - M_Root.Header.LenChunkHeader));
                        f_lenBody = true;
                        return _lenBody;
                    }
                }
                private bool f_lenBodyExpected;
                private int _lenBodyExpected;

                /// <remarks>
                /// Reference: <a href="https://android.googlesource.com/platform/system/core/+/e8d02c50d7/libsparse/sparse_read.cpp#184">Source</a>
                /// </remarks>
                /// <remarks>
                /// Reference: <a href="https://android.googlesource.com/platform/system/core/+/e8d02c50d7/libsparse/sparse_read.cpp#215">Source</a>
                /// </remarks>
                /// <remarks>
                /// Reference: <a href="https://android.googlesource.com/platform/system/core/+/e8d02c50d7/libsparse/sparse_read.cpp#249">Source</a>
                /// </remarks>
                /// <remarks>
                /// Reference: <a href="https://android.googlesource.com/platform/system/core/+/e8d02c50d7/libsparse/sparse_read.cpp#270">Source</a>
                /// </remarks>
                public int LenBodyExpected
                {
                    get
                    {
                        if (f_lenBodyExpected)
                            return _lenBodyExpected;
                        _lenBodyExpected = (int)((ChunkType == AndroidSparse.ChunkTypes.Raw ? ((int)M_Root.Header.BlockSize * NumBodyBlocks) : (ChunkType == AndroidSparse.ChunkTypes.Fill ? 4 : (ChunkType == AndroidSparse.ChunkTypes.DontCare ? 0 : (ChunkType == AndroidSparse.ChunkTypes.Crc32 ? 4 : -1)))));
                        f_lenBodyExpected = true;
                        return _lenBodyExpected;
                    }
                }
                private ChunkTypes _chunkType;
                private ushort _reserved1;
                private uint _numBodyBlocks;
                private uint _lenChunk;
                private AndroidSparse m_root;
                private AndroidSparse.Chunk m_parent;
                public ChunkTypes ChunkType { get { return _chunkType; } }
                public ushort Reserved1 { get { return _reserved1; } }

                /// <summary>
                /// size of the chunk body in blocks in output image
                /// </summary>
                public uint NumBodyBlocks { get { return _numBodyBlocks; } }

                /// <summary>
                /// in bytes of chunk input file including chunk header and data
                /// </summary>
                public uint LenChunk { get { return _lenChunk; } }
                public AndroidSparse M_Root { get { return m_root; } }
                public AndroidSparse.Chunk M_Parent { get { return m_parent; } }
            }
            private ChunkHeader _header;
            private object _body;
            private AndroidSparse m_root;
            private AndroidSparse m_parent;
            private byte[] __raw_header;
            public ChunkHeader Header { get { return _header; } }
            public object Body { get { return _body; } }
            public AndroidSparse M_Root { get { return m_root; } }
            public AndroidSparse M_Parent { get { return m_parent; } }
            public byte[] M_RawHeader { get { return __raw_header; } }
        }
        public partial class Version : KaitaiStruct
        {
            public static Version FromFile(string fileName)
            {
                return new Version(new KaitaiStream(fileName));
            }

            public Version(KaitaiStream p__io, AndroidSparse.FileHeaderPrefix p__parent = null, AndroidSparse p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _major = m_io.ReadU2le();
                if (!(Major == 1))
                {
                    throw new ValidationNotEqualError(1, Major, M_Io, "/types/version/seq/0");
                }
                _minor = m_io.ReadU2le();
            }
            private ushort _major;
            private ushort _minor;
            private AndroidSparse m_root;
            private AndroidSparse.FileHeaderPrefix m_parent;
            public ushort Major { get { return _major; } }
            public ushort Minor { get { return _minor; } }
            public AndroidSparse M_Root { get { return m_root; } }
            public AndroidSparse.FileHeaderPrefix M_Parent { get { return m_parent; } }
        }
        private FileHeaderPrefix _headerPrefix;
        private FileHeader _header;
        private List<Chunk> _chunks;
        private AndroidSparse m_root;
        private KaitaiStruct m_parent;
        private byte[] __raw_header;

        /// <summary>
        /// internal; access `_root.header` instead
        /// </summary>
        public FileHeaderPrefix HeaderPrefix { get { return _headerPrefix; } }
        public FileHeader Header { get { return _header; } }
        public List<Chunk> Chunks { get { return _chunks; } }
        public AndroidSparse M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
        public byte[] M_RawHeader { get { return __raw_header; } }
    }
}
