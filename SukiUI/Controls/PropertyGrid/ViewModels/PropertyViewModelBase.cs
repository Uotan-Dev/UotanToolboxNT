using SukiUI.Helpers;
using System.ComponentModel;
using System.Reflection;

namespace SukiUI.Controls
{
    public abstract class PropertyViewModelBase<T> : SukiObservableObject, IPropertyViewModel<T?>
    {
        private readonly string _propertyName;

        private T? _value;

        public T? Value
        {
            get => _value;
            set
            {
                if (SetAndRaise(ref _value, value))
                {
                    ViewModelSetter(value);
                }
            }
        }

        object? IPropertyViewModel.Value
        {
            get => Value;
            set => Value = (T?)value;
        }

        public string DisplayName { get; }
        public bool IsReadOnly { get; init; }
        protected PropertyInfo PropertyInfo { get; }
        protected INotifyPropertyChanged Viewmodel { get; }

        public PropertyViewModelBase(INotifyPropertyChanged viewmodel, string displayName, PropertyInfo propertyInfo)
        {
            Viewmodel = viewmodel;
            DisplayName = displayName;
            PropertyInfo = propertyInfo;
            IsReadOnly = !propertyInfo.CanWrite;
            _propertyName = propertyInfo.Name;
            _value = ViewModelGetter();
            Viewmodel.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_propertyName == e.PropertyName)
            {
                Value = ViewModelGetter();
            }
        }

        protected T? ViewModelGetter()
        {
            return (T?)PropertyInfo.GetValue(Viewmodel);
        }

        protected void ViewModelSetter(T? newValue)
        {
            if (PropertyInfo.CanWrite)
            {
                PropertyInfo.SetValue(Viewmodel, newValue);
            }
        }

        public void Dispose()
        {
            Viewmodel.PropertyChanged -= OnPropertyChanged;
        }
    }
}