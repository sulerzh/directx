using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class RenderTarget : GraphicsResource
    {
        public RenderTarget.CustomRenderTargetDisposeCallback CustomRenderTargetDispose;

        public Texture RenderTargetTexture { get; private set; }

        public RenderTargetDepthStencilMode DepthStencilMode { get; private set; }

        public Rect ScissorRect { get; set; }

        internal RenderTarget(Texture renderTargetTexture, RenderTargetDepthStencilMode depthStencilMode)
        {
            this.RenderTargetTexture = renderTargetTexture;
            this.DepthStencilMode = depthStencilMode;
            this.ScissorRect = new Rect(0, 0, renderTargetTexture.Width, renderTargetTexture.Height);
        }

        public static RenderTarget Create(Texture renderTargetTexture, RenderTargetDepthStencilMode depthStencilMode)
        {
            return new D3D11RenderTarget(renderTargetTexture, depthStencilMode);
        }

        public abstract bool IsReady();

        public delegate void CustomRenderTargetDisposeCallback(RenderTarget rt, Action finishDispose);
    }
}
