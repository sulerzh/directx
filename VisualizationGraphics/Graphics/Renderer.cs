using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class Renderer : RendererCore
    {
        public abstract bool Initialized { get; }

        public abstract bool Msaa { get; set; }

        public abstract int FrameCount { get; }

        public abstract int FrameLatency { get; }

        public abstract int FrameWidth { get; }

        public abstract int FrameHeight { get; }

        public abstract string CorruptionErrorMessage { get; }

        public abstract FrameProfiler Profiler { get; }

        public abstract ResourceManager Resources { get; }

        public event RendererErrorEventHandler OnInternalError;

        public event RendererInfoEventHander OnInformation;

        public event RendererPresentEventHandler OnPresent;

        public static Renderer Create()
        {
            return new D3D11Renderer();
        }

        public abstract bool Initialize(IntPtr renderWindowHandle, int windowWidth, int windowHeight, bool msaa, bool forceOffscreenComposition);

        public abstract bool Initialize(int renderTargetWidth, int renderTargetHeight, bool msaa, bool isForOfflineVideo);

        public abstract void UpdateRenderTargetSize(int renderTargetWidth, int renderTargetHeight);

        public abstract bool BeginFrame(Color4F? clearColor);

        public abstract void EndFrame();

        public abstract void SetRasterizerStateNoLock(RasterizerState state);

        public abstract void SetDepthStencilStateNoLock(DepthStencilState state);

        public abstract void SetBlendStateNoLock(BlendState state);

        public abstract Texture CreateTexture(Image textureData, bool mipMapping, bool allowView, TextureUsage usage);

        public abstract void SetTextureNoLock(int slot, Texture texture);

        public abstract void SetTextureFromBackBuffer(int slot);

        public abstract void SetTextureView(int slot, TextureView textureView);

        public abstract void SetVertexSourceNoLock(VertexBuffer[] vertexBuffers);

        public void SetVertexSourceNoLock(VertexBuffer vertexBuffer)
        {
            this.SetVertexSource(new VertexBuffer[] { vertexBuffer });
        }

        public abstract void SetIndexSourceNoLock(IndexBuffer indexBuffer);

        public abstract void DrawNoLock(int startVertex, int vertexCount, PrimitiveTopology topology);

        public abstract void DrawIndexedNoLock(int startIndex, int indexCount, PrimitiveTopology topology);

        public abstract void DrawInstancedNoLock(int startVertex, int vertexCountPerInstance, int startInstance, int instanceCount, PrimitiveTopology topology);

        public abstract void DrawIndexedInstancedNoLock(int startIndex, int indexCountPerInstance, int startInstance, int instanceCount, PrimitiveTopology topology);

        public abstract void SetVertexTextureNoLock(int slot, Texture texture);

        public abstract void SetVertexTextureNoLock(int slot, Texture[] texture);

        public abstract void SetStreamBufferNoLock(StreamBuffer buffer);

        public abstract void SetEffectNoLock(Effect effect);

        public abstract void RenderLockEnter();

        public abstract void RenderLockExit();

        public abstract bool TryGetRenderTargetFrame(out RenderTarget frame);

        public abstract void ReleaseRenderTargetFrame();

        internal abstract void SetEndFrame(RenderTarget renderTarget);

        public abstract void CopyRenderTargetData(RenderTarget source, Rect sourceArea, ReadableBitmap destination);

        public abstract void CopyRenderTargetData(RenderTarget source, RenderTarget destination, bool flush);

        public abstract void Flush();

        internal void Notify(string message)
        {
            if (this.OnInformation == null)
                return;
            this.OnInformation(this, new RendererInfoEventArgs(message));
        }

        internal void NotifyError(string message, Exception e)
        {
            if (this.OnInternalError == null)
                return;
            this.OnInternalError(this, new RendererErrorEventArgs(message, e));
        }

        protected void RaiseOnPresent(RenderTarget backbuffer)
        {
            if (this.OnPresent == null)
                return;
            this.OnPresent(this, new RendererPresentEventArgs(backbuffer));
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            if (this.AppParameters != null)
                this.AppParameters.Dispose();
            if (this.FrameParameters == null)
                return;
            this.FrameParameters.Dispose();
        }
    }
}
