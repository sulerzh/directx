using System.Windows;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class DragItemHandler<T> : DragHandler<T>, IDragItemHandler<T>, IDragItemHandler, IDragHandler
  {
    public ProcessDraggedItemDelegate<T> ProcessDraggedItemCallback { get; set; }

    public DragItemHandler(ProcessDraggedItemDelegate<T> dragItemCallback = null)
    {
      this.ProcessDraggedItemCallback = dragItemCallback;
    }

    public void ProcessDraggedItem(T item, DragDropEffects effects)
    {
      if (this.ProcessDraggedItemCallback == null)
        return;
      this.ProcessDraggedItemCallback(item, effects);
    }

    public void ProcessDraggedItem(object item, DragDropEffects effects)
    {
      if (!(item is T))
        return;
      this.ProcessDraggedItem((T) item, effects);
    }
  }
}
