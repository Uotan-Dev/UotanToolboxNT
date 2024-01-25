using System.ComponentModel;
using System.Reflection;

namespace SukiUI.Controls
{
    public sealed class ComplexTypeViewModel : PropertyViewModelBase<INotifyPropertyChanged>
    {
        public ComplexTypeViewModel(INotifyPropertyChanged viewmodel, string displayName, PropertyInfo propertyInfo)
            : base(viewmodel, displayName, propertyInfo)
        {
            IsReadOnly = true;
        }
    }
}
