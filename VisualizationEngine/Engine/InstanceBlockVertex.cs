using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
    internal struct InstanceBlockVertex : IVertex
    {
        public float X;
        public float Y;
        public float Z;
        public float Value;
        public short GeoIndex;
        public short Shift;
        public short ColorIndex;
        public short RenderPriority;

        public VertexFormat Format
        {
            get
            {
                return VertexFormat.Create(
                    new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3), 
                    new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float), 
                    new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Short2),
                    new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Short2));
            }
        }
    }
}
