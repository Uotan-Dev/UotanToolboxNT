using System.ComponentModel;
using System.Reflection;

namespace SukiUI.Controls
{
    public sealed class StringViewModel : PropertyViewModelBase<string?>
    {
        public StringViewModel(INotifyPropertyChanged viewmodel, string displayName, PropertyInfo propertyInfo)
            : base(viewmodel, displayName, propertyInfo)
        {
        }
    }
}