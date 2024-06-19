using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace UotanToolbox.Features.Customizedflash;

public partial class CustomizedflashViewModel : MainPageBase
{
    [ObservableProperty] private string _fastbootFile, _fastbootdFile, _adbsideloadFile, _fastbootupdatedFile;

    public CustomizedflashViewModel() : base("自定义刷入", MaterialIconKind.WrenchCogOutline, -500)
    {
    }
}