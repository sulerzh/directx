namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class DeferredRenderer : RendererCore
    {
        public static DeferredRenderer Create(Renderer immediateRenderer)
        {
            return (DeferredRenderer)new D3D11DeferredRenderer((D3D11Renderer)immediateRenderer);
        }

        public abstract void Begin();

        public abstract void End();
    }
}
