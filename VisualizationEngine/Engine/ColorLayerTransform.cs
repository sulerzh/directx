// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.ColorLayerTransform
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  public class ColorLayerTransform : ColorTransform
  {
    private Matrix4x4D matrix;
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

    public override Matrix4x4D GetMatrix()
    {
      if (this.updated)
      {
        this.updated = false;
        this.matrix = this.ToYIQMatrix * Matrix4x4D.Scaling(1.0, 0.0, 0.0) * Matrix4x4D.Translation(this.ToYIQMatrix.Transform(new Vector3D((double) this.color.R, (double) this.color.G, (double) this.color.B)) * Matrix4x4D.Scaling(0.0, 1.0, 1.0)) * Matrix4x4D.Invert(this.ToYIQMatrix);
      }
      return this.matrix;
    }
  }
}
