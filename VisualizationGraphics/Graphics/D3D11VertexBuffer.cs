using Microsoft.Data.Visualization.DirectX.Direct3D11;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11VertexBuffer : VertexBuffer
    {
        private D3DBuffer internalBuffer;
        private bool ownInternalBuffer;

        public D3D11VertexBuffer(IntPtr data, Type vertexType, VertexFormat vertexFormat, int vertexCount, bool immutable)
            : base(data, vertexType, vertexFormat, vertexCount, immutable)
        {
            this.isDirty = data != IntPtr.Zero;
            this.ownInternalBuffer = true;
        }

        internal D3D11VertexBuffer(Type vertexType, VertexFormat vertexFormat, int vertexCount, D3DBuffer buffer)
            : base(IntPtr.Zero, vertexType, vertexFormat, vertexCount, false)
        {
            this.internalBuffer = buffer;
            this.ownInternalBuffer = false;
        }

        internal D3DBuffer GetD3D11Buffer(D3DDevice device, DeviceContext context)
        {
            if (this.Disposed)
            {
                D3DDeviceExtension.NotifyError(device, "Attempting to use a disposed vertex buffer", (Exception)null);
                return null;
            }
            else
            {
                if (this.internalBuffer == null)
                {
                    this.Register((Renderer)device.Tag);
                    D3D11VertexFormat d3D11VertexFormat = (D3D11VertexFormat)this.Format;
                    if (d3D11VertexFormat != null && this.SourceData != IntPtr.Zero)
                    {
                        BufferDescription description = new BufferDescription()
                        {
                            ByteWidth = (uint)(d3D11VertexFormat.GetVertexSizeInBytes() * this.VertexCount),
                            Usage = this.Immutable ? Usage.Immutable : Usage.Default,
                            BindingOptions = BindingOptions.VertexBuffer,
                            CpuAccessOptions = CpuAccessOptions.None,
                            MiscellaneousResourceOptions = MiscellaneousResourceOptions.None,
                            StructureByteStride = 0U
                        };
                        
                        SubresourceData subresourceData = new SubresourceData()
                        {
                            SystemMemory = this.SourceData,
                            SystemMemoryPitch = 0U,
                            SystemMemorySlicePitch = 0U
                        };
                        
                        try
                        {
                            this.internalBuffer = device.CreateBuffer(description, subresourceData);
                            if (this.Immutable)
                            {
                                Marshal.FreeCoTaskMem(this.SourceData);
                                this.SourceData = IntPtr.Zero;
                            }
                        }
                        catch (Exception ex)
                        {
                            D3DDeviceExtension.NotifyError(device, "An error occurred while creating a vertex buffer (likely due to OOM).", ex);
                        }
                    }
                    if (this.internalBuffer == null)
                        return null;
                }
                else if (this.isDirty && this.SourceData != IntPtr.Zero)
                    context.UpdateSubresource(this.internalBuffer, 0U, this.SourceData, 0U, 0U);
                this.isDirty = false;
                return this.internalBuffer;
            }
        }

        internal override int GetEstimatedSystemMemoryUsage()
        {
            if (this.SourceData != IntPtr.Zero)
                return this.VertexFormat.GetVertexSizeInBytes() * this.VertexCount;
            return 0;
        }

        internal override int GetEstimatedVideoMemoryUsage()
        {
            if (this.internalBuffer != null && this.ownInternalBuffer)
                return this.VertexFormat.GetVertexSizeInBytes() * this.VertexCount;
            return 0;
        }

        protected override bool Reset()
        {
            this.ReleaseGraphicsResources();
            return !this.Immutable;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            this.ReleaseGraphicsResources();
        }

        private void ReleaseGraphicsResources()
        {
            if (this.internalBuffer == null || !this.ownInternalBuffer)
                return;
            this.internalBuffer.Dispose();
            this.internalBuffer = null;
        }
    }
}
