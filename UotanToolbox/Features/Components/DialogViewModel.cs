using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;

namespace UotanToolbox.Features.Dashboard;

public partial class DialogViewModel : ObservableObject
{
    [RelayCommand]
    public void CloseDialog()
    {
        SukiHost.CloseDialog();
    }
}