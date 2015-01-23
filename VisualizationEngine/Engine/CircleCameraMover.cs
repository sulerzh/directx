// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.CircleCameraMover
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Utilities;
using System;
using System.Diagnostics;

namespace Microsoft.Data.Visualization.Engine
{
  internal class CircleCameraMover : CameraMover
  {
    private static double maxSpeed = Math.PI / 30.0;
    private double sweep;
    private CameraSnapshot startSnapshot;

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

    internal CircleCameraMover(CameraSnapshot start, double speedParameter, TimeSpan moveDuration)
      : base(moveDuration.TotalSeconds)
    {
      if (start == null)
        throw new ArgumentNullException("start");
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "CircleCameraMover constructor.");
      this.startSnapshot = start.Clone();
      this.currentSnapshot = start.Clone();
      this.sweep = this.duration * (speedParameter * CircleCameraMover.maxSpeed);
    }

    protected override void UpdateCurrentSnapshot(double time)
    {
      this.currentSnapshot.Rotation = this.startSnapshot.Rotation + time / this.duration * this.sweep;
    }

    protected override DifferentiableCameraSnapshot GetDifferentiableSnapshot(double time)
    {
      this.UpdateCurrentSnapshot(time);
      return new DifferentiableCameraSnapshot(this.currentSnapshot)
      {
        Rotation = {
          Derivative = this.sweep / this.duration
        }
      };
    }
  }
}
