// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.InvertTransform
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  public class InvertTransform : ColorTransform
  {
    private Matrix4x4D matrix;

    public InvertTransform()
    {
      this.matrix = Matrix4x4D.Scaling(-1.0, -1.0, -1.0) * Matrix4x4D.Translation(1.0, 1.0, 1.0);
    }

    public override Matrix4x4D GetMatrix()
    {
      return this.matrix;
    }
  }
}
