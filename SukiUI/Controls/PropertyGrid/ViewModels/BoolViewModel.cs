using System.ComponentModel;
using System.Reflection;

namespace SukiUI.Controls
{
    public sealed class BoolViewModel : PropertyViewModelBase<bool?>
    {
        public BoolViewModel(INotifyPropertyChanged viewmodel, string displayName, PropertyInfo propertyInfo)
            : base(viewmodel, displayName, propertyInfo)
        {
        }
    }
}