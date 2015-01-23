// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.CameraSnapshotBlender
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.MathExtensions;
using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal class CameraSnapshotBlender
  {
    public Hermite Latitude;
    public Hermite Longitude;
    public Hermite Rotation;
    public Hermite PivotAngle;
    public Hermite Distance;

    public double ElapsedTime { get; set; }

    public double StartTime { get; set; }

    public double EndTime { get; set; }

    public CameraSnapshotBlender(double startTime, double endTime, DifferentiableCameraSnapshot startSnapshot, DifferentiableCameraSnapshot endSnapshot)
    {
      this.ElapsedTime = startTime;
      this.StartTime = startTime;
      this.EndTime = endTime;
      double closestRepresentation1 = MathEx.GetClosestRepresentation(endSnapshot.Rotation.Value, startSnapshot.Rotation.Value, Math.PI);
      double closestRepresentation2 = MathEx.GetClosestRepresentation(endSnapshot.Longitude.Value, startSnapshot.Longitude.Value, Math.PI);
      this.Latitude = new Hermite(startTime, endTime, startSnapshot.Latitude, endSnapshot.Latitude);
      this.Longitude = new Hermite(startTime, endTime, startSnapshot.Longitude.Value, startSnapshot.Longitude.Derivative, closestRepresentation2, endSnapshot.Longitude.Derivative);
      this.Rotation = new Hermite(startTime, endTime, startSnapshot.Rotation.Value, startSnapshot.Rotation.Derivative, closestRepresentation1, endSnapshot.Rotation.Derivative);
      this.PivotAngle = new Hermite(startTime, endTime, startSnapshot.PivotAngle, endSnapshot.PivotAngle);
      this.Distance = new Hermite(startTime, endTime, startSnapshot.Distance, endSnapshot.Distance);
    }

    private static double PullIntoInterval(double t, double start, double end)
    {
      if (t <= start)
        return t + 2.0 * (end - start);
      if (t > end)
        return t - 2.0 * (end - start);
      else
        return t;
    }

    public CameraSnapshot GetValue(double time)
    {
      this.ElapsedTime = time;
      return new CameraSnapshot(CameraSnapshotBlender.PullIntoInterval(this.Latitude.Evaluate(time), -180.0, 180.0), this.Longitude.Evaluate(time), CameraSnapshotBlender.PullIntoInterval(this.Rotation.Evaluate(time), -1.0 * Math.PI, Math.PI), this.PivotAngle.Evaluate(time), this.Distance.Evaluate(time));
    }

    public void Reset()
    {
      this.ElapsedTime = this.StartTime;
    }
  }
}
