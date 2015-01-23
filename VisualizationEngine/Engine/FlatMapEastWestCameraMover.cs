// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.FlatMapEastWestCameraMover
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Utilities;
using System;
using System.Diagnostics;

namespace Microsoft.Data.Visualization.Engine
{
  internal class FlatMapEastWestCameraMover : EastWestCameraMover
  {
    private double maxMapRadians = Math.PI;
    private const double maxSpeed = 0.314159265358979;

    internal FlatMapEastWestCameraMover(CameraSnapshot start, double speedParameter, CameraSnapshot nextScene, TimeSpan moveDuration, double maxRadians = 3.14159265358979)
      : base(start, speedParameter, moveDuration, Math.PI / 10.0)
    {
      this.maxMapRadians = maxRadians;
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "FlatMapEastwardCameraMover constructor.");
      if (nextScene == null || Math.Abs(nextScene.Longitude - this.GetLongitude(-1.0)) >= Math.Abs(nextScene.Longitude - this.GetLongitude(1.0)))
        return;
      this.sweep = -this.sweep;
    }

    private bool GetAdjustedLongitude(double fraction, out double longitude)
    {
      longitude = this.startSnapshot.Longitude + fraction * this.sweep;
      double num1 = this.maxMapRadians;
      double num2 = this.maxMapRadians * 2.0;
      double num3 = longitude >= 0.0 ? 1.0 : -1.0;
      longitude *= num3;
      int num4 = (int) ((longitude + num1) / num2);
      bool flag = num4 % 2 != 0;
      if (flag)
        longitude = (double) num4 * num2 - longitude;
      else
        longitude -= (double) num4 * num2;
      longitude *= num3;
      return flag;
    }

    protected override double GetLongitude(double fraction)
    {
      double longitude;
      this.GetAdjustedLongitude(fraction, out longitude);
      return longitude;
    }

    protected override DifferentiableCameraSnapshot GetDifferentiableSnapshot(double time)
    {
      double longitude;
      return new DifferentiableCameraSnapshot(this.startSnapshot)
      {
        Longitude = {
          Derivative = !this.GetAdjustedLongitude(time / this.duration, out longitude) ? this.sweep / this.duration : -this.sweep / this.duration,
          Value = longitude
        }
      };
    }
  }
}
