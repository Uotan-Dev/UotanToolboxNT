using Avalonia.Controls;
using Avalonia.Threading;

namespace SukiUI.Demo.Features.Splash;

public partial class SplashView : UserControl
{
    public SplashView()
    {
        SukiTheme.GetInstance().OnBaseThemeChanged += _ =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                TextBlockWithInline.InvalidateVisual();
            });
        };
        InitializeComponent();
    }
}