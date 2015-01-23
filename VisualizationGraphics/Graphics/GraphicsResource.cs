using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class GraphicsResource : DisposableResource
    {
        private Renderer owner;

        public event EventHandler OnReset;

        protected abstract bool Reset();

        internal abstract int GetEstimatedSystemMemoryUsage();

        internal abstract int GetEstimatedVideoMemoryUsage();

        internal void Register(Renderer renderer)
        {
            renderer.Resources.RegisterResource(this);
            this.owner = renderer;
        }

        internal void ResetResource()
        {
            if (this.Reset())
                return;
            this.InvokeOnReset();
        }

        protected void InvokeOnReset()
        {
            if (this.OnReset == null)
                return;
            this.OnReset(this, new EventArgs());
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing || this.owner == null)
                return;
            this.owner.Resources.UnregisterResource(this);
        }
    }
}
