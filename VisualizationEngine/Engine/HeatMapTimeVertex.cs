using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
    internal struct HeatMapTimeVertex : IVertex
    {
        public float StartTime;
        public float EndTime;

        public VertexFormat Format
        {
            get
            {
                return VertexFormat.Create(HeatMapVertexFormat.VertexComponents[2]);
            }
        }

        public HeatMapTimeVertex(float start, float end)
        {
            this.StartTime = start;
            this.EndTime = end;
        }
    }
}
