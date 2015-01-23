using Microsoft.Data.Visualization.DirectX.Direct3D11;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11RenderParameters : RenderParameters
    {
        private IntPtr paramData = IntPtr.Zero;
        private D3DBuffer parametersBuffer;

        internal override bool IsDirty
        {
            set
            {
                base.IsDirty = value;
            }
        }

        public D3D11RenderParameters(IRenderParameter[] parameters)
            : base(parameters)
        {
        }

        public D3DBuffer GetParametersBuffer(D3DDevice device, DeviceContext context)
        {
            int cb = this.SizeInBytes + (this.SizeInBytes % 16 > 0 ? 16 - this.SizeInBytes % 16 : 0);
            if (this.paramData == IntPtr.Zero)
                this.paramData = Marshal.AllocCoTaskMem(cb);
            if (this.IsDirty)
            {
                IntPtr blob = this.paramData;
                foreach (IRenderParameter renderParameter in this.allParameters)
                {
                    int num = ((int)(blob.ToInt64() - this.paramData.ToInt64()) % 16 + renderParameter.SizeInBytes) % 32;
                    if (num > 16)
                        blob += 32 - num;
                    renderParameter.CopyDataToBlob(blob);
                    blob += renderParameter.SizeInBytes;
                }
            }
            if (this.parametersBuffer == null)
            {
                this.Register((Renderer)device.Tag);
                this.parametersBuffer = device.CreateBuffer(new BufferDescription()
                {
                    Usage = Usage.Default,
                    ByteWidth = (uint)cb,
                    BindingOptions = BindingOptions.ConstantBuffer,
                    CpuAccessOptions = CpuAccessOptions.None,
                    MiscellaneousResourceOptions = MiscellaneousResourceOptions.None,
                    StructureByteStride = 0U
                }, new SubresourceData()
                {
                    SystemMemory = this.paramData,
                    SystemMemoryPitch = 0U,
                    SystemMemorySlicePitch = 0U
                });
            }
            else if (this.IsDirty)
                context.UpdateSubresource(this.parametersBuffer, 0U, this.paramData, 0U, 0U);
            this.IsDirty = false;
            return this.parametersBuffer;
        }

        internal override int GetEstimatedSystemMemoryUsage()
        {
            return this.SizeInBytes;
        }

        internal override int GetEstimatedVideoMemoryUsage()
        {
            if (this.parametersBuffer == null)
                return 0;
            else
                return this.SizeInBytes;
        }

        protected override bool Reset()
        {
            this.ReleaseGraphicsResources();
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (this.paramData != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(this.paramData);
                this.paramData = IntPtr.Zero;
            }
            if (!disposing)
                return;
            this.ReleaseGraphicsResources();
        }

        private void ReleaseGraphicsResources()
        {
            if (this.parametersBuffer == null)
                return;
            this.parametersBuffer.Dispose();
            this.parametersBuffer = null;
        }
    }
}
