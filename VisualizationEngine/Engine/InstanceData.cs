using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
    internal class InstanceData
    {
        public Coordinates Location;
        public float Value;
        public short Color;
        public short Shift;
        public short SourceShift;
        public DateTime? StartTime;
        public DateTime? EndTime;
        public InstanceId Id;
        public bool FirstInstance;
    }
}
