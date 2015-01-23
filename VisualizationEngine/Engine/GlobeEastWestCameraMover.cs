// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.GlobeEastWestCameraMover
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.MathExtensions;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Diagnostics;

namespace Microsoft.Data.Visualization.Engine
{
  internal class GlobeEastWestCameraMover : EastWestCameraMover
  {
    private const double maxSpeed = 0.20943951023932;

    internal GlobeEastWestCameraMover(CameraSnapshot start, double speedParameter, CameraSnapshot nextScene, TimeSpan moveDuration)
      : base(start, speedParameter, moveDuration, Math.PI / 15.0)
    {
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "GlobeEastwardCameraMover constructor.");
      if (nextScene == null)
        return;
      double longitude = nextScene.Longitude;
      double closestRepresentation = MathEx.GetClosestRepresentation(this.GetLongitude(1.0), longitude, Math.PI);
      if (Math.Abs(MathEx.GetClosestRepresentation(this.GetLongitude(-1.0), longitude, Math.PI) - longitude) >= Math.Abs(closestRepresentation - longitude))
        return;
      this.sweep = -this.sweep;
    }

    protected override double GetLongitude(double fraction)
    {
      return MathEx.GetNormalized(this.startSnapshot.Longitude + fraction * this.sweep, Math.PI);
    }

    protected override DifferentiableCameraSnapshot GetDifferentiableSnapshot(double time)
    {
      this.UpdateCurrentSnapshot(time);
      return new DifferentiableCameraSnapshot(this.currentSnapshot)
      {
        Longitude = {
          Derivative = this.sweep / this.duration
        }
      };
    }
  }
}
