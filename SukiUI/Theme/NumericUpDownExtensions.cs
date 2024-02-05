using Avalonia;
using Avalonia.Controls;

namespace SukiUI.Theme
{
    public class NumericUpDownExtensions
    {
        
        
            public static readonly AttachedProperty<string> UnitProperty =
                AvaloniaProperty.RegisterAttached<NumericUpDown, string>("Unit", typeof(NumericUpDown), defaultValue: "");

            public static string GetUnit(NumericUpDown textBox)
            {
                return textBox.GetValue(UnitProperty);
            }

            public static void SetUnit(NumericUpDown textBox, string value)
            {
                textBox.SetValue(UnitProperty, value);
            }
    }
}