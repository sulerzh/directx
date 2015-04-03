using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
    /// <summary>
    /// instance顶点格式，包含定点和法线
    /// </summary>
    internal class InstanceVertexFormat
    {
        private static VertexComponent[] components;

        public static VertexComponent[] Components
        {
            get
            {
                if (components == null)
                    InitializeVertexFormat();
                return components;
            }
        }

        private InstanceVertexFormat()
        {
        }

        private static void InitializeVertexFormat()
        {
            components = new VertexComponent[2]
            {
                new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Short4AsFloats,
                    VertexComponentClassification.PerVertexData, 0),
                new VertexComponent(VertexSemantic.Normal, VertexComponentDataType.Short4AsFloats,
                    VertexComponentClassification.PerVertexData, 0)
            };
        }
    }
}
