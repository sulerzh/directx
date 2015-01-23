// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.RegionVertex
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  internal struct RegionVertex : IVertex
  {
    public float X;
    public float Y;
    public float Z;

    public VertexFormat Format
    {
      get
      {
        return VertexFormat.Create(new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3, VertexComponentClassification.PerVertexData, 0));
      }
    }

    public RegionVertex(Vector3D position)
    {
      this.X = (float) position.X;
      this.Y = (float) position.Y;
      this.Z = (float) position.Z;
    }
  }
}
