using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
    internal struct HitTestVertex : IVertex
    {
        public uint HitTestId;

        public VertexFormat Format
        {
            get
            {
                return VertexFormat.Create(
                    new VertexComponent(VertexSemantic.Color, VertexComponentDataType.UnsignedByte4AsFloats));
            }
        }
    }
}
