using Microsoft.Data.Visualization.Engine.VectorMath;
using System.Windows;

namespace Microsoft.Data.Visualization.Engine
{
    public class SingleTouchPoint
    {
        public int Id;
        public Point PosCurrent;
        public Point PosStart;
        public Vector3D WorldPosStart;
        public Vector3D WorldPosCur;
        public bool HasFirstWorldSet;
        public bool UsedForGesture;
    }
}
