﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.InstanceTime
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
  internal struct InstanceTime : IVertex
  {
    public float StartTime;
    public float EndTime;

    public VertexFormat Format
    {
      get
      {
        return VertexFormat.Create(new VertexComponent(VertexSemantic.TexCoord, VertexComponentDataType.Float2));
      }
    }
  }
}
