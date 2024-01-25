using System;
using System.ComponentModel;

namespace SukiUI.Controls
{
    public interface IPropertyViewModel : INotifyPropertyChanged, IDisposable
    {
        object? Value { get; set; }
    }
}