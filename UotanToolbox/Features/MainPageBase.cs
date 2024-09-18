using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace UotanToolbox.Features;

public abstract partial class MainPageBase(string displayName, MaterialIconKind icon, int index = 0) : ObservableValidator
{
    [ObservableProperty] string _displayName = displayName;
    [ObservableProperty] MaterialIconKind _icon = icon;
    [ObservableProperty] int _index = index;
}