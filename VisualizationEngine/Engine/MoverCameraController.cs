// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.MoverCameraController
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

namespace Microsoft.Data.Visualization.Engine
{
  internal class MoverCameraController : CameraController
  {
    private bool releaseControlWhenComplete;
    private bool completed;

    public CameraMover Mover { get; private set; }

    public override bool Completed
    {
      get
      {
        return this.completed;
      }
    }

    public void SetMover(CameraMover mover, bool releaseControl)
    {
      if (mover != null)
      {
        mover.Reset();
        mover.TakeOverFrom(this.Mover);
      }
      this.Mover = mover;
      this.releaseControlWhenComplete = releaseControl;
      this.completed = false;
    }

    public override CameraSnapshot Update(SceneState state)
    {
      if (this.Mover == null)
        return CameraSnapshot.Default();
      CameraSnapshot cameraSnapshot = this.Mover.UpdateCameraSnapshot(state).Clone();
      if (this.releaseControlWhenComplete && this.Mover.MoveCompleted)
        this.completed = true;
      return cameraSnapshot;
    }
  }
}
