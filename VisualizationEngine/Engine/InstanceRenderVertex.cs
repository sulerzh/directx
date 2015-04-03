using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
    internal struct InstanceRenderVertex : IVertex
    {
        public float X;
        public float Y;
        public float Z;
        public float Width;
        public float Height;
        public float ColorId;
        public float FrameId;
        public float RenderPriority;

        public VertexFormat Format
        {
            get
            {
                return VertexFormat.Create(
                    new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3), 
                    new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2), 
                    new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2), 
                    new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float));
            }
        }
    }
}
