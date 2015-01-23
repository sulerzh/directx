// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.CameraMover
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal abstract class CameraMover
  {
    private const double MaxOvertime = 0.1;
    protected const double MaxBlendDuration = 1.0;
    protected double currentTime;
    private bool paused;
    private bool playing;
    private bool endsAbruptly;
    protected double elapsedTime;
    protected double duration;
    internal CameraSnapshotBlender startBlender;
    internal CameraSnapshotBlender endBlender;
    protected CameraSnapshot currentSnapshot;

    internal abstract CameraSnapshot Start { get; }

    internal abstract CameraSnapshot End { get; }

    internal virtual bool MoveCompleted
    {
      get
      {
        return this.elapsedTime >= this.duration;
      }
    }

    internal CameraMover(double moveDuration)
    {
      this.duration = moveDuration;
    }

    internal void Pause()
    {
      this.paused = true;
      this.playing = false;
    }

    internal void Resume()
    {
      this.paused = false;
    }

    internal void Reset()
    {
      this.paused = this.playing = false;
      this.elapsedTime = 0.0;
      if (this.endBlender == null)
        return;
      this.endBlender.Reset();
    }

    protected void Update(double time)
    {
      if (this.playing)
        this.elapsedTime += time - this.currentTime;
      else
        this.playing = !this.paused;
      this.currentTime = time;
    }

    internal void TakeOverFrom(CameraMover other)
    {
      if (other != null)
      {
        this.currentTime = other.currentTime;
        this.elapsedTime = other.elapsedTime - other.duration;
        this.playing = true;
      }
      if (this.startBlender != null)
        return;
      if (other == null || other.GetEndBlender() == null)
      {
        double num = Math.Min(this.duration / 2.0, 2.0);
        DifferentiableCameraSnapshot startSnapshot = new DifferentiableCameraSnapshot(this.Start);
        DifferentiableCameraSnapshot differentiableSnapshot = this.GetDifferentiableSnapshot(num);
        this.startBlender = new CameraSnapshotBlender(0.0, num, startSnapshot, differentiableSnapshot);
      }
      else
        this.startBlender = other.GetEndBlender();
    }

    internal void SetEndBlender(CameraMover other)
    {
      double startTime;
      double num;
      DifferentiableCameraSnapshot endSnapshot;
      if (other == null)
      {
        startTime = -Math.Min(this.duration / 2.0, 2.0);
        num = 0.0;
        endSnapshot = new DifferentiableCameraSnapshot(this.End);
        this.endsAbruptly = true;
      }
      else
      {
        num = Math.Min(Math.Min(this.duration, other.duration) / 2.0, 1.0);
        startTime = -num;
        endSnapshot = other.GetDifferentiableSnapshot(num);
      }
      DifferentiableCameraSnapshot differentiableSnapshot = this.GetDifferentiableSnapshot(this.duration + startTime);
      this.endBlender = new CameraSnapshotBlender(startTime, num, differentiableSnapshot, endSnapshot);
    }

    private CameraSnapshotBlender GetEndBlender()
    {
      return this.endBlender;
    }

    protected CameraSnapshot GetSnapshot(double time)
    {
      this.UpdateCurrentSnapshot(time);
      return this.currentSnapshot.Clone();
    }

    internal virtual CameraSnapshot UpdateCameraSnapshot(SceneState state)
    {
      this.Update(state.ElapsedSeconds);
      if (this.elapsedTime < this.startBlender.EndTime)
        this.currentSnapshot = this.startBlender.GetValue(this.elapsedTime);
      else if (this.elapsedTime - this.duration > this.endBlender.StartTime)
      {
        if (this.MoveCompleted)
        {
          if (this.endsAbruptly)
          {
            this.currentSnapshot = this.End;
            this.paused = true;
          }
          if (!state.OfflineRender)
            this.paused = this.elapsedTime > this.duration + 0.1;
        }
        if (!this.paused)
          this.currentSnapshot = this.endBlender.GetValue(this.elapsedTime - this.duration);
      }
      else
        this.UpdateCurrentSnapshot(this.elapsedTime);
      return this.currentSnapshot;
    }

    protected abstract void UpdateCurrentSnapshot(double time);

    protected abstract DifferentiableCameraSnapshot GetDifferentiableSnapshot(double time);
  }
}
