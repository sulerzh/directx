using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public abstract class DropHandler : IDropHandler
  {
    private Dictionary<Type, object> _typeValidationCallbacks = new Dictionary<Type, object>();

    public void AddDroppableType<T>(ValidateDropItemDelegate<T> validateCallback = null)
    {
      this._typeValidationCallbacks[typeof (T)] = (object) validateCallback;
    }

    public virtual bool CanDropItem(object item)
    {
      return this.ValidateDropItem(item) == null;
    }

    public virtual string ValidateDropItem(object item)
    {
      Type type1 = item.GetType();
      Type type2 = typeof (ValidateDropItemDelegate<>).MakeGenericType(type1);
      if (!this._typeValidationCallbacks.ContainsKey(type1))
        return string.Empty;
      object obj = this._typeValidationCallbacks[type1];
      if (obj == null)
        return (string) null;
      return type2.GetMethod("Invoke").Invoke(obj, new object[1]
      {
        item
      }) as string;
    }
  }
}
