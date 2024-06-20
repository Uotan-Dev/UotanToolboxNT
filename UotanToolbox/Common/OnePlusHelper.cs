using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UotanToolbox.Common
{
    public class DeviceConfig
    {
        public int Version { get; set; }
        public string? Cm { get; set; }
        public int ParamMode { get; set; }
    }

    public class OnePlus(string ModelVerifyPrjName, int serial, int ATOBuild = 0, int Flash_Mode = 0, int cf = 0, string[] supported_functions = null)
    {
        private readonly Dictionary<string, DeviceConfig> _deviceConfigs = new Dictionary<string, DeviceConfig>
            {
            // OP5, cheeseburger
            {"16859", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // OP5t, dumpling
            {"17801", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // OP6, enchilada
            {"17819", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // OP6t, fajita
            {"18801", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // OP6t T-Mo, fajitat
            {"18811", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // Oneplus 7 Pro 5G Sprint, guacamoles
            {"18825", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // Oneplus 7 Pro 5G EE and Elisa, guacamoleg
            {"18827", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // Oneplus 7 Pro T-Mo, guacamolet
            {"18831", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // Oneplus 7t, hotdogb
            {"18865", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // Oneplus 7t T-Mo, hotdogt
            {"19863", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // Oneplus 7t Pro, hotdog
            {"19801", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // Oneplus 7t Pro 5G T-Mo, hotdogg
            {"19861", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // OP8, instantnoodle
            {"19821", new DeviceConfig {Version = 2, Cm = "0cffee8a", ParamMode = 0}},
            // OP8 T-Mo, instantnoodlet
            {"19855", new DeviceConfig {Version = 2, Cm = "0cffee8a", ParamMode = 0}},
            // OP8, instantnoodle
            {"19821", new DeviceConfig {Version = 2, Cm = "6d9215b4", ParamMode = 0}},
            // OP8 Verizon, instantnoodlev
            {"19867", new DeviceConfig {Version = 2, Cm = "4107b2d4", ParamMode = 0}},
            // OP8 Visible, instantnoodlevis
            {"19868", new DeviceConfig {Version = 2, Cm = "178d8213", ParamMode = 0}},
            // OP8 Pro, instantnoodlep
            {"19811", new DeviceConfig {Version = 2, Cm = "40217c07", ParamMode = 0}},
            // OP8t, kebab
            {"19805", new DeviceConfig {Version = 2, Cm = "1a5ec176", ParamMode = 0}},
            // OP8t T-Mo, kebabt
            {"20809", new DeviceConfig {Version = 2, Cm = "d6bc8c36", ParamMode = 0}},
            // OP9 Pro, lemonadep
            {"19815", new DeviceConfig {Version = 2, Cm = "9c151c7f", ParamMode = 0}},
            {"20859", new DeviceConfig {Version = 2, Cm = "9c151c7f", ParamMode = 0}},
            {"20857", new DeviceConfig {Version = 2, Cm = "9c151c7f", ParamMode = 0}},
            // OP9, lemonade
            {"19825", new DeviceConfig {Version = 2, Cm = "0898dcd6", ParamMode = 0}},
            {"20851", new DeviceConfig {Version = 2, Cm = "0898dcd6", ParamMode = 0}},
            {"20852", new DeviceConfig {Version = 2, Cm = "0898dcd6", ParamMode = 0}},
            {"20853", new DeviceConfig {Version = 2, Cm = "0898dcd6", ParamMode = 0}},
            // OP9R, lemonades
            {"20828", new DeviceConfig {Version = 2, Cm = "f498b60f", ParamMode = 0}},
            {"20838", new DeviceConfig {Version = 2, Cm = "f498b60f", ParamMode = 0}},
            // OP9 TMO, lemonadet
            {"20854", new DeviceConfig {Version = 2, Cm = "16225d4e", ParamMode = 0}},
            // OP9 Pro TMO, lemonadept
            {"2085A", new DeviceConfig {Version = 2, Cm = "7f19519a", ParamMode = 0}},
            // OP Nord, avicii
            {"20801", new DeviceConfig {Version = 2, Cm = "eacf50e7", ParamMode = 0}},
            // OP N10 5G Metro, billie8t
            {"20885", new DeviceConfig {Version = 3, Cm = "3a403a71", ParamMode = 1}},
            // OP N10 5G Global, billie8
            {"20886", new DeviceConfig {Version = 3, Cm = "b8bd9e39", ParamMode = 1}},
            // OP N10 5G TMO, billie8t
            {"20888", new DeviceConfig {Version = 3, Cm = "142f1bd7", ParamMode = 1}},
            // OP N10 5G Europe, billie8
            {"20889", new DeviceConfig {Version = 3, Cm = "f2056ae1", ParamMode = 1}},
            // OP N100 Metro, billie2t
            {"20880", new DeviceConfig {Version = 3, Cm = "6ccf5913", ParamMode = 1}},
            // OP N100 Global, billie2
            {"20881", new DeviceConfig {Version = 3, Cm = "fa9ff378", ParamMode = 1}},
            // OP N100 TMO, billie2t
            {"20882", new DeviceConfig {Version = 3, Cm = "4ca1e84e", ParamMode = 1}},
            // OP N100 Europe, billie2
            {"20883", new DeviceConfig {Version = 3, Cm = "ad9dba4a", ParamMode = 1}},
            // dre8t
            {"20818", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // dre8m
            {"2083C", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // dre9
            {"2083D", new DeviceConfig {Version = 1, Cm = null, ParamMode = 0}},
            // op nord ce, ebba
            {"20813", new DeviceConfig {Version = 2, Cm = "48ad7b61", ParamMode = 0}},
        };
        /// <summary>
        /// 根据设备代号来检索对应的算法序号，CM值与param模式
        /// </summary>
        /// <param name="ModelVerifyPrjName"></param>
        /// <returns>设备配置</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public DeviceConfig GetConfig(string ModelVerifyPrjName)
        {
            if (_deviceConfigs.TryGetValue(ModelVerifyPrjName, out DeviceConfig config))
            {
                return config;
            }
            else
            {
                throw new KeyNotFoundException($"The device configuration for key '{ModelVerifyPrjName}' was not found.");
            }
        }
    }
}


    

