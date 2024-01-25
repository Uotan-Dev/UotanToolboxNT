using Avalonia;
using Avalonia.Controls;
using System;

namespace SukiUI.Extensions
{
    public static class ResourceNodeExtensions
    {
        /// <summary>
        /// Finds the specified resource by searching up the logical tree and then global styles.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="key">The resource key.</param>
        /// <returns>The resource, or <see cref="AvaloniaProperty.UnsetValue"/> if not found.</returns>
        public static object FindRequiredResource(this IResourceHost? control, object key)
        {
            control = control ?? throw new ArgumentNullException(nameof(control));

            return control.FindResource(key) ?? throw new ArgumentNullException("Could not find required resource with the specified key");
        }
    }
}