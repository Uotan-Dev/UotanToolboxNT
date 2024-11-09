// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild



using Kaitai;

namespace UotanToolbox.Common.ROMHelper.Kaitai_Struct
{

    /// <remarks>
    /// Reference: <a href="https://source.android.com/docs/core/architecture/bootloader/boot-image-header">Source</a>
    /// </remarks>
    public partial class AndroidImg : KaitaiStruct
    {
        public static AndroidImg FromFile(string fileName)
        {
            return new AndroidImg(new KaitaiStream(fileName));
        }

        public AndroidImg(KaitaiStream p__io, KaitaiStruct p__parent = null, AndroidImg p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            f_kernelImg = false;
            f_tagsOffset = false;
            f_ramdiskOffset = false;
            f_secondOffset = false;
            f_kernelOffset = false;
            f_dtbOffset = false;
            f_dtbImg = false;
            f_ramdiskImg = false;
            f_recoveryDtboImg = false;
            f_secondImg = false;
            f_base = false;
            _read();
        }
        private void _read()
        {
            _magic = m_io.ReadBytes(8);
            if (!(KaitaiStream.ByteArrayCompare(Magic, new byte[] { 65, 78, 68, 82, 79, 73, 68, 33 }) == 0))
            {
                throw new ValidationNotEqualError(new byte[] { 65, 78, 68, 82, 79, 73, 68, 33 }, Magic, M_Io, "/seq/0");
            }
            _kernel = new Load(m_io, this, m_root);
            _ramdisk = new Load(m_io, this, m_root);
            _second = new Load(m_io, this, m_root);
            _tagsLoad = m_io.ReadU4le();
            _pageSize = m_io.ReadU4le();
            _headerVersion = m_io.ReadU4le();
            _osVersion = new OsVersion(m_io, this, m_root);
            _name = System.Text.Encoding.GetEncoding("ASCII").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(16), 0, false));
            _cmdline = System.Text.Encoding.GetEncoding("ASCII").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(512), 0, false));
            _sha = m_io.ReadBytes(32);
            _extraCmdline = System.Text.Encoding.GetEncoding("ASCII").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(1024), 0, false));
            if (HeaderVersion > 0)
            {
                _recoveryDtbo = new SizeOffset(m_io, this, m_root);
            }
            if (HeaderVersion > 0)
            {
                _bootHeaderSize = m_io.ReadU4le();
            }
            if (HeaderVersion > 1)
            {
                _dtb = new LoadLong(m_io, this, m_root);
            }
        }
        public partial class Load : KaitaiStruct
        {
            public static Load FromFile(string fileName)
            {
                return new Load(new KaitaiStream(fileName));
            }

            public Load(KaitaiStream p__io, AndroidImg p__parent = null, AndroidImg p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _size = m_io.ReadU4le();
                _addr = m_io.ReadU4le();
            }
            private uint _size;
            private uint _addr;
            private AndroidImg m_root;
            private AndroidImg m_parent;
            public uint Size { get { return _size; } }
            public uint Addr { get { return _addr; } }
            public AndroidImg M_Root { get { return m_root; } }
            public AndroidImg M_Parent { get { return m_parent; } }
        }
        public partial class LoadLong : KaitaiStruct
        {
            public static LoadLong FromFile(string fileName)
            {
                return new LoadLong(new KaitaiStream(fileName));
            }

            public LoadLong(KaitaiStream p__io, AndroidImg p__parent = null, AndroidImg p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _size = m_io.ReadU4le();
                _addr = m_io.ReadU8le();
            }
            private uint _size;
            private ulong _addr;
            private AndroidImg m_root;
            private AndroidImg m_parent;
            public uint Size { get { return _size; } }
            public ulong Addr { get { return _addr; } }
            public AndroidImg M_Root { get { return m_root; } }
            public AndroidImg M_Parent { get { return m_parent; } }
        }
        public partial class SizeOffset : KaitaiStruct
        {
            public static SizeOffset FromFile(string fileName)
            {
                return new SizeOffset(new KaitaiStream(fileName));
            }

            public SizeOffset(KaitaiStream p__io, AndroidImg p__parent = null, AndroidImg p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _size = m_io.ReadU4le();
                _offset = m_io.ReadU8le();
            }
            private uint _size;
            private ulong _offset;
            private AndroidImg m_root;
            private AndroidImg m_parent;
            public uint Size { get { return _size; } }
            public ulong Offset { get { return _offset; } }
            public AndroidImg M_Root { get { return m_root; } }
            public AndroidImg M_Parent { get { return m_parent; } }
        }
        public partial class OsVersion : KaitaiStruct
        {
            public static OsVersion FromFile(string fileName)
            {
                return new OsVersion(new KaitaiStream(fileName));
            }

            public OsVersion(KaitaiStream p__io, AndroidImg p__parent = null, AndroidImg p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_month = false;
                f_patch = false;
                f_year = false;
                f_major = false;
                f_minor = false;
                _read();
            }
            private void _read()
            {
                _version = m_io.ReadU4le();
            }
            private bool f_month;
            private int _month;
            public int Month
            {
                get
                {
                    if (f_month)
                        return _month;
                    _month = (int)(Version & 15);
                    f_month = true;
                    return _month;
                }
            }
            private bool f_patch;
            private int _patch;
            public int Patch
            {
                get
                {
                    if (f_patch)
                        return _patch;
                    _patch = (int)(Version >> 11 & 127);
                    f_patch = true;
                    return _patch;
                }
            }
            private bool f_year;
            private int _year;
            public int Year
            {
                get
                {
                    if (f_year)
                        return _year;
                    _year = (int)((Version >> 4 & 127) + 2000);
                    f_year = true;
                    return _year;
                }
            }
            private bool f_major;
            private int _major;
            public int Major
            {
                get
                {
                    if (f_major)
                        return _major;
                    _major = (int)(Version >> 25 & 127);
                    f_major = true;
                    return _major;
                }
            }
            private bool f_minor;
            private int _minor;
            public int Minor
            {
                get
                {
                    if (f_minor)
                        return _minor;
                    _minor = (int)(Version >> 18 & 127);
                    f_minor = true;
                    return _minor;
                }
            }
            private uint _version;
            private AndroidImg m_root;
            private AndroidImg m_parent;
            public uint Version { get { return _version; } }
            public AndroidImg M_Root { get { return m_root; } }
            public AndroidImg M_Parent { get { return m_parent; } }
        }
        private bool f_kernelImg;
        private byte[] _kernelImg;
        public byte[] KernelImg
        {
            get
            {
                if (f_kernelImg)
                    return _kernelImg;
                long _pos = m_io.Pos;
                m_io.Seek(PageSize);
                _kernelImg = m_io.ReadBytes(Kernel.Size);
                m_io.Seek(_pos);
                f_kernelImg = true;
                return _kernelImg;
            }
        }
        private bool f_tagsOffset;
        private int _tagsOffset;

        /// <summary>
        /// tags offset from base
        /// </summary>
        public int TagsOffset
        {
            get
            {
                if (f_tagsOffset)
                    return _tagsOffset;
                _tagsOffset = (int)(TagsLoad - Base);
                f_tagsOffset = true;
                return _tagsOffset;
            }
        }
        private bool f_ramdiskOffset;
        private int _ramdiskOffset;

        /// <summary>
        /// ramdisk offset from base
        /// </summary>
        public int RamdiskOffset
        {
            get
            {
                if (f_ramdiskOffset)
                    return _ramdiskOffset;
                _ramdiskOffset = (int)(Ramdisk.Addr > 0 ? Ramdisk.Addr - Base : 0);
                f_ramdiskOffset = true;
                return _ramdiskOffset;
            }
        }
        private bool f_secondOffset;
        private int _secondOffset;

        /// <summary>
        /// 2nd bootloader offset from base
        /// </summary>
        public int SecondOffset
        {
            get
            {
                if (f_secondOffset)
                    return _secondOffset;
                _secondOffset = (int)(Second.Addr > 0 ? Second.Addr - Base : 0);
                f_secondOffset = true;
                return _secondOffset;
            }
        }
        private bool f_kernelOffset;
        private int _kernelOffset;

        /// <summary>
        /// kernel offset from base
        /// </summary>
        public int KernelOffset
        {
            get
            {
                if (f_kernelOffset)
                    return _kernelOffset;
                _kernelOffset = (int)(Kernel.Addr - Base);
                f_kernelOffset = true;
                return _kernelOffset;
            }
        }
        private bool f_dtbOffset;
        private int? _dtbOffset;

        /// <summary>
        /// dtb offset from base
        /// </summary>
        public int? DtbOffset
        {
            get
            {
                if (f_dtbOffset)
                    return _dtbOffset;
                if (HeaderVersion > 1)
                {
                    _dtbOffset = (int)(Dtb.Addr > 0 ? Dtb.Addr - (ulong)Base : 0);
                }
                f_dtbOffset = true;
                return _dtbOffset;
            }
        }
        private bool f_dtbImg;
        private byte[] _dtbImg;
        public byte[] DtbImg
        {
            get
            {
                if (f_dtbImg)
                    return _dtbImg;
                if (HeaderVersion > 1 && Dtb.Size > 0)
                {
                    long _pos = m_io.Pos;
                    m_io.Seek((PageSize + Kernel.Size + Ramdisk.Size + Second.Size + RecoveryDtbo.Size + PageSize - 1) / PageSize * PageSize);
                    _dtbImg = m_io.ReadBytes(Dtb.Size);
                    m_io.Seek(_pos);
                    f_dtbImg = true;
                }
                return _dtbImg;
            }
        }
        private bool f_ramdiskImg;
        private byte[] _ramdiskImg;
        public byte[] RamdiskImg
        {
            get
            {
                if (f_ramdiskImg)
                    return _ramdiskImg;
                if (Ramdisk.Size > 0)
                {
                    long _pos = m_io.Pos;
                    m_io.Seek((PageSize + Kernel.Size + PageSize - 1) / PageSize * PageSize);
                    _ramdiskImg = m_io.ReadBytes(Ramdisk.Size);
                    m_io.Seek(_pos);
                    f_ramdiskImg = true;
                }
                return _ramdiskImg;
            }
        }
        private bool f_recoveryDtboImg;
        private byte[] _recoveryDtboImg;
        public byte[] RecoveryDtboImg
        {
            get
            {
                if (f_recoveryDtboImg)
                    return _recoveryDtboImg;
                if (HeaderVersion > 0 && RecoveryDtbo.Size > 0)
                {
                    long _pos = m_io.Pos;
                    m_io.Seek((long)RecoveryDtbo.Offset);
                    _recoveryDtboImg = m_io.ReadBytes(RecoveryDtbo.Size);
                    m_io.Seek(_pos);
                    f_recoveryDtboImg = true;
                }
                return _recoveryDtboImg;
            }
        }
        private bool f_secondImg;
        private byte[] _secondImg;
        public byte[] SecondImg
        {
            get
            {
                if (f_secondImg)
                    return _secondImg;
                if (Second.Size > 0)
                {
                    long _pos = m_io.Pos;
                    m_io.Seek((PageSize + Kernel.Size + Ramdisk.Size + PageSize - 1) / PageSize * PageSize);
                    _secondImg = m_io.ReadBytes(Second.Size);
                    m_io.Seek(_pos);
                    f_secondImg = true;
                }
                return _secondImg;
            }
        }
        private bool f_base;
        private int _base;

        /// <summary>
        /// base loading address
        /// </summary>
        public int Base
        {
            get
            {
                if (f_base)
                    return _base;
                _base = (int)(Kernel.Addr - 32768);
                f_base = true;
                return _base;
            }
        }
        private byte[] _magic;
        private Load _kernel;
        private Load _ramdisk;
        private Load _second;
        private uint _tagsLoad;
        private uint _pageSize;
        private uint _headerVersion;
        private OsVersion _osVersion;
        private string _name;
        private string _cmdline;
        private byte[] _sha;
        private string _extraCmdline;
        private SizeOffset _recoveryDtbo;
        private uint? _bootHeaderSize;
        private LoadLong _dtb;
        private AndroidImg m_root;
        private KaitaiStruct m_parent;
        public byte[] Magic { get { return _magic; } }
        public Load Kernel { get { return _kernel; } }
        public Load Ramdisk { get { return _ramdisk; } }
        public Load Second { get { return _second; } }
        public uint TagsLoad { get { return _tagsLoad; } }
        public uint PageSize { get { return _pageSize; } }
        public uint HeaderVersion { get { return _headerVersion; } }
        public OsVersion Osversion { get { return _osVersion; } }
        public string Name { get { return _name; } }
        public string Cmdline { get { return _cmdline; } }
        public byte[] Sha { get { return _sha; } }
        public string ExtraCmdline { get { return _extraCmdline; } }
        public SizeOffset RecoveryDtbo { get { return _recoveryDtbo; } }
        public uint? BootHeaderSize { get { return _bootHeaderSize; } }
        public LoadLong Dtb { get { return _dtb; } }
        public AndroidImg M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
