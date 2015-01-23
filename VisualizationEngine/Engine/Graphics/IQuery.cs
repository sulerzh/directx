namespace Microsoft.Data.Visualization.Engine.Graphics
{
  internal interface IQuery
  {
    bool IsRelevant(SpatialIndex.Node node);

    void ProcessLeaf(SpatialIndex.Node node);
  }
}
