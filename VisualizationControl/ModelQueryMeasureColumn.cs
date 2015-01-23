namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class ModelQueryMeasureColumn : ModelQueryColumn
  {
    public AggregationFunction AggregationFunction { get; set; }

    public ModelQueryMeasureColumn.AccumulationType Accumulate { get; set; }

    public ModelQueryIndexedKeyColumn ModelQueryIndexedKey { get; set; }

    public double Min { get; set; }

    public double Max { get; set; }

    public ModelQueryKeyColumn ModelQueryKeyAlias { get; set; }

    public override void Reset()
    {
      this.Min = this.Max = double.NaN;
      this.ModelQueryKeyAlias = (ModelQueryKeyColumn) null;
      base.Reset();
    }

    public enum AccumulationType
    {
      NoAccumulation,
      AccumulateByBuckets,
      AccumulateByBucketsAndIndex,
    }
  }
}
