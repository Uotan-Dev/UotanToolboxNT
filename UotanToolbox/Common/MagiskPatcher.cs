using SukiUI.Theme;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UotanToolbox.Common
{
    internal class MagiskPatcher(string imgPath,bool verity=true,bool encrypt=true,bool vbmeta =false,bool rec =false,bool legacysar=true)
    {
        private readonly string _imgPath = imgPath;
        private readonly bool _verity = verity;
        private readonly bool _encrypt = encrypt;
        private readonly bool _vbmeta = vbmeta;
        private readonly bool _rec = rec;
        private readonly bool _legacysar = legacysar;
        public bool Setup()
        {
            return true;
        }



    }
}
