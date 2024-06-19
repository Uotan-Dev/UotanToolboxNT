using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using System.Globalization;
using System.Resources;

namespace UotanToolbox.Features.Wiredflash;

public partial class WiredflashViewModel : MainPageBase
{
    [ObservableProperty] private string _fastbootFile, _fastbootdFile, _adbsideloadFile, _fastbootupdatedFile;

    private static readonly ResourceManager resMgr = new ResourceManager("UotanToolbox.Assets.Resources", typeof(App).Assembly);
    private static string GetTranslation(string key) => resMgr.GetString(key, CultureInfo.CurrentCulture) ?? "?????";

    public WiredflashViewModel() : base(GetTranslation("Sidebar_WiredFlash"), MaterialIconKind.WrenchCogOutline, -600)
    {
    }
}