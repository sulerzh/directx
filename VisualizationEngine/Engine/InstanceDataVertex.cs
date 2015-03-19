using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
    internal struct InstanceDataVertex : IVertex
    {
        public float X;
        public float Y;
        public float Z;
        public float WidthOrHeight;
        public float Angle;
        public float Shift;

        public VertexFormat Format
        {
            get
            {
                return null;
            }
        }
    }
}
