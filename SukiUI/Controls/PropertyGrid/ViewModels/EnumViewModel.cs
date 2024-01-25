using Avalonia.Collections;
using System;
using System.ComponentModel;
using System.Reflection;

namespace SukiUI.Controls
{
    public sealed class EnumViewModel : PropertyViewModelBase<Enum?>
    {
        public IAvaloniaReadOnlyList<object> Values { get; }

        public EnumViewModel(INotifyPropertyChanged viewmodel, string displayName, PropertyInfo propertyInfo)
            : base(viewmodel, displayName, propertyInfo)
        {
            var temp = new AvaloniaList<object>();
            foreach (var item in Enum.GetValues(propertyInfo.PropertyType))
            {
                temp.Add(item);
            }

            Values = temp;
        }
    }
}