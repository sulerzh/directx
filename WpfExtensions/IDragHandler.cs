namespace Microsoft.Data.Visualization.WpfExtensions
{
    public interface IDragHandler
    {
        bool CanDragItem(object item);
    }
    public interface IDragHandler<T> : IDragHandler
    {
        bool CanDragItem(T item);
    }
}
