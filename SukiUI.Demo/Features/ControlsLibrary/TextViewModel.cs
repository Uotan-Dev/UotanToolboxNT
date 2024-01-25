using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Controls;
using System.Threading.Tasks;

namespace SukiUI.Demo.Features.ControlsLibrary;

public partial class TextViewModel() : DemoPageBase("Text", MaterialIconKind.Text)
{
    private const string DefaultText = "Hello, World!";

    [ObservableProperty] private string _textBoxValue = DefaultText;
    [ObservableProperty] private string _textBlockValue = DefaultText;

    [RelayCommand]
    public Task HyperlinkClicked()
    {
        return SukiHost.ShowToast("Clicked a hyperlink", "You clicked the hyperlink on the Text page.");
    }
}