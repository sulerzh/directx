// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.PushInCameraMover
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.MathExtensions;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Diagnostics;

namespace Microsoft.Data.Visualization.Engine
{
  internal class PushInCameraMover : CameraMover
  {
    private double speed;
    private double a;
    private double b;
    private double c;
    private bool autoFocus;
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

    internal PushInCameraMover(CameraSnapshot start, double effectSpeed, TimeSpan moveDuration)
      : base(moveDuration.TotalSeconds)
    {
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "PushInCameraMover constructor.");
      this.speed = effectSpeed;
      this.startSnapshot = start.Clone();
      this.currentSnapshot = start.Clone();
      this.a = 7.83927971443699E-05;
      this.b = this.startSnapshot.Distance - this.a;
      double d = (this.startSnapshot.Distance * 0.75 - this.a) / this.b;
      this.c = d >= 1E-06 ? -Math.Log(d) / (TourStep.DefaultSceneDuration * TourStep.DefaultSpeedFactor) : 0.0;
      this.autoFocus = Math.Abs(this.startSnapshot.PivotAngle - DefaultCameraController.GetAutoPivotAngle(this.startSnapshot.Distance)) <= 1E-06;
    }

    protected override void UpdateCurrentSnapshot(double time)
    {
      this.currentSnapshot.Distance = this.a + this.b * Math.Exp(-this.c * this.speed * time);
      if (!this.autoFocus)
        return;
      this.currentSnapshot.PivotAngle = DefaultCameraController.GetAutoPivotAngle(this.currentSnapshot.Distance);
    }

    protected override DifferentiableCameraSnapshot GetDifferentiableSnapshot(double time)
    {
      DifferentiableCameraSnapshot differentiableCameraSnapshot = new DifferentiableCameraSnapshot(this.currentSnapshot);
      double derivative = -this.c * this.speed;
      DifferentiableScalar differentiableScalar = new DifferentiableScalar(derivative * time, derivative);
      differentiableCameraSnapshot.Distance = this.b * differentiableScalar.Exp;
      differentiableCameraSnapshot.Distance.Value += this.a;
      if (this.autoFocus)
        differentiableCameraSnapshot.PivotAngle = DefaultCameraController.GetAutoPivotAngle(differentiableCameraSnapshot.Distance);
      return differentiableCameraSnapshot;
    }
  }
}
