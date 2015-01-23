using System;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11IndexBuffer : IndexBuffer
    {
        private DirectX.Direct3D11.D3DBuffer internalBuffer;

        public D3D11IndexBuffer(IntPtr data, bool use32BitIndices, int indexCount, bool immutable)
            : base(data, use32BitIndices, indexCount, immutable)
        {
            this.isDirty = data != IntPtr.Zero;
        }

        internal DirectX.Direct3D11.D3DBuffer GetD3D11Buffer(
            DirectX.Direct3D11.D3DDevice device, 
            DirectX.Direct3D11.DeviceContext context)
        {
            if (this.Disposed)
            {
                D3DDeviceExtension.NotifyError(device, "Attempting to use a disposed index buffer", (Exception)null);
                return null;
            }
            else
            {
                if (this.internalBuffer == null && this.SourceData != IntPtr.Zero)
                {
                    this.Register((Renderer)device.Tag);
                    int idxByteCount = this.Use32BitIndices ? 4 : 2;
                    DirectX.Direct3D11.BufferDescription description =
                        new DirectX.Direct3D11.BufferDescription()
                        {
                            ByteWidth = (uint) (idxByteCount*this.IndexCount),
                            Usage = this.Immutable ? DirectX.Direct3D11.Usage.Immutable : DirectX.Direct3D11.Usage.Default,
                            BindingOptions = DirectX.Direct3D11.BindingOptions.IndexBuffer,
                            CpuAccessOptions = DirectX.Direct3D11.CpuAccessOptions.None,
                            MiscellaneousResourceOptions = DirectX.Direct3D11.MiscellaneousResourceOptions.None,
                            StructureByteStride = 0U
                        };
                    DirectX.Direct3D11.SubresourceData subresourceData =
                        new DirectX.Direct3D11.SubresourceData()
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
                        D3DDeviceExtension.NotifyError(device, "An error occured while creating an index buffer (likely due to OOM).", ex);
                    }
                    if (this.internalBuffer == null)
                        return null;
                }
                else if (this.IsDirty && this.SourceData != IntPtr.Zero)
                    context.UpdateSubresource(this.internalBuffer, 0U, this.SourceData, 0U, 0U);
                this.isDirty = false;
                return this.internalBuffer;
            }
        }

        internal override int GetEstimatedSystemMemoryUsage()
        {
            if (this.SourceData == IntPtr.Zero) return 0;
            int idxByteCount = this.Use32BitIndices ? 4 : 2;
            return this.IndexCount * idxByteCount;
        }

        internal override int GetEstimatedVideoMemoryUsage()
        {
            if (this.internalBuffer == null) 
                return 0;
            int idxByteCount = this.Use32BitIndices ? 4 : 2;
            return this.IndexCount * idxByteCount;
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
            if (this.internalBuffer == null)
                return;
            this.internalBuffer.Dispose();
            this.internalBuffer = null;
        }
    }
}
