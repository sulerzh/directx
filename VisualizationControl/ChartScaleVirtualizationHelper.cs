using Microsoft.Reporting.Windows.Common.Internal;
using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  internal class ChartScaleVirtualizationHelper : IHierarchyVirtualizationHelper
  {
    private Func<int, int, ChartDataPoint> pointGenerator;
    private Func<ChartDataPoint, Tuple<int, int>> pointIndexRetriever;
    private int size;

    public event EventHandler LeavesChanged;

    public ChartScaleVirtualizationHelper(Func<int, int, ChartDataPoint> pointGenerator, Func<ChartDataPoint, Tuple<int, int>> pointIndexRetriever, int size)
    {
      this.pointGenerator = pointGenerator;
      this.pointIndexRetriever = pointIndexRetriever;
      this.size = size;
    }

    public int GetLeafCount()
    {
      return this.size;
    }

    public object GetLeaf(int leafIndex)
    {
      return (object) this.pointGenerator(1, leafIndex);
    }

    public int GetLeafIndex(object item)
    {
      return this.pointIndexRetriever((ChartDataPoint) item).Item2;
    }

    public object GetParent(object item)
    {
      return (object) null;
    }

    public int GetChildCount(object item)
    {
      return 0;
    }

    public object GetChildAt(object item, int index)
    {
      return (object) null;
    }

    public int GetIndex(object item)
    {
      return this.pointIndexRetriever((ChartDataPoint) item).Item2;
    }

    internal void RaiseLeavesChanged()
    {
      this.LeavesChanged((object) this, new EventArgs());
    }
  }
}
