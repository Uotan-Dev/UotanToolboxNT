using Material.Icons;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Modifypartition;

public partial class ModifypartitionViewModel : MainPageBase
{
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public ModifypartitionViewModel() : base(GetTranslation("Sidebar_ModifyPartition"), MaterialIconKind.ChartPieOutline, -300, "修改分区删除分区创建分区读取分区表解除数量限制")
    {
    }
}