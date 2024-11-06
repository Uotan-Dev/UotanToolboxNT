// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using System.Collections.Generic;

namespace Kaitai
{

    /// <summary>
    /// A bootloader for Android used on various devices powered by Qualcomm
    /// Snapdragon chips:
    /// 
    /// &lt;https://en.wikipedia.org/wiki/Devices_using_Qualcomm_Snapdragon_processors&gt;
    /// 
    /// Although not all of the Snapdragon based Android devices use this particular
    /// bootloader format, it is known that devices with the following chips have used
    /// it (example devices are given for each chip):
    /// 
    /// * APQ8064 ([devices](https://en.wikipedia.org/wiki/Devices_using_Qualcomm_Snapdragon_processors#Snapdragon_S4_Pro))
    ///   - Nexus 4 &quot;mako&quot;: [sample][sample-mako] ([other samples][others-mako]),
    ///     [releasetools.py](https://android.googlesource.com/device/lge/mako/+/33f0114/releasetools.py#98)
    /// 
    /// * MSM8974AA ([devices](https://en.wikipedia.org/wiki/Devices_using_Qualcomm_Snapdragon_processors#Snapdragon_800,_801_and_805_(2013/14)))
    ///   - Nexus 5 &quot;hammerhead&quot;: [sample][sample-hammerhead] ([other samples][others-hammerhead]),
    ///     [releasetools.py](https://android.googlesource.com/device/lge/hammerhead/+/7618a7d/releasetools.py#116)
    /// 
    /// * MSM8992 ([devices](https://en.wikipedia.org/wiki/Devices_using_Qualcomm_Snapdragon_processors#Snapdragon_808_and_810_(2015)))
    ///   - Nexus 5X &quot;bullhead&quot;: [sample][sample-bullhead] ([other samples][others-bullhead]),
    ///     [releasetools.py](https://android.googlesource.com/device/lge/bullhead/+/2994b6b/releasetools.py#126)
    /// 
    /// * APQ8064-1AA ([devices](https://en.wikipedia.org/wiki/Devices_using_Qualcomm_Snapdragon_processors#Snapdragon_600_(2013)))
    ///   - Nexus 7 \[2013] (Mobile) &quot;deb&quot; &lt;a href=&quot;#doc-note-data-after-img-bodies&quot;&gt;(\**)&lt;/a&gt;: [sample][sample-deb] ([other samples][others-deb]),
    ///     [releasetools.py](https://android.googlesource.com/device/asus/deb/+/14c1638/releasetools.py#105)
    ///   - Nexus 7 \[2013] (Wi-Fi) &quot;flo&quot; &lt;a href=&quot;#doc-note-data-after-img-bodies&quot;&gt;(\**)&lt;/a&gt;: [sample][sample-flo] ([other samples][others-flo]),
    ///     [releasetools.py](https://android.googlesource.com/device/asus/flo/+/9d9fee9/releasetools.py#130)
    /// 
    /// * MSM8996 Pro-AB ([devices](https://en.wikipedia.org/wiki/Devices_using_Qualcomm_Snapdragon_processors#Snapdragon_820_and_821_(2016)))
    ///   - Pixel &quot;sailfish&quot; &lt;a href=&quot;#doc-note-bootloader-size&quot;&gt;(\*)&lt;/a&gt;:
    ///     [sample][sample-sailfish] ([other samples][others-sailfish])
    ///   - Pixel XL &quot;marlin&quot; &lt;a href=&quot;#doc-note-bootloader-size&quot;&gt;(\*)&lt;/a&gt;:
    ///     [sample][sample-marlin] ([other samples][others-marlin])
    /// 
    /// * MSM8998 ([devices](https://en.wikipedia.org/wiki/Devices_using_Qualcomm_Snapdragon_processors#Snapdragon_835_(2017)))
    ///   - Pixel 2 &quot;walleye&quot; &lt;a href=&quot;#doc-note-bootloader-size&quot;&gt;(\*)&lt;/a&gt;: [sample][sample-walleye] ([other samples][others-walleye])
    ///   - Pixel 2 XL &quot;taimen&quot;: [sample][sample-taimen] ([other samples][others-taimen])
    /// 
    /// &lt;small id=&quot;doc-note-bootloader-size&quot;&gt;(\*)
    /// `bootloader_size` is equal to the size of the whole file (not just `img_bodies` as usual).
    /// &lt;/small&gt;
    /// 
    /// &lt;small id=&quot;doc-note-data-after-img-bodies&quot;&gt;(\**)
    /// There are some data after the end of `img_bodies`.
    /// &lt;/small&gt;
    /// 
    /// ---
    /// 
    /// On the other hand, devices with these chips **do not** use this format:
    /// 
    /// * &lt;del&gt;APQ8084&lt;/del&gt; ([devices](https://en.wikipedia.org/wiki/Devices_using_Qualcomm_Snapdragon_processors#Snapdragon_800,_801_and_805_(2013/14)))
    ///   - Nexus 6 &quot;shamu&quot;: [sample][foreign-sample-shamu] ([other samples][foreign-others-shamu]),
    ///     [releasetools.py](https://android.googlesource.com/device/moto/shamu/+/df9354d/releasetools.py#12) -
    ///     uses &quot;Motoboot packed image format&quot; instead
    /// 
    /// * &lt;del&gt;MSM8994&lt;/del&gt; ([devices](https://en.wikipedia.org/wiki/Devices_using_Qualcomm_Snapdragon_processors#Snapdragon_808_and_810_(2015)))
    ///   - Nexus 6P &quot;angler&quot;: [sample][foreign-sample-angler] ([other samples][foreign-others-angler]),
    ///     [releasetools.py](https://android.googlesource.com/device/huawei/angler/+/cf92cd8/releasetools.py#29) -
    ///     uses &quot;Huawei Bootloader packed image format&quot; instead
    /// 
    /// [sample-mako]: https://androidfilehost.com/?fid=96039337900113996 &quot;bootloader-mako-makoz30f.img&quot;
    /// [others-mako]: https://androidfilehost.com/?w=search&amp;s=bootloader-mako&amp;type=files
    /// 
    /// [sample-hammerhead]: https://androidfilehost.com/?fid=385035244224410247 &quot;bootloader-hammerhead-hhz20h.img&quot;
    /// [others-hammerhead]: https://androidfilehost.com/?w=search&amp;s=bootloader-hammerhead&amp;type=files
    /// 
    /// [sample-bullhead]: https://androidfilehost.com/?fid=11410963190603870177 &quot;bootloader-bullhead-bhz32c.img&quot;
    /// [others-bullhead]: https://androidfilehost.com/?w=search&amp;s=bootloader-bullhead&amp;type=files
    /// 
    /// [sample-deb]: https://androidfilehost.com/?fid=23501681358552487 &quot;bootloader-deb-flo-04.02.img&quot;
    /// [others-deb]: https://androidfilehost.com/?w=search&amp;s=bootloader-deb-flo&amp;type=files
    /// 
    /// [sample-flo]: https://androidfilehost.com/?fid=23991606952593542 &quot;bootloader-flo-flo-04.05.img&quot;
    /// [others-flo]: https://androidfilehost.com/?w=search&amp;s=bootloader-flo-flo&amp;type=files
    /// 
    /// [sample-sailfish]: https://androidfilehost.com/?fid=6006931924117907154 &quot;bootloader-sailfish-8996-012001-1904111134.img&quot;
    /// [others-sailfish]: https://androidfilehost.com/?w=search&amp;s=bootloader-sailfish&amp;type=files
    /// 
    /// [sample-marlin]: https://androidfilehost.com/?fid=6006931924117907131 &quot;bootloader-marlin-8996-012001-1904111134.img&quot;
    /// [others-marlin]: https://androidfilehost.com/?w=search&amp;s=bootloader-marlin&amp;type=files
    /// 
    /// [sample-walleye]: https://androidfilehost.com/?fid=14943124697586348540 &quot;bootloader-walleye-mw8998-003.0085.00.img&quot;
    /// [others-walleye]: https://androidfilehost.com/?w=search&amp;s=bootloader-walleye&amp;type=files
    /// 
    /// [sample-taimen]: https://androidfilehost.com/?fid=14943124697586348536 &quot;bootloader-taimen-tmz30m.img&quot;
    /// [others-taimen]: https://androidfilehost.com/?w=search&amp;s=bootloader-taimen&amp;type=files
    /// 
    /// [foreign-sample-shamu]: https://androidfilehost.com/?fid=745849072291678307 &quot;bootloader-shamu-moto-apq8084-72.04.img&quot;
    /// [foreign-others-shamu]: https://androidfilehost.com/?w=search&amp;s=bootloader-shamu&amp;type=files
    /// 
    /// [foreign-sample-angler]: https://androidfilehost.com/?fid=11410963190603870158 &quot;bootloader-angler-angler-03.84.img&quot;
    /// [foreign-others-angler]: https://androidfilehost.com/?w=search&amp;s=bootloader-angler&amp;type=files
    /// 
    /// ---
    /// 
    /// The `bootloader-*.img` samples referenced above originally come from factory
    /// images packed in ZIP archives that can be found on the page [Factory Images
    /// for Nexus and Pixel Devices](https://developers.google.com/android/images) on
    /// the Google Developers site. Note that the codenames on that page may be
    /// different than the ones that are written in the list above. That's because the
    /// Google page indicates **ROM codenames** in headings (e.g. &quot;occam&quot; for Nexus 4)
    /// but the above list uses **model codenames** (e.g. &quot;mako&quot; for Nexus 4) because
    /// that is how the original `bootloader-*.img` files are identified. For most
    /// devices, however, these code names are the same.
    /// </summary>
    /// <remarks>
    /// Reference: <a href="https://android.googlesource.com/device/lge/hammerhead/+/7618a7d/releasetools.py">Source</a>
    /// </remarks>
    public partial class AndroidBootldrQcom : KaitaiStruct
    {
        public static AndroidBootldrQcom FromFile(string fileName)
        {
            return new AndroidBootldrQcom(new KaitaiStream(fileName));
        }

        public AndroidBootldrQcom(KaitaiStream p__io, KaitaiStruct p__parent = null, AndroidBootldrQcom p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            f_imgBodies = false;
            _read();
        }
        private void _read()
        {
            _magic = m_io.ReadBytes(8);
            if (!((KaitaiStream.ByteArrayCompare(Magic, new byte[] { 66, 79, 79, 84, 76, 68, 82, 33 }) == 0)))
            {
                throw new ValidationNotEqualError(new byte[] { 66, 79, 79, 84, 76, 68, 82, 33 }, Magic, M_Io, "/seq/0");
            }
            _numImages = m_io.ReadU4le();
            _ofsImgBodies = m_io.ReadU4le();
            _bootloaderSize = m_io.ReadU4le();
            _imgHeaders = new List<ImgHeader>();
            for (var i = 0; i < NumImages; i++)
            {
                _imgHeaders.Add(new ImgHeader(m_io, this, m_root));
            }
        }
        public partial class ImgHeader : KaitaiStruct
        {
            public static ImgHeader FromFile(string fileName)
            {
                return new ImgHeader(new KaitaiStream(fileName));
            }

            public ImgHeader(KaitaiStream p__io, AndroidBootldrQcom p__parent = null, AndroidBootldrQcom p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _name = System.Text.Encoding.GetEncoding("ASCII").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(64), 0, false));
                _lenBody = m_io.ReadU4le();
            }
            private string _name;
            private uint _lenBody;
            private AndroidBootldrQcom m_root;
            private AndroidBootldrQcom m_parent;
            public string Name { get { return _name; } }
            public uint LenBody { get { return _lenBody; } }
            public AndroidBootldrQcom M_Root { get { return m_root; } }
            public AndroidBootldrQcom M_Parent { get { return m_parent; } }
        }
        public partial class ImgBody : KaitaiStruct
        {
            public ImgBody(int p_idx, KaitaiStream p__io, AndroidBootldrQcom p__parent = null, AndroidBootldrQcom p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _idx = p_idx;
                f_imgHeader = false;
                _read();
            }
            private void _read()
            {
                _body = m_io.ReadBytes(ImgHeader.LenBody);
            }
            private bool f_imgHeader;
            private ImgHeader _imgHeader;
            public ImgHeader ImgHeader
            {
                get
                {
                    if (f_imgHeader)
                        return _imgHeader;
                    _imgHeader = (ImgHeader) (M_Root.ImgHeaders[Idx]);
                    f_imgHeader = true;
                    return _imgHeader;
                }
            }
            private byte[] _body;
            private int _idx;
            private AndroidBootldrQcom m_root;
            private AndroidBootldrQcom m_parent;
            public byte[] Body { get { return _body; } }
            public int Idx { get { return _idx; } }
            public AndroidBootldrQcom M_Root { get { return m_root; } }
            public AndroidBootldrQcom M_Parent { get { return m_parent; } }
        }
        private bool f_imgBodies;
        private List<ImgBody> _imgBodies;
        public List<ImgBody> ImgBodies
        {
            get
            {
                if (f_imgBodies)
                    return _imgBodies;
                long _pos = m_io.Pos;
                m_io.Seek(OfsImgBodies);
                _imgBodies = new List<ImgBody>();
                for (var i = 0; i < NumImages; i++)
                {
                    _imgBodies.Add(new ImgBody(i, m_io, this, m_root));
                }
                m_io.Seek(_pos);
                f_imgBodies = true;
                return _imgBodies;
            }
        }
        private byte[] _magic;
        private uint _numImages;
        private uint _ofsImgBodies;
        private uint _bootloaderSize;
        private List<ImgHeader> _imgHeaders;
        private AndroidBootldrQcom m_root;
        private KaitaiStruct m_parent;
        public byte[] Magic { get { return _magic; } }
        public uint NumImages { get { return _numImages; } }
        public uint OfsImgBodies { get { return _ofsImgBodies; } }

        /// <summary>
        /// According to all available `releasetools.py` versions from AOSP (links are
        /// in the top-level `/doc`), this should determine only the size of
        /// `img_bodies` - there is [an assertion](
        /// https://android.googlesource.com/device/lge/hammerhead/+/7618a7d/releasetools.py#167)
        /// for it.
        /// 
        /// However, files for certain Pixel devices (see `/doc`) apparently declare
        /// the entire file size here (i.e. including also fields from `magic` to
        /// `img_headers`). So if you interpreted `bootloader_size` as the size of
        /// `img_bodies` substream in these files, you would exceed the end of file.
        /// Although you could check that it fits in the file before attempting to
        /// create a substream of that size, you wouldn't know if it's meant to
        /// specify the size of just `img_bodies` or the size of the entire bootloader
        /// payload (whereas there may be additional data after the end of payload)
        /// until parsing `img_bodies` (or at least summing sizes from `img_headers`,
        /// but that's stupid).
        /// 
        /// So this field isn't reliable enough to be used as the size of any
        /// substream. If you want to check if it has a reasonable value, do so in
        /// your application code.
        /// </summary>
        public uint BootloaderSize { get { return _bootloaderSize; } }
        public List<ImgHeader> ImgHeaders { get { return _imgHeaders; } }
        public AndroidBootldrQcom M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
