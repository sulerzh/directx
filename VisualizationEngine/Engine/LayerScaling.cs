// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.LayerScaling
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal class LayerScaling
  {
    private double creationTime = -1.0;
    private const float MinDistanceForAutoScaling = 0.003f;
    private const float MaxDistanceForAutoScaling = 0.5f;
    private const float AutoScalingGlobalScale = 3.5f;
    private const float EaseInTime = 0.8f;
    private bool lockViewScale;
    private double lockedViewScale;

    public float ViewScale { get; private set; }

    public float ViewScaleUnbound { get; private set; }

    public float EaseInScale { get; private set; }

    public LayerScaling()
    {
      this.ViewScale = -1f;
      this.ViewScaleUnbound = -1f;
    }

    public void LockViewScale(double scale)
    {
      this.lockedViewScale = scale;
      this.lockViewScale = true;
    }

    public void UnlockViewScale()
    {
      this.lockViewScale = false;
    }

    public void SetCreationTime()
    {
      this.creationTime = -1.0;
    }

    public void Update(SceneState state)
    {
      this.ViewScale = this.lockViewScale ? (float) this.lockedViewScale : Math.Max(3.0f / 1000.0f, Math.Min(0.5f, (float) state.CameraSnapshot.Distance)) * 3.5f;
      this.ViewScaleUnbound = (float) state.CameraSnapshot.Distance * 3.5f;
      if (this.creationTime == -1.0)
        this.creationTime = state.ElapsedSeconds;
      this.EaseInScale = (float) Math.Sin(Math.Min(1.0, (state.ElapsedSeconds - this.creationTime) / 0.800000011920929) * (Math.PI / 2.0));
      this.ViewScale *= this.EaseInScale;
      this.ViewScaleUnbound *= this.EaseInScale;
    }
  }
}
