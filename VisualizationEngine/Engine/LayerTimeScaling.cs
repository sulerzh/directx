// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.LayerTimeScaling
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal class LayerTimeScaling
  {
    public const float RampTime = 0.25f;

    public float VisualTime { get; private set; }

    public float VisualTimeScale { get; private set; }

    public bool VisualTimeFreezeEnabled { get; private set; }

    public float VisualTimeFreeze { get; private set; }

    public bool TimeEnabled { get; private set; }

    public void Update(DateTime? minTime, DateTime? maxTime, SceneState state)
    {
      DateTime dateTime1 = minTime.HasValue ? minTime.Value : DateTime.MaxValue;
      DateTime dateTime2 = maxTime.HasValue ? maxTime.Value : DateTime.MinValue;
      double num1 = (dateTime2 - dateTime1).TotalMilliseconds;
      if (num1 == 0.0)
        num1 = 0.25 * state.VisualTimeToRealtimeRatio * 1000.0;
      double num2 = (state.VisualTime - dateTime1).TotalMilliseconds / num1;
      double num3 = num1 / state.VisualTimeToRealtimeRatio / 1000.0;
      this.VisualTime = (float) num2;
      this.VisualTimeScale = (float) num3;
      this.VisualTimeFreezeEnabled = state.VisualTimeFreeze.HasValue;
      if (state.VisualTimeFreeze.HasValue)
        this.VisualTimeFreeze = (float) ((state.VisualTimeFreeze.Value - dateTime1).TotalMilliseconds / num1);
      this.TimeEnabled = !(dateTime1 == DateTime.MaxValue) || !(dateTime2 == DateTime.MinValue);
    }
  }
}
