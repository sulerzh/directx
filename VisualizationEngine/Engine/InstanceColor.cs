using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
    internal struct InstanceColor : IVertex
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

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
