using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls
{
    public partial class GroupBox : UserControl
    {
        public GroupBox()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<object?> HeaderProperty =
            AvaloniaProperty.Register<GroupBox, object?>(nameof(Header), defaultValue: "Header");

        public object? Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }
    }
}