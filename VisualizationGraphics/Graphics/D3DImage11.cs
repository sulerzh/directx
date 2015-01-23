using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class D3DImage11 : Internal.D3DImage11
    {
        private const int MaxExceptionCount = 30;
        private int exceptionCount;
        private RenderTarget currentResource;

        public void EnsureNotUsing(RenderTarget rt)
        {
            if (this.currentResource != rt)
                return;
            this.Lock();
            this.SetBackBuffer(null);
            this.Unlock();
        }

        public bool SetBackBuffer(RenderTarget renderTarget)
        {
            if (renderTarget != this.currentResource)
            {
                try
                {
                    this.SetBackBuffer11(renderTarget == null ? IntPtr.Zero : renderTarget.RenderTargetTexture.NativeResource);
                    this.currentResource = renderTarget;
                    this.exceptionCount = 0;
                }
                catch (Exception ex)
                {
                    if (++this.exceptionCount > MaxExceptionCount)
                        throw new DirectX9ForWpfException(ex.ToString());
                    return false;
                }
            }
            return true;
        }

        public void Invalidate()
        {
            this.Lock();
            this.SetBackBuffer(null);
            this.Unlock();
        }
    }
}
