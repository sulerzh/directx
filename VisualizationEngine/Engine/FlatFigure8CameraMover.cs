// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.FlatFigure8CameraMover
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.MathExtensions;
using Microsoft.Data.Visualization.Engine.VectorMath;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Diagnostics;

namespace Microsoft.Data.Visualization.Engine
{
  internal class FlatFigure8CameraMover : FlatLineCameraMover
  {
    private static double maxFrequency = 0.08;
    private double altitude;
    private DifferentiableScalar altitudeAsDifferentiable;
    private double horizontalDistance;
    private DifferentiableScalar horizontalDistanceAsDifferentiable;
    private double timeFactor;
    private double deviation;

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

    internal FlatFigure8CameraMover(CameraSnapshot midSnapshot, double factor, TimeSpan moveDuration, Vector2D repeller)
      : base(midSnapshot, moveDuration)
    {
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Figure8CameraMover constructor.");
      this.altitude = this.currentSnapshot.Distance * Math.Cos(this.currentSnapshot.PivotAngle);
      this.altitudeAsDifferentiable = new DifferentiableScalar(this.altitude, 0.0);
      this.horizontalDistance = this.currentSnapshot.Distance * Math.Abs(Math.Sin(this.currentSnapshot.PivotAngle));
      this.horizontalDistanceAsDifferentiable = new DifferentiableScalar(this.horizontalDistance, 0.0);
      this.timeFactor = 2.0 * Math.PI * FlatFigure8CameraMover.maxFrequency;
      double length = this.GetLength(0.2 * factor);
      this.deviation = 0.15 * length;
      this.path = new MercatorLine(midSnapshot.Longitude, midSnapshot.Latitude, midSnapshot.Rotation + Math.PI / 2.0, length);
      if (Vector2D.DistanceSq(this.path.Start, repeller) > Vector2D.DistanceSq(this.path.End, repeller))
        return;
      this.path.Reverse();
    }

    protected override void UpdateCurrentSnapshot(double time)
    {
      double a = time * this.timeFactor;
      Vector2D point = this.path.GetPoint(Math.Sin(a));
      double y = Math.Max(this.altitude + this.deviation * Math.Sin(2.0 * a), 7.83927971443699E-05);
      this.currentSnapshot.Latitude = point.Y;
      this.currentSnapshot.Longitude = point.X;
      this.currentSnapshot.PivotAngle = -Math.Atan(this.horizontalDistance / y);
      this.currentSnapshot.Distance = MathEx.Hypot(this.horizontalDistance, y);
    }

    protected override DifferentiableCameraSnapshot GetDifferentiableSnapshot(double time)
    {
      DifferentiableScalar differentiableScalar = new DifferentiableScalar(time * this.timeFactor, this.timeFactor);
      DifferentiableScalar longitude;
      DifferentiableScalar y1;
      this.path.GetPoint(differentiableScalar.Sin, out longitude, out y1);
      DifferentiableScalar y2 = DifferentiableScalar.Max(this.altitudeAsDifferentiable + this.deviation * (2.0 * differentiableScalar).Sin, 7.83927971443699E-05);
      DifferentiableScalar hypot;
      DifferentiableScalar.Hypot(this.horizontalDistanceAsDifferentiable, y2, out hypot);
      return new DifferentiableCameraSnapshot(y1, longitude, this.rotationAngle, -(this.horizontalDistanceAsDifferentiable / y2).Atan, hypot);
    }
  }
}
