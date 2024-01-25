using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;

namespace SukiUI.Demo.Features.Dashboard;

public partial class DialogViewModel : ObservableObject
{
    [RelayCommand]
    public void CloseDialog()
    {
        SukiHost.CloseDialog();
    }
}