using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class RenderParameterBool : RenderParameterBase
    {
        private bool data;

        public override int SizeInBytes
        {
            get
            {
                return 4;
            }
        }

        public bool Value
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

        public RenderParameterBool(string name)
            : base(name)
        {
        }

        public override unsafe void CopyDataToBlob(IntPtr blob)
        {
            *((int*)blob.ToPointer()) = this.data ? 1 : 0;
        }
    }
}
