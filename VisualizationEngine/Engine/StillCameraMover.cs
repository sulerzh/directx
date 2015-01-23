// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.StillCameraMover
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Utilities;
using System.Diagnostics;

namespace Microsoft.Data.Visualization.Engine
{
  internal class StillCameraMover : CameraMover
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

    internal override bool MoveCompleted
    {
      get
      {
        return true;
      }
    }

    internal StillCameraMover(CameraSnapshot snapshot)
      : base(0.0)
    {
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "StillCameraMover constructor.");
      this.duration = 0.0;
      this.definingSnapshot = snapshot.Clone();
      this.currentSnapshot = snapshot.Clone();
    }

    internal override CameraSnapshot UpdateCameraSnapshot(SceneState state)
    {
      this.currentTime = state.ElapsedSeconds;
      return this.currentSnapshot;
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
