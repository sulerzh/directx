// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.ScreenColorTransform
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  public class ScreenColorTransform : ColorTransform
  {
    private Matrix4x4D matrix = Matrix4x4D.Identity;
    private Color4F color;
    private bool updated;

    public Color4F Color
    {
      get
      {
        return this.color;
      }
      set
      {
        this.color = value;
        this.updated = true;
      }
    }

    public double Multiplier { get; set; }

    public ScreenColorTransform()
    {
      this.Multiplier = 1.0;
    }

    public override Matrix4x4D GetMatrix()
    {
      if (this.updated)
      {
        this.updated = false;
        Matrix4x4D matrix4x4D1 = Matrix4x4D.Scaling(1.0 - (double) this.color.R, 1.0 - (double) this.color.G, 1.0 - (double) this.color.B);
        Matrix4x4D matrix4x4D2 = Matrix4x4D.Scaling(-1.0, -1.0, -1.0);
        Matrix4x4D matrix4x4D3 = Matrix4x4D.Scaling(this.Multiplier, this.Multiplier, this.Multiplier);
        Matrix4x4D matrix4x4D4 = Matrix4x4D.Translation(1.0, 1.0, 1.0);
        this.matrix = matrix4x4D2 * matrix4x4D4 * matrix4x4D1 * matrix4x4D2 * matrix4x4D3 * matrix4x4D4;
      }
      return this.matrix;
    }
  }
}
