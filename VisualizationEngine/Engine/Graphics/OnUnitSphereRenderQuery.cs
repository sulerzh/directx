// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.Graphics.OnUnitSphereRenderQuery
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
  internal class OnUnitSphereRenderQuery : RenderQuery
  {
    internal OnUnitSphereRenderQuery(Matrix4x4D projection, float scale, InstanceLayer layer, List<int> queryResult)
      : base(projection, scale, layer, queryResult)
    {
    }

    protected override Box3D Get3DBounds(SpatialIndex.Node node)
    {
      return node.Bounds3D;
    }
  }
}
