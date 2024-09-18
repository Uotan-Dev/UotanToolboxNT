using System.IO;
using Avalonia.Controls.Notifications;
using SukiUI.Dialogs;

namespace UotanToolbox.Common
{
    // 修补FRP文件的代码，工具箱暂未启用此功能！-zicai
    public class FrpPatcher
    {
        readonly string _filePath;
        readonly string _function;
        ISukiDialogManager dialogManager;
        public FrpPatcher(string filePath, string function)
        {
            _filePath = filePath;
            _function = function.ToLower();
        }
        public bool Run()
        {
            byte target = 0x02;

            if (_function == "oemunlockon")
            {
                target = 0x01;
            }

            if (_function == "oemunlockoff")
            {
                target = 0x00;
            }

            if (_function is not "oemunlockon" and not "oemunlockoff")
            {
                _ = dialogManager.CreateDialog().WithTitle("Error").WithActionButton("知道了", _ => { }, true).WithContent("{%c_e%}参数错误{%c_i%}{\n}").TryShow();
                return false;
            }

            if (!File.Exists(_filePath))
            {
                _ = dialogManager.CreateDialog().WithTitle("Error").WithActionButton("知道了", _ => { }, true).WithContent("{%c_e%}找不到{_filePath}{%c_i%}{\n}").TryShow();
                return false;
            }

            var fileBytes = File.ReadAllBytes(_filePath);
            var lastByte = fileBytes[^1];

            if (lastByte is not 0x00 and not 0x01)
            {
                _ = dialogManager.CreateDialog().WithTitle("Error").WithActionButton("知道了", _ => { }, true).WithContent("frp文件末尾1字节16进制数值不是00或01").TryShow();
                return false;
            }

            if (lastByte == target)
            {
                return true;
            }

            try
            {
                var bytes = File.ReadAllBytes(_filePath);
                bytes[^1] = target;
                File.WriteAllBytes(_filePath, bytes);
                _ = dialogManager.CreateDialog().WithTitle("Error").OfType(NotificationType.Error).WithContent("frp文件修补成功").Dismiss().ByClickingBackground().TryShow();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}