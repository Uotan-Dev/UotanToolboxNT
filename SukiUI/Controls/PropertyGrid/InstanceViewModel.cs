using Avalonia.Collections;
using SukiUI.Helpers;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace SukiUI.Controls
{
    public class InstanceViewModel : SukiObservableObject, IDisposable
    {
        public INotifyPropertyChanged ViewModel { get; }

        public IAvaloniaReadOnlyList<CategoryViewModel> Categories { get; }

        public InstanceViewModel(INotifyPropertyChanged viewModel)
        {
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            Categories = GenerateCategories(viewModel);
        }

        private static string? GetCategory(PropertyInfo property)
        {
            var attributes = property.GetCustomAttributes<CategoryAttribute>(false);
            if (attributes.Any())
            {
                return attributes.First().Category;
            }

            return null;
        }

        private static string? GetDisplayName(PropertyInfo property)
        {
            var attributes = property.GetCustomAttributes<DisplayNameAttribute>(false);
            if (attributes.Any())
            {
                return attributes.First().DisplayName;
            }

            return null;
        }

        /// <summary>
        /// Factory creating all the categories for a given instance of a ViewModel implementing <see cref="INotifyPropertyChanged"/>.
        /// </summary>
        /// <param name="viewModel">the ViewModel instance, to generate/show controls/categories for</param>
        /// <returns>CategoryViewModels holding representations for each public non static property</returns>
        public virtual IAvaloniaReadOnlyList<CategoryViewModel> GenerateCategories(INotifyPropertyChanged viewModel)
        {
            var properties = viewModel
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToList();

            var categories = properties
                .Select(prop => (Property: prop, Category: GetCategory(prop), DisplayName: GetDisplayName(prop)))
                .Where(p => p.Category is not null)
                .Distinct()
                .GroupBy(p => p.Category);

            var categoryViewModels = new AvaloniaList<CategoryViewModel>();
            foreach (var grouping in categories)
            {
                var propertyViewModels = new AvaloniaList<IPropertyViewModel>();
                foreach (var (Property, Category, DisplayName) in grouping)
                {
                    var propertyViewModel = default(IPropertyViewModel?);
                    var displayname = DisplayName ?? Property.Name;

                    if (Property.PropertyType == typeof(string))
                    {
                        propertyViewModel = new StringViewModel(viewModel, displayname, Property);
                    }
                    else if (Property.PropertyType == typeof(int) || Property.PropertyType == typeof(int?))
                    {
                        propertyViewModel = new IntegerViewModel(viewModel, displayname, Property);
                    }
                    else if (Property.PropertyType == typeof(long) || Property.PropertyType == typeof(long?))
                    {
                        propertyViewModel = new LongViewModel(viewModel, displayname, Property);
                    }
                    else if (Property.PropertyType == typeof(double) || Property.PropertyType == typeof(double?))
                    {
                        propertyViewModel = new DoubleViewModel(viewModel, displayname, Property);
                    }
                    else if (Property.PropertyType == typeof(float) || Property.PropertyType == typeof(float?))
                    {
                        propertyViewModel = new FloatViewModel(viewModel, displayname, Property);
                    }
                    else if (Property.PropertyType == typeof(decimal) || Property.PropertyType == typeof(decimal?))
                    {
                        propertyViewModel = new DecimalViewModel(viewModel, displayname, Property);
                    }
                    else if (Property.PropertyType == typeof(bool) || Property.PropertyType == typeof(bool?))
                    {
                        propertyViewModel = new BoolViewModel(viewModel, displayname, Property);
                    }
                    else if (Property.PropertyType.IsEnum)
                    {
                        propertyViewModel = new EnumViewModel(viewModel, displayname, Property);
                    }
                    else if (Property.PropertyType == typeof(DateTime) || Property.PropertyType == typeof(DateTime?))
                    {
                        propertyViewModel = new DateTimeViewModel(viewModel, displayname, Property);
                    }
                    else if (Property.PropertyType == typeof(DateTimeOffset) || Property.PropertyType == typeof(DateTimeOffset?))
                    {
                        propertyViewModel = new DateTimeOffsetViewModel(viewModel, displayname, Property);
                    }
                    else
                    {
                        var propertyValue = Property.GetValue(viewModel) as INotifyPropertyChanged;
                        if (propertyValue is INotifyPropertyChanged childViewModel)
                        {
                            propertyViewModel = new ComplexTypeViewModel(viewModel, displayname, Property);
                        }
                    }

                    if (propertyViewModel is not null)
                    {
                        propertyViewModels.Add(propertyViewModel);
                    }
                }

                var categoryViewModel = new CategoryViewModel(grouping.Key!, propertyViewModels);
                categoryViewModels.Add(categoryViewModel);
            }

            return categoryViewModels;
        }

        public void Dispose()
        {
            foreach (var category in Categories)
            {
                foreach (var property in category.Properties)
                {
                    property.Dispose();
                }
            }
        }
    }
}