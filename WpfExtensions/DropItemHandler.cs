using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class DropItemHandler : DropHandler, IDropItemHandler, IDropHandler
  {
    private Dictionary<Type, object> _typeDropCallbacks = new Dictionary<Type, object>();

    public void DropItem(object item)
    {
      Type type1 = item.GetType();
      Type type2 = typeof (Action<>).MakeGenericType(type1);
      if (!this._typeDropCallbacks.ContainsKey(type1))
        return;
      object obj = this._typeDropCallbacks[type1];
      type2.GetMethod("Invoke").Invoke(obj, new object[1]
      {
        item
      });
    }

    public void AddDroppableTypeOnDrop<T>(Action<T> onDropCallback)
    {
      if (onDropCallback == null)
        throw new ArgumentNullException("onDropCallback");
      this._typeDropCallbacks[typeof (T)] = (object) onDropCallback;
    }

    public void AddDroppableTypeHandlers<T>(Action<T> onDropCallback, ValidateDropItemDelegate<T> validateCallback = null)
    {
      this.AddDroppableTypeOnDrop<T>(onDropCallback);
      this.AddDroppableType<T>(validateCallback);
    }
  }
}
