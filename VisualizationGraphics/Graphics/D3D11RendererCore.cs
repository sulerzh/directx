using Microsoft.Data.Visualization.DirectX.Direct3D;
using Microsoft.Data.Visualization.DirectX.Direct3D11;
using Microsoft.Data.Visualization.DirectX.Graphics;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11RendererCore : RendererCore
    {
        private Stack<RenderTarget[]> savedTargets = new Stack<RenderTarget[]>();
        private D3D11StreamBuffer currentStreamBuffer;
        private DepthStencilState disabledRasterizer;
        private DepthStencilState currentDepthStencilState;
        private DepthStencilState savedDepthStencilState;
        private RenderTarget[] currentTargets;
        private PrimitiveTopology currentTopology;
        private D3D11Effect currentEffect;
        private bool effectJustUpdated;
        private VertexBuffer[] currentVertexSource;
        private ApplicationRenderParameters appParameters;
        private FrameRenderParameters frameParameters;

        public D3D11RenderTarget CurrentRenderTarget { get; private set; }

        public D3D11RenderTarget BackBufferRenderTarget { get; set; }

        public D3DDevice Device { get; private set; }

        public DeviceContext Context { get; private set; }

        public override ApplicationRenderParameters AppParameters
        {
            get
            {
                return this.appParameters;
            }
        }

        public override FrameRenderParameters FrameParameters
        {
            get
            {
                return this.frameParameters;
            }
        }

        public D3D11RendererCore(D3DDevice device, DeviceContext context)
        {
            this.Device = device;
            this.Context = context;
            this.appParameters = new ApplicationRenderParameters();
            this.frameParameters = new FrameRenderParameters();
            this.disabledRasterizer = DepthStencilState.Create(new DepthStencilStateDescription()
            {
                StencilEnable = false,
                DepthEnable = false
            });
        }

        public void UpdateDevice(D3DDevice device, DeviceContext context)
        {
            this.Device = device;
            this.Context = context;
        }

        public override void SetRasterizerState(RasterizerState state)
        {
            ((D3D11RasterizerState)state).SetState(this.Device, this.Context);
        }

        public override void SetDepthStencilState(DepthStencilState state)
        {
            if (this.currentStreamBuffer != null && state != this.disabledRasterizer)
                return;
            this.currentDepthStencilState = state;
            ((D3D11DepthStencilState)state).SetState(this.Device, this.Context);
        }

        public override void SetBlendState(BlendState state)
        {
            ((D3D11BlendState)state).SetState(this.Device, this.Context);
        }

        public override void BeginRenderTargetFrame(RenderTarget renderTarget, Color4F? clearColor)
        {
            this.BeginRenderTargetFrame(new RenderTarget[1]
      {
        renderTarget
      }, clearColor, false);
        }

        public override void BeginRenderTargetFrame(RenderTarget[] renderTargets, Color4F? clearColor, bool attachMainRenderTarget)
        {
            if (this.BackBufferRenderTarget != this.CurrentRenderTarget)
                this.savedTargets.Push(this.currentTargets);
            if (attachMainRenderTarget)
            {
                RenderTarget[] renderTargetArray = new RenderTarget[renderTargets.Length + 1];
                renderTargetArray[0] = this.CurrentRenderTarget;
                Array.Copy(renderTargets, 0, renderTargetArray, 1, renderTargets.Length);
                renderTargets = renderTargetArray;
            }
            int num = 1;
            while (num < renderTargets.Length)
                ++num;
            DepthStencilView depthStencilView = ((D3D11RenderTarget)renderTargets[0]).GetDepthStencilView(this.Device, this.Context);
            if (renderTargets[renderTargets.Length - 1] != this.CurrentRenderTarget)
                this.SetRenderTarget(renderTargets);
            if (clearColor.HasValue)
            {
                for (int index = 0; index < renderTargets.Length; ++index)
                {
                    if (!attachMainRenderTarget || index != 0)
                        this.Context.ClearRenderTargetView(((D3D11RenderTarget)renderTargets[index]).GetRenderTargetView(this.Device, this.Context), new ColorRgba(clearColor.Value.Rgba()));
                }
            }
            if (depthStencilView == null || attachMainRenderTarget)
                return;
            float depth = 1f;
            if (depthStencilView.Description.Format == Format.D32Float)
                depth = 0.0f;
            this.Context.ClearDepthStencilView(depthStencilView, ClearOptions.Depth | ClearOptions.Stencil, depth, (byte)0);
        }

        public override void EndRenderTargetFrame()
        {
            if (this.savedTargets.Count > 0)
            {
                this.SetRenderTarget(this.savedTargets.Pop());
            }
            else
            {
                if (this.CurrentRenderTarget == this.BackBufferRenderTarget)
                    return;
                this.SetRenderTarget(this.BackBufferRenderTarget);
            }
        }

        public void SetRenderTarget(RenderTarget renderTarget)
        {
            this.SetRenderTarget(new D3D11RenderTarget[] { (D3D11RenderTarget)renderTarget });
        }

        public void SetRenderTarget(RenderTarget[] renderTargets)
        {
            DepthStencilView depthStencilView = ((D3D11RenderTarget)renderTargets[0]).GetDepthStencilView(this.Device, this.Context);
            if (depthStencilView == null &&
                renderTargets[0].DepthStencilMode == RenderTargetDepthStencilMode.Inherited &&
                (this.CurrentRenderTarget.RenderTargetTexture.Width == renderTargets[0].RenderTargetTexture.Width &&
                this.CurrentRenderTarget.RenderTargetTexture.Height == renderTargets[0].RenderTargetTexture.Height))
                depthStencilView = this.Context.OM.RenderTargets.DepthStencilView;
            RenderTargetView[] renderTargetViewArray = new RenderTargetView[renderTargets.Length];
            for (int index = 0; index < renderTargets.Length; ++index)
                renderTargetViewArray[index] = ((D3D11RenderTarget)renderTargets[index]).GetRenderTargetView(this.Device, this.Context);
            this.Context.OM.RenderTargets = new OutputMergerRenderTargets(renderTargetViewArray, depthStencilView);
            Viewport[] viewportArray = new Viewport[renderTargets.Length];
            for (int index = 0; index < renderTargets.Length; ++index)
                viewportArray[index] = ((D3D11RenderTarget)renderTargets[index]).GetViewport();
            this.Context.RS.Viewports = viewportArray;
            D3DRect[] d3DrectArray = new D3DRect[renderTargets.Length];
            for (int index = 0; index < renderTargets.Length; ++index)
                d3DrectArray[index] = new D3DRect()
                {
                    Left = renderTargets[index].ScissorRect.X,
                    Top = renderTargets[index].ScissorRect.Y,
                    Right = renderTargets[index].ScissorRect.X + renderTargets[index].ScissorRect.Width,
                    Bottom = renderTargets[index].ScissorRect.Y + renderTargets[index].ScissorRect.Height
                };
            this.Context.RS.ScissorRects = d3DrectArray;
            this.AppParameters.PixelDimensions.Value = new VectorMath.Vector2F(2f / (float)renderTargets[0].RenderTargetTexture.Width, 2f / (float)renderTargets[0].RenderTargetTexture.Height);
            this.AppParameters.FrameBufferPixelDimensions.Value = new VectorMath.Vector2F(2f / (float)this.BackBufferRenderTarget.RenderTargetTexture.Width, 2f / (float)this.BackBufferRenderTarget.RenderTargetTexture.Height);
            this.CurrentRenderTarget = (D3D11RenderTarget)renderTargets[renderTargets.Length - 1];
            this.currentTargets = renderTargets;
        }

        public override void ClearCurrentDepthTarget(float clearDepth)
        {
            DepthStencilView depthStencilView = this.CurrentRenderTarget.GetDepthStencilView(this.Device, this.Context);
            if (depthStencilView == null)
                return;
            this.Context.ClearDepthStencilView(depthStencilView, ClearOptions.Depth, clearDepth, 0);
        }

        public override void ClearCurrentStencilTarget(int clearStencil)
        {
            DepthStencilView depthStencilView = this.CurrentRenderTarget.GetDepthStencilView(this.Device, this.Context);
            if (depthStencilView == null)
                return;
            this.Context.ClearDepthStencilView(depthStencilView, ClearOptions.Stencil, 0.0f, (byte)clearStencil);
        }

        public override void SetTexture(int slot, Texture texture)
        {
            ShaderResourceView shaderResourceView = texture == null ? null : ((D3D11Texture)texture).GetResourceView(this.Device, this.Context);
            this.Context.PS.SetShaderResourceOne((uint)slot, shaderResourceView);
        }

        public override void SetVertexTexture(int slot, Texture texture)
        {
            ShaderResourceView shaderResourceView = texture == null ? null : ((D3D11Texture)texture).GetResourceView(this.Device, this.Context);
            this.Context.VS.SetShaderResourceOne((uint)slot, shaderResourceView);
        }

        public override void SetEffect(Effect effect)
        {
            if (this.currentEffect == effect)
                return;
            this.currentEffect = (D3D11Effect)effect;
            this.currentEffect.SetEffect(this.Device, this.Context);
            this.effectJustUpdated = true;
        }

        public override void SetVertexSource(VertexBuffer[] vertexBuffers)
        {
            if (this.currentVertexSource == vertexBuffers)
                return;
            DirectX.Direct3D11.VertexBuffer[] vertexBuffers1;
            if (vertexBuffers != null)
            {
                vertexBuffers1 = new DirectX.Direct3D11.VertexBuffer[vertexBuffers.Length];
                for (int index = 0; index < vertexBuffers.Length; ++index)
                {
                    D3DBuffer buffer = vertexBuffers[index] == null ? null : ((D3D11VertexBuffer)vertexBuffers[index]).GetD3D11Buffer(this.Device, this.Context);
                    vertexBuffers1[index] = new DirectX.Direct3D11.VertexBuffer(buffer, buffer == null ? 0U : (uint)vertexBuffers[index].VertexFormat.GetVertexSizeInBytes(), 0U);
                }
            }
            else
                vertexBuffers1 = new DirectX.Direct3D11.VertexBuffer[1]
        {
          new DirectX.Direct3D11.VertexBuffer(null, 0U, 0U)
        };
            this.Context.IA.SetVertexBuffers(0U, vertexBuffers1);
            this.currentVertexSource = vertexBuffers;
        }

        public override void SetIndexSource(IndexBuffer indexBuffer)
        {
            if (indexBuffer != null)
                this.Context.IA.IndexBuffer = new DirectX.Direct3D11.IndexBuffer(((D3D11IndexBuffer)indexBuffer).GetD3D11Buffer(this.Device, this.Context), indexBuffer.Use32BitIndices ? Format.R32UInt : Format.R16UInt, 0U);
            else
                this.Context.IA.IndexBuffer = new DirectX.Direct3D11.IndexBuffer(null, Format.Unknown, 0U);
        }

        public override void Draw(int startVertex, int vertexCount, PrimitiveTopology topology)
        {
            this.SetTopology(topology);
            this.UpdateEffectData();
            this.Context.Draw((uint)vertexCount, (uint)startVertex);
        }

        public override void DrawIndexed(int startIndex, int indexCount, PrimitiveTopology topology)
        {
            this.SetTopology(topology);
            this.UpdateEffectData();
            this.Context.DrawIndexed((uint)indexCount, (uint)startIndex, 0);
        }

        public override void DrawInstanced(int startVertex, int vertexCountPerInstance, int startInstance, int instanceCount, PrimitiveTopology topology)
        {
            this.SetTopology(topology);
            this.UpdateEffectData();
            this.Context.DrawInstanced((uint)vertexCountPerInstance, (uint)instanceCount, (uint)startVertex, (uint)startInstance);
        }

        public override void DrawIndexedInstanced(int startIndex, int indexCountPerInstance, int startInstance, int instanceCount, PrimitiveTopology topology)
        {
            this.SetTopology(topology);
            this.UpdateEffectData();
            this.Context.DrawIndexedInstanced((uint)indexCountPerInstance, (uint)instanceCount, (uint)startIndex, 0, (uint)startInstance);
        }

        public override void SetStreamBuffer(StreamBuffer buffer)
        {
            if (this.currentStreamBuffer == null && buffer == null)
                return;
            if (this.currentDepthStencilState != this.disabledRasterizer)
                this.savedDepthStencilState = this.currentDepthStencilState;
            this.SetDepthStencilState(this.disabledRasterizer);
            if (this.currentStreamBuffer != null && buffer != this.currentStreamBuffer)
                this.currentStreamBuffer.EndStreamFrame(this, this.currentStreamBuffer.GetDataEnabled);
            this.currentStreamBuffer = (D3D11StreamBuffer)buffer;
            if (this.currentStreamBuffer != null)
            {
                this.Context.SO.SetTargets(new D3DBuffer[]
                {
                    this.currentStreamBuffer.GetD3D11Buffer(this)
                }, new uint[1]);
            }
            else
            {
                this.Context.SO.SetTargets(null, null);
                if (this.currentDepthStencilState != this.disabledRasterizer || this.savedDepthStencilState == null)
                    return;
                this.SetDepthStencilState(this.savedDepthStencilState);
                this.savedDepthStencilState = null;
            }
        }

        private void UpdateEffectData()
        {
            this.UpdateEffectParameters();
            this.currentEffect.UpdateDataBuffer(this.Device, this.Context, 0);
        }

        private void UpdateEffectParameters()
        {
            if (this.effectJustUpdated || this.AppParameters.RenderParameters.IsDirty)
            {
                D3DBuffer[] constantBuffers = new D3DBuffer[1]
        {
          ((D3D11RenderParameters) this.AppParameters.RenderParameters).GetParametersBuffer(this.Device, this.Context)
        };
                this.Context.VS.SetConstantBuffers(0U, constantBuffers);
                this.Context.PS.SetConstantBuffers(0U, constantBuffers);
                if (this.Context.GS.Shader != null)
                    this.Context.GS.SetConstantBuffers(0U, constantBuffers);
            }
            if (this.effectJustUpdated || this.FrameParameters.RenderParameters.IsDirty)
            {
                D3DBuffer[] constantBuffers = new D3DBuffer[1]
        {
          ((D3D11RenderParameters) this.FrameParameters.RenderParameters).GetParametersBuffer(this.Device, this.Context)
        };
                this.Context.VS.SetConstantBuffers(1U, constantBuffers);
                this.Context.PS.SetConstantBuffers(1U, constantBuffers);
                if (this.Context.GS.Shader != null)
                    this.Context.GS.SetConstantBuffers(1U, constantBuffers);
            }
            if (this.currentEffect.EffectParameters != null && (this.effectJustUpdated || this.currentEffect.EffectParameters.IsDirty))
                this.currentEffect.UpdateParameters(this.Device, this.Context, 2, false);
            if (this.currentEffect.SharedEffectParameters != null)
            {
                bool flag = false;
                for (int index = 0; index < this.currentEffect.SharedEffectParameters.Length; ++index)
                {
                    if (this.currentEffect.SharedEffectParameters[index].IsDirty)
                    {
                        flag = true;
                        break;
                    }
                }
                if (this.effectJustUpdated || flag)
                    this.currentEffect.UpdateParameters(this.Device, this.Context, 3, true);
            }
            this.effectJustUpdated = false;
        }

        public void Reset()
        {
            this.currentTopology = PrimitiveTopology.None;
        }

        public RendererCoreState GetCoreState()
        {
            return new RendererCoreState()
            {
                AppParameters = this.appParameters,
                FrameParameters = this.frameParameters,
                BackBufferRenderTarget = this.BackBufferRenderTarget,
                CurrentTargets = this.currentTargets,
                CurrentRenderTarget = this.CurrentRenderTarget
            };
        }

        public void SetCoreState(RendererCoreState state)
        {
            this.BackBufferRenderTarget = state.BackBufferRenderTarget;
            this.CurrentRenderTarget = state.CurrentRenderTarget;
            this.currentTargets = (RenderTarget[])state.CurrentTargets.Clone();
            this.savedTargets.Clear();
            state.AppParameters.CopyTo(this.appParameters);
            state.FrameParameters.CopyTo(this.frameParameters);
            this.currentStreamBuffer = null;
            this.currentDepthStencilState = null;
            this.savedDepthStencilState = null;
            this.currentTopology = PrimitiveTopology.None;
            this.currentEffect = null;
            this.effectJustUpdated = false;
            this.currentVertexSource = null;
        }

        private static DirectX.Direct3D11.PrimitiveTopology GetTopology(PrimitiveTopology topology)
        {
            switch (topology)
            {
                case PrimitiveTopology.LineList:
                    return DirectX.Direct3D11.PrimitiveTopology.LineList;
                case PrimitiveTopology.LineStrip:
                    return DirectX.Direct3D11.PrimitiveTopology.LineStrip;
                case PrimitiveTopology.TriangleList:
                    return DirectX.Direct3D11.PrimitiveTopology.TriangleList;
                case PrimitiveTopology.TriangleStrip:
                    return DirectX.Direct3D11.PrimitiveTopology.TriangleStrip;
                case PrimitiveTopology.TriangleListWithAdjacency:
                    return DirectX.Direct3D11.PrimitiveTopology.TriangleListAdjacency;
                default:
                    return DirectX.Direct3D11.PrimitiveTopology.PointList;
            }
        }

        private void SetTopology(PrimitiveTopology topology)
        {
            if (topology == this.currentTopology)
                return;
            this.currentTopology = topology;
            this.Context.IA.PrimitiveTopology = D3D11RendererCore.GetTopology(topology);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            if (this.disabledRasterizer != null)
            {
                this.disabledRasterizer.Dispose();
                this.disabledRasterizer = null;
            }
            if (this.appParameters != null)
                this.appParameters.Dispose();
            if (this.frameParameters == null)
                return;
            this.frameParameters.Dispose();
        }
    }
}
