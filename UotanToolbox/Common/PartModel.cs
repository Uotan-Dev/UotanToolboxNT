using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UotanToolbox.Common
{
    public class PartModel
    {
        public string Id { get; set; }
        public string Startpoint { get; set; }
        public string Endpoint { get; set; }
        public string Size { get; set; }
        public string Format { get; set; }
        public string Name { get; set; }
        public string Sign { get; set; }
        public PartModel(string _id, string _startpoint, string _endpoint, string _size, string _format, string _name, string _sign)
        {
            Id = _id;
            Startpoint = _startpoint;
            Endpoint = _endpoint;
            Size = _size;
            Format = _format;
            Name = _name;
            Sign = _sign;
        }
    }
}
