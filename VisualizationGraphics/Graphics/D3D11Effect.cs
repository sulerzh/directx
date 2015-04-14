using System;
using System.IO;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11Effect : Effect
    {
        private DirectX.Direct3D11.InputLayout inputLayout;
        private DirectX.Direct3D11.VertexShader vertexShader;
        private DirectX.Direct3D11.GeometryShader geometryShader;
        private DirectX.Direct3D11.PixelShader pixelShader;

        public D3D11Effect(EffectDefinition definition)
            : base(definition)
        {
        }

        internal void SetEffect(DirectX.Direct3D11.D3DDevice device, DirectX.Direct3D11.DeviceContext context)
        {
            if (this.Samplers != null)
            {
                for (int slot = 0; slot < this.Samplers.Length; ++slot)
                    ((D3D11TextureSampler)this.Samplers[slot]).SetSampler(device, context, slot);
            }
            try
            {
                if (this.vertexShader == null && this.geometryShader == null && this.pixelShader == null)
                    this.Register((Renderer)device.Tag);
                if (this.vertexShader == null)
                {
                    this.vertexShader = this.CreateVertexShader(device, this.vertexShaderData);
                    this.vertexShaderData.Seek(0L, SeekOrigin.Begin);
                    if (this.inputLayout == null)
                    {
                        D3D11VertexFormat d3D11VertexFormat = (D3D11VertexFormat)this.VertexFormat;
                        if (d3D11VertexFormat != null)
                            this.inputLayout = device.CreateInputLayout(d3D11VertexFormat.GetFormat(), this.vertexShaderData);
                    }
                }
                if (this.geometryShader == null && (this.geometryShaderData != null || this.vertexShaderData != null))
                    this.geometryShader = this.CreateGeometryShader(device, this.geometryShaderData, this.vertexShaderData);
                if (this.pixelShader == null && this.pixelShaderData != null)
                    this.pixelShader = device.CreatePixelShader(this.pixelShaderData);
                context.VS.Shader = this.vertexShader;
                context.GS.Shader = this.geometryShader;
                context.PS.Shader = this.pixelShader;
                context.IA.InputLayout = this.inputLayout;
            }
            catch (Exception ex)
            {
                D3DDeviceExtension.NotifyError(device, "An error occurred while creating/updating a rendering effect.", ex);
            }
        }

        protected virtual DirectX.Direct3D11.GeometryShader CreateGeometryShader(DirectX.Direct3D11.D3DDevice device, Stream data, Stream vsData)
        {
            if (data == null)
                return null;
            else
                return device.CreateGeometryShader(data);
        }

        protected virtual DirectX.Direct3D11.VertexShader CreateVertexShader(DirectX.Direct3D11.D3DDevice device, Stream vsData)
        {
            return device.CreateVertexShader(vsData);
        }

        internal void UpdateParameters(DirectX.Direct3D11.D3DDevice device, DirectX.Direct3D11.DeviceContext context, int contextBufferSlot, bool updateSharedParams)
        {
            RenderParameters[] renderParametersArray = updateSharedParams
                ? this.SharedEffectParameters
                : (RenderParameters[]) new D3D11RenderParameters[]
                {
                    (D3D11RenderParameters) this.EffectParameters
                };

            if (renderParametersArray == null)
                return;
            DirectX.Direct3D11.D3DBuffer[] constantBuffers = new DirectX.Direct3D11.D3DBuffer[renderParametersArray.Length];
            for (int i = 0; i < constantBuffers.Length; ++i)
                constantBuffers[i] = ((D3D11RenderParameters)renderParametersArray[i]).GetParametersBuffer(device, context);
            context.VS.SetConstantBuffers((uint)contextBufferSlot, constantBuffers);
            if (this.pixelShader != null)
                context.PS.SetConstantBuffers((uint)contextBufferSlot, constantBuffers);
            if (this.geometryShader == null)
                return;
            context.GS.SetConstantBuffers((uint)contextBufferSlot, constantBuffers);
        }

        internal void UpdateDataBuffer(DirectX.Direct3D11.D3DDevice device, DirectX.Direct3D11.DeviceContext context, int dataBufferSlot)
        {
            if (this.Data == null)
                return;
            ((D3D11EffectData)this.Data).SetResourceView(device, context, dataBufferSlot);
        }

        internal override int GetEstimatedSystemMemoryUsage()
        {
            return
                (this.vertexShaderData != null ? (int)this.vertexShaderData.Length : 0) + 
                (this.pixelShaderData != null ? (int)this.pixelShaderData.Length : 0) + 
                (this.geometryShaderData != null ? (int)this.geometryShaderData.Length : 0);
        }

        internal override int GetEstimatedVideoMemoryUsage()
        {
            return this.GetEstimatedSystemMemoryUsage();
        }

        protected override bool Reset()
        {
            this.DisposeShaders();
            if (this.vertexShaderData != null)
                this.vertexShaderData.Seek(0L, SeekOrigin.Begin);
            if (this.pixelShaderData != null)
                this.pixelShaderData.Seek(0L, SeekOrigin.Begin);
            if (this.geometryShaderData != null)
                this.geometryShaderData.Seek(0L, SeekOrigin.Begin);
            return true;
        }

        private void DisposeShaders()
        {
            if (this.vertexShader != null)
            {
                this.vertexShader.Dispose();
                this.vertexShader = null;
            }
            if (this.geometryShader != null)
            {
                this.geometryShader.Dispose();
                this.geometryShader = null;
            }
            if (this.pixelShader != null)
            {
                this.pixelShader.Dispose();
                this.pixelShader = null;
            }
            if (this.inputLayout == null)
                return;
            this.inputLayout.Dispose();
            this.inputLayout = null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            this.DisposeShaders();
            if (this.vertexShaderData != null)
            {
                this.vertexShaderData.Dispose();
                this.vertexShaderData = null;
            }
            if (this.pixelShaderData != null)
            {
                this.pixelShaderData.Dispose();
                this.pixelShaderData = null;
            }
            if (this.geometryShaderData == null)
                return;
            this.geometryShaderData.Dispose();
            this.geometryShaderData = null;
        }
    }
}
