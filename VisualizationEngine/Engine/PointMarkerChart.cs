using Microsoft.Data.Visualization.Engine.VectorMath;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
    internal class PointMarkerChart : Chart
    {
        private static Vector2F fixedScale = new Vector2F(1f, 1f);

        internal override float FixedDimension
        {
            get
            {
                return (float)this.instanceWidth;
            }
        }

        internal override Vector2F FixedScale
        {
            get
            {
                return PointMarkerChart.fixedScale;
            }
        }

        internal override LayerType ChartType
        {
            get
            {
                return LayerType.PointMarkerChart;
            }
        }

        internal override int PrivateVisualsCount
        {
            get
            {
                return 0;
            }
        }

        internal PointMarkerChart()
        {
            this.instanceWidth = (double)Column.Width;
            this.instanceHeight = (double)Column.Height;
            this.Altitude = 0.0f;
            this.ShadowScale = Column.ShadowScale;
        }

        internal override Vector2F GetVariableScale(double scale)
        {
            return Vector2F.Empty;
        }

        protected override double ComputeMaxAbsValue(List<InstanceData> instanceList, bool timeBased, int first)
        {
            return 1.0;
        }
    }
}
