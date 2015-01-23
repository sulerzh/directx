namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class EffectTechnique : DisposableResource
    {
        private bool initialized;

        protected RasterizerState Rasterizer { get; set; }

        protected BlendState Blend { get; set; }

        protected DepthStencilState DepthStencil { get; set; }

        protected Effect Effect { get; set; }

        internal void BindEffect(RendererCore renderer)
        {
            this.EnsureInitialized();
            this.Update();
            renderer.SetEffect(this.Effect);
            this.BindStates(renderer);
        }

        private void BindStates(RendererCore renderer)
        {
            if (this.Rasterizer != null)
                renderer.SetRasterizerState(this.Rasterizer);
            if (this.Blend != null)
                renderer.SetBlendState(this.Blend);
            if (this.DepthStencil == null)
                return;
            renderer.SetDepthStencilState(this.DepthStencil);
        }

        private void EnsureInitialized()
        {
            if (this.initialized)
                return;
            this.Initialize();
            this.initialized = true;
        }

        protected abstract void Initialize();

        protected abstract void Update();
    }
}
