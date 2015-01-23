// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.StillEffectCameraMover
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Utilities;
using System;
using System.Diagnostics;

namespace Microsoft.Data.Visualization.Engine
{
  internal class StillEffectCameraMover : CameraMover
  {
    private CameraSnapshot definingSnapshot;

    internal override CameraSnapshot Start
    {
      get
      {
        return this.definingSnapshot;
      }
    }

    internal override CameraSnapshot End
    {
      get
      {
        return this.definingSnapshot;
      }
    }

    internal StillEffectCameraMover(CameraSnapshot snapshot, TimeSpan moveDuration)
      : base(moveDuration.TotalSeconds)
    {
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "StillEffectCameraMover constructor.");
      this.definingSnapshot = snapshot.Clone();
      this.currentSnapshot = snapshot.Clone();
    }

    protected override void UpdateCurrentSnapshot(double time)
    {
    }

    protected override DifferentiableCameraSnapshot GetDifferentiableSnapshot(double time)
    {
      return new DifferentiableCameraSnapshot(this.currentSnapshot);
    }
  }
}
