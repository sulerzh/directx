using Microsoft.Data.Visualization.Engine.Graphics;
using System;

namespace Microsoft.Data.Visualization.Engine
{
    internal class GatherAccumulateProcessBlock
    {
        public IndexBuffer PositiveIndices { get; set; }

        public IndexBuffer NegativeIndices { get; set; }

        public VertexBuffer Instances { get; set; }

        public VertexBuffer InstancesTime { get; set; }

        public VertexBuffer InstancesHitId { get; set; }

        public int MaxShift { get; set; }

        public Tuple<uint, uint> PositiveSubset { get; set; }

        public Tuple<uint, uint> NegativeSubset { get; set; }

        public InstanceBlock Owner { get; set; }
    }
}
