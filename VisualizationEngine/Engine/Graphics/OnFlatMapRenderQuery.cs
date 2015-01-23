using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
  internal class OnFlatMapRenderQuery : RenderQuery
  {
    internal OnFlatMapRenderQuery(Matrix4x4D projection, float scale, InstanceLayer layer, List<int> queryResult)
      : base(projection, scale, layer, queryResult)
    {
    }

    protected override Box3D Get3DBounds(SpatialIndex.Node node)
    {
      return new Box3D(1.0, 1.0, node.LongLatBounds.minY, node.LongLatBounds.maxY, node.LongLatBounds.minX, node.LongLatBounds.maxX);
    }
  }
}
