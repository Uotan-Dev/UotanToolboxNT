using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Advancedflash;

public partial class AdvancedflashViewModel : MainPageBase
{
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    [ObservableProperty]
    private AvaloniaList<FalshPartModel> falshPartModel = [];

    public AdvancedflashViewModel() : base(GetTranslation("Advancedflash_Name"), MaterialIconKind.CableData, -500)
    {

    }
}

public partial class FalshPartModel : ObservableObject
{
    [ObservableProperty]
    private bool select;

    [ObservableProperty]
    private bool selectDis = true;

    [ObservableProperty]
    private string command = string.Empty;

    [ObservableProperty]
    private string size = string.Empty;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string fileName = string.Empty;

    [ObservableProperty]
    private string fullFilePath = string.Empty;
}