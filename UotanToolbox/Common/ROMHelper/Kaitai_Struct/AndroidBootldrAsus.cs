// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using Kaitai;
using System.Collections.Generic;

namespace UotanToolbox.Common.ROMHelper.Kaitai_Struct
{

    /// <summary>
    /// A bootloader image which only seems to have been used on a few ASUS
    /// devices. The encoding is ASCII, because the `releasetools.py` script
    /// is written using Python 2, where the default encoding is ASCII.
    /// 
    /// A test file can be found in the firmware files for the &quot;fugu&quot; device,
    /// which can be downloaded from &lt;https://developers.google.com/android/images&gt;
    /// </summary>
    /// <remarks>
    /// Reference: <a href="https://android.googlesource.com/device/asus/fugu/+/android-8.1.0_r5/releasetools.py">Source</a>
    /// </remarks>
    public partial class AndroidBootldrAsus : KaitaiStruct
    {
        public static AndroidBootldrAsus FromFile(string fileName)
        {
            return new AndroidBootldrAsus(new KaitaiStream(fileName));
        }

        public AndroidBootldrAsus(KaitaiStream p__io, KaitaiStruct p__parent = null, AndroidBootldrAsus p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            _read();
        }
        private void _read()
        {
            _magic = m_io.ReadBytes(8);
            if (!(KaitaiStream.ByteArrayCompare(Magic, new byte[] { 66, 79, 79, 84, 76, 68, 82, 33 }) == 0))
            {
                throw new ValidationNotEqualError(new byte[] { 66, 79, 79, 84, 76, 68, 82, 33 }, Magic, M_Io, "/seq/0");
            }
            _revision = m_io.ReadU2le();
            if (!(Revision >= 2))
            {
                throw new ValidationLessThanError(2, Revision, M_Io, "/seq/1");
            }
            _reserved1 = m_io.ReadU2le();
            _reserved2 = m_io.ReadU4le();
            _images = new List<Image>();
            for (var i = 0; i < 3; i++)
            {
                _images.Add(new Image(m_io, this, m_root));
            }
        }
        public partial class Image : KaitaiStruct
        {
            public static Image FromFile(string fileName)
            {
                return new Image(new KaitaiStream(fileName));
            }

            public Image(KaitaiStream p__io, AndroidBootldrAsus p__parent = null, AndroidBootldrAsus p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_fileName = false;
                _read();
            }
            private void _read()
            {
                _chunkId = System.Text.Encoding.GetEncoding("ASCII").GetString(m_io.ReadBytes(8));
                if (!(ChunkId == "IFWI!!!!" || ChunkId == "DROIDBT!" || ChunkId == "SPLASHS!"))
                {
                    throw new ValidationNotAnyOfError(ChunkId, M_Io, "/types/image/seq/0");
                }
                _lenBody = m_io.ReadU4le();
                _flags = m_io.ReadU1();
                {
                    byte M_ = Flags;
                    if (!((M_ & 1) != 0))
                    {
                        throw new ValidationExprError(Flags, M_Io, "/types/image/seq/2");
                    }
                }
                _reserved1 = m_io.ReadU1();
                _reserved2 = m_io.ReadU1();
                _reserved3 = m_io.ReadU1();
                _body = m_io.ReadBytes(LenBody);
            }
            private bool f_fileName;
            private string _fileName;
            public string FileName
            {
                get
                {
                    if (f_fileName)
                        return _fileName;
                    _fileName = ChunkId == "IFWI!!!!" ? "ifwi.bin" : ChunkId == "DROIDBT!" ? "droidboot.img" : ChunkId == "SPLASHS!" ? "splashscreen.img" : "";
                    f_fileName = true;
                    return _fileName;
                }
            }
            private string _chunkId;
            private uint _lenBody;
            private byte _flags;
            private byte _reserved1;
            private byte _reserved2;
            private byte _reserved3;
            private byte[] _body;
            private AndroidBootldrAsus m_root;
            private AndroidBootldrAsus m_parent;
            public string ChunkId { get { return _chunkId; } }
            public uint LenBody { get { return _lenBody; } }
            public byte Flags { get { return _flags; } }
            public byte Reserved1 { get { return _reserved1; } }
            public byte Reserved2 { get { return _reserved2; } }
            public byte Reserved3 { get { return _reserved3; } }
            public byte[] Body { get { return _body; } }
            public AndroidBootldrAsus M_Root { get { return m_root; } }
            public AndroidBootldrAsus M_Parent { get { return m_parent; } }
        }
        private byte[] _magic;
        private ushort _revision;
        private ushort _reserved1;
        private uint _reserved2;
        private List<Image> _images;
        private AndroidBootldrAsus m_root;
        private KaitaiStruct m_parent;
        public byte[] Magic { get { return _magic; } }
        public ushort Revision { get { return _revision; } }
        public ushort Reserved1 { get { return _reserved1; } }
        public uint Reserved2 { get { return _reserved2; } }

        /// <summary>
        /// Only three images are included: `ifwi.bin`, `droidboot.img`
        /// and `splashscreen.img`
        /// </summary>
        public List<Image> Images { get { return _images; } }
        public AndroidBootldrAsus M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
