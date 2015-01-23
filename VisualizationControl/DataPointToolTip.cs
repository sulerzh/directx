using Microsoft.Data.Visualization.Engine;
using Microsoft.Reporting.Windows.Common.Internal;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class DataPointToolTip : TableFieldToolTipViewModel, IAutomationNameProvider
  {
    public string AutomationName
    {
      get
      {
        return string.Empty;
      }
    }

    public DataPointToolTip(GeoVisualization visualization, InstanceId id)
      : base(visualization, id, false)
    {
    }
  }
}
