// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.RegionScaleEventArgs
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System;

namespace Microsoft.Data.Visualization.Engine
{
  public class RegionScaleEventArgs : EventArgs
  {
    public RegionLayerShadingMode ShadingMode { get; private set; }

    public double[] MinValues { get; private set; }

    public double[] MaxValues { get; private set; }

    internal RegionScaleEventArgs(ValueRangeCalculator ranges, double[] minValues, double[] maxValues, RegionLayerShadingMode shadingMode, double maxAbsValue)
    {
      this.ShadingMode = shadingMode;
      switch (shadingMode)
      {
        case RegionLayerShadingMode.FullBleed:
          this.MinValues = (double[]) null;
          this.MaxValues = (double[]) null;
          break;
        case RegionLayerShadingMode.Global:
          this.MinValues = new double[minValues.Length];
          this.MaxValues = new double[maxValues.Length];
          for (int index = 0; index < minValues.Length; ++index)
            this.MinValues[index] = ranges.MinOverallMaxValue;
          for (int index = 0; index < maxValues.Length; ++index)
            this.MaxValues[index] = ranges.MaxOverallValue;
          break;
        case RegionLayerShadingMode.Local:
          this.MinValues = new double[minValues.Length];
          this.MaxValues = new double[maxValues.Length];
          for (int index = 0; index < minValues.Length; ++index)
            this.MinValues[index] = ranges.MinLocalMaxValue;
          for (int index = 0; index < maxValues.Length; ++index)
            this.MaxValues[index] = ranges.MaxLocalMaxValue;
          break;
        case RegionLayerShadingMode.ShiftGlobal:
          this.MinValues = minValues;
          this.MaxValues = maxValues;
          break;
      }
      if (this.MinValues == null)
        return;
      for (int index = 0; index < this.MinValues.Length; ++index)
      {
        if (this.MinValues[index] == this.MaxValues[index])
          this.MinValues[index] = double.NaN;
        if (double.IsPositiveInfinity(this.MinValues[index]) || double.IsNegativeInfinity(this.MinValues[index]) || (this.MinValues[index] == double.MinValue || this.MinValues[index] == double.MaxValue))
          this.MinValues[index] = double.NaN;
        if (double.IsPositiveInfinity(this.MaxValues[index]) || double.IsNegativeInfinity(this.MaxValues[index]) || (this.MaxValues[index] == double.MinValue || this.MaxValues[index] == double.MaxValue))
          this.MaxValues[index] = double.NaN;
        if (shadingMode != RegionLayerShadingMode.Local)
        {
          this.MinValues[index] *= maxAbsValue;
          this.MaxValues[index] *= maxAbsValue;
        }
      }
    }
  }
}
