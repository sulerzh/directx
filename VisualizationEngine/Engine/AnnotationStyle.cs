﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.AnnotationStyle
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  public class AnnotationStyle
  {
    public Vector3F TailDimensions { get; set; }

    public float OutlineWidth { get; set; }

    public float AnnotationScale { get; set; }

    public Color4F OutlineColor { get; set; }

    public Color4F FillColor { get; set; }

    public AnnotationStyle()
    {
      this.TailDimensions = new Vector3F(38f, 28f, 17f);
      this.AnnotationScale = 1f;
      this.OutlineWidth = 1f;
      this.OutlineColor = new Color4F(1f, 0.0f, 0.0f, 0.0f);
      this.FillColor = new Color4F(1f, 1f, 1f, 1f);
    }

    public void CopyFrom(AnnotationStyle style)
    {
      this.OutlineColor = style.OutlineColor;
      this.FillColor = style.FillColor;
      this.AnnotationScale = style.AnnotationScale;
      this.TailDimensions = style.TailDimensions;
      this.OutlineWidth = style.OutlineWidth;
    }

    public static AnnotationStyle MakeDefault()
    {
      return new AnnotationStyle()
      {
        OutlineColor = new Color4F(0.8f, 0.527f, 0.527f, 0.527f),
        FillColor = new Color4F(1f, 1f, 1f, 1f),
        AnnotationScale = 1f,
        OutlineWidth = 1.414f,
        TailDimensions = new Vector3F(38f, 28f, 17f)
      };
    }
  }
}
