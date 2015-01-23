using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class DateTimeFilterPredicateProperties : FilterPredicateProperties
  {
    public DateTime? AllowedMin { get; set; }

    public DateTime? AllowedMax { get; set; }
  }
}
