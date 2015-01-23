using Microsoft.Data.Visualization.Engine;
using System;
using System.Windows.Media;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class ChartDataPoint : IEquatable<ChartDataPoint>
  {
    public string XValue { get; private set; }

    public double YValue { get; private set; }

    public InstanceId InstanceId { get; private set; }

    public Color Color { get; private set; }

    public int ShiftIndex { get; private set; }

    public DataPointToolTip ToolTip { get; private set; }

    public ChartDataPoint(string xName, double yValue, InstanceId instanceId, Color color, int index, DataPointToolTip tooltip)
    {
      this.XValue = xName;
      this.YValue = yValue;
      this.InstanceId = instanceId;
      this.Color = color;
      this.ShiftIndex = index;
      this.ToolTip = tooltip;
    }

    public bool Equals(ChartDataPoint other)
    {
      return other.InstanceId == this.InstanceId;
    }

    public override bool Equals(object obj)
    {
      ChartDataPoint chartDataPoint = obj as ChartDataPoint;
      if (chartDataPoint != null)
        return chartDataPoint.InstanceId == this.InstanceId;
      else
        return false;
    }
  }
}
