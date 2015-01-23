using System;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class VertexBuffer : GraphicsResource
    {
        private Type vertexType;
        protected bool isDirty;

        protected IntPtr SourceData { get; set; }

        protected VertexFormat Format { get; set; }

        protected bool Immutable { get; set; }

        public int VertexCount { get; private set; }

        public VertexFormat VertexFormat
        {
            get
            {
                return this.Format;
            }
        }

        public bool IsDirty
        {
            get
            {
                return this.isDirty;
            }
        }

        protected VertexBuffer(IntPtr data, Type vertexType, VertexFormat vertexFormat, int vertexCount, bool immutable)
        {
            this.SourceData = data;
            this.vertexType = vertexType;
            this.Format = vertexFormat;
            this.VertexCount = vertexCount;
            this.Immutable = immutable;
        }

        public static VertexBuffer Create<VertexType>(VertexType[] data, bool immutable) where VertexType : IVertex, new()
        {
            return VertexBuffer.Create<VertexType>(data, data.Length, immutable);
        }

        public static VertexBuffer Create<VertexType>(VertexType[] data, int vertexCount, bool immutable) where VertexType : IVertex, new()
        {
            int vetexSize = Marshal.SizeOf(typeof(VertexType));
            IntPtr buffer = Marshal.AllocCoTaskMem(vetexSize * Math.Max(data == null ? 0 : data.Length, vertexCount));
            if (data != null)
            {
                IntPtr ptr = buffer;
                for (int i = 0; i < data.Length; ++i)
                {
                    Marshal.StructureToPtr(data[i], ptr, false);
                    ptr += vetexSize;
                }
            }
            return new D3D11VertexBuffer(buffer, typeof(VertexType), new VertexType().Format, vertexCount, immutable);
        }

        public IntPtr GetData()
        {
            return this.SourceData;
        }

        public bool Update<VertexType>(VertexType[] data, int sourceIndex, int destIndex, int vertexCount)
        {
            if (this.Immutable || data == null ||
                this.VertexCount < vertexCount + destIndex || vertexCount + sourceIndex > data.Length || 
                this.vertexType != typeof(VertexType) || this.SourceData == IntPtr.Zero)
                return false;
            int vetexSize = Marshal.SizeOf(typeof(VertexType));
            IntPtr ptr = this.SourceData + vetexSize * destIndex;
            for (int i = sourceIndex; i < sourceIndex + vertexCount; ++i)
            {
                Marshal.StructureToPtr(data[i], ptr, false);
                ptr += vetexSize;
            }
            this.SetDirty();
            return true;
        }

        public unsafe bool Zero()
        {
            if (this.Immutable)
                return false;
            int cb = this.VertexCount * this.Format.GetVertexSizeInBytes();
            int mod = cb % 4;
            if (this.SourceData == IntPtr.Zero)
                this.SourceData = Marshal.AllocCoTaskMem(cb);
            int* srcIntArray = (int*)this.SourceData.ToPointer();
            for (int i = 0; i < (cb - mod) / 4; ++i)
                srcIntArray[i] = 0;
            byte* srcByteArray = (byte*)this.SourceData.ToPointer();
            for (int i = 0; i < mod; ++i)
                srcByteArray[cb - mod + i] = 0;
            this.SetDirty();
            return true;
        }

        public void SetDirty()
        {
            this.isDirty = true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!(this.SourceData != IntPtr.Zero))
                return;
            Marshal.FreeCoTaskMem(this.SourceData);
            this.SourceData = IntPtr.Zero;
        }
    }
}
