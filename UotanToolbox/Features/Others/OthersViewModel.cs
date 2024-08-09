using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using System.ComponentModel.DataAnnotations;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Others;

public partial class OthersViewModel : MainPageBase
{
    [ObservableProperty][Range(1d, 4d)] private float _fontScale = 0, _windowScale = 0, _transitionScale = 0, _animationLast = 0;

    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public OthersViewModel() : base(GetTranslation("Sidebar_Others"), MaterialIconKind.WrenchCogOutline, 300, "显示属性电池模拟非充电模拟无线充电模拟USB充电模拟直流充电锁屏时间图标隐藏字体调节动画速度窗口缩放过渡缩放动画时长")
    {
    }
}