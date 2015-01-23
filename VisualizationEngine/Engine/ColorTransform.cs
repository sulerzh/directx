// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.ColorTransform
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  public abstract class ColorTransform : ColorOperation
  {
    private readonly Vector3D luminanceVector = new Vector3D(0.222, 0.707, 0.071);
    private readonly Matrix4x4D toYIQ = Matrix4x4D.TransposeMatrix(new Matrix4x4D()
    {
      M11 = 0.299,
      M12 = 0.587,
      M13 = 0.114,
      M21 = 0.595716,
      M22 = -0.274453,
      M23 = -0.321263,
      M31 = 0.211456,
      M32 = -0.522591,
      M33 = 0.311135,
      M44 = 1.0
    });

    protected Vector3D LuminanceVector
    {
      get
      {
        return this.luminanceVector;
      }
    }

    protected Matrix4x4D ToYIQMatrix
    {
      get
      {
        return this.toYIQ;
      }
    }

    public abstract Matrix4x4D GetMatrix();
  }
}
