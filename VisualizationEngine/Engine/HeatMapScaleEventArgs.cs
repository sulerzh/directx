// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.HeatMapScaleEventArgs
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System;

namespace Microsoft.Data.Visualization.Engine
{
  public class HeatMapScaleEventArgs : EventArgs
  {
    public float MinValue { get; private set; }

    public float MaxValue { get; private set; }

    internal HeatMapScaleEventArgs(float min, float max)
    {
      this.MinValue = min;
      this.MaxValue = max;
    }
  }
}
