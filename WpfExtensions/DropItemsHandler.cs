using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class DropItemsHandler : DropHandler, IDropItemsHandler, IDropHandler
  {
    private Dictionary<Type, object> _typeDropCallbacks = new Dictionary<Type, object>();

    public virtual void DropItem(object item, int index)
    {
      Type type1 = item.GetType();
      Type type2 = typeof (DropItemIntoCollectionDelegate<>).MakeGenericType(type1);
      if (!this._typeDropCallbacks.ContainsKey(type1))
        return;
      object obj = this._typeDropCallbacks[type1];
      type2.GetMethod("Invoke").Invoke(obj, new object[2]
      {
        item,
        (object) index
      });
    }

    public void AddDroppableTypeOnDrop<T>(DropItemIntoCollectionDelegate<T> onDropCallback)
    {
      if (onDropCallback == null)
        throw new ArgumentNullException("onDropCallback");
      this._typeDropCallbacks[typeof (T)] = (object) onDropCallback;
    }

    public void AddDefaultDropCollection<T>(Collection<T> collection)
    {
      if (collection == null)
        throw new ArgumentNullException("collection");
      this._typeDropCallbacks[typeof (T)] = (object) (DropItemIntoCollectionDelegate<T>) ((item, index) => collection.Insert(index, item));
    }

    public void AddDroppableTypeHandlers<T>(DropItemIntoCollectionDelegate<T> onDropCallback, ValidateDropItemDelegate<T> validateCallback = null)
    {
      this.AddDroppableTypeOnDrop<T>(onDropCallback);
      this.AddDroppableType<T>(validateCallback);
    }

    public void AddDroppableTypeHandlers<T>(Collection<T> collection, ValidateDropItemDelegate<T> validateCallback = null)
    {
      this.AddDefaultDropCollection<T>(collection);
      this.AddDroppableType<T>(validateCallback);
    }
  }
}
