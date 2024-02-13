using Avalonia.Collections;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UotanToolbox.Common
{
    internal class Global
    {
        public static string System = "Windows";
        public static AvaloniaList<string> deviceslist;
        public static string thisdevice = null;
    }

    public static class GlobalData
    {
        public static SukiUIDemoViewModel SukiUIDemoViewModelInstance { get; set; }
    }

}