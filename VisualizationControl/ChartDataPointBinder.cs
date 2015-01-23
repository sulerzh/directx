using Microsoft.Reporting.Windows.Chart.Internal;
using Microsoft.Reporting.Windows.Common.Internal;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class ChartDataPointBinder : ItemsBinder<Microsoft.Reporting.Windows.Chart.Internal.DataPoint>
  {
    public override void UpdateTarget(Microsoft.Reporting.Windows.Chart.Internal.DataPoint target, object source, string propertyName)
    {
      ChartDataPoint chartDataPoint = source as ChartDataPoint;
      StackedColumnDataPoint stackedColumnDataPoint = target as StackedColumnDataPoint;
      if (stackedColumnDataPoint == null)
        return;
      stackedColumnDataPoint.XValue = (object) chartDataPoint.XValue;
      stackedColumnDataPoint.YValue = (object) chartDataPoint.YValue;
      stackedColumnDataPoint.Tag = (object) chartDataPoint.InstanceId;
      stackedColumnDataPoint.LabelContent = (object) chartDataPoint.XValue;
      stackedColumnDataPoint.ToolTipContent = (IAutomationNameProvider) chartDataPoint.ToolTip;
      stackedColumnDataPoint.MaxWidth = 25.0;
    }
  }
}
