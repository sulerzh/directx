using System;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
    public class WeakDelegate<TInstance, TReturn> where TInstance : class
    {
        private readonly WeakReference<TInstance> _weakInstance;

        public Func<TInstance, TReturn> OnInvokeAction { get; set; }

        public WeakDelegate(TInstance instance)
        {
            if ((object)instance == null)
                throw new ArgumentNullException("instance");
            this._weakInstance = new WeakReference<TInstance>(instance);
        }

        public TReturn OnInvoke()
        {
            TInstance target;
            if (this._weakInstance.TryGetTarget(out target) && this.OnInvokeAction != null)
                return this.OnInvokeAction(target);
            else
                return default(TReturn);
        }
    }

    public class WeakDelegate<TInstance, TParam, TReturn> where TInstance : class
    {
        private readonly WeakReference<TInstance> _weakInstance;

        public Func<TInstance, TParam, TReturn> OnInvokeAction { get; set; }

        public WeakDelegate(TInstance instance)
        {
            if ((object)instance == null)
                throw new ArgumentNullException("instance");
            this._weakInstance = new WeakReference<TInstance>(instance);
        }

        public TReturn OnInvoke(TParam param)
        {
            TInstance target;
            if (this._weakInstance.TryGetTarget(out target) && this.OnInvokeAction != null)
                return this.OnInvokeAction(target, param);
            else
                return default(TReturn);
        }
    }

    public class WeakDelegate<TInstance, TParam1, TParam2, TReturn> where TInstance : class
    {
        private readonly WeakReference<TInstance> _weakInstance;

        public Func<TInstance, TParam1, TParam2, TReturn> OnInvokeAction { get; set; }

        public WeakDelegate(TInstance instance)
        {
            if ((object)instance == null)
                throw new ArgumentNullException("instance");
            this._weakInstance = new WeakReference<TInstance>(instance);
        }

        public TReturn OnInvoke(TParam1 param1, TParam2 param2)
        {
            TInstance target;
            if (this._weakInstance.TryGetTarget(out target) && this.OnInvokeAction != null)
                return this.OnInvokeAction(target, param1, param2);
            else
                return default(TReturn);
        }
    }
}
