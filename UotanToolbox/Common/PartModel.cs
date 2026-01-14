namespace UotanToolbox.Common
{
    public class PartModel(string _iD, string _startPoint, string _endPoint, string _size, string _format, string _name, string _sign)
    {
        public string ID { get; set; } = _iD;
        public string StartPoint { get; set; } = _startPoint;
        public string EndPoint { get; set; } = _endPoint;
        public string Size { get; set; } = _size;
        public string Format { get; set; } = _format;
        public string Name { get; set; } = _name;
        public string Sign { get; set; } = _sign;
    }
}
