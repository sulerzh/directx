using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
    internal struct InstanceTime : IVertex
    {
        public float StartTime;
        public float EndTime;

        public VertexFormat Format
        {
            get
            {
                return VertexFormat.Create(new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2));
            }
        }
    }
}
