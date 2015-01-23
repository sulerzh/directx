using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class BufferedRenderTarget : DisposableResource
    {
        private ConcurrentQueue<Tuple<RenderTarget, Texture>> availableQueue =
            new ConcurrentQueue<Tuple<RenderTarget, Texture>>();
        private ConcurrentQueue<Tuple<RenderTarget, Texture>> producedQueue =
            new ConcurrentQueue<Tuple<RenderTarget, Texture>>();
        private object flipLock = new object();
        private bool isOnScreenMode = true;
        private RenderTarget targetBuffer;
        private RenderTarget clientBuffer;
        private Texture targetTexture;
        private Texture clientTexture;
        private ReadableBitmap enforceLatencyBitmap;
        private RenderTarget latestBuffer;
        private Renderer renderer;

        public static int BufferLatency { get { return 3; } }

        internal RenderTarget TargetBuffer
        {
            get
            {
                return this.targetBuffer;
            }
        }

        public BufferedRenderTarget(int renderTargetWidth, int renderTargetHeight, PixelFormat renderTargetFormat, Renderer renderer, bool isOfflineMode)
        {
            this.Initialize(renderTargetWidth, renderTargetHeight, renderTargetFormat, renderer, isOfflineMode);
        }

        private void Initialize(int renderTargetWidth, int renderTargetHeight, PixelFormat renderTargetFormat, Renderer renderer, bool isOfflineMode)
        {
            this.isOnScreenMode = !isOfflineMode;
            using (Image textureData = new Image(IntPtr.Zero, renderTargetWidth, renderTargetHeight, renderTargetFormat))
            {
                for (int index = 0; index < BufferedRenderTarget.BufferLatency + 1; ++index)
                {
                    Tuple<RenderTarget, Texture> tuple;
                    if (this.isOnScreenMode)
                    {
                        tuple = new Tuple<RenderTarget, Texture>(null, null);
                    }
                    else
                    {
                        Texture texture = renderer.CreateTexture(textureData, false, false, TextureUsage.RenderTarget);
                        tuple = new Tuple<RenderTarget, Texture>(RenderTarget.Create(texture, RenderTargetDepthStencilMode.None), texture);
                    }
                    this.availableQueue.Enqueue(tuple);
                }
                this.clientTexture = renderer.CreateTexture(textureData, false, false, TextureUsage.SharedRenderTarget);
                this.clientBuffer = RenderTarget.Create(this.clientTexture, RenderTargetDepthStencilMode.None);
                this.targetTexture = renderer.CreateTexture(textureData, false, false, renderer.Msaa ? TextureUsage.MultiSampledRenderTarget : TextureUsage.RenderTarget);
                this.targetBuffer = RenderTarget.Create(this.targetTexture, RenderTargetDepthStencilMode.Enabled);
            }
            this.enforceLatencyBitmap = ReadableBitmap.Create(8, 8, renderTargetFormat);
            this.renderer = renderer;
        }

        public bool TryGetReadyBuffer(out RenderTarget buffer)
        {
            buffer = this.clientBuffer;
            if (buffer == null)
                return false;
            try
            {
                if (!this.isOnScreenMode)
                    this.renderer.RenderLockEnter();
                lock (this.flipLock)
                {
                    Tuple<RenderTarget, Texture> tuple = null;
                    if (this.producedQueue.TryPeek(out tuple) && (this.isOnScreenMode || tuple.Item1.IsReady()) &&
                        this.producedQueue.TryDequeue(out tuple))
                    {
                        if (!this.isOnScreenMode)
                            this.renderer.CopyRenderTargetData(tuple.Item1, this.clientBuffer, true);
                        this.availableQueue.Enqueue(tuple);
                        return true;
                    }
                    else
                        return false;
                }
            }
            finally
            {
                if (!this.isOnScreenMode)
                    this.renderer.RenderLockExit();
            }
        }

        public RenderTarget PeekLatestFrame()
        {
            return this.latestBuffer;
        }

        internal void EndFrame()
        {
            try
            {
                if (this.isOnScreenMode)
                    Monitor.Enter(this.flipLock);
                Tuple<RenderTarget, Texture> result = null;
                if (this.availableQueue.TryDequeue(out result))
                {
                    RenderTarget renderTarget = !this.isOnScreenMode ? result.Item1 : this.clientBuffer;
                    this.renderer.CopyRenderTargetData(this.targetBuffer, renderTarget, true);
                    this.renderer.SetEndFrame(renderTarget);
                    this.latestBuffer = renderTarget;
                    this.producedQueue.Enqueue(result);
                }
            }
            finally
            {
                if (this.isOnScreenMode)
                    Monitor.Exit(this.flipLock);
            }
            this.EnsureLatency();
        }

        private void EnsureLatency()
        {
            Tuple<RenderTarget, Texture> result;
            if (this.producedQueue.Count <= 1 || !this.producedQueue.TryPeek(out result))
                return;
            this.renderer.CopyRenderTargetData(this.isOnScreenMode ? this.clientBuffer : result.Item1, 
                new Rect(0, 0, this.enforceLatencyBitmap.Width, this.enforceLatencyBitmap.Height), 
                this.enforceLatencyBitmap);
            int pitch;
            this.enforceLatencyBitmap.LockDataImmediate(out pitch);
            this.enforceLatencyBitmap.Unlock();
        }

        internal bool Flip()
        {
            return this.availableQueue.Count != 0;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            if (this.clientBuffer.CustomRenderTargetDispose != null)
                this.clientBuffer.CustomRenderTargetDispose(this.clientBuffer, this.ReleaseGraphicsResources);
            else
                this.ReleaseGraphicsResources();
        }

        private void ReleaseGraphicsResources()
        {
            this.latestBuffer = null;
            lock (this.flipLock)
            {
                Tuple<RenderTarget, Texture> tuple;
                while (this.availableQueue.TryDequeue(out tuple))
                {
                    if (tuple.Item1 != null)
                        tuple.Item1.Dispose();
                    if (tuple.Item2 != null)
                        tuple.Item2.Dispose();
                }
                while (this.producedQueue.TryDequeue(out tuple))
                {
                    if (tuple.Item1 != null)
                        tuple.Item1.Dispose();
                    if (tuple.Item2 != null)
                        tuple.Item2.Dispose();
                }
            }
            if (this.clientTexture != null)
            {
                this.clientTexture.Dispose();
                this.clientTexture = null;
            }
            if (this.targetTexture != null)
            {
                this.targetTexture.Dispose();
                this.targetTexture = null;
            }
            if (this.clientBuffer != null)
            {
                this.clientBuffer.Dispose();
                this.clientBuffer = null;
            }
            if (this.targetBuffer != null)
            {
                this.targetBuffer.Dispose();
                this.targetBuffer = null;
            }
            if (this.enforceLatencyBitmap == null)
                return;
            this.enforceLatencyBitmap.Dispose();
            this.enforceLatencyBitmap = null;
        }
    }
}
