namespace Microsoft.Data.Visualization.VisualizationControls
{
  public static class AggregationFunctionExtensions
  {
    public static string DisplayString(this AggregationFunction aggregationFunction)
    {
      switch (aggregationFunction)
      {
        case AggregationFunction.None:
          return Resources.AggregationFunction_None;
        case AggregationFunction.Sum:
          return Resources.AggregationFunction_Sum;
        case AggregationFunction.Average:
          return Resources.AggregationFunction_Average;
        case AggregationFunction.Count:
          return Resources.AggregationFunction_CountParenthesized;
        case AggregationFunction.Min:
          return Resources.AggregationFunction_Min;
        case AggregationFunction.Max:
          return Resources.AggregationFunction_Max;
        case AggregationFunction.DistinctCount:
          return Resources.AggregationFunction_DistinctCountParenthesized;
        case AggregationFunction.UserDefined:
          return string.Empty;
        default:
          return ((object) aggregationFunction).ToString();
      }
    }
  }
}
