namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class RendererCoreState
    {
        public RenderTarget[] CurrentTargets;
        public ApplicationRenderParameters AppParameters;
        public FrameRenderParameters FrameParameters;
        public D3D11RenderTarget CurrentRenderTarget;
        public D3D11RenderTarget BackBufferRenderTarget;
    }
}
