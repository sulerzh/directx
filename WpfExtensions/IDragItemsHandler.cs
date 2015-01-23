using System.Windows;

namespace Microsoft.Data.Visualization.WpfExtensions
{
    public interface IDragItemsHandler : IDragHandler
    {
        int ProcessDraggedItem(object item, DragDropEffects effects);
    }

    public interface IDragItemsHandler<T> : IDragItemsHandler, IDragHandler
    {
        int ProcessDraggedItem(T item, DragDropEffects effects);
    }
}
