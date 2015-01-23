using System;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
    public class WeakEventListener<TInstance> where TInstance : class
    {
        private readonly WeakReference _weakInstance;

        private Action<TInstance> OnEventAction { get; set; }

        public Action<WeakEventListener<TInstance>> OnDetachAction { get; set; }

        public WeakEventListener(TInstance instance, Action<TInstance> action)
        {
            if ((object)instance == null)
                throw new ArgumentNullException("instance");
            if (action == null)
                throw new ArgumentNullException("action");
            this._weakInstance = new WeakReference((object)instance);
            this.OnEventAction = action;
        }

        public void OnEvent()
        {
            TInstance instance = (TInstance)this._weakInstance.Target;
            if ((object)instance != null)
            {
                if (this.OnEventAction == null)
                    return;
                this.OnEventAction(instance);
            }
            else
                this.Detach();
        }

        public void Detach()
        {
            if (this.OnDetachAction == null)
                return;
            this.OnDetachAction(this);
            this.OnDetachAction = (Action<WeakEventListener<TInstance>>)null;
        }
    }

    public class WeakEventListener<TInstance, TSource> where TInstance : class
    {
        private readonly WeakReference _weakInstance;

        public Action<TInstance, TSource> OnEventAction { get; set; }

        public Action<WeakEventListener<TInstance, TSource>> OnDetachAction { get; set; }

        public WeakEventListener(TInstance instance)
        {
            if ((object)instance == null)
                throw new ArgumentNullException("instance");
            this._weakInstance = new WeakReference((object)instance);
        }

        public void OnEvent(TSource source)
        {
            TInstance instance = (TInstance)this._weakInstance.Target;
            if ((object)instance != null)
            {
                if (this.OnEventAction == null)
                    return;
                this.OnEventAction(instance, source);
            }
            else
                this.Detach();
        }

        public void Detach()
        {
            if (this.OnDetachAction == null)
                return;
            this.OnDetachAction(this);
            this.OnDetachAction = (Action<WeakEventListener<TInstance, TSource>>)null;
        }
    }

    public class WeakEventListener<TInstance, TSource, TEventArgs> where TInstance : class
    {
        private readonly WeakReference _weakInstance;

        public Action<TInstance, TSource, TEventArgs> OnEventAction { get; set; }

        public Action<WeakEventListener<TInstance, TSource, TEventArgs>> OnDetachAction { get; set; }

        public WeakEventListener(TInstance instance)
        {
            if ((object)instance == null)
                throw new ArgumentNullException("instance");
            this._weakInstance = new WeakReference((object)instance);
        }

        public void OnEvent(TSource source, TEventArgs eventArgs)
        {
            TInstance instance = (TInstance)this._weakInstance.Target;
            if ((object)instance != null)
            {
                if (this.OnEventAction == null)
                    return;
                this.OnEventAction(instance, source, eventArgs);
            }
            else
                this.Detach();
        }

        public void Detach()
        {
            if (this.OnDetachAction == null)
                return;
            this.OnDetachAction(this);
            this.OnDetachAction = (Action<WeakEventListener<TInstance, TSource, TEventArgs>>)null;
        }
    }

    public class WeakEventListener<TInstance, TSource, TEventArgs, TEventArgs1> where TInstance : class
    {
        private readonly WeakReference _weakInstance;

        public Action<TInstance, TSource, TEventArgs, TEventArgs1> OnEventAction { get; set; }

        public Action<WeakEventListener<TInstance, TSource, TEventArgs, TEventArgs1>> OnDetachAction { get; set; }

        public WeakEventListener(TInstance instance)
        {
            if ((object)instance == null)
                throw new ArgumentNullException("instance");
            this._weakInstance = new WeakReference((object)instance);
        }

        public void OnEvent(TSource source, TEventArgs eventArgs, TEventArgs1 eventArgs1)
        {
            TInstance instance = (TInstance)this._weakInstance.Target;
            if ((object)instance != null)
            {
                if (this.OnEventAction == null)
                    return;
                this.OnEventAction(instance, source, eventArgs, eventArgs1);
            }
            else
                this.Detach();
        }

        public void Detach()
        {
            if (this.OnDetachAction == null)
                return;
            this.OnDetachAction(this);
            this.OnDetachAction = (Action<WeakEventListener<TInstance, TSource, TEventArgs, TEventArgs1>>)null;
        }
    }
}
