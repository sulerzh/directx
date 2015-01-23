// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.SaturationTransform
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  public class SaturationTransform : ColorTransform
  {
    private double saturation = 1.0;
    private Matrix4x4D transform;
    private bool updated;

    public double Saturation
    {
      get
      {
        return this.saturation;
      }
      set
      {
        this.saturation = value;
        this.updated = true;
      }
    }

    public SaturationTransform()
    {
      this.transform = Matrix4x4D.Identity;
    }

    public override Matrix4x4D GetMatrix()
    {
      if (this.updated)
      {
        this.updated = false;
        double num = 1.0 - this.saturation;
        this.transform.M11 = num * this.LuminanceVector.X + this.saturation;
        this.transform.M12 = num * this.LuminanceVector.X;
        this.transform.M13 = num * this.LuminanceVector.X;
        this.transform.M21 = num * this.LuminanceVector.Y;
        this.transform.M22 = num * this.LuminanceVector.Y + this.saturation;
        this.transform.M23 = num * this.LuminanceVector.Y;
        this.transform.M31 = num * this.LuminanceVector.Z;
        this.transform.M32 = num * this.LuminanceVector.Z;
        this.transform.M33 = num * this.LuminanceVector.Z + this.saturation;
      }
      return this.transform;
    }
  }
}
