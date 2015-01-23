﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.StreamOutHitVertex
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
  internal struct StreamOutHitVertex : IVertex
  {
    public float R;
    public float G;
    public float B;
    public float A;

    public VertexFormat Format
    {
      get
      {
        return VertexFormat.Create(new VertexComponent(VertexSemantic.Color, VertexComponentDataType.Float4));
      }
    }
  }
}