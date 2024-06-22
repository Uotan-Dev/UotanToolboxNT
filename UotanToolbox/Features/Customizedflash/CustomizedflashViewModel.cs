using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Customizedflash;

public partial class CustomizedflashViewModel : MainPageBase
{
    [ObservableProperty] private string _fastbootFile, _fastbootdFile, _adbsideloadFile, _fastbootupdatedFile;

    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public CustomizedflashViewModel() : base(GetTranslation("Sidebar_Customizedflash"), MaterialIconKind.WrenchCogOutline, -500)
    {
    }
}