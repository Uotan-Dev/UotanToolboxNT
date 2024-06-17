using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls
{
    public partial class MessageBox : UserControl
    {
        public MessageBox()
        {
            InitializeComponent();
        }
        
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            e.NameScope.Get<Button>("ButtonAction").Click += ((sender, args) => _onActionCallback?.Invoke());
        }
        
        public Action? _onActionCallback;
        
        public static readonly StyledProperty<object?> IconProperty =
            AvaloniaProperty.Register<MessageBox, object?>(nameof(Icon));

        public object? Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
    
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<MessageBox, string>(nameof(Title));

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
    
        public static readonly StyledProperty<bool> ShowActionButtonProperty =
            AvaloniaProperty.Register<MessageBox, bool>(nameof(ShowActionButton));

        public bool ShowActionButton
        {
            get => GetValue(ShowActionButtonProperty);
            set => SetValue(ShowActionButtonProperty, value);
        }
    
        public static readonly StyledProperty<string> ActionButtonContentProperty =
            AvaloniaProperty.Register<MessageBox, string>(nameof(ActionButtonContent));

        public string ActionButtonContent
        {
            get => GetValue(ActionButtonContentProperty);
            set => SetValue(ActionButtonContentProperty, value);
        }
    }
}