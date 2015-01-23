using Microsoft.Reporting.Windows.Chart.Internal;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  internal class ChartCategoryBinder : ICategoryBinder
  {
    public void Bind(Category category, object model)
    {
      ChartDataPoint chartDataPoint = model as ChartDataPoint;
      category.Key = (object) chartDataPoint.XValue;
      category.Content = (object) chartDataPoint.XValue;
    }
  }
}
