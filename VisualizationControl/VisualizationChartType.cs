using Microsoft.Data.Visualization.WpfExtensions;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public enum VisualizationChartType
  {
    [DisplayString(typeof (Resources), "FieldWellVisualization_ChartType_StackedColumn")] StackedColumn,
    [DisplayString(typeof (Resources), "FieldWellVisualization_ChartType_ClusteredColumn")] ClusteredColumn,
    [DisplayString(typeof (Resources), "FieldWellVisualization_ChartType_Bubble")] Bubble,
    [DisplayString(typeof (Resources), "FieldWellVisualization_ChartType_HeatMap")] HeatMap,
    [DisplayString(typeof (Resources), "FieldWellVisualization_ChartType_Region")] Region,
  }
}
