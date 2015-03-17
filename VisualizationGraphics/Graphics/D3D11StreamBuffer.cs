using Microsoft.Data.Visualization.DirectX;
using Microsoft.Data.Visualization.DirectX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11StreamBuffer : StreamBuffer
    {
        private Queue<BufferInfo> inUseReadableBuffers = new Queue<BufferInfo>();
        private Queue<BufferInfo> availableReadableBuffers = new Queue<BufferInfo>();
        private D3D11VertexFormat format;
        private Array mostRecentData;
        private int mostRecentDataVertexCount;
        private BufferInfo mostRecentBuffer;
        private int mostRecentBufferVertexCount;
        private D3D11Renderer renderer;

        public D3D11StreamBuffer(VertexFormat vertexFormat, int vertexCount)
            : base(vertexCount)
        {
            this.format = (D3D11VertexFormat)vertexFormat;
        }

        public override VertexType[] GetData<VertexType>(out int vertexCount)
        {
            this.ReadData<VertexType>(false);
            vertexCount = this.mostRecentDataVertexCount;
            return (VertexType[])this.mostRecentData;
        }

        public override VertexType[] GetDataImmediate<VertexType>(out int vertexCount)
        {
            this.ReadData<VertexType>(true);
            vertexCount = this.mostRecentDataVertexCount;
            return (VertexType[])this.mostRecentData;
        }

        public override VertexBuffer GetVertexBuffer(out int vertexCount)
        {
            vertexCount = 0;
            if (this.renderer == null)
                return null;
            this.UpdateMostRecentBuffer();
            if (this.mostRecentBuffer == null)
                return null;
            vertexCount = this.mostRecentBufferVertexCount;
            return new D3D11VertexBuffer(typeof(int), this.format, this.mostRecentBufferVertexCount, this.mostRecentBuffer.SourceBuffer);
        }

        private void UpdateMostRecentBuffer()
        {
            if (this.renderer == null)
                return;
            BufferInfo bufferInfo = null;
            int vertexCount;
            lock (this.renderer.ContextLock)
                bufferInfo = this.GetMostRecentData(false, out vertexCount);
            if (bufferInfo == null)
                return;
            if (this.mostRecentBuffer != null)
                this.availableReadableBuffers.Enqueue(this.mostRecentBuffer);
            this.mostRecentBuffer = bufferInfo;
            this.mostRecentBufferVertexCount = vertexCount;
        }

        public override VertexBuffer PeekVertexBuffer()
        {
            D3DBuffer buffer;
            if (this.inUseReadableBuffers.Count == 0)
            {
                if (this.availableReadableBuffers.Count != 1)
                    return null;
                buffer = this.availableReadableBuffers.Peek().SourceBuffer;
            }
            else
                buffer = this.inUseReadableBuffers.Peek().SourceBuffer;
            return new D3D11VertexBuffer(typeof(int), this.format, this.VertexCount, buffer);
        }

        public override void ResetBuffer()
        {
            while (this.inUseReadableBuffers.Count > 0)
                this.availableReadableBuffers.Enqueue(this.inUseReadableBuffers.Dequeue());
            this.mostRecentBuffer = null;
            this.mostRecentBufferVertexCount = 0;
            this.mostRecentData = null;
            this.mostRecentDataVertexCount = 0;
        }

        internal void ReadData<VertexType>(bool waitUntilAvailable) where VertexType : struct, IVertex
        {
            if (this.renderer == null)
                return;
            int count = this.inUseReadableBuffers.Count;
            try
            {
                lock (this.renderer.ContextLock)
                {
                    int vetexCount;
                    BufferInfo bufferInfo = this.GetMostRecentData(waitUntilAvailable, out vetexCount);
                    if (bufferInfo == null)
                    {
                        if (this.mostRecentBuffer == null)
                            return;
                        bufferInfo = this.mostRecentBuffer;
                        vetexCount = this.mostRecentBufferVertexCount;
                    }
                    MappedSubresource res = this.renderer.Context.Map(bufferInfo.DestBuffer, 0U, Map.Read, MapOptions.None);
                    if (!(res.Data != IntPtr.Zero))
                        return;
                    this.mostRecentDataVertexCount = vetexCount;
                    if (this.mostRecentData == null || this.mostRecentData.Length < vetexCount)
                        this.mostRecentData = new VertexType[vetexCount];
                    int size = default(VertexType).Format.GetVertexSizeInBytes();
                    for (int i = 0; i < vetexCount; ++i)
                        this.mostRecentData.SetValue(Marshal.PtrToStructure(res.Data + i * size, typeof(VertexType)), i);
                    this.renderer.Context.Unmap(bufferInfo.DestBuffer, 0U);
                    this.availableReadableBuffers.Enqueue(bufferInfo);
                }
            }
            catch (DirectXException ex)
            {
                this.renderer.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.renderer.CheckDeviceRemoved(ex);
            }
        }

        private BufferInfo GetMostRecentData(bool waitUntilAvailable, out int vertexCount)
        {
            if (!this.GetDataEnabled)
            {
                vertexCount = -1;
                return null;
            }
            vertexCount = 0;
            if (this.inUseReadableBuffers.Count == 0)
                return null;
            D3D11StreamBuffer.BufferInfo bufferInfo = null;
            int vetexCount;
            do
            {
                vetexCount = this.inUseReadableBuffers.Peek().GetVertexCount(waitUntilAvailable || this.inUseReadableBuffers.Count > 1);
                if (vetexCount >= 0)
                {
                    if (bufferInfo != null)
                        this.availableReadableBuffers.Enqueue(bufferInfo);
                    bufferInfo = this.inUseReadableBuffers.Dequeue();
                    vertexCount = vetexCount;
                }
            }
            while (vetexCount >= 0 && this.inUseReadableBuffers.Count > 0);
            return bufferInfo;
        }

        internal void EndStreamFrame(D3D11RendererCore d3dRenderer, bool copyResource)
        {
            if (this.renderer == null)
                this.Register((Renderer)d3dRenderer.Device.Tag);
            this.renderer = (D3D11Renderer)d3dRenderer.Device.Tag;
            BufferInfo buffer = this.GetBuffer(d3dRenderer.Device);
            if (this.GetDataEnabled)
                buffer.EndQuery();
            if (!copyResource)
                return;
            d3dRenderer.Context.CopyResource(buffer.DestBuffer, buffer.SourceBuffer);
        }

        internal D3DBuffer GetD3D11Buffer(D3D11RendererCore d3d11Renderer)
        {
            if (this.renderer == null)
                this.Register((Renderer)d3d11Renderer.Device.Tag);
            this.renderer = (D3D11Renderer)d3d11Renderer.Device.Tag;
            try
            {
                this.UpdateMostRecentBuffer();
                BufferInfo bufferInfo = this.PeekBuffer(this.renderer.Device);
                if (this.GetDataEnabled)
                    bufferInfo.BeginQuery();
                return bufferInfo.SourceBuffer;
            }
            catch (DirectXException ex)
            {
                this.renderer.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.renderer.CheckDeviceRemoved(ex);
            }
            return null;
        }

        private D3DBuffer CreateStagingBuffer(D3DDevice device)
        {
            return device.CreateBuffer(new BufferDescription()
            {
                ByteWidth = (uint)(this.format.GetVertexSizeInBytes() * this.VertexCount),
                Usage = Usage.Staging,
                BindingOptions = BindingOptions.None,
                CpuAccessOptions = CpuAccessOptions.Read,
                MiscellaneousResourceOptions = MiscellaneousResourceOptions.None,
                StructureByteStride = 0U
            });
        }

        private D3DBuffer CreateSourceBuffer(D3DDevice device)
        {
            return device.CreateBuffer(new BufferDescription()
            {
                ByteWidth = (uint)(this.format.GetVertexSizeInBytes() * this.VertexCount),
                Usage = Usage.Default,
                BindingOptions = BindingOptions.VertexBuffer | BindingOptions.StreamOutput,
                CpuAccessOptions = CpuAccessOptions.None,
                MiscellaneousResourceOptions = MiscellaneousResourceOptions.None,
                StructureByteStride = 0U
            });
        }

        private BufferInfo PeekBuffer(D3DDevice device)
        {
            if (this.availableReadableBuffers.Count == 0)
            {
                if (!this.GetDataEnabled && this.inUseReadableBuffers.Count > 0)
                    this.availableReadableBuffers.Enqueue(this.inUseReadableBuffers.Dequeue());
                else
                    this.availableReadableBuffers.Enqueue(this.CreateBufferInfo(device, this.renderer.Context));
            }
            return this.availableReadableBuffers.Peek();
        }

        private BufferInfo GetBuffer(D3DDevice device)
        {
            BufferInfo bufferInfo = this.availableReadableBuffers.Count <= 0 ? this.CreateBufferInfo(device, this.renderer.Context) : this.availableReadableBuffers.Dequeue();
            this.inUseReadableBuffers.Enqueue(bufferInfo);
            return bufferInfo;
        }

        private BufferInfo CreateBufferInfo(D3DDevice device, DeviceContext context)
        {
            D3DBuffer destBuffer = null;
            if (this.GetDataEnabled)
                destBuffer = this.CreateStagingBuffer(device);
            return new BufferInfo(this.CreateSourceBuffer(device), destBuffer, device, context);
        }

        internal override int GetEstimatedSystemMemoryUsage()
        {
            return this.format.GetVertexSizeInBytes() * this.VertexCount * (this.inUseReadableBuffers.Count + this.availableReadableBuffers.Count);
        }

        internal override int GetEstimatedVideoMemoryUsage()
        {
            return this.GetEstimatedSystemMemoryUsage();
        }

        protected override bool Reset()
        {
            this.ReleaseGraphicsResources();
            return true;
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
            foreach (DisposableResource disposableResource in this.inUseReadableBuffers)
                disposableResource.Dispose();
            this.inUseReadableBuffers.Clear();
            foreach (DisposableResource disposableResource in this.availableReadableBuffers)
                disposableResource.Dispose();
            this.availableReadableBuffers.Clear();
            if (this.mostRecentBuffer != null)
                this.mostRecentBuffer.Dispose();
            this.mostRecentBuffer = null;
        }

        private class BufferInfo : DisposableResource
        {
            public D3DBuffer DestBuffer;
            public D3DBuffer SourceBuffer;
            private DeviceContext context;
            private D3DQuery query;
            private IntPtr queryData;

            public BufferInfo(D3DBuffer sourceBuffer, D3DBuffer destBuffer, D3DDevice d3dDevice, DeviceContext d3dContext)
            {
                this.SourceBuffer = sourceBuffer;
                this.DestBuffer = destBuffer;
                this.context = d3dContext;
                this.query = d3dDevice.CreateQuery(new QueryDescription()
                {
                    Query = Query.StreamOutputStatistics,
                    MiscellaneousQueryOptions = MiscellaneousQueryOptions.None
                });
                this.queryData = Marshal.AllocCoTaskMem((int)this.query.DataSize);
            }

            public void BeginQuery()
            {
                this.context.Begin(this.query);
            }

            public void EndQuery()
            {
                this.context.End(this.query);
            }

            public unsafe int GetVertexCount(bool waitUntilAvailable)
            {
                bool flag;
                if (waitUntilAvailable)
                {
                    while (!this.context.GetData(
                        this.query, this.queryData, this.query.DataSize, 
                        waitUntilAvailable ? AsyncGetDataOptions.None : AsyncGetDataOptions.DoNotFlush))
                    { }
                    flag = true;
                }
                else
                    flag = this.context.GetData(this.query, this.queryData, this.query.DataSize, waitUntilAvailable ? AsyncGetDataOptions.None : AsyncGetDataOptions.DoNotFlush);
                if (!flag)
                    return -1;
                ulong num1 = ((ulong*)this.queryData.ToPointer())[0];
                long num2 = ((long*)this.queryData.ToPointer())[1];
                return (int)num1;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (this.DestBuffer != null)
                        this.DestBuffer.Dispose();
                    this.SourceBuffer.Dispose();
                    this.query.Dispose();
                }
                Marshal.FreeCoTaskMem(this.queryData);
                this.queryData = IntPtr.Zero;
            }
        }
    }
}
