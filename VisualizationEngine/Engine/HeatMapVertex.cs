using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
    internal struct HeatMapVertex : IVertex
    {
        public Vector3F Position;
        public float Value;

        public VertexFormat Format
        {
            get
            {
                return VertexFormat.Create(HeatMapVertexFormat.VertexComponents[0], HeatMapVertexFormat.VertexComponents[1]);
            }
        }

        public HeatMapVertex(Vector3F position, float value)
        {
            this.Position = position;
            this.Value = value;
        }
    }
}
