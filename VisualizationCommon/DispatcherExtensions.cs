using System;
using System.Security.Permissions;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
    public static class DispatcherExtensions
    {
        public static void CheckedInvoke(this Dispatcher dispatcher, Action action, bool async = false)
        {
            if (dispatcher == null || action == null)
                return;
            if (dispatcher.CheckAccess())
                action();
            else if (async)
                dispatcher.BeginInvoke(action);
            else
                dispatcher.Invoke(action, new object[0]);
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void DoEvents(this Dispatcher dispatcher)
        {
            DispatcherFrame frame = new DispatcherFrame();
            Func<object, object> func = delegate(object f)
            {
                ((DispatcherFrame)f).Continue = false;
                return (object)null;
            };
            dispatcher.BeginInvoke(DispatcherPriority.Background, func, (object)frame);
            Dispatcher.PushFrame(frame);
        }
    }
}
