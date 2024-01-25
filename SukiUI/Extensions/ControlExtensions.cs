using Avalonia.Controls;
using System;

namespace SukiUI.Extensions
{
    /// <summary>
    /// Adds common functionality to <see cref="Control"/>.
    /// </summary>
    public static class ControlExtensions
    {
        /// <summary>
        /// Finds the named control in the scope of the specified control.
        /// </summary>
        /// <typeparam name="T">The type of the control to find.</typeparam>
        /// <param name="control">The control to look in.</param>
        /// <param name="name">The name of the control to find.</param>
        /// <returns>The control or throws if not found.</returns>
        /// <exception cref="ArgumentNullException" />
        public static T FindRequiredControl<T>(this Control? control, string name) where T : Control
        {
            control = control ?? throw new ArgumentNullException(nameof(control));

            return control.FindControl<T>(name) ?? throw new ArgumentNullException("Could not find required control with the specified name");
        }
    }
}