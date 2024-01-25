namespace SukiUI.Controls
{
    public interface IPropertyViewModel<T> : IPropertyViewModel
    {
        new T Value { get; set; }
    }
}