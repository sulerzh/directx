// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.EastWestCameraMover
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Utilities;
using System;
using System.Diagnostics;

namespace Microsoft.Data.Visualization.Engine
{
  internal abstract class EastWestCameraMover : CameraMover
  {
    protected double sweep;
    protected CameraSnapshot startSnapshot;

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
        return this.GetSnapshot(this.duration);
      }
    }

    internal EastWestCameraMover(CameraSnapshot start, double speedParameter, TimeSpan moveDuration, double maxSpeed)
      : base(moveDuration.TotalSeconds)
    {
      if (start == null)
        throw new ArgumentNullException("start");
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "EastwardCameraMover constructor.");
      this.startSnapshot = start.Clone();
      this.currentSnapshot = start.Clone();
      this.sweep = this.duration * (speedParameter * maxSpeed * Math.Max(Math.Min(start.Distance, 1.0), 1E-06));
    }

    protected abstract double GetLongitude(double fraction);

    protected override void UpdateCurrentSnapshot(double time)
    {
      this.currentSnapshot.Longitude = this.GetLongitude(time / this.duration);
    }
  }
}
