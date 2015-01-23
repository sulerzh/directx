using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class RenderParameterMatrix4x4F : RenderParameterBase
    {
        private Matrix4x4F data;

        public override int SizeInBytes
        {
            get
            {
                return 64;
            }
        }

        public Matrix4x4F Value
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

        public RenderParameterMatrix4x4F(string name)
            : base(name)
        {
        }

        public override unsafe void CopyDataToBlob(IntPtr blob)
        {
            var numPtr = (float*)blob.ToPointer();
            numPtr[0] = this.data.M11;
            numPtr[1] = this.data.M12;
            numPtr[2] = this.data.M13;
            numPtr[3] = this.data.M14;
            numPtr[4] = this.data.M21;
            numPtr[5] = this.data.M22;
            numPtr[6] = this.data.M23;
            numPtr[7] = this.data.M24;
            numPtr[8] = this.data.M31;
            numPtr[9] = this.data.M32;
            numPtr[10] = this.data.M33;
            numPtr[11] = this.data.M34;
            numPtr[12] = this.data.M41;
            numPtr[13] = this.data.M42;
            numPtr[14] = this.data.M43;
            numPtr[15] = this.data.M44;
        }
    }
}
