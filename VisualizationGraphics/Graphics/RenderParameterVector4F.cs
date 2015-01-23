using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class RenderParameterVector4F : RenderParameterBase
    {
        private Vector4F data;

        public override int SizeInBytes
        {
            get
            {
                return 16;
            }
        }

        public Vector4F Value
        {
            get
            {
                return this.data;
            }
            set
            {
                if (this.data.Equals((object)value))
                    return;
                this.data = value;
                this.NotifyParents();
            }
        }

        public RenderParameterVector4F(string name)
            : base(name)
        {
        }

        public override unsafe void CopyDataToBlob(IntPtr blob)
        {
            var numPtr = (float*)blob.ToPointer();
            numPtr[0] = this.data.X;
            numPtr[1] = this.data.Y;
            numPtr[2] = this.data.Z;
            numPtr[3] = this.data.W;
        }
    }
}
