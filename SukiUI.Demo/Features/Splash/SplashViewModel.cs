using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Demo.Features.Dashboard;
using SukiUI.Demo.Services;

namespace SukiUI.Demo.Features.Splash;

public partial class SplashViewModel(PageNavigationService nav) : DemoPageBase("Welcome", MaterialIconKind.Hand, int.MinValue)
{
    [RelayCommand]
    public void OpenDashboard()
    {
        nav.RequestNavigation<DashboardViewModel>();
    }
}