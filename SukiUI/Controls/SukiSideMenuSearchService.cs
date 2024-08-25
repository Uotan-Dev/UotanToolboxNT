using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using ReactiveUI;




namespace SukiUI.Controls
{
    // sirdoombox will kill me because of this
    internal class SukiSideMenuService : INotifyPropertyChanged
    {
        
        private static SukiSideMenuService _instance;

        public static SukiSideMenuService Instance 
        {
            get
            {
                if (_instance == null)
                    _instance = new SukiSideMenuService();

                return _instance;
            }
        }
        
        string search ="";
        public string Search
        {
            get => search;
            set
            {
                if (value != search)
                {
                    search = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Search"));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        
    }
    
    public class SideMenuSearchToVisibilityConverter : IMultiValueConverter
    {
        public static readonly SideMenuSearchToVisibilityConverter Instance = new();

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
        
            string search = (string)values[0];
            string header = (string)values[1];
           
            if(header.ToLower().Contains(search.ToLower()))
                return 200d;

            return 0d;
        
        }

        public object ConvertBack(object? value, Type targetType, 
            object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}