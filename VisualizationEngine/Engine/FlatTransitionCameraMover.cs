// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.FlatTransitionCameraMover
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.MathExtensions;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal class FlatTransitionCameraMover : CameraMover
  {
    private double endRotation;
    private CameraSnapshot startSnapshot;
    private CameraSnapshot endSnapshot;
    private MercatorLine path;
    private DistanceAnimator trajectory;

    internal override CameraSnapshot Start
    {
      get
      {
        return this.startSnapshot;
      }
    }

    internal override CameraSnapshot End
    {
      get
      {
        return this.endSnapshot;
      }
    }

    internal FlatTransitionCameraMover(CameraSnapshot start, CameraSnapshot end, TimeSpan moveDuration)
      : base(moveDuration.TotalSeconds)
    {
      this.startSnapshot = start.Clone();
      this.endSnapshot = end.Clone();
      this.currentSnapshot = start.Clone();
      this.path = new MercatorLine(new Vector2D(this.startSnapshot.Longitude, this.startSnapshot.Latitude), new Vector2D(this.endSnapshot.Longitude, this.endSnapshot.Latitude));
      this.endRotation = this.endSnapshot.Rotation;
      if (this.endRotation < this.startSnapshot.Rotation - Math.PI)
        this.endRotation += 2.0 * Math.PI;
      else if (this.endRotation > this.startSnapshot.Rotation + Math.PI)
        this.endRotation -= 2.0 * Math.PI;
      this.trajectory = new DistanceAnimator(this.path.Length / this.duration, start.Distance, end.Distance);
    }

    protected override void UpdateCurrentSnapshot(double time)
    {
      double t = time / this.duration;
      double s = 1.0 - t;
      Vector2D point = this.path.GetPoint(t);
      this.currentSnapshot.Latitude = point.Y;
      this.currentSnapshot.Longitude = point.X;
      this.currentSnapshot.Rotation = s * this.startSnapshot.Rotation + t * this.endRotation;
      this.currentSnapshot.PivotAngle = s * this.startSnapshot.PivotAngle + t * this.endSnapshot.PivotAngle;
      this.currentSnapshot.Distance = this.trajectory.GetValue(s, t);
    }

    protected override DifferentiableCameraSnapshot GetDifferentiableSnapshot(double time)
    {
      DifferentiableScalar t = new DifferentiableScalar(time / this.duration, 1.0 / this.duration);
      DifferentiableScalar s = new DifferentiableScalar(1.0 - t.Value, -t.Derivative);
      DifferentiableScalar longitude;
      DifferentiableScalar y;
      this.path.GetPoint(t, out longitude, out y);
      return new DifferentiableCameraSnapshot(y, longitude, s * this.startSnapshot.Rotation + t * this.endRotation, s * this.startSnapshot.PivotAngle + t * this.endSnapshot.PivotAngle, this.trajectory.GetValue(s, t));
    }
  }
}
