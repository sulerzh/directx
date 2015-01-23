// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.RegionBufferToken
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
  internal class RegionBufferToken
  {
    public readonly IndexBuffer Indices;
    public readonly VertexBuffer Vertices;
    public readonly int StartVertex;
    public readonly int VertexCount;

    public RegionBufferToken(VertexBuffer vb, IndexBuffer ib, int startVertex, int vertexCount)
    {
      this.Vertices = vb;
      this.Indices = ib;
      this.StartVertex = startVertex;
      this.VertexCount = vertexCount;
    }
  }
}
