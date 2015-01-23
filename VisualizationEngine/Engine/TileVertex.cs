// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.TileVertex
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  internal struct TileVertex : IVertex
  {
    public float X;
    public float Y;
    public float Z;
    private short tu;
    private short tv;

    public Vector3F Position
    {
      set
      {
        this.X = value.X;
        this.Y = value.Y;
        this.Z = value.Z;
      }
    }

    public float Tu
    {
      set
      {
        this.tu = (short) ((double) value * (double) short.MaxValue);
      }
    }

    public float Tv
    {
      set
      {
        this.tv = (short) ((double) value * (double) short.MaxValue);
      }
    }

    public VertexFormat Format
    {
      get
      {
        return VertexFormat.Create(new VertexComponent(VertexSemantic.Position, VertexComponentDataType.Float3), new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Short2AsFloats));
      }
    }
  }
}
