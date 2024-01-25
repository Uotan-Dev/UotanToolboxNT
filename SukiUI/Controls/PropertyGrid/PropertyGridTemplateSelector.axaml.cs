using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

namespace SukiUI.Controls;

public partial class PropertyGridTemplateSelector : ResourceDictionary, IDataTemplate
{
    public bool UseSukiHost { get; set; } = true;

    public PropertyGridTemplateSelector()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public Control? Build(object? param)
    {
        if (param is null)
        {
            return null;
        }

        var type = param.GetType();
        var key = type.Name;

        if (TryGetResource(key, null, out var resource))
        {
            if (resource is IDataTemplate template)
            {
                return template.Build(param);
            }
        }

        return null;
    }

    public bool Match(object? data)
    {
        if (data is null)
        {
            return false;
        }

        var type = data.GetType();
        var key = type.Name;

        if (string.IsNullOrEmpty(key))
        {
            return false;
        }

        if (ContainsKey(key) == false)
        {
            return false;
        }

        return true;
    }

    private async void OnMoreInfoClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Control control)
        {
            return;
        }

        if (UseSukiHost)
        {
            if (control.DataContext is not ComplexTypeViewModel childViewModel || childViewModel.Value is null)
            {
                return;
            }

            SukiHost.ShowDialog(new PropertyGridDialog()
            {
                DataContext = childViewModel.Value
            }, true, true);
        }
        else
        {
            var root = control.GetVisualRoot();
            if (root is not Window parentWindow || control.DataContext is not ComplexTypeViewModel childViewModel || childViewModel.Value is null)
            {
                return;
            }

            var window = new PropertyGridWindow()
            {
                DataContext = childViewModel.Value
            };

            await window.ShowDialog(parentWindow);
        }
    }
}