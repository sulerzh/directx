using System;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class EffectData : GraphicsResource
    {
        protected bool isDirty;
        protected int dataSize;
        protected int dataCount;
        protected VertexComponentDataType componentType;
        protected EffectData sourceEffectData;

        protected IntPtr SourceData { get; set; }

        public int Count
        {
            get
            {
                return this.dataCount;
            }
        }

        public static EffectData Create()
        {
            return new D3D11EffectData();
        }

        public void SetData<DataType>(DataType[] data, VertexComponentDataType dataType) where DataType : struct
        {
            if (data == null)
            {
                IntPtr sourceData = this.SourceData;
                Marshal.FreeCoTaskMem(this.SourceData);
                this.SourceData = IntPtr.Zero;
                this.isDirty = true;
            }
            else
            {
                int num = Marshal.SizeOf(typeof(DataType));
                this.dataSize = num * data.Length;
                this.dataCount = data.Length;
                this.SourceData = Marshal.AllocCoTaskMem(this.dataSize);
                this.componentType = dataType;
                IntPtr sourceData = this.SourceData;
                for (int i = 0; i < data.Length; ++i)
                {
                    Marshal.StructureToPtr(data[i], sourceData, false);
                    sourceData += num;
                }
            }
        }

        public void SetData(EffectData effectData)
        {
            if (effectData == null)
                return;
            this.DisposeData();
            this.sourceEffectData = effectData;
        }

        public void SetDirty()
        {
            this.isDirty = true;
        }

        public IntPtr GetData()
        {
            return this.SourceData;
        }

        private void DisposeData()
        {
            if (!(this.SourceData != IntPtr.Zero))
                return;
            Marshal.FreeCoTaskMem(this.SourceData);
            this.SourceData = IntPtr.Zero;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.DisposeData();
        }
    }
}
