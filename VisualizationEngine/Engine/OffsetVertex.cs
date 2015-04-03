using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
    internal struct OffsetVertex : IVertex
    {
        public short X;
        public short Y;
        public short Z;
        public short R;
        public short NX;
        public short NY;
        public short NZ;
        public short Unused;

        public VertexFormat Format
        {
            get
            {
                return VertexFormat.Create(InstanceVertexFormat.Components[0], InstanceVertexFormat.Components[1]);
            }
        }
    }
}
