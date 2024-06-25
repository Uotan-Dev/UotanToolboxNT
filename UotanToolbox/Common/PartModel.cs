using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UotanToolbox.Common
{
    public class PartModel
    {
        public string ID { get; set; }
        public string StartPoint { get; set; }
        public string EndPoint { get; set; }
        public string Size { get; set; }
        public string Format { get; set; }
        public string Name { get; set; }
        public string Sign { get; set; }
        public PartModel(string _iD, string _startPoint, string _endPoint, string _size, string _format, string _name, string _sign)
        {
            ID = _iD;
            StartPoint = _startPoint;
            EndPoint = _endPoint;
            Size = _size;
            Format = _format;
            Name = _name;
            Sign = _sign;
        }
    }
}
