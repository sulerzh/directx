using Microsoft.Data.Visualization.WpfExtensions;
using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  [Serializable]
  public enum AggregationFunction
  {
    [DisplayString(typeof (Resources), "AggregationFunction_None")] None,
    [DisplayString(typeof (Resources), "AggregationFunction_Sum")] Sum,
    [DisplayString(typeof (Resources), "AggregationFunction_Average")] Average,
    [DisplayString(typeof (Resources), "AggregationFunction_Count")] Count,
    [DisplayString(typeof (Resources), "AggregationFunction_Min")] Min,
    [DisplayString(typeof (Resources), "AggregationFunction_Max")] Max,
    [DisplayString(typeof (Resources), "AggregationFunction_DistinctCount")] DistinctCount,
    [DisplayString(typeof (Resources), "AggregationFunction_UserDefined")] UserDefined,
  }
}
