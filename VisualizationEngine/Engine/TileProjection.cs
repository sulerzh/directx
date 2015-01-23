// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.TileProjection
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  internal abstract class TileProjection : DisposableResource
  {
    public abstract TileExtent GetExtent(Tile tile);

    public abstract SphereD ComputeBoundingSphere(Tile tile, out Vector3D[] corners);

    public abstract void InitializeVertexBuffer(Tile tile, VertexBuffer vertexBuffer, double flatteningFactor);

    public abstract IndexBuffer GetTileIndexBuffer();
  }
}
