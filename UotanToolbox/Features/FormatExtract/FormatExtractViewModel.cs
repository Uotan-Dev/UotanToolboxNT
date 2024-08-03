using Material.Icons;
using UotanToolbox.Common;

namespace UotanToolbox.Features.FormatExtract;

public partial class FormatExtractViewModel : MainPageBase
{
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public FormatExtractViewModel() : base(GetTranslation("Sidebar_FormatExtract"), MaterialIconKind.AccountHardHatOutline, -400, "格式化&提取QCNqcnSuperEmptysuperempty格式化提取分区物理分区虚拟分区EXT4F2FSFAT32EXFATNTFSext4f2fsfat32exfatntfs")
    {
    }
}