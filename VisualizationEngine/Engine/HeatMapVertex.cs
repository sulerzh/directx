// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.HeatMapVertex
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  internal struct HeatMapVertex : IVertex
  {
    public Vector3F Position;
    public float Value;

    public VertexFormat Format
    {
      get
      {
        return VertexFormat.Create(HeatMapVertexFormat.VertexComponents[0], HeatMapVertexFormat.VertexComponents[1]);
      }
    }

    public HeatMapVertex(Vector3F position, float value)
    {
      this.Position = position;
      this.Value = value;
    }
  }
}
