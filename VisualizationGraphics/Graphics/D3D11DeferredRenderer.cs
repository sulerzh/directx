using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11DeferredRenderer : DeferredRenderer
    {
        private D3D11RendererCore core;

        public override FrameRenderParameters FrameParameters
        {
            get
            {
                return this.core.FrameParameters;
            }
        }

        public override ApplicationRenderParameters AppParameters
        {
            get
            {
                return this.core.AppParameters;
            }
        }

        public D3D11DeferredRenderer(D3D11Renderer immediateRenderer)
        {
            if (!immediateRenderer.Initialized)
                throw new InvalidOperationException("The immediate renderer is not initialized.");
            DirectX.Direct3D11.D3DDevice device = immediateRenderer.Device;
            DirectX.Direct3D11.DeviceContext deferredContext = device.CreateDeferredContext(0U);
            this.core = new D3D11RendererCore(device, deferredContext);
        }

        public override void Begin()
        {
            D3D11Renderer d3D11Renderer = (D3D11Renderer)this.core.Device.Tag;
            lock (d3D11Renderer.ContextLock)
                this.core.SetCoreState(d3D11Renderer.GetCoreState());
        }

        public override void End()
        {
            lock (((D3D11Renderer)this.core.Device.Tag).ContextLock)
            {
                DirectX.Direct3D11.CommandList cmdList = this.core.Context.FinishCommandList(false);
                this.core.Device.ImmediateContext.ExecuteCommandList(cmdList, true);
                cmdList.Dispose();
            }
        }

        public override void BeginRenderTargetFrame(RenderTarget renderTarget, Color4F? clearColor)
        {
            this.core.BeginRenderTargetFrame(renderTarget, clearColor);
        }

        public override void BeginRenderTargetFrame(RenderTarget[] renderTargets, Color4F? clearColor, bool attachMainRenderTarget)
        {
            this.core.BeginRenderTargetFrame(renderTargets, clearColor, attachMainRenderTarget);
        }

        public override void ClearCurrentDepthTarget(float clearDepth)
        {
            this.core.ClearCurrentDepthTarget(clearDepth);
        }

        public override void ClearCurrentStencilTarget(int clearStencil)
        {
            this.core.ClearCurrentStencilTarget(clearStencil);
        }

        public override void Draw(int startVertex, int vertexCount, PrimitiveTopology topology)
        {
            this.core.Draw(startVertex, vertexCount, topology);
        }

        public override void DrawIndexed(int startIndex, int indexCount, PrimitiveTopology topology)
        {
            this.core.DrawIndexed(startIndex, indexCount, topology);
        }

        public override void DrawIndexedInstanced(int startIndex, int indexCountPerInstance, int startInstance, int instanceCount, PrimitiveTopology topology)
        {
            this.core.DrawIndexedInstanced(startIndex, indexCountPerInstance, startInstance, instanceCount, topology);
        }

        public override void DrawInstanced(int startVertex, int vertexCountPerInstance, int startInstance, int instanceCount, PrimitiveTopology topology)
        {
            this.core.DrawInstanced(startVertex, vertexCountPerInstance, startInstance, instanceCount, topology);
        }

        public override void EndRenderTargetFrame()
        {
            this.core.EndRenderTargetFrame();
        }

        public override void SetBlendState(BlendState state)
        {
            this.core.SetBlendState(state);
        }

        public override void SetDepthStencilState(DepthStencilState state)
        {
            this.core.SetDepthStencilState(state);
        }

        public override void SetRasterizerState(RasterizerState state)
        {
            this.core.SetRasterizerState(state);
        }

        public override void SetStreamBuffer(StreamBuffer buffer)
        {
            this.core.SetStreamBuffer(buffer);
        }

        public override void SetEffect(Effect effect)
        {
            this.core.SetEffect(effect);
        }

        public override void SetTexture(int slot, Texture texture)
        {
            this.core.SetTexture(slot, texture);
        }

        public override void SetVertexTexture(int slot, Texture texture)
        {
            this.core.SetVertexTexture(slot, texture);
        }

        public override void SetIndexSource(IndexBuffer indexBuffer)
        {
            this.core.SetIndexSource(indexBuffer);
        }

        public override void SetVertexSource(VertexBuffer[] vertexBuffers)
        {
            this.core.SetVertexSource(vertexBuffers);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing || this.core == null)
                return;
            this.core.Dispose();
        }
    }
}
