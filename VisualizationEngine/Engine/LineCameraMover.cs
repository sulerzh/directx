// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.LineCameraMover
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.MathExtensions;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal class LineCameraMover : CameraMover
  {
    protected GreatCircleArc path;
    protected double relativeAngle;
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

    internal LineCameraMover(CameraSnapshot midSnapshot, TimeSpan moveDuration, double angle)
      : base(moveDuration.TotalSeconds)
    {
      this.relativeAngle = angle;
      this.currentSnapshot = midSnapshot.Clone();
      this.pivotAngle = new DifferentiableScalar(midSnapshot.PivotAngle, 0.0);
      this.distance = new DifferentiableScalar(midSnapshot.Distance, 0.0);
    }

    protected double GetArcAngle(double factor)
    {
      return factor * 2.0 * Math.Sin(this.currentSnapshot.Distance * Math.PI / 32.0);
    }

    protected void ConstructPath(CameraSnapshot midSnapshot, double arcAngle)
    {
      Vector3D vector3D = Coordinates.GeoTo3D(midSnapshot.Longitude, midSnapshot.Latitude);
      Vector3D directionVector = Coordinates.GetDirectionVector(vector3D, midSnapshot.Rotation - this.relativeAngle);
      this.path = new GreatCircleArc(vector3D, directionVector, arcAngle);
    }

    protected override void UpdateCurrentSnapshot(double time)
    {
      Coordinates coordinates;
      double azimuth;
      this.path.GetGeoCoordinates(time / this.duration, out coordinates, out azimuth);
      this.currentSnapshot.Latitude = coordinates.Latitude;
      this.currentSnapshot.Longitude = coordinates.Longitude;
      this.currentSnapshot.Rotation = azimuth + this.relativeAngle;
    }

    protected override DifferentiableCameraSnapshot GetDifferentiableSnapshot(double time)
    {
      DifferentiableScalar latitude;
      DifferentiableScalar longitude;
      DifferentiableScalar azimuth;
      this.path.GetGeoCoordinates(new DifferentiableScalar(time / this.duration, 1.0 / this.duration), out latitude, out longitude, out azimuth);
      return new DifferentiableCameraSnapshot(latitude, longitude, azimuth + new DifferentiableScalar(this.relativeAngle, 0.0), this.pivotAngle, this.distance);
    }
  }
}
