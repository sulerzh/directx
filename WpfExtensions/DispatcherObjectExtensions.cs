using System;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public static class DispatcherObjectExtensions
  {
    public static void CheckedInvoke(this DispatcherObject dispatcherObject, Action action)
    {
      if (action == null)
        return;
      if (dispatcherObject.Dispatcher.CheckAccess())
        action();
      else
        dispatcherObject.Dispatcher.Invoke((Action) (() => action()), new object[0]);
    }
  }
}
