// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using System.Collections.Generic;
using Kaitai;

namespace UotanToolbox.Common.ROMHelper.Kaitai_Struct
{

    /// <summary>
    /// Android OS applications executables are typically stored in its own
    /// format, optimized for more efficient execution in Dalvik virtual
    /// machine.
    /// 
    /// This format is loosely similar to Java .class file format and
    /// generally holds the similar set of data: i.e. classes, methods,
    /// fields, annotations, etc.
    /// </summary>
    /// <remarks>
    /// Reference: <a href="https://source.android.com/docs/core/runtime/dex-format">Source</a>
    /// </remarks>
    public partial class Dex : KaitaiStruct
    {
        public static Dex FromFile(string fileName)
        {
            return new Dex(new KaitaiStream(fileName));
        }


        public enum ClassAccessFlags
        {
            Public = 1,
            Private = 2,
            Protected = 4,
            Static = 8,
            Final = 16,
            Interface = 512,
            Abstract = 1024,
            Synthetic = 4096,
            Annotation = 8192,
            Enum = 16384,
        }
        public Dex(KaitaiStream p__io, KaitaiStruct p__parent = null, Dex p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            f_stringIds = false;
            f_methodIds = false;
            f_linkData = false;
            f_map = false;
            f_classDefs = false;
            f_data = false;
            f_typeIds = false;
            f_protoIds = false;
            f_fieldIds = false;
            _read();
        }
        private void _read()
        {
            _header = new HeaderItem(m_io, this, m_root);
        }
        public partial class HeaderItem : KaitaiStruct
        {
            public static HeaderItem FromFile(string fileName)
            {
                return new HeaderItem(new KaitaiStream(fileName));
            }


            public enum EndianConstant
            {
                EndianConstant = 305419896,
                ReverseEndianConstant = 2018915346,
            }
            public HeaderItem(KaitaiStream p__io, Dex p__parent = null, Dex p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _magic = m_io.ReadBytes(4);
                if (!(KaitaiStream.ByteArrayCompare(Magic, new byte[] { 100, 101, 120, 10 }) == 0))
                {
                    throw new ValidationNotEqualError(new byte[] { 100, 101, 120, 10 }, Magic, M_Io, "/types/header_item/seq/0");
                }
                _versionStr = System.Text.Encoding.GetEncoding("ascii").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(4), 0, false));
                _checksum = m_io.ReadU4le();
                _signature = m_io.ReadBytes(20);
                _fileSize = m_io.ReadU4le();
                _headerSize = m_io.ReadU4le();
                _endianTag = (EndianConstant)m_io.ReadU4le();
                _linkSize = m_io.ReadU4le();
                _linkOff = m_io.ReadU4le();
                _mapOff = m_io.ReadU4le();
                _stringIdsSize = m_io.ReadU4le();
                _stringIdsOff = m_io.ReadU4le();
                _typeIdsSize = m_io.ReadU4le();
                _typeIdsOff = m_io.ReadU4le();
                _protoIdsSize = m_io.ReadU4le();
                _protoIdsOff = m_io.ReadU4le();
                _fieldIdsSize = m_io.ReadU4le();
                _fieldIdsOff = m_io.ReadU4le();
                _methodIdsSize = m_io.ReadU4le();
                _methodIdsOff = m_io.ReadU4le();
                _classDefsSize = m_io.ReadU4le();
                _classDefsOff = m_io.ReadU4le();
                _dataSize = m_io.ReadU4le();
                _dataOff = m_io.ReadU4le();
            }
            private byte[] _magic;
            private string _versionStr;
            private uint _checksum;
            private byte[] _signature;
            private uint _fileSize;
            private uint _headerSize;
            private EndianConstant _endianTag;
            private uint _linkSize;
            private uint _linkOff;
            private uint _mapOff;
            private uint _stringIdsSize;
            private uint _stringIdsOff;
            private uint _typeIdsSize;
            private uint _typeIdsOff;
            private uint _protoIdsSize;
            private uint _protoIdsOff;
            private uint _fieldIdsSize;
            private uint _fieldIdsOff;
            private uint _methodIdsSize;
            private uint _methodIdsOff;
            private uint _classDefsSize;
            private uint _classDefsOff;
            private uint _dataSize;
            private uint _dataOff;
            private Dex m_root;
            private Dex m_parent;
            public byte[] Magic { get { return _magic; } }
            public string VersionStr { get { return _versionStr; } }

            /// <summary>
            /// adler32 checksum of the rest of the file (everything but magic and this field);
            /// used to detect file corruption
            /// </summary>
            public uint Checksum { get { return _checksum; } }

            /// <summary>
            /// SHA-1 signature (hash) of the rest of the file (everything but magic, checksum,
            /// and this field); used to uniquely identify files
            /// </summary>
            public byte[] Signature { get { return _signature; } }

            /// <summary>
            /// size of the entire file (including the header), in bytes
            /// </summary>
            public uint FileSize { get { return _fileSize; } }

            /// <summary>
            /// size of the header (this entire section), in bytes. This allows for at
            /// least a limited amount of backwards/forwards compatibility without
            /// invalidating the format.
            /// </summary>
            public uint HeaderSize { get { return _headerSize; } }
            public EndianConstant EndianTag { get { return _endianTag; } }

            /// <summary>
            /// size of the link section, or 0 if this file isn't statically linked
            /// </summary>
            public uint LinkSize { get { return _linkSize; } }

            /// <summary>
            /// offset from the start of the file to the link section, or 0 if link_size == 0.
            /// The offset, if non-zero, should be to an offset into the link_data section.
            /// The format of the data pointed at is left unspecified by this document;
            /// this header field (and the previous) are left as hooks for use by runtime implementations.
            /// </summary>
            public uint LinkOff { get { return _linkOff; } }

            /// <summary>
            /// offset from the start of the file to the map item.
            /// The offset, which must be non-zero, should be to an offset into the data
            /// section, and the data should be in the format specified by &quot;map_list&quot; below.
            /// </summary>
            public uint MapOff { get { return _mapOff; } }

            /// <summary>
            /// count of strings in the string identifiers list
            /// </summary>
            public uint StringIdsSize { get { return _stringIdsSize; } }

            /// <summary>
            /// offset from the start of the file to the string identifiers list,
            /// or 0 if string_ids_size == 0 (admittedly a strange edge case).
            /// The offset, if non-zero, should be to the start of the string_ids section.
            /// </summary>
            public uint StringIdsOff { get { return _stringIdsOff; } }

            /// <summary>
            /// count of elements in the type identifiers list, at most 65535
            /// </summary>
            public uint TypeIdsSize { get { return _typeIdsSize; } }

            /// <summary>
            /// offset from the start of the file to the type identifiers list,
            /// or 0 if type_ids_size == 0 (admittedly a strange edge case).
            /// The offset, if non-zero, should be to the start of the type_ids section.
            /// </summary>
            public uint TypeIdsOff { get { return _typeIdsOff; } }

            /// <summary>
            /// count of elements in the prototype identifiers list, at most 65535
            /// </summary>
            public uint ProtoIdsSize { get { return _protoIdsSize; } }

            /// <summary>
            /// offset from the start of the file to the prototype identifiers list,
            /// or 0 if proto_ids_size == 0 (admittedly a strange edge case).
            /// The offset, if non-zero, should be to the start of the proto_ids section.
            /// </summary>
            public uint ProtoIdsOff { get { return _protoIdsOff; } }

            /// <summary>
            /// count of elements in the field identifiers list
            /// </summary>
            public uint FieldIdsSize { get { return _fieldIdsSize; } }

            /// <summary>
            /// offset from the start of the file to the field identifiers list,
            /// or 0 if field_ids_size == 0.
            /// The offset, if non-zero, should be to the start of the field_ids section.
            /// </summary>
            public uint FieldIdsOff { get { return _fieldIdsOff; } }

            /// <summary>
            /// count of elements in the method identifiers list
            /// </summary>
            public uint MethodIdsSize { get { return _methodIdsSize; } }

            /// <summary>
            /// offset from the start of the file to the method identifiers list,
            /// or 0 if method_ids_size == 0.
            /// The offset, if non-zero, should be to the start of the method_ids section.
            /// </summary>
            public uint MethodIdsOff { get { return _methodIdsOff; } }

            /// <summary>
            /// count of elements in the class definitions list
            /// </summary>
            public uint ClassDefsSize { get { return _classDefsSize; } }

            /// <summary>
            /// offset from the start of the file to the class definitions list,
            /// or 0 if class_defs_size == 0 (admittedly a strange edge case).
            /// The offset, if non-zero, should be to the start of the class_defs section.
            /// </summary>
            public uint ClassDefsOff { get { return _classDefsOff; } }

            /// <summary>
            /// Size of data section in bytes. Must be an even multiple of sizeof(uint).
            /// </summary>
            public uint DataSize { get { return _dataSize; } }

            /// <summary>
            /// offset from the start of the file to the start of the data section.
            /// </summary>
            public uint DataOff { get { return _dataOff; } }
            public Dex M_Root { get { return m_root; } }
            public Dex M_Parent { get { return m_parent; } }
        }
        public partial class MapList : KaitaiStruct
        {
            public static MapList FromFile(string fileName)
            {
                return new MapList(new KaitaiStream(fileName));
            }

            public MapList(KaitaiStream p__io, Dex p__parent = null, Dex p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _size = m_io.ReadU4le();
                _list = new List<MapItem>();
                for (var i = 0; i < Size; i++)
                {
                    _list.Add(new MapItem(m_io, this, m_root));
                }
            }
            private uint _size;
            private List<MapItem> _list;
            private Dex m_root;
            private Dex m_parent;
            public uint Size { get { return _size; } }
            public List<MapItem> List { get { return _list; } }
            public Dex M_Root { get { return m_root; } }
            public Dex M_Parent { get { return m_parent; } }
        }
        public partial class EncodedValue : KaitaiStruct
        {
            public static EncodedValue FromFile(string fileName)
            {
                return new EncodedValue(new KaitaiStream(fileName));
            }


            public enum ValueTypeEnum
            {
                Byte = 0,
                Short = 2,
                Char = 3,
                Int = 4,
                Long = 6,
                Float = 16,
                Double = 17,
                MethodType = 21,
                MethodHandle = 22,
                String = 23,
                Type = 24,
                Field = 25,
                Method = 26,
                Enum = 27,
                Array = 28,
                Annotation = 29,
                Null = 30,
                Boolean = 31,
            }
            public EncodedValue(KaitaiStream p__io, KaitaiStruct p__parent = null, Dex p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _valueArg = m_io.ReadBitsIntBe(3);
                _valueType = (ValueTypeEnum)m_io.ReadBitsIntBe(5);
                m_io.AlignToByte();
                switch (ValueType)
                {
                    case ValueTypeEnum.Int:
                        {
                            _value = m_io.ReadS4le();
                            break;
                        }
                    case ValueTypeEnum.Annotation:
                        {
                            _value = new EncodedAnnotation(m_io, this, m_root);
                            break;
                        }
                    case ValueTypeEnum.Long:
                        {
                            _value = m_io.ReadS8le();
                            break;
                        }
                    case ValueTypeEnum.MethodHandle:
                        {
                            _value = m_io.ReadU4le();
                            break;
                        }
                    case ValueTypeEnum.Byte:
                        {
                            _value = m_io.ReadS1();
                            break;
                        }
                    case ValueTypeEnum.Array:
                        {
                            _value = new EncodedArray(m_io, this, m_root);
                            break;
                        }
                    case ValueTypeEnum.MethodType:
                        {
                            _value = m_io.ReadU4le();
                            break;
                        }
                    case ValueTypeEnum.Short:
                        {
                            _value = m_io.ReadS2le();
                            break;
                        }
                    case ValueTypeEnum.Method:
                        {
                            _value = m_io.ReadU4le();
                            break;
                        }
                    case ValueTypeEnum.Double:
                        {
                            _value = m_io.ReadF8le();
                            break;
                        }
                    case ValueTypeEnum.Float:
                        {
                            _value = m_io.ReadF4le();
                            break;
                        }
                    case ValueTypeEnum.Type:
                        {
                            _value = m_io.ReadU4le();
                            break;
                        }
                    case ValueTypeEnum.Enum:
                        {
                            _value = m_io.ReadU4le();
                            break;
                        }
                    case ValueTypeEnum.Field:
                        {
                            _value = m_io.ReadU4le();
                            break;
                        }
                    case ValueTypeEnum.String:
                        {
                            _value = m_io.ReadU4le();
                            break;
                        }
                    case ValueTypeEnum.Char:
                        {
                            _value = m_io.ReadU2le();
                            break;
                        }
                }
            }
            private ulong _valueArg;
            private ValueTypeEnum _valueType;
            private object _value;
            private Dex m_root;
            private KaitaiStruct m_parent;
            public ulong ValueArg { get { return _valueArg; } }
            public ValueTypeEnum ValueType { get { return _valueType; } }
            public object Value { get { return _value; } }
            public Dex M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class CallSiteIdItem : KaitaiStruct
        {
            public static CallSiteIdItem FromFile(string fileName)
            {
                return new CallSiteIdItem(new KaitaiStream(fileName));
            }

            public CallSiteIdItem(KaitaiStream p__io, KaitaiStruct p__parent = null, Dex p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _callSiteOff = m_io.ReadU4le();
            }
            private uint _callSiteOff;
            private Dex m_root;
            private KaitaiStruct m_parent;

            /// <summary>
            /// offset from the start of the file to call site definition.
            /// 
            /// The offset should be in the data section, and the data there should
            /// be in the format specified by &quot;call_site_item&quot; below.
            /// </summary>
            public uint CallSiteOff { get { return _callSiteOff; } }
            public Dex M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class MethodIdItem : KaitaiStruct
        {
            public static MethodIdItem FromFile(string fileName)
            {
                return new MethodIdItem(new KaitaiStream(fileName));
            }

            public MethodIdItem(KaitaiStream p__io, Dex p__parent = null, Dex p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_className = false;
                f_protoDesc = false;
                f_methodName = false;
                _read();
            }
            private void _read()
            {
                _classIdx = m_io.ReadU2le();
                _protoIdx = m_io.ReadU2le();
                _nameIdx = m_io.ReadU4le();
            }
            private bool f_className;
            private string _className;

            /// <summary>
            /// the definer of this method
            /// </summary>
            public string ClassName
            {
                get
                {
                    if (f_className)
                        return _className;
                    _className = M_Root.TypeIds[ClassIdx].TypeName;
                    f_className = true;
                    return _className;
                }
            }
            private bool f_protoDesc;
            private string _protoDesc;

            /// <summary>
            /// the short-form descriptor of the prototype of this method
            /// </summary>
            public string ProtoDesc
            {
                get
                {
                    if (f_protoDesc)
                        return _protoDesc;
                    _protoDesc = M_Root.ProtoIds[ProtoIdx].ShortyDesc;
                    f_protoDesc = true;
                    return _protoDesc;
                }
            }
            private bool f_methodName;
            private string _methodName;

            /// <summary>
            /// the name of this method
            /// </summary>
            public string MethodName
            {
                get
                {
                    if (f_methodName)
                        return _methodName;
                    _methodName = M_Root.StringIds[(int)NameIdx].Value.Data;
                    f_methodName = true;
                    return _methodName;
                }
            }
            private ushort _classIdx;
            private ushort _protoIdx;
            private uint _nameIdx;
            private Dex m_root;
            private Dex m_parent;

            /// <summary>
            /// index into the type_ids list for the definer of this method.
            /// This must be a class or array type, and not a primitive type.
            /// </summary>
            public ushort ClassIdx { get { return _classIdx; } }

            /// <summary>
            /// index into the proto_ids list for the prototype of this method
            /// </summary>
            public ushort ProtoIdx { get { return _protoIdx; } }

            /// <summary>
            /// index into the string_ids list for the name of this method.
            /// The string must conform to the syntax for MemberName, defined above.
            /// </summary>
            public uint NameIdx { get { return _nameIdx; } }
            public Dex M_Root { get { return m_root; } }
            public Dex M_Parent { get { return m_parent; } }
        }
        public partial class TypeItem : KaitaiStruct
        {
            public static TypeItem FromFile(string fileName)
            {
                return new TypeItem(new KaitaiStream(fileName));
            }

            public TypeItem(KaitaiStream p__io, TypeList p__parent = null, Dex p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_value = false;
                _read();
            }
            private void _read()
            {
                _typeIdx = m_io.ReadU2le();
            }
            private bool f_value;
            private string _value;
            public string Value
            {
                get
                {
                    if (f_value)
                        return _value;
                    _value = M_Root.TypeIds[TypeIdx].TypeName;
                    f_value = true;
                    return _value;
                }
            }
            private ushort _typeIdx;
            private Dex m_root;
            private TypeList m_parent;
            public ushort TypeIdx { get { return _typeIdx; } }
            public Dex M_Root { get { return m_root; } }
            public TypeList M_Parent { get { return m_parent; } }
        }
        public partial class TypeIdItem : KaitaiStruct
        {
            public static TypeIdItem FromFile(string fileName)
            {
                return new TypeIdItem(new KaitaiStream(fileName));
            }

            public TypeIdItem(KaitaiStream p__io, Dex p__parent = null, Dex p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_typeName = false;
                _read();
            }
            private void _read()
            {
                _descriptorIdx = m_io.ReadU4le();
            }
            private bool f_typeName;
            private string _typeName;
            public string TypeName
            {
                get
                {
                    if (f_typeName)
                        return _typeName;
                    _typeName = M_Root.StringIds[(int)DescriptorIdx].Value.Data;
                    f_typeName = true;
                    return _typeName;
                }
            }
            private uint _descriptorIdx;
            private Dex m_root;
            private Dex m_parent;

            /// <summary>
            /// index into the string_ids list for the descriptor string of this type.
            /// The string must conform to the syntax for TypeDescriptor, defined above.
            /// </summary>
            public uint DescriptorIdx { get { return _descriptorIdx; } }
            public Dex M_Root { get { return m_root; } }
            public Dex M_Parent { get { return m_parent; } }
        }
        public partial class AnnotationElement : KaitaiStruct
        {
            public static AnnotationElement FromFile(string fileName)
            {
                return new AnnotationElement(new KaitaiStream(fileName));
            }

            public AnnotationElement(KaitaiStream p__io, EncodedAnnotation p__parent = null, Dex p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _nameIdx = new VlqBase128Le(m_io);
                _value = new EncodedValue(m_io, this, m_root);
            }
            private VlqBase128Le _nameIdx;
            private EncodedValue _value;
            private Dex m_root;
            private EncodedAnnotation m_parent;

            /// <summary>
            /// element name, represented as an index into the string_ids section.
            /// 
            /// The string must conform to the syntax for MemberName, defined above.
            /// </summary>
            public VlqBase128Le NameIdx { get { return _nameIdx; } }

            /// <summary>
            /// element value
            /// </summary>
            public EncodedValue Value { get { return _value; } }
            public Dex M_Root { get { return m_root; } }
            public EncodedAnnotation M_Parent { get { return m_parent; } }
        }
        public partial class EncodedField : KaitaiStruct
        {
            public static EncodedField FromFile(string fileName)
            {
                return new EncodedField(new KaitaiStream(fileName));
            }

            public EncodedField(KaitaiStream p__io, ClassDataItem p__parent = null, Dex p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _fieldIdxDiff = new VlqBase128Le(m_io);
                _accessFlags = new VlqBase128Le(m_io);
            }
            private VlqBase128Le _fieldIdxDiff;
            private VlqBase128Le _accessFlags;
            private Dex m_root;
            private ClassDataItem m_parent;

            /// <summary>
            /// index into the field_ids list for the identity of this field
            /// (includes the name and descriptor), represented as a difference
            /// from the index of previous element in the list.
            /// 
            /// The index of the first element in a list is represented directly.
            /// </summary>
            public VlqBase128Le FieldIdxDiff { get { return _fieldIdxDiff; } }

            /// <summary>
            /// access flags for the field (public, final, etc.).
            /// 
            /// See &quot;access_flags Definitions&quot; for details.
            /// </summary>
            public VlqBase128Le AccessFlags { get { return _accessFlags; } }
            public Dex M_Root { get { return m_root; } }
            public ClassDataItem M_Parent { get { return m_parent; } }
        }
        public partial class EncodedArrayItem : KaitaiStruct
        {
            public static EncodedArrayItem FromFile(string fileName)
            {
                return new EncodedArrayItem(new KaitaiStream(fileName));
            }

            public EncodedArrayItem(KaitaiStream p__io, ClassDefItem p__parent = null, Dex p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _value = new EncodedArray(m_io, this, m_root);
            }
            private EncodedArray _value;
            private Dex m_root;
            private ClassDefItem m_parent;
            public EncodedArray Value { get { return _value; } }
            public Dex M_Root { get { return m_root; } }
            public ClassDefItem M_Parent { get { return m_parent; } }
        }
        public partial class ClassDataItem : KaitaiStruct
        {
            public static ClassDataItem FromFile(string fileName)
            {
                return new ClassDataItem(new KaitaiStream(fileName));
            }

            public ClassDataItem(KaitaiStream p__io, ClassDefItem p__parent = null, Dex p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _staticFieldsSize = new VlqBase128Le(m_io);
                _instanceFieldsSize = new VlqBase128Le(m_io);
                _directMethodsSize = new VlqBase128Le(m_io);
                _virtualMethodsSize = new VlqBase128Le(m_io);
                _staticFields = new List<EncodedField>();
                for (var i = 0; i < StaticFieldsSize.Value; i++)
                {
                    _staticFields.Add(new EncodedField(m_io, this, m_root));
                }
                _instanceFields = new List<EncodedField>();
                for (var i = 0; i < InstanceFieldsSize.Value; i++)
                {
                    _instanceFields.Add(new EncodedField(m_io, this, m_root));
                }
                _directMethods = new List<EncodedMethod>();
                for (var i = 0; i < DirectMethodsSize.Value; i++)
                {
                    _directMethods.Add(new EncodedMethod(m_io, this, m_root));
                }
                _virtualMethods = new List<EncodedMethod>();
                for (var i = 0; i < VirtualMethodsSize.Value; i++)
                {
                    _virtualMethods.Add(new EncodedMethod(m_io, this, m_root));
                }
            }
            private VlqBase128Le _staticFieldsSize;
            private VlqBase128Le _instanceFieldsSize;
            private VlqBase128Le _directMethodsSize;
            private VlqBase128Le _virtualMethodsSize;
            private List<EncodedField> _staticFields;
            private List<EncodedField> _instanceFields;
            private List<EncodedMethod> _directMethods;
            private List<EncodedMethod> _virtualMethods;
            private Dex m_root;
            private ClassDefItem m_parent;

            /// <summary>
            /// the number of static fields defined in this item
            /// </summary>
            public VlqBase128Le StaticFieldsSize { get { return _staticFieldsSize; } }

            /// <summary>
            /// the number of instance fields defined in this item
            /// </summary>
            public VlqBase128Le InstanceFieldsSize { get { return _instanceFieldsSize; } }

            /// <summary>
            /// the number of direct methods defined in this item
            /// </summary>
            public VlqBase128Le DirectMethodsSize { get { return _directMethodsSize; } }

            /// <summary>
            /// the number of virtual methods defined in this item
            /// </summary>
            public VlqBase128Le VirtualMethodsSize { get { return _virtualMethodsSize; } }

            /// <summary>
            /// the defined static fields, represented as a sequence of encoded elements.
            /// 
            /// The fields must be sorted by field_idx in increasing order.
            /// </summary>
            public List<EncodedField> StaticFields { get { return _staticFields; } }

            /// <summary>
            /// the defined instance fields, represented as a sequence of encoded elements.
            /// 
            /// The fields must be sorted by field_idx in increasing order.
            /// </summary>
            public List<EncodedField> InstanceFields { get { return _instanceFields; } }

            /// <summary>
            /// the defined direct (any of static, private, or constructor) methods,
            /// represented as a sequence of encoded elements.
            /// 
            /// The methods must be sorted by method_idx in increasing order.
            /// </summary>
            public List<EncodedMethod> DirectMethods { get { return _directMethods; } }

            /// <summary>
            /// the defined virtual (none of static, private, or constructor) methods,
            /// represented as a sequence of encoded elements.
            /// 
            /// This list should not include inherited methods unless overridden by
            /// the class that this item represents.
            /// 
            /// The methods must be sorted by method_idx in increasing order.
            /// 
            /// The method_idx of a virtual method must not be the same as any direct method.
            /// </summary>
            public List<EncodedMethod> VirtualMethods { get { return _virtualMethods; } }
            public Dex M_Root { get { return m_root; } }
            public ClassDefItem M_Parent { get { return m_parent; } }
        }
        public partial class FieldIdItem : KaitaiStruct
        {
            public static FieldIdItem FromFile(string fileName)
            {
                return new FieldIdItem(new KaitaiStream(fileName));
            }

            public FieldIdItem(KaitaiStream p__io, Dex p__parent = null, Dex p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_className = false;
                f_typeName = false;
                f_fieldName = false;
                _read();
            }
            private void _read()
            {
                _classIdx = m_io.ReadU2le();
                _typeIdx = m_io.ReadU2le();
                _nameIdx = m_io.ReadU4le();
            }
            private bool f_className;
            private string _className;

            /// <summary>
            /// the definer of this field
            /// </summary>
            public string ClassName
            {
                get
                {
                    if (f_className)
                        return _className;
                    _className = M_Root.TypeIds[ClassIdx].TypeName;
                    f_className = true;
                    return _className;
                }
            }
            private bool f_typeName;
            private string _typeName;

            /// <summary>
            /// the type of this field
            /// </summary>
            public string TypeName
            {
                get
                {
                    if (f_typeName)
                        return _typeName;
                    _typeName = M_Root.TypeIds[TypeIdx].TypeName;
                    f_typeName = true;
                    return _typeName;
                }
            }
            private bool f_fieldName;
            private string _fieldName;

            /// <summary>
            /// the name of this field
            /// </summary>
            public string FieldName
            {
                get
                {
                    if (f_fieldName)
                        return _fieldName;
                    _fieldName = M_Root.StringIds[(int)NameIdx].Value.Data;
                    f_fieldName = true;
                    return _fieldName;
                }
            }
            private ushort _classIdx;
            private ushort _typeIdx;
            private uint _nameIdx;
            private Dex m_root;
            private Dex m_parent;

            /// <summary>
            /// index into the type_ids list for the definer of this field.
            /// This must be a class type, and not an array or primitive type.
            /// </summary>
            public ushort ClassIdx { get { return _classIdx; } }

            /// <summary>
            /// index into the type_ids list for the type of this field
            /// </summary>
            public ushort TypeIdx { get { return _typeIdx; } }

            /// <summary>
            /// index into the string_ids list for the name of this field.
            /// The string must conform to the syntax for MemberName, defined above.
            /// </summary>
            public uint NameIdx { get { return _nameIdx; } }
            public Dex M_Root { get { return m_root; } }
            public Dex M_Parent { get { return m_parent; } }
        }
        public partial class EncodedAnnotation : KaitaiStruct
        {
            public static EncodedAnnotation FromFile(string fileName)
            {
                return new EncodedAnnotation(new KaitaiStream(fileName));
            }

            public EncodedAnnotation(KaitaiStream p__io, EncodedValue p__parent = null, Dex p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _typeIdx = new VlqBase128Le(m_io);
                _size = new VlqBase128Le(m_io);
                _elements = new List<AnnotationElement>();
                for (var i = 0; i < Size.Value; i++)
                {
                    _elements.Add(new AnnotationElement(m_io, this, m_root));
                }
            }
            private VlqBase128Le _typeIdx;
            private VlqBase128Le _size;
            private List<AnnotationElement> _elements;
            private Dex m_root;
            private EncodedValue m_parent;

            /// <summary>
            /// type of the annotation.
            /// 
            /// This must be a class (not array or primitive) type.
            /// </summary>
            public VlqBase128Le TypeIdx { get { return _typeIdx; } }

            /// <summary>
            /// number of name-value mappings in this annotation
            /// </summary>
            public VlqBase128Le Size { get { return _size; } }

            /// <summary>
            /// elements of the annotation, represented directly in-line (not as offsets).
            /// 
            /// Elements must be sorted in increasing order by string_id index.
            /// </summary>
            public List<AnnotationElement> Elements { get { return _elements; } }
            public Dex M_Root { get { return m_root; } }
            public EncodedValue M_Parent { get { return m_parent; } }
        }
        public partial class ClassDefItem : KaitaiStruct
        {
            public static ClassDefItem FromFile(string fileName)
            {
                return new ClassDefItem(new KaitaiStream(fileName));
            }

            public ClassDefItem(KaitaiStream p__io, Dex p__parent = null, Dex p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_typeName = false;
                f_classData = false;
                f_staticValues = false;
                _read();
            }
            private void _read()
            {
                _classIdx = m_io.ReadU4le();
                _accessFlags = (ClassAccessFlags)m_io.ReadU4le();
                _superclassIdx = m_io.ReadU4le();
                _interfacesOff = m_io.ReadU4le();
                _sourceFileIdx = m_io.ReadU4le();
                _annotationsOff = m_io.ReadU4le();
                _classDataOff = m_io.ReadU4le();
                _staticValuesOff = m_io.ReadU4le();
            }
            private bool f_typeName;
            private string _typeName;
            public string TypeName
            {
                get
                {
                    if (f_typeName)
                        return _typeName;
                    _typeName = M_Root.TypeIds[(int)ClassIdx].TypeName;
                    f_typeName = true;
                    return _typeName;
                }
            }
            private bool f_classData;
            private ClassDataItem _classData;
            public ClassDataItem ClassData
            {
                get
                {
                    if (f_classData)
                        return _classData;
                    if (ClassDataOff != 0)
                    {
                        long _pos = m_io.Pos;
                        m_io.Seek(ClassDataOff);
                        _classData = new ClassDataItem(m_io, this, m_root);
                        m_io.Seek(_pos);
                        f_classData = true;
                    }
                    return _classData;
                }
            }
            private bool f_staticValues;
            private EncodedArrayItem _staticValues;
            public EncodedArrayItem StaticValues
            {
                get
                {
                    if (f_staticValues)
                        return _staticValues;
                    if (StaticValuesOff != 0)
                    {
                        long _pos = m_io.Pos;
                        m_io.Seek(StaticValuesOff);
                        _staticValues = new EncodedArrayItem(m_io, this, m_root);
                        m_io.Seek(_pos);
                        f_staticValues = true;
                    }
                    return _staticValues;
                }
            }
            private uint _classIdx;
            private ClassAccessFlags _accessFlags;
            private uint _superclassIdx;
            private uint _interfacesOff;
            private uint _sourceFileIdx;
            private uint _annotationsOff;
            private uint _classDataOff;
            private uint _staticValuesOff;
            private Dex m_root;
            private Dex m_parent;

            /// <summary>
            /// index into the type_ids list for this class.
            /// 
            /// This must be a class type, and not an array or primitive type.
            /// </summary>
            public uint ClassIdx { get { return _classIdx; } }

            /// <summary>
            /// access flags for the class (public, final, etc.).
            /// 
            /// See &quot;access_flags Definitions&quot; for details.
            /// </summary>
            public ClassAccessFlags AccessFlags { get { return _accessFlags; } }

            /// <summary>
            /// index into the type_ids list for the superclass,
            /// or the constant value NO_INDEX if this class has no superclass
            /// (i.e., it is a root class such as Object).
            /// 
            /// If present, this must be a class type, and not an array or primitive type.
            /// </summary>
            public uint SuperclassIdx { get { return _superclassIdx; } }

            /// <summary>
            /// offset from the start of the file to the list of interfaces, or 0 if there are none.
            /// 
            /// This offset should be in the data section, and the data there should
            /// be in the format specified by &quot;type_list&quot; below. Each of the elements
            /// of the list must be a class type (not an array or primitive type),
            /// and there must not be any duplicates.
            /// </summary>
            public uint InterfacesOff { get { return _interfacesOff; } }

            /// <summary>
            /// index into the string_ids list for the name of the file containing
            /// the original source for (at least most of) this class, or the
            /// special value NO_INDEX to represent a lack of this information.
            /// 
            /// The debug_info_item of any given method may override this source file,
            /// but the expectation is that most classes will only come from one source file.
            /// </summary>
            public uint SourceFileIdx { get { return _sourceFileIdx; } }

            /// <summary>
            /// offset from the start of the file to the annotations structure for
            /// this class, or 0 if there are no annotations on this class.
            /// 
            /// This offset, if non-zero, should be in the data section, and the data
            /// there should be in the format specified by &quot;annotations_directory_item&quot;
            /// below,with all items referring to this class as the definer.
            /// </summary>
            public uint AnnotationsOff { get { return _annotationsOff; } }

            /// <summary>
            /// offset from the start of the file to the associated class data for this
            /// item, or 0 if there is no class data for this class.
            /// 
            /// (This may be the case, for example, if this class is a marker interface.)
            /// 
            /// The offset, if non-zero, should be in the data section, and the data
            /// there should be in the format specified by &quot;class_data_item&quot; below,
            /// with all items referring to this class as the definer.
            /// </summary>
            public uint ClassDataOff { get { return _classDataOff; } }

            /// <summary>
            /// offset from the start of the file to the list of initial values for
            /// static fields, or 0 if there are none (and all static fields are to be
            /// initialized with 0 or null).
            /// 
            /// This offset should be in the data section, and the data there should
            /// be in the format specified by &quot;encoded_array_item&quot; below.
            /// 
            /// The size of the array must be no larger than the number of static fields
            /// declared by this class, and the elements correspond to the static fields
            /// in the same order as declared in the corresponding field_list.
            /// 
            /// The type of each array element must match the declared type of its
            /// corresponding field.
            /// 
            /// If there are fewer elements in the array than there are static fields,
            /// then the leftover fields are initialized with a type-appropriate 0 or null.
            /// </summary>
            public uint StaticValuesOff { get { return _staticValuesOff; } }
            public Dex M_Root { get { return m_root; } }
            public Dex M_Parent { get { return m_parent; } }
        }
        public partial class TypeList : KaitaiStruct
        {
            public static TypeList FromFile(string fileName)
            {
                return new TypeList(new KaitaiStream(fileName));
            }

            public TypeList(KaitaiStream p__io, ProtoIdItem p__parent = null, Dex p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _size = m_io.ReadU4le();
                _list = new List<TypeItem>();
                for (var i = 0; i < Size; i++)
                {
                    _list.Add(new TypeItem(m_io, this, m_root));
                }
            }
            private uint _size;
            private List<TypeItem> _list;
            private Dex m_root;
            private ProtoIdItem m_parent;
            public uint Size { get { return _size; } }
            public List<TypeItem> List { get { return _list; } }
            public Dex M_Root { get { return m_root; } }
            public ProtoIdItem M_Parent { get { return m_parent; } }
        }
        public partial class StringIdItem : KaitaiStruct
        {
            public static StringIdItem FromFile(string fileName)
            {
                return new StringIdItem(new KaitaiStream(fileName));
            }

            public StringIdItem(KaitaiStream p__io, Dex p__parent = null, Dex p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_value = false;
                _read();
            }
            private void _read()
            {
                _stringDataOff = m_io.ReadU4le();
            }
            public partial class StringDataItem : KaitaiStruct
            {
                public static StringDataItem FromFile(string fileName)
                {
                    return new StringDataItem(new KaitaiStream(fileName));
                }

                public StringDataItem(KaitaiStream p__io, StringIdItem p__parent = null, Dex p__root = null) : base(p__io)
                {
                    m_parent = p__parent;
                    m_root = p__root;
                    _read();
                }
                private void _read()
                {
                    _utf16Size = new VlqBase128Le(m_io);
                    _data = System.Text.Encoding.GetEncoding("ascii").GetString(m_io.ReadBytes(Utf16Size.Value));
                }
                private VlqBase128Le _utf16Size;
                private string _data;
                private Dex m_root;
                private StringIdItem m_parent;
                public VlqBase128Le Utf16Size { get { return _utf16Size; } }
                public string Data { get { return _data; } }
                public Dex M_Root { get { return m_root; } }
                public StringIdItem M_Parent { get { return m_parent; } }
            }
            private bool f_value;
            private StringDataItem _value;
            public StringDataItem Value
            {
                get
                {
                    if (f_value)
                        return _value;
                    long _pos = m_io.Pos;
                    m_io.Seek(StringDataOff);
                    _value = new StringDataItem(m_io, this, m_root);
                    m_io.Seek(_pos);
                    f_value = true;
                    return _value;
                }
            }
            private uint _stringDataOff;
            private Dex m_root;
            private Dex m_parent;

            /// <summary>
            /// offset from the start of the file to the string data for this item.
            /// The offset should be to a location in the data section, and the data
            /// should be in the format specified by &quot;string_data_item&quot; below.
            /// There is no alignment requirement for the offset.
            /// </summary>
            public uint StringDataOff { get { return _stringDataOff; } }
            public Dex M_Root { get { return m_root; } }
            public Dex M_Parent { get { return m_parent; } }
        }
        public partial class ProtoIdItem : KaitaiStruct
        {
            public static ProtoIdItem FromFile(string fileName)
            {
                return new ProtoIdItem(new KaitaiStream(fileName));
            }

            public ProtoIdItem(KaitaiStream p__io, Dex p__parent = null, Dex p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_shortyDesc = false;
                f_paramsTypes = false;
                f_returnType = false;
                _read();
            }
            private void _read()
            {
                _shortyIdx = m_io.ReadU4le();
                _returnTypeIdx = m_io.ReadU4le();
                _parametersOff = m_io.ReadU4le();
            }
            private bool f_shortyDesc;
            private string _shortyDesc;

            /// <summary>
            /// short-form descriptor string of this prototype, as pointed to by shorty_idx
            /// </summary>
            public string ShortyDesc
            {
                get
                {
                    if (f_shortyDesc)
                        return _shortyDesc;
                    _shortyDesc = M_Root.StringIds[(int)ShortyIdx].Value.Data;
                    f_shortyDesc = true;
                    return _shortyDesc;
                }
            }
            private bool f_paramsTypes;
            private TypeList _paramsTypes;

            /// <summary>
            /// list of parameter types for this prototype
            /// </summary>
            public TypeList ParamsTypes
            {
                get
                {
                    if (f_paramsTypes)
                        return _paramsTypes;
                    if (ParametersOff != 0)
                    {
                        KaitaiStream io = M_Root.M_Io;
                        long _pos = io.Pos;
                        io.Seek(ParametersOff);
                        _paramsTypes = new TypeList(io, this, m_root);
                        io.Seek(_pos);
                        f_paramsTypes = true;
                    }
                    return _paramsTypes;
                }
            }
            private bool f_returnType;
            private string _returnType;

            /// <summary>
            /// return type of this prototype
            /// </summary>
            public string ReturnType
            {
                get
                {
                    if (f_returnType)
                        return _returnType;
                    _returnType = M_Root.TypeIds[(int)ReturnTypeIdx].TypeName;
                    f_returnType = true;
                    return _returnType;
                }
            }
            private uint _shortyIdx;
            private uint _returnTypeIdx;
            private uint _parametersOff;
            private Dex m_root;
            private Dex m_parent;

            /// <summary>
            /// index into the string_ids list for the short-form descriptor string of this prototype.
            /// The string must conform to the syntax for ShortyDescriptor, defined above,
            /// and must correspond to the return type and parameters of this item.
            /// </summary>
            public uint ShortyIdx { get { return _shortyIdx; } }

            /// <summary>
            /// index into the type_ids list for the return type of this prototype
            /// </summary>
            public uint ReturnTypeIdx { get { return _returnTypeIdx; } }

            /// <summary>
            /// offset from the start of the file to the list of parameter types for this prototype,
            /// or 0 if this prototype has no parameters.
            /// This offset, if non-zero, should be in the data section, and the data
            /// there should be in the format specified by &quot;type_list&quot; below.
            /// Additionally, there should be no reference to the type void in the list.
            /// </summary>
            public uint ParametersOff { get { return _parametersOff; } }
            public Dex M_Root { get { return m_root; } }
            public Dex M_Parent { get { return m_parent; } }
        }
        public partial class EncodedMethod : KaitaiStruct
        {
            public static EncodedMethod FromFile(string fileName)
            {
                return new EncodedMethod(new KaitaiStream(fileName));
            }

            public EncodedMethod(KaitaiStream p__io, ClassDataItem p__parent = null, Dex p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _methodIdxDiff = new VlqBase128Le(m_io);
                _accessFlags = new VlqBase128Le(m_io);
                _codeOff = new VlqBase128Le(m_io);
            }
            private VlqBase128Le _methodIdxDiff;
            private VlqBase128Le _accessFlags;
            private VlqBase128Le _codeOff;
            private Dex m_root;
            private ClassDataItem m_parent;

            /// <summary>
            /// index into the method_ids list for the identity of this method
            /// (includes the name and descriptor), represented as a difference
            /// from the index of previous element in the list.
            /// 
            /// The index of the first element in a list is represented directly.
            /// </summary>
            public VlqBase128Le MethodIdxDiff { get { return _methodIdxDiff; } }

            /// <summary>
            /// access flags for the field (public, final, etc.).
            /// 
            /// See &quot;access_flags Definitions&quot; for details.
            /// </summary>
            public VlqBase128Le AccessFlags { get { return _accessFlags; } }

            /// <summary>
            /// offset from the start of the file to the code structure for this method,
            /// or 0 if this method is either abstract or native.
            /// 
            /// The offset should be to a location in the data section.
            /// 
            /// The format of the data is specified by &quot;code_item&quot; below.
            /// </summary>
            public VlqBase128Le CodeOff { get { return _codeOff; } }
            public Dex M_Root { get { return m_root; } }
            public ClassDataItem M_Parent { get { return m_parent; } }
        }
        public partial class MapItem : KaitaiStruct
        {
            public static MapItem FromFile(string fileName)
            {
                return new MapItem(new KaitaiStream(fileName));
            }


            public enum MapItemType
            {
                HeaderItem = 0,
                StringIdItem = 1,
                TypeIdItem = 2,
                ProtoIdItem = 3,
                FieldIdItem = 4,
                MethodIdItem = 5,
                ClassDefItem = 6,
                CallSiteIdItem = 7,
                MethodHandleItem = 8,
                MapList = 4096,
                TypeList = 4097,
                AnnotationSetRefList = 4098,
                AnnotationSetItem = 4099,
                ClassDataItem = 8192,
                CodeItem = 8193,
                StringDataItem = 8194,
                DebugInfoItem = 8195,
                AnnotationItem = 8196,
                EncodedArrayItem = 8197,
                AnnotationsDirectoryItem = 8198,
            }
            public MapItem(KaitaiStream p__io, MapList p__parent = null, Dex p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _type = (MapItemType)m_io.ReadU2le();
                _unused = m_io.ReadU2le();
                _size = m_io.ReadU4le();
                _offset = m_io.ReadU4le();
            }
            private MapItemType _type;
            private ushort _unused;
            private uint _size;
            private uint _offset;
            private Dex m_root;
            private MapList m_parent;

            /// <summary>
            /// type of the items; see table below
            /// </summary>
            public MapItemType Type { get { return _type; } }

            /// <summary>
            /// (unused)
            /// </summary>
            public ushort Unused { get { return _unused; } }

            /// <summary>
            /// count of the number of items to be found at the indicated offset
            /// </summary>
            public uint Size { get { return _size; } }

            /// <summary>
            /// offset from the start of the file to the items in question
            /// </summary>
            public uint Offset { get { return _offset; } }
            public Dex M_Root { get { return m_root; } }
            public MapList M_Parent { get { return m_parent; } }
        }
        public partial class EncodedArray : KaitaiStruct
        {
            public static EncodedArray FromFile(string fileName)
            {
                return new EncodedArray(new KaitaiStream(fileName));
            }

            public EncodedArray(KaitaiStream p__io, KaitaiStruct p__parent = null, Dex p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _size = new VlqBase128Le(m_io);
                _values = new List<EncodedValue>();
                for (var i = 0; i < Size.Value; i++)
                {
                    _values.Add(new EncodedValue(m_io, this, m_root));
                }
            }
            private VlqBase128Le _size;
            private List<EncodedValue> _values;
            private Dex m_root;
            private KaitaiStruct m_parent;
            public VlqBase128Le Size { get { return _size; } }
            public List<EncodedValue> Values { get { return _values; } }
            public Dex M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        private bool f_stringIds;
        private List<StringIdItem> _stringIds;

        /// <summary>
        /// string identifiers list.
        /// 
        /// These are identifiers for all the strings used by this file, either for
        /// internal naming (e.g., type descriptors) or as constant objects referred to by code.
        /// 
        /// This list must be sorted by string contents, using UTF-16 code point values
        /// (not in a locale-sensitive manner), and it must not contain any duplicate entries.
        /// </summary>
        public List<StringIdItem> StringIds
        {
            get
            {
                if (f_stringIds)
                    return _stringIds;
                long _pos = m_io.Pos;
                m_io.Seek(Header.StringIdsOff);
                _stringIds = new List<StringIdItem>();
                for (var i = 0; i < Header.StringIdsSize; i++)
                {
                    _stringIds.Add(new StringIdItem(m_io, this, m_root));
                }
                m_io.Seek(_pos);
                f_stringIds = true;
                return _stringIds;
            }
        }
        private bool f_methodIds;
        private List<MethodIdItem> _methodIds;

        /// <summary>
        /// method identifiers list.
        /// 
        /// These are identifiers for all methods referred to by this file,
        /// whether defined in the file or not.
        /// 
        /// This list must be sorted, where the defining type (by type_id index
        /// is the major order, method name (by string_id index) is the intermediate
        /// order, and method prototype (by proto_id index) is the minor order.
        /// 
        /// The list must not contain any duplicate entries.
        /// </summary>
        public List<MethodIdItem> MethodIds
        {
            get
            {
                if (f_methodIds)
                    return _methodIds;
                long _pos = m_io.Pos;
                m_io.Seek(Header.MethodIdsOff);
                _methodIds = new List<MethodIdItem>();
                for (var i = 0; i < Header.MethodIdsSize; i++)
                {
                    _methodIds.Add(new MethodIdItem(m_io, this, m_root));
                }
                m_io.Seek(_pos);
                f_methodIds = true;
                return _methodIds;
            }
        }
        private bool f_linkData;
        private byte[] _linkData;

        /// <summary>
        /// data used in statically linked files.
        /// 
        /// The format of the data in this section is left unspecified by this document.
        /// 
        /// This section is empty in unlinked files, and runtime implementations may
        /// use it as they see fit.
        /// </summary>
        public byte[] LinkData
        {
            get
            {
                if (f_linkData)
                    return _linkData;
                long _pos = m_io.Pos;
                m_io.Seek(Header.LinkOff);
                _linkData = m_io.ReadBytes(Header.LinkSize);
                m_io.Seek(_pos);
                f_linkData = true;
                return _linkData;
            }
        }
        private bool f_map;
        private MapList _map;
        public MapList Map
        {
            get
            {
                if (f_map)
                    return _map;
                long _pos = m_io.Pos;
                m_io.Seek(Header.MapOff);
                _map = new MapList(m_io, this, m_root);
                m_io.Seek(_pos);
                f_map = true;
                return _map;
            }
        }
        private bool f_classDefs;
        private List<ClassDefItem> _classDefs;

        /// <summary>
        /// class definitions list.
        /// 
        /// The classes must be ordered such that a given class's superclass and
        /// implemented interfaces appear in the list earlier than the referring class.
        /// 
        /// Furthermore, it is invalid for a definition for the same-named class to
        /// appear more than once in the list.
        /// </summary>
        public List<ClassDefItem> ClassDefs
        {
            get
            {
                if (f_classDefs)
                    return _classDefs;
                long _pos = m_io.Pos;
                m_io.Seek(Header.ClassDefsOff);
                _classDefs = new List<ClassDefItem>();
                for (var i = 0; i < Header.ClassDefsSize; i++)
                {
                    _classDefs.Add(new ClassDefItem(m_io, this, m_root));
                }
                m_io.Seek(_pos);
                f_classDefs = true;
                return _classDefs;
            }
        }
        private bool f_data;
        private byte[] _data;

        /// <summary>
        /// data area, containing all the support data for the tables listed above.
        /// 
        /// Different items have different alignment requirements, and padding bytes
        /// are inserted before each item if necessary to achieve proper alignment.
        /// </summary>
        public byte[] Data
        {
            get
            {
                if (f_data)
                    return _data;
                long _pos = m_io.Pos;
                m_io.Seek(Header.DataOff);
                _data = m_io.ReadBytes(Header.DataSize);
                m_io.Seek(_pos);
                f_data = true;
                return _data;
            }
        }
        private bool f_typeIds;
        private List<TypeIdItem> _typeIds;

        /// <summary>
        /// type identifiers list.
        /// 
        /// These are identifiers for all types (classes, arrays, or primitive types)
        /// referred to by this file, whether defined in the file or not.
        /// 
        /// This list must be sorted by string_id index, and it must not contain any duplicate entries.
        /// </summary>
        public List<TypeIdItem> TypeIds
        {
            get
            {
                if (f_typeIds)
                    return _typeIds;
                long _pos = m_io.Pos;
                m_io.Seek(Header.TypeIdsOff);
                _typeIds = new List<TypeIdItem>();
                for (var i = 0; i < Header.TypeIdsSize; i++)
                {
                    _typeIds.Add(new TypeIdItem(m_io, this, m_root));
                }
                m_io.Seek(_pos);
                f_typeIds = true;
                return _typeIds;
            }
        }
        private bool f_protoIds;
        private List<ProtoIdItem> _protoIds;

        /// <summary>
        /// method prototype identifiers list.
        /// 
        /// These are identifiers for all prototypes referred to by this file.
        /// 
        /// This list must be sorted in return-type (by type_id index) major order,
        /// and then by argument list (lexicographic ordering, individual arguments
        /// ordered by type_id index). The list must not contain any duplicate entries.
        /// </summary>
        public List<ProtoIdItem> ProtoIds
        {
            get
            {
                if (f_protoIds)
                    return _protoIds;
                long _pos = m_io.Pos;
                m_io.Seek(Header.ProtoIdsOff);
                _protoIds = new List<ProtoIdItem>();
                for (var i = 0; i < Header.ProtoIdsSize; i++)
                {
                    _protoIds.Add(new ProtoIdItem(m_io, this, m_root));
                }
                m_io.Seek(_pos);
                f_protoIds = true;
                return _protoIds;
            }
        }
        private bool f_fieldIds;
        private List<FieldIdItem> _fieldIds;

        /// <summary>
        /// field identifiers list.
        /// 
        /// These are identifiers for all fields referred to by this file, whether defined in the file or not.
        /// 
        /// This list must be sorted, where the defining type (by type_id index)
        /// is the major order, field name (by string_id index) is the intermediate
        /// order, and type (by type_id index) is the minor order.
        /// 
        /// The list must not contain any duplicate entries.
        /// </summary>
        public List<FieldIdItem> FieldIds
        {
            get
            {
                if (f_fieldIds)
                    return _fieldIds;
                long _pos = m_io.Pos;
                m_io.Seek(Header.FieldIdsOff);
                _fieldIds = new List<FieldIdItem>();
                for (var i = 0; i < Header.FieldIdsSize; i++)
                {
                    _fieldIds.Add(new FieldIdItem(m_io, this, m_root));
                }
                m_io.Seek(_pos);
                f_fieldIds = true;
                return _fieldIds;
            }
        }
        private HeaderItem _header;
        private Dex m_root;
        private KaitaiStruct m_parent;
        public HeaderItem Header { get { return _header; } }
        public Dex M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
