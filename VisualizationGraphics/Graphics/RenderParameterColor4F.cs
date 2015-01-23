using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class RenderParameterColor4F : RenderParameterBase
    {
        private Color4F data;

        public override int SizeInBytes
        {
            get
            {
                return 16;
            }
        }

        public Color4F Value
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

        public RenderParameterColor4F(string name)
            : base(name)
        {
        }

        public override unsafe void CopyDataToBlob(IntPtr blob)
        {
            var numPtr = (float*)blob.ToPointer();
            numPtr[0] = this.data.R;
            numPtr[1] = this.data.G;
            numPtr[2] = this.data.B;
            numPtr[3] = this.data.A;
        }
    }
}
