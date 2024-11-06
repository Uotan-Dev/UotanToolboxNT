// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using System.Collections.Generic;

namespace Kaitai
{

    /// <summary>
    /// Format of `bootloader-*.img` files found in factory images of certain Android devices from Huawei:
    /// 
    /// * Nexus 6P &quot;angler&quot;: [sample][sample-angler] ([other samples][others-angler]),
    ///   [releasetools.py](https://android.googlesource.com/device/huawei/angler/+/cf92cd8/releasetools.py#29)
    /// 
    /// [sample-angler]: https://androidfilehost.com/?fid=11410963190603870158 &quot;bootloader-angler-angler-03.84.img&quot;
    /// [others-angler]: https://androidfilehost.com/?w=search&amp;s=bootloader-angler&amp;type=files
    /// 
    /// All image versions can be found in factory images at
    /// &lt;https://developers.google.com/android/images&gt; for the specific device. To
    /// avoid having to download an entire ZIP archive when you only need one file
    /// from it, install [remotezip](https://github.com/gtsystem/python-remotezip) and
    /// use its [command line
    /// tool](https://github.com/gtsystem/python-remotezip#command-line-tool) to list
    /// members in the archive and then to download only the file you want.
    /// </summary>
    /// <remarks>
    /// Reference: <a href="https://android.googlesource.com/device/huawei/angler/+/673cfb9/releasetools.py">Source</a>
    /// </remarks>
    /// <remarks>
    /// Reference: <a href="https://source.codeaurora.org/quic/la/device/qcom/common/tree/meta_image/meta_format.h?h=LA.UM.6.1.1&amp;id=a68d284aee85">Source</a>
    /// </remarks>
    /// <remarks>
    /// Reference: <a href="https://source.codeaurora.org/quic/la/device/qcom/common/tree/meta_image/meta_image.c?h=LA.UM.6.1.1&amp;id=a68d284aee85">Source</a>
    /// </remarks>
    public partial class AndroidBootldrHuawei : KaitaiStruct
    {
        public static AndroidBootldrHuawei FromFile(string fileName)
        {
            return new AndroidBootldrHuawei(new KaitaiStream(fileName));
        }

        public AndroidBootldrHuawei(KaitaiStream p__io, KaitaiStruct p__parent = null, AndroidBootldrHuawei p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            _read();
        }
        private void _read()
        {
            _metaHeader = new MetaHdr(m_io, this, m_root);
            _headerExt = m_io.ReadBytes((MetaHeader.LenMetaHeader - 76));
            __raw_imageHeader = m_io.ReadBytes(MetaHeader.LenImageHeader);
            var io___raw_imageHeader = new KaitaiStream(__raw_imageHeader);
            _imageHeader = new ImageHdr(io___raw_imageHeader, this, m_root);
        }
        public partial class MetaHdr : KaitaiStruct
        {
            public static MetaHdr FromFile(string fileName)
            {
                return new MetaHdr(new KaitaiStream(fileName));
            }

            public MetaHdr(KaitaiStream p__io, AndroidBootldrHuawei p__parent = null, AndroidBootldrHuawei p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _magic = m_io.ReadBytes(4);
                if (!((KaitaiStream.ByteArrayCompare(Magic, new byte[] { 60, 214, 26, 206 }) == 0)))
                {
                    throw new ValidationNotEqualError(new byte[] { 60, 214, 26, 206 }, Magic, M_Io, "/types/meta_hdr/seq/0");
                }
                _version = new Version(m_io, this, m_root);
                _imageVersion = System.Text.Encoding.GetEncoding("ASCII").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(64), 0, false));
                _lenMetaHeader = m_io.ReadU2le();
                _lenImageHeader = m_io.ReadU2le();
            }
            private byte[] _magic;
            private Version _version;
            private string _imageVersion;
            private ushort _lenMetaHeader;
            private ushort _lenImageHeader;
            private AndroidBootldrHuawei m_root;
            private AndroidBootldrHuawei m_parent;
            public byte[] Magic { get { return _magic; } }
            public Version Version { get { return _version; } }
            public string ImageVersion { get { return _imageVersion; } }
            public ushort LenMetaHeader { get { return _lenMetaHeader; } }
            public ushort LenImageHeader { get { return _lenImageHeader; } }
            public AndroidBootldrHuawei M_Root { get { return m_root; } }
            public AndroidBootldrHuawei M_Parent { get { return m_parent; } }
        }
        public partial class Version : KaitaiStruct
        {
            public static Version FromFile(string fileName)
            {
                return new Version(new KaitaiStream(fileName));
            }

            public Version(KaitaiStream p__io, AndroidBootldrHuawei.MetaHdr p__parent = null, AndroidBootldrHuawei p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _major = m_io.ReadU2le();
                _minor = m_io.ReadU2le();
            }
            private ushort _major;
            private ushort _minor;
            private AndroidBootldrHuawei m_root;
            private AndroidBootldrHuawei.MetaHdr m_parent;
            public ushort Major { get { return _major; } }
            public ushort Minor { get { return _minor; } }
            public AndroidBootldrHuawei M_Root { get { return m_root; } }
            public AndroidBootldrHuawei.MetaHdr M_Parent { get { return m_parent; } }
        }
        public partial class ImageHdr : KaitaiStruct
        {
            public static ImageHdr FromFile(string fileName)
            {
                return new ImageHdr(new KaitaiStream(fileName));
            }

            public ImageHdr(KaitaiStream p__io, AndroidBootldrHuawei p__parent = null, AndroidBootldrHuawei p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _entries = new List<ImageHdrEntry>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _entries.Add(new ImageHdrEntry(m_io, this, m_root));
                        i++;
                    }
                }
            }
            private List<ImageHdrEntry> _entries;
            private AndroidBootldrHuawei m_root;
            private AndroidBootldrHuawei m_parent;

            /// <summary>
            /// The C generator program defines `img_header` as a [fixed size
            /// array](https://source.codeaurora.org/quic/la/device/qcom/common/tree/meta_image/meta_image.c?h=LA.UM.6.1.1&amp;id=a68d284aee85#n42)
            /// of `img_header_entry_t` structs with length `MAX_IMAGES` (which is
            /// defined as `16`).
            /// 
            /// This means that technically there will always be 16 `image_hdr`
            /// entries, the first *n* entries being used (filled with real values)
            /// and the rest left unused with all bytes zero.
            /// 
            /// To check if an entry is used, use the `is_used` attribute.
            /// </summary>
            public List<ImageHdrEntry> Entries { get { return _entries; } }
            public AndroidBootldrHuawei M_Root { get { return m_root; } }
            public AndroidBootldrHuawei M_Parent { get { return m_parent; } }
        }
        public partial class ImageHdrEntry : KaitaiStruct
        {
            public static ImageHdrEntry FromFile(string fileName)
            {
                return new ImageHdrEntry(new KaitaiStream(fileName));
            }

            public ImageHdrEntry(KaitaiStream p__io, AndroidBootldrHuawei.ImageHdr p__parent = null, AndroidBootldrHuawei p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_isUsed = false;
                f_body = false;
                _read();
            }
            private void _read()
            {
                _name = System.Text.Encoding.GetEncoding("ASCII").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(72), 0, false));
                _ofsBody = m_io.ReadU4le();
                _lenBody = m_io.ReadU4le();
            }
            private bool f_isUsed;
            private bool _isUsed;

            /// <remarks>
            /// Reference: <a href="https://source.codeaurora.org/quic/la/device/qcom/common/tree/meta_image/meta_image.c?h=LA.UM.6.1.1&amp;id=a68d284aee85#n119">Source</a>
            /// </remarks>
            public bool IsUsed
            {
                get
                {
                    if (f_isUsed)
                        return _isUsed;
                    _isUsed = (bool) ( ((OfsBody != 0) && (LenBody != 0)) );
                    f_isUsed = true;
                    return _isUsed;
                }
            }
            private bool f_body;
            private byte[] _body;
            public byte[] Body
            {
                get
                {
                    if (f_body)
                        return _body;
                    if (IsUsed) {
                        KaitaiStream io = M_Root.M_Io;
                        long _pos = io.Pos;
                        io.Seek(OfsBody);
                        _body = io.ReadBytes(LenBody);
                        io.Seek(_pos);
                        f_body = true;
                    }
                    return _body;
                }
            }
            private string _name;
            private uint _ofsBody;
            private uint _lenBody;
            private AndroidBootldrHuawei m_root;
            private AndroidBootldrHuawei.ImageHdr m_parent;

            /// <summary>
            /// partition name
            /// </summary>
            public string Name { get { return _name; } }
            public uint OfsBody { get { return _ofsBody; } }
            public uint LenBody { get { return _lenBody; } }
            public AndroidBootldrHuawei M_Root { get { return m_root; } }
            public AndroidBootldrHuawei.ImageHdr M_Parent { get { return m_parent; } }
        }
        private MetaHdr _metaHeader;
        private byte[] _headerExt;
        private ImageHdr _imageHeader;
        private AndroidBootldrHuawei m_root;
        private KaitaiStruct m_parent;
        private byte[] __raw_imageHeader;
        public MetaHdr MetaHeader { get { return _metaHeader; } }
        public byte[] HeaderExt { get { return _headerExt; } }
        public ImageHdr ImageHeader { get { return _imageHeader; } }
        public AndroidBootldrHuawei M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
        public byte[] M_RawImageHeader { get { return __raw_imageHeader; } }
    }
}
