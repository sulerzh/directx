// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.HueTransform
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
  public class HueTransform : ColorTransform
  {
    private Matrix4x4D matrix;
    private float hue;
    private bool updated;

    public float HueRotation
    {
      get
      {
        return this.hue;
      }
      set
      {
        this.hue = value;
        this.updated = true;
      }
    }

    public HueTransform()
    {
      this.matrix = Matrix4x4D.Identity;
    }

    public override Matrix4x4D GetMatrix()
    {
      if (this.updated)
      {
        this.updated = false;
        this.matrix = this.ToYIQMatrix * Matrix4x4D.RotationX(-(double) this.hue * (Math.PI / 180.0)) * Matrix4x4D.Invert(this.ToYIQMatrix);
      }
      return this.matrix;
    }
  }
}
