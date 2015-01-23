using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11EffectData : EffectData
    {
        private DirectX.Direct3D11.D3DBuffer dataBuffer;
        private DirectX.Direct3D11.ShaderResourceView view;

        internal void SetResourceView(DirectX.Direct3D11.D3DDevice device, DirectX.Direct3D11.DeviceContext context, int slot)
        {
            if (this.sourceEffectData != null)
            {
                ((D3D11EffectData)this.sourceEffectData).SetResourceView(device, context, slot);
            }
            else
            {
                if (this.dataSize != 0)
                {
                    int num = this.dataCount;
                }
                if (this.dataBuffer == null && this.SourceData != IntPtr.Zero)
                {
                    this.Register((Renderer)device.Tag);
                    this.dataBuffer = device.CreateBuffer(
                        new DirectX.Direct3D11.BufferDescription()
                        {
                            Usage = DirectX.Direct3D11.Usage.Default,
                            ByteWidth = (uint) this.dataSize,
                            BindingOptions = DirectX.Direct3D11.BindingOptions.ShaderResource,
                            CpuAccessOptions = DirectX.Direct3D11.CpuAccessOptions.None,
                            MiscellaneousResourceOptions = DirectX.Direct3D11.MiscellaneousResourceOptions.None,
                            StructureByteStride = 0U
                        },
                        new DirectX.Direct3D11.SubresourceData()
                        {
                            SystemMemory = this.SourceData,
                            SystemMemoryPitch = 0U,
                            SystemMemorySlicePitch = 0U
                        });
                }
                else if (this.isDirty)
                {
                    if (this.SourceData != IntPtr.Zero)
                        context.UpdateSubresource(this.dataBuffer, 0U, this.SourceData, 0U, 0U);
                    else
                        this.DisposeData();
                }
                this.isDirty = false;
                if (this.dataBuffer == null)
                    return;
                if (this.view == null)
                {
                    DirectX.Direct3D11.ShaderResourceViewDescription description = 
                        new DirectX.Direct3D11.ShaderResourceViewDescription()
                    {
                        Format = D3D11VertexFormat.GetD3DFormat(this.componentType),
                        ViewDimension = DirectX.Direct3D11.ShaderResourceViewDimension.Buffer,
                        Buffer = new DirectX.Direct3D11.BufferShaderResourceView()
                        {
                            ElementOffset = 0U,
                            ElementWidth = (uint)this.dataSize / (uint)VertexComponent.GetComponentSize(this.componentType)
                        }
                    };
                    this.view = device.CreateShaderResourceView(this.dataBuffer, description);
                }
                context.VS.SetShaderResources(
                    (uint)slot, 
                    new DirectX.Direct3D11.ShaderResourceView[] { this.view });
            }
        }

        internal override int GetEstimatedSystemMemoryUsage()
        {
            return 0;
        }

        internal override int GetEstimatedVideoMemoryUsage()
        {
            if (this.dataBuffer == null)
                return 0;
            else
                return (int)this.dataBuffer.Description.ByteWidth;
        }

        protected override bool Reset()
        {
            this.DisposeData();
            return true;
        }

        private void DisposeData()
        {
            if (this.view != null)
            {
                this.view.Dispose();
                this.view = null;
            }
            if (this.dataBuffer == null)
                return;
            this.dataBuffer.Dispose();
            this.dataBuffer = null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            this.DisposeData();
        }
    }
}
