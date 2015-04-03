using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
    internal struct RegionVertex : IVertex
    {
        public float X;
        public float Y;
        public float Z;

        public VertexFormat Format
        {
            get
            {
                return VertexFormat.Create(
                    new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3, VertexComponentClassification.PerVertexData, 0));
            }
        }

        public RegionVertex(Vector3D position)
        {
            this.X = (float)position.X;
            this.Y = (float)position.Y;
            this.Z = (float)position.Z;
        }
    }
}
