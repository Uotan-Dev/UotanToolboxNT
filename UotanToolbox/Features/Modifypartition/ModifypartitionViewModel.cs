using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using System.Linq;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Modifypartition;

public partial class ModifypartitionViewModel : MainPageBase
{
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public ModifypartitionViewModel() : base(GetTranslation("Sidebar_ModifyPartition"), MaterialIconKind.WrenchCogOutline, -400)
    {
    }
}