namespace Microsoft.Data.Visualization.WpfExtensions
{
  public abstract class DragHandler<T> : IDragHandler<T>, IDragHandler
  {
    public CanDragItemDelegate<T> CanDragItemCallback { get; set; }

    public virtual bool CanDragItem(object item)
    {
      if (item is T)
        return this.CanDragItem((T) item);
      else
        return true;
    }

    public virtual bool CanDragItem(T item)
    {
      if (this.CanDragItemCallback != null)
        return this.CanDragItemCallback(item);
      else
        return true;
    }
  }
}
