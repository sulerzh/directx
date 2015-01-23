using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class RenderParameterInt : RenderParameterBase
    {
        private int data;

        public override int SizeInBytes
        {
            get
            {
                return 4;
            }
        }

        public int Value
        {
            get
            {
                return this.data;
            }
            set
            {
                if (this.data.Equals(value))
                    return;
                this.data = value;
                this.NotifyParents();
            }
        }

        public RenderParameterInt(string name)
            : base(name)
        {
        }

        public override unsafe void CopyDataToBlob(IntPtr blob)
        {
            *(int*)blob.ToPointer() = this.data;
        }
    }
}
