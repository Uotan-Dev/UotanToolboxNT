// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using Kaitai;
using System.Collections.Generic;

namespace UotanToolbox.Common.ROMHelper.Kaitai_Struct
{
    public partial class CpioOldLe : KaitaiStruct
    {
        public static CpioOldLe FromFile(string fileName)
        {
            return new CpioOldLe(new KaitaiStream(fileName));
        }

        public CpioOldLe(KaitaiStream p__io, KaitaiStruct p__parent = null, CpioOldLe p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            _read();
        }
        private void _read()
        {
            _files = new List<File>();
            {
                var i = 0;
                while (!m_io.IsEof)
                {
                    _files.Add(new File(m_io, this, m_root));
                    i++;
                }
            }
        }
        public partial class File : KaitaiStruct
        {
            public static File FromFile(string fileName)
            {
                return new File(new KaitaiStream(fileName));
            }

            public File(KaitaiStream p__io, CpioOldLe p__parent = null, CpioOldLe p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _header = new FileHeader(m_io, this, m_root);
                _pathName = m_io.ReadBytes(Header.PathNameSize - 1);
                _stringTerminator = m_io.ReadBytes(1);
                if (!(KaitaiStream.ByteArrayCompare(StringTerminator, new byte[] { 0 }) == 0))
                {
                    throw new ValidationNotEqualError(new byte[] { 0 }, StringTerminator, M_Io, "/types/file/seq/2");
                }
                if (KaitaiStream.Mod(Header.PathNameSize, 2) == 1)
                {
                    _pathNamePadding = m_io.ReadBytes(1);
                    if (!(KaitaiStream.ByteArrayCompare(PathNamePadding, new byte[] { 0 }) == 0))
                    {
                        throw new ValidationNotEqualError(new byte[] { 0 }, PathNamePadding, M_Io, "/types/file/seq/3");
                    }
                }
                _fileData = m_io.ReadBytes(Header.FileSize.Value);
                if (KaitaiStream.Mod(Header.FileSize.Value, 2) == 1)
                {
                    _fileDataPadding = m_io.ReadBytes(1);
                    if (!(KaitaiStream.ByteArrayCompare(FileDataPadding, new byte[] { 0 }) == 0))
                    {
                        throw new ValidationNotEqualError(new byte[] { 0 }, FileDataPadding, M_Io, "/types/file/seq/5");
                    }
                }
                if (KaitaiStream.ByteArrayCompare(PathName, new byte[] { 84, 82, 65, 73, 76, 69, 82, 33, 33, 33 }) == 0 && Header.FileSize.Value == 0)
                {
                    _endOfFilePadding = m_io.ReadBytesFull();
                }
            }
            private FileHeader _header;
            private byte[] _pathName;
            private byte[] _stringTerminator;
            private byte[] _pathNamePadding;
            private byte[] _fileData;
            private byte[] _fileDataPadding;
            private byte[] _endOfFilePadding;
            private CpioOldLe m_root;
            private CpioOldLe m_parent;
            public FileHeader Header { get { return _header; } }
            public byte[] PathName { get { return _pathName; } }
            public byte[] StringTerminator { get { return _stringTerminator; } }
            public byte[] PathNamePadding { get { return _pathNamePadding; } }
            public byte[] FileData { get { return _fileData; } }
            public byte[] FileDataPadding { get { return _fileDataPadding; } }
            public byte[] EndOfFilePadding { get { return _endOfFilePadding; } }
            public CpioOldLe M_Root { get { return m_root; } }
            public CpioOldLe M_Parent { get { return m_parent; } }
        }
        public partial class FileHeader : KaitaiStruct
        {
            public static FileHeader FromFile(string fileName)
            {
                return new FileHeader(new KaitaiStream(fileName));
            }

            public FileHeader(KaitaiStream p__io, File p__parent = null, CpioOldLe p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _magic = m_io.ReadBytes(2);
                if (!(KaitaiStream.ByteArrayCompare(Magic, new byte[] { 199, 113 }) == 0))
                {
                    throw new ValidationNotEqualError(new byte[] { 199, 113 }, Magic, M_Io, "/types/file_header/seq/0");
                }
                _deviceNumber = m_io.ReadU2le();
                _inodeNumber = m_io.ReadU2le();
                _mode = m_io.ReadU2le();
                _userId = m_io.ReadU2le();
                _groupId = m_io.ReadU2le();
                _numberOfLinks = m_io.ReadU2le();
                _rDeviceNumber = m_io.ReadU2le();
                _modificationTime = new FourByteUnsignedInteger(m_io, this, m_root);
                _pathNameSize = m_io.ReadU2le();
                _fileSize = new FourByteUnsignedInteger(m_io, this, m_root);
            }
            private byte[] _magic;
            private ushort _deviceNumber;
            private ushort _inodeNumber;
            private ushort _mode;
            private ushort _userId;
            private ushort _groupId;
            private ushort _numberOfLinks;
            private ushort _rDeviceNumber;
            private FourByteUnsignedInteger _modificationTime;
            private ushort _pathNameSize;
            private FourByteUnsignedInteger _fileSize;
            private CpioOldLe m_root;
            private File m_parent;
            public byte[] Magic { get { return _magic; } }
            public ushort DeviceNumber { get { return _deviceNumber; } }
            public ushort InodeNumber { get { return _inodeNumber; } }
            public ushort Mode { get { return _mode; } }
            public ushort UserId { get { return _userId; } }
            public ushort GroupId { get { return _groupId; } }
            public ushort NumberOfLinks { get { return _numberOfLinks; } }
            public ushort RDeviceNumber { get { return _rDeviceNumber; } }
            public FourByteUnsignedInteger ModificationTime { get { return _modificationTime; } }
            public ushort PathNameSize { get { return _pathNameSize; } }
            public FourByteUnsignedInteger FileSize { get { return _fileSize; } }
            public CpioOldLe M_Root { get { return m_root; } }
            public File M_Parent { get { return m_parent; } }
        }
        public partial class FourByteUnsignedInteger : KaitaiStruct
        {
            public static FourByteUnsignedInteger FromFile(string fileName)
            {
                return new FourByteUnsignedInteger(new KaitaiStream(fileName));
            }

            public FourByteUnsignedInteger(KaitaiStream p__io, FileHeader p__parent = null, CpioOldLe p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_value = false;
                _read();
            }
            private void _read()
            {
                _mostSignificantBits = m_io.ReadU2le();
                _leastSignificantBits = m_io.ReadU2le();
            }
            private bool f_value;
            private int _value;
            public int Value
            {
                get
                {
                    if (f_value)
                        return _value;
                    _value = LeastSignificantBits + (MostSignificantBits << 16);
                    f_value = true;
                    return _value;
                }
            }
            private ushort _mostSignificantBits;
            private ushort _leastSignificantBits;
            private CpioOldLe m_root;
            private FileHeader m_parent;
            public ushort MostSignificantBits { get { return _mostSignificantBits; } }
            public ushort LeastSignificantBits { get { return _leastSignificantBits; } }
            public CpioOldLe M_Root { get { return m_root; } }
            public FileHeader M_Parent { get { return m_parent; } }
        }
        private List<File> _files;
        private CpioOldLe m_root;
        private KaitaiStruct m_parent;
        public List<File> Files { get { return _files; } }
        public CpioOldLe M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
