using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls
{
    public partial class Loading : UserControl
    {
        public Loading()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}