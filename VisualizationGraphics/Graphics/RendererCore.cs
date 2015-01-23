namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class RendererCore : DisposableResource
    {
        public abstract ApplicationRenderParameters AppParameters { get; }

        public abstract FrameRenderParameters FrameParameters { get; }

        internal RendererCore()
        {
        }

        public abstract void SetRasterizerState(RasterizerState state);

        public abstract void SetDepthStencilState(DepthStencilState state);

        public abstract void SetBlendState(BlendState state);

        public abstract void BeginRenderTargetFrame(RenderTarget renderTarget, Color4F? clearColor);

        public abstract void BeginRenderTargetFrame(RenderTarget[] renderTargets, Color4F? clearColor, bool attachMainRenderTarget);

        public abstract void EndRenderTargetFrame();

        public abstract void ClearCurrentDepthTarget(float clearDepth);

        public abstract void ClearCurrentStencilTarget(int clearStencil);

        public abstract void SetTexture(int slot, Texture texture);

        public abstract void SetVertexTexture(int slot, Texture texture);

        public abstract void SetEffect(Effect effect);

        public void SetEffect(EffectTechnique effectTechnique)
        {
            effectTechnique.BindEffect(this);
        }

        public abstract void SetVertexSource(VertexBuffer[] vertexBuffers);

        public void SetVertexSource(VertexBuffer vertexBuffer)
        {
            this.SetVertexSource(new VertexBuffer[] { vertexBuffer });
        }

        public abstract void SetIndexSource(IndexBuffer indexBuffer);

        public abstract void Draw(int startVertex, int vertexCount, PrimitiveTopology topology);

        public abstract void DrawIndexed(int startIndex, int indexCount, PrimitiveTopology topology);

        public abstract void DrawInstanced(int startVertex, int vertexCountPerInstance, int startInstance, int instanceCount, PrimitiveTopology topology);

        public abstract void DrawIndexedInstanced(int startIndex, int indexCountPerInstance, int startInstance, int instanceCount, PrimitiveTopology topology);

        public abstract void SetStreamBuffer(StreamBuffer buffer);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            this.AppParameters.Dispose();
            this.FrameParameters.Dispose();
        }
    }
}
