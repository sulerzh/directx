// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.FlatLineCameraMover
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.MathExtensions;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal class FlatLineCameraMover : CameraMover
  {
    protected MercatorLine path;
    protected DifferentiableScalar rotationAngle;
    private DifferentiableScalar pivotAngle;
    private DifferentiableScalar distance;

    internal override CameraSnapshot Start
    {
      get
      {
        return this.GetSnapshot(0.0);
      }
    }

    internal override CameraSnapshot End
    {
      get
      {
        return this.GetSnapshot(this.duration);
      }
    }

    internal FlatLineCameraMover(CameraSnapshot midSnapshot, TimeSpan moveDuration)
      : base(moveDuration.TotalSeconds)
    {
      this.currentSnapshot = midSnapshot.Clone();
      this.pivotAngle = new DifferentiableScalar(midSnapshot.PivotAngle, 0.0);
      this.distance = new DifferentiableScalar(midSnapshot.Distance, 0.0);
      this.rotationAngle = new DifferentiableScalar(midSnapshot.Rotation, 0.0);
    }

    protected double GetLength(double factor)
    {
      return factor * 2.0 * Math.Sin(this.currentSnapshot.Distance * Math.PI / 32.0);
    }

    protected override void UpdateCurrentSnapshot(double time)
    {
      Vector2D point = this.path.GetPoint(time / this.duration);
      this.currentSnapshot.Longitude = point.X;
      this.currentSnapshot.Latitude = point.Y;
    }

    protected override DifferentiableCameraSnapshot GetDifferentiableSnapshot(double time)
    {
      this.UpdateCurrentSnapshot(time);
      DifferentiableScalar t = new DifferentiableScalar(time / this.duration, 1.0 / this.duration);
      DifferentiableScalar differentiableScalar = new DifferentiableScalar(1.0, 0.0) - t;
      DifferentiableScalar longitude;
      DifferentiableScalar y;
      this.path.GetPoint(t, out longitude, out y);
      return new DifferentiableCameraSnapshot(y, longitude, this.rotationAngle, this.pivotAngle, this.distance);
    }
  }
}
