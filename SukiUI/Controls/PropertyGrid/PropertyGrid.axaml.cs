using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace SukiUI.Controls
{
    public partial class PropertyGrid : UserControl
    {
        static PropertyGrid()
        {
            ItemProperty.Changed.Subscribe(OnItemChanged);
        }

        public PropertyGrid()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<INotifyPropertyChanged?> ItemProperty = AvaloniaProperty.Register<PropertyGrid, INotifyPropertyChanged?>(nameof(Item), defaultValue: null);

        public INotifyPropertyChanged? Item
        {
            get { return GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly StyledProperty<InstanceViewModel?> InstanceProperty = AvaloniaProperty.Register<PropertyGrid, InstanceViewModel?>(nameof(Instance), defaultValue: null);

        public InstanceViewModel? Instance
        {
            get { return GetValue(InstanceProperty); }
            set { SetValue(InstanceProperty, value); }
        }

        private static void OnItemChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is PropertyGrid propertyGrid)
            {
                propertyGrid.OnItemChanged(e.NewValue);
            }
        }

        private void OnItemChanged(object? newValue)
        {
            SetItem(newValue as INotifyPropertyChanged);
        }

        private void SetItem(INotifyPropertyChanged? item)
        {
            if (item is null)
            {
                return;
            }

            Instance = new InstanceViewModel(item);
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);

            Instance?.Dispose();
        }
    }
}