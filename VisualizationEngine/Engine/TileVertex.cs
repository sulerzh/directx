using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
    internal struct TileVertex : IVertex
    {
        public float X;
        public float Y;
        public float Z;
        private short tu;
        private short tv;

        public Vector3F Position
        {
            set
            {
                this.X = value.X;
                this.Y = value.Y;
                this.Z = value.Z;
            }
        }

        public float Tu
        {
            set
            {
                this.tu = (short)(value * short.MaxValue);
            }
        }

        public float Tv
        {
            set
            {
                this.tv = (short)(value * short.MaxValue);
            }
        }

        public VertexFormat Format
        {
            get
            {
                return VertexFormat.Create(
                    new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3), 
                    new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Short2AsFloats));
            }
        }
    }
}
