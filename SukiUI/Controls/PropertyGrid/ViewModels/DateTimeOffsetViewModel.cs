using System;
using System.ComponentModel;
using System.Reflection;

namespace SukiUI.Controls
{
    public sealed class DateTimeOffsetViewModel : PropertyViewModelBase<DateTimeOffset?>
    {
        public DateTimeOffsetViewModel(INotifyPropertyChanged viewmodel, string displayName, PropertyInfo propertyInfo)
            : base(viewmodel, displayName, propertyInfo)
        {
        }
    }
}