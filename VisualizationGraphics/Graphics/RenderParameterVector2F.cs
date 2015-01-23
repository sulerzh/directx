using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class RenderParameterVector2F : RenderParameterBase
    {
        private Vector2F data;

        public override int SizeInBytes
        {
            get
            {
                return 8;
            }
        }

        public Vector2F Value
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

        public RenderParameterVector2F(string name)
            : base(name)
        {
        }

        public override unsafe void CopyDataToBlob(IntPtr blob)
        {
            var numPtr = (float*)blob.ToPointer();
            numPtr[0] = this.data.X;
            numPtr[1] = this.data.Y;
        }
    }
}
