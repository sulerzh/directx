// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.Figure8CameraMover
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
  internal class Figure8CameraMover : LineCameraMover
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

    internal Figure8CameraMover(CameraSnapshot midSnapshot, double factor, TimeSpan moveDuration, Vector3D repeller)
      : base(midSnapshot, moveDuration, Math.PI / 2.0)
    {
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Figure8CameraMover constructor.");
      this.altitude = this.currentSnapshot.Distance * Math.Cos(this.currentSnapshot.PivotAngle);
      this.altitudeAsDifferentiable = new DifferentiableScalar(this.altitude, 0.0);
      this.horizontalDistance = this.currentSnapshot.Distance * Math.Abs(Math.Sin(this.currentSnapshot.PivotAngle));
      this.horizontalDistanceAsDifferentiable = new DifferentiableScalar(this.horizontalDistance, 0.0);
      this.timeFactor = 2.0 * Math.PI * Figure8CameraMover.maxFrequency;
      double arcAngle = this.GetArcAngle(0.2 * factor);
      this.deviation = 0.15 * arcAngle;
      this.ConstructPath(midSnapshot, arcAngle);
      if (Vector3D.DistanceSq(this.path.StartPoint, repeller) > Vector3D.DistanceSq(this.path.EndPoint, repeller))
        return;
      this.path.Reverse();
      this.relativeAngle = -this.relativeAngle;
    }

    protected override void UpdateCurrentSnapshot(double time)
    {
      double a = time * this.timeFactor;
      Coordinates coordinates;
      double azimuth;
      this.path.GetGeoCoordinates(Math.Sin(a), out coordinates, out azimuth);
      double y = Math.Max(this.altitude + this.deviation * Math.Sin(2.0 * a), 7.83927971443699E-05);
      this.currentSnapshot.Latitude = coordinates.Latitude;
      this.currentSnapshot.Longitude = coordinates.Longitude;
      this.currentSnapshot.Rotation = azimuth + this.relativeAngle;
      this.currentSnapshot.PivotAngle = -Math.Atan(this.horizontalDistance / y);
      this.currentSnapshot.Distance = MathEx.Hypot(this.horizontalDistance, y);
    }

    protected override DifferentiableCameraSnapshot GetDifferentiableSnapshot(double time)
    {
      DifferentiableScalar differentiableScalar = new DifferentiableScalar(time * this.timeFactor, this.timeFactor);
      DifferentiableScalar latitude;
      DifferentiableScalar longitude;
      DifferentiableScalar azimuth;
      this.path.GetGeoCoordinates(differentiableScalar.Sin, out latitude, out longitude, out azimuth);
      DifferentiableScalar y = DifferentiableScalar.Max(this.altitudeAsDifferentiable + this.deviation * (2.0 * differentiableScalar).Sin, 7.83927971443699E-05);
      DifferentiableScalar hypot;
      DifferentiableScalar.Hypot(this.horizontalDistanceAsDifferentiable, y, out hypot);
      return new DifferentiableCameraSnapshot(latitude, longitude, azimuth + new DifferentiableScalar(this.relativeAngle, 0.0), -(this.horizontalDistanceAsDifferentiable / y).Atan, hypot);
    }
  }
}
