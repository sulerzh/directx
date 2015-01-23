// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.CrossColumnOutline
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal class CrossColumnOutline : PolygonalOutlineColumnVisual
  {
    internal CrossColumnOutline()
      : base(8)
    {
      this.StartAngle = Math.Atan2(-3.0, -1.0) - Math.PI / 2.0;
      this.AngleIncrement = 0.0;
      this.CreateMesh();
    }

    protected override Vector2D[] Create2DPositions()
    {
      Vector2D vector2D1 = new Vector2D(CrossColumnVisual.WingLength, 0.0);
      Vector2D vector2D2 = new Vector2D(0.0, CrossColumnVisual.WingLength);
      Vector2D vector2D3 = new Vector2D(CrossColumnVisual.WingWidth, 0.0);
      Vector2D vector2D4 = new Vector2D(0.0, CrossColumnVisual.WingWidth);
      double num = Constants.SqrtOf2 / 2.0;
      Vector2D vector2D5 = new Vector2D(num, -num);
      Vector2D vector2D6 = new Vector2D(num, num);
      return new Vector2D[16]
      {
        -vector2D2 - vector2D3,
        -Vector2D.YVector,
        -vector2D2 + vector2D3,
        vector2D5,
        vector2D1 - vector2D4,
        Vector2D.XVector,
        vector2D1 + vector2D4,
        vector2D6,
        vector2D2 + vector2D3,
        Vector2D.YVector,
        vector2D2 - vector2D3,
        -vector2D5,
        -vector2D1 + vector2D4,
        -Vector2D.XVector,
        -vector2D1 - vector2D4,
        -vector2D6
      };
    }
  }
}
