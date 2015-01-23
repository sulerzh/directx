using System.Collections.ObjectModel;
using System.Windows;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class DragItemsHandler<T> : DragHandler<T>, IDragItemsHandler<T>, IDragItemsHandler, IDragHandler
  {
    public ProcessDraggedItemFromCollectionDelegate<T> ProcessDraggedItemFromCollectionCallback { get; set; }

    private bool IgnoreRemove { get; set; }

    public DragItemsHandler(Collection<T> collection, bool ignoreRemove = false)
    {
      DragItemsHandler<T> dragItemsHandler = this;
      this.IgnoreRemove = ignoreRemove;
      if (collection == null)
        return;
      this.CanDragItemCallback = (CanDragItemDelegate<T>) (item => collection.Contains(item));
      this.ProcessDraggedItemFromCollectionCallback = (ProcessDraggedItemFromCollectionDelegate<T>) ((item, effects) =>
      {
        if (dragItemsHandler.IgnoreRemove || (effects & DragDropEffects.Move) == DragDropEffects.None)
          return -1;
        int index = collection.IndexOf(item);
        collection.RemoveAt(index);
        return index;
      });
    }

    public int ProcessDraggedItem(object item, DragDropEffects effects)
    {
      if (item is T)
        return this.ProcessDraggedItem((T) item, effects);
      else
        return -1;
    }

    public int ProcessDraggedItem(T item, DragDropEffects effects)
    {
      if (this.ProcessDraggedItemFromCollectionCallback != null)
        return this.ProcessDraggedItemFromCollectionCallback(item, effects);
      else
        return -1;
    }
  }
}
