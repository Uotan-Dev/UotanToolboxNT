using Material.Icons;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Basicflash;

public partial class BasicflashViewModel : MainPageBase
{
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public BasicflashViewModel() : base(GetTranslation("Sidebar_Basicflash"), MaterialIconKind.CableData, 50, "基本刷入解锁文件解锁码解锁基本命令解锁刷入Recoveryrecovery临时启动安装ADB和Fastboot驱动adbfastboot补丁高通9008驱动小米设备USB3.0usb3.0修补BootbootMagiskmagiskAnyKernelSUanykernelsu")
    {
    }
}