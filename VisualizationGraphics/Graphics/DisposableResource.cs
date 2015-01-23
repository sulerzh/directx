using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class DisposableResource : IDisposable
    {
        private bool disposed;

        public bool Disposed
        {
            get
            {
                return this.disposed;
            }
        }

        ~DisposableResource()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            if (this.disposed)
                return;
            this.Dispose(true);
            GC.SuppressFinalize(this);
            this.disposed = true;
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
