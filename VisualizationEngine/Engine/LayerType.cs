using System;

namespace Microsoft.Data.Visualization.Engine
{
    [Serializable]
    public enum LayerType
    {
        None,
        PointMarkerChart,
        BubbleChart,
        ColumnChart,
        ClusteredColumnChart,
        StackedColumnChart,
        PieChart,
        HeatMapChart,
        RegionChart,
    }
}
