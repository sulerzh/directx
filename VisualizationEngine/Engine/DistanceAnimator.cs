// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.DistanceAnimator
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.MathExtensions;

namespace Microsoft.Data.Visualization.Engine
{
  internal class DistanceAnimator
  {
    private double peak;
    private double startDistance;
    private double endDistance;

    internal DistanceAnimator(double arcAngle, double start, double end)
    {
      this.startDistance = start;
      this.endDistance = end;
      this.peak = 8.0 * arcAngle;
    }

    internal double GetValue(double s, double t)
    {
      return s * this.startDistance + t * this.endDistance + t * s * this.peak;
    }

    internal DifferentiableScalar GetValue(DifferentiableScalar s, DifferentiableScalar t)
    {
      return s * this.startDistance + t * this.endDistance + t * s * this.peak;
    }
  }
}
