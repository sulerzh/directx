using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
    /// <summary>
    /// 热力图定点格式，包含位置和值，以及起始/结束时间
    /// </summary>
    internal class HeatMapVertexFormat
    {
        public static VertexComponent[] VertexComponents = new VertexComponent[3]
        {
            new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3,
                VertexComponentClassification.PerVertexData, 0),
            new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float,
                VertexComponentClassification.PerVertexData, 0),
            new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2,
                VertexComponentClassification.PerVertexData, 1)
        };

        public static VertexComponent[] Components
        {
            get
            {
                return HeatMapVertexFormat.VertexComponents;
            }
        }
    }
}
