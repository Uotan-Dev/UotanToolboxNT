using System.ComponentModel;
using System.Reflection;

namespace SukiUI.Controls
{
    public sealed class DecimalViewModel : PropertyViewModelBase<decimal?>
    {
        public DecimalViewModel(INotifyPropertyChanged viewmodel, string displayName, PropertyInfo propertyInfo)
            : base(viewmodel, displayName, propertyInfo)
        {
        }
    }
}