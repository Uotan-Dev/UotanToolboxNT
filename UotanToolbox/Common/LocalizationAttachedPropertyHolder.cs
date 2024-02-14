using Avalonia.Controls;
using Avalonia;
using System;
using UotanToolbox.Assets;

namespace UotanToolbox.Common
{
    public class LocalizationAttachedPropertyHolder
    {
        public static AvaloniaProperty<string> UidProperty =
            AvaloniaProperty.RegisterAttached<LocalizationAttachedPropertyHolder, AvaloniaObject, string>("Uid");

        static LocalizationAttachedPropertyHolder()
        {
            TextBlock.TextProperty.Changed.Subscribe(next =>
            {
                var uid = GetUid(next.Sender);
                if (uid != null)
                {
                    next.Sender.SetValue(TextBlock.TextProperty, Resources.ResourceManager.GetString(uid.ToString()));
                }
            });

            ContentControl.ContentProperty.Changed.Subscribe(next =>
            {
                var uid = GetUid(next.Sender);
                if (uid != null)
                {
                    next.Sender.SetValue(ContentControl.ContentProperty, Resources.ResourceManager.GetString(uid.ToString()));
                }
            });

            TextBox.WatermarkProperty.Changed.Subscribe(next =>
            {
                var uid = GetUid(next.Sender);
                if (uid != null)
                {
                    next.Sender.SetValue(TextBox.WatermarkProperty, Resources.ResourceManager.GetString(uid.ToString()));
                }
            });
        }

        public static void SetUid(AvaloniaObject target, string value)
        {
            target.SetValue(UidProperty, value);
        }

        public static string GetUid(AvaloniaObject target)
        {
            return (string)target.GetValue(UidProperty);
        }
    }
}
