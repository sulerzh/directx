using System;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class IndexBuffer : GraphicsResource
    {
        protected bool isDirty;

        public bool IsDirty
        {
            get
            {
                return this.isDirty;
            }
        }

        public int IndexCount { get; private set; }

        public bool Use32BitIndices { get; private set; }

        protected IntPtr SourceData { get; set; }

        protected bool Immutable { get; set; }

        protected IndexBuffer(IntPtr data, bool use32BitIndices, int indexCount, bool immutable)
        {
            this.SourceData = data;
            this.Use32BitIndices = use32BitIndices;
            this.IndexCount = indexCount;
            this.Immutable = immutable;
        }

        public static IndexBuffer Create<IndexType>(IndexType[] data, bool immutable) where IndexType : struct
        {
            return IndexBuffer.Create<IndexType>(data, data.Length, immutable);
        }

        public static IndexBuffer Create<IndexType>(IndexType[] data, int indexCount, bool immutable) where IndexType : struct
        {
            int size = Marshal.SizeOf(typeof(IndexType));
            IntPtr indexBuffer = Marshal.AllocCoTaskMem(size * Math.Max(data == null ? 0 : data.Length, indexCount));
            if (data != null)
            {
                IntPtr currsor = indexBuffer;
                for (int i = 0; i < data.Length; ++i)
                {
                    Marshal.StructureToPtr(data[i], currsor, false);
                    currsor += size;
                }
            }
            return new D3D11IndexBuffer(indexBuffer, Marshal.SizeOf(typeof(IndexType)) == 4, indexCount, immutable);
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

        public IntPtr GetData()
        {
            return this.SourceData;
        }

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        protected static extern void CopyMemory(IntPtr Destination, IntPtr Source, uint Length);
    }
}
