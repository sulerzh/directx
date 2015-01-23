using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class RenderParameterFloat : RenderParameterBase
    {
        private float data;

        public override int SizeInBytes
        {
            get
            {
                return 4;
            }
        }

        public float Value
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

        public RenderParameterFloat(string name)
            : base(name)
        {
        }

        public override unsafe void CopyDataToBlob(IntPtr blob)
        {
            *((float*)blob.ToPointer()) = this.data;
        }
    }
}
