using System;
using System.ComponentModel;
using System.Reflection;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
  public class PropertyUndoItem<TProp, TTarget> : UndoItem where TTarget : PropertyChangeNotificationBase
  {
    private TProp oldValue;
    private readonly WeakReference handler;
    private readonly PropertyInfo propertyInfo;
    private readonly Action<TTarget, TProp> postUndoAction;

    public TProp OldValue
    {
      get
      {
        return this.oldValue;
      }
    }

    public PropertyUndoItem(string description, TTarget target, string propertyName, TProp oldValue, PropertyChangingEventHandler undoHandler, UndoItem.TransactionState state = UndoItem.TransactionState.None, Action<TTarget, TProp> postUndoAction = null)
      : base(description, (object) target, state)
    {
      this.oldValue = oldValue;
      this.handler = new WeakReference((object) undoHandler);
      this.propertyInfo = PropertyInfoEx.GetCachedPropertyInfo(target.GetType(), propertyName);
      this.postUndoAction = postUndoAction;
    }

    protected override UndoItem Execute()
    {
      TProp prop = (TProp) this.propertyInfo.GetValue(this.Target, (object[]) null);
      PropertyChangingEventHandler changingEventHandler = this.handler.Target as PropertyChangingEventHandler;
      if (changingEventHandler != null)
        ((PropertyChangeNotificationBase) this.Target).PropertyChanging -= changingEventHandler;
      this.propertyInfo.SetValue(this.Target, (object) this.oldValue, (object[]) null);
      if (changingEventHandler != null)
        ((PropertyChangeNotificationBase) this.Target).PropertyChanging += changingEventHandler;
      this.oldValue = prop;
      if (this.postUndoAction != null)
        this.postUndoAction((TTarget) this.Target, this.oldValue);
      return base.Execute();
    }
  }
}
