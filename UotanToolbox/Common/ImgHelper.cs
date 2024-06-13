using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UotanToolbox.Common
{
    internal class ImgHelper
    {

    }
    public class BootImg(string ImgPath = "")
    {
        private readonly string _ImgPath =ImgPath;
        public bool Init()
        {
            return true;
        }
    }
}
