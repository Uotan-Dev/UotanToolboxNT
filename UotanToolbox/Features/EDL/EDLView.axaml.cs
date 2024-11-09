using Avalonia.Controls;
using UotanToolbox.Common;

namespace UotanToolbox.Features.EDL;

public partial class EDLView : UserControl
{
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public EDLView()
    {
        InitializeComponent();
    }
}