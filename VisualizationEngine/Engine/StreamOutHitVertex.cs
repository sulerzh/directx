using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
    internal struct StreamOutHitVertex : IVertex
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public VertexFormat Format
        {
            get
            {
                return VertexFormat.Create(
                    new VertexComponent(VertexSemantic.Color, VertexComponentDataType.Float4));
            }
        }
    }
}
