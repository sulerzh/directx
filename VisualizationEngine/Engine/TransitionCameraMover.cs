// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.TransitionCameraMover
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.MathExtensions;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal class TransitionCameraMover : CameraMover
  {
    private double startAngle;
    private double endAngle;
    private CameraSnapshot startSnapshot;
    private CameraSnapshot endSnapshot;
    private GreatCircleArc path;
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

    internal TransitionCameraMover(CameraSnapshot start, CameraSnapshot end, TimeSpan moveDuration)
      : base(moveDuration.TotalSeconds)
    {
      this.startSnapshot = start.Clone();
      this.endSnapshot = end.Clone();
      this.currentSnapshot = start.Clone();
      this.path = new GreatCircleArc(Coordinates.GeoTo3D(this.startSnapshot.Longitude, this.startSnapshot.Latitude), Coordinates.GeoTo3D(this.endSnapshot.Longitude, this.endSnapshot.Latitude));
      this.startAngle = TransitionCameraMover.GetAzimuth(this.path.StartPoint, this.path.StartTangent, this.startSnapshot.Rotation);
      this.endAngle = TransitionCameraMover.GetAzimuth(this.path.EndPoint, this.path.EndTangent, this.endSnapshot.Rotation);
      if (this.endAngle < this.startAngle - Math.PI)
        this.endAngle += 2.0 * Math.PI;
      else if (this.endAngle > this.startAngle + Math.PI)
        this.endAngle -= 2.0 * Math.PI;
      this.trajectory = new DistanceAnimator(this.path.ArcAngle / this.duration, start.Distance, end.Distance);
    }

    private static double GetAzimuth(Vector3D position, Vector3D direction, double nearTo)
    {
      double azimuth = Coordinates.GetAzimuth(position, direction);
      return MathEx.GetClosestRepresentation(nearTo - azimuth, 0.0, Math.PI);
    }

    protected override void UpdateCurrentSnapshot(double time)
    {
      double num = time / this.duration;
      double s = 1.0 - num;
      Coordinates coordinates;
      double azimuth;
      this.path.GetGeoCoordinates(num, out coordinates, out azimuth);
      this.currentSnapshot.Latitude = coordinates.Latitude;
      this.currentSnapshot.Longitude = coordinates.Longitude;
      this.currentSnapshot.Rotation = azimuth + s * this.startAngle + num * this.endAngle;
      this.currentSnapshot.PivotAngle = s * this.startSnapshot.PivotAngle + num * this.endSnapshot.PivotAngle;
      this.currentSnapshot.Distance = this.trajectory.GetValue(s, num);
    }

    protected override DifferentiableCameraSnapshot GetDifferentiableSnapshot(double time)
    {
      DifferentiableScalar differentiableScalar = new DifferentiableScalar(time / this.duration, 1.0 / this.duration);
      DifferentiableScalar s = new DifferentiableScalar(1.0 - differentiableScalar.Value, -differentiableScalar.Derivative);
      DifferentiableScalar latitude;
      DifferentiableScalar longitude;
      DifferentiableScalar azimuth;
      this.path.GetGeoCoordinates(differentiableScalar, out latitude, out longitude, out azimuth);
      return new DifferentiableCameraSnapshot(latitude, longitude, azimuth + s * this.startAngle + differentiableScalar * this.endAngle, s * this.startSnapshot.PivotAngle + differentiableScalar * this.endSnapshot.PivotAngle, this.trajectory.GetValue(s, differentiableScalar));
    }
  }
}
