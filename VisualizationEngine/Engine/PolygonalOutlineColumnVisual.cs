namespace Microsoft.Data.Visualization.Engine
{
  internal class PolygonalOutlineColumnVisual : PolygonalColumnVisual
  {
    internal PolygonalOutlineColumnVisual(int walls)
      : base(walls, true)
    {
      this.CreateMesh();
    }

    protected override void SetTriangleIndexes(short vertex1, short vertex2, short vertex3, short adjacent1, short adjacent2, short adjacent3, ref int raw, ref int withAdjacency)
    {
      this.MeshIndicesWithAdjacency[withAdjacency++] = vertex1;
      this.MeshIndicesWithAdjacency[withAdjacency++] = adjacent1;
      this.MeshIndicesWithAdjacency[withAdjacency++] = vertex2;
      this.MeshIndicesWithAdjacency[withAdjacency++] = adjacent2;
      this.MeshIndicesWithAdjacency[withAdjacency++] = vertex3;
      this.MeshIndicesWithAdjacency[withAdjacency++] = adjacent3;
    }
  }
}
