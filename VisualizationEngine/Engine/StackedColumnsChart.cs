// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.StackedColumnsChart
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.MathExtensions;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
  internal class StackedColumnsChart : ColumnChart
  {
    internal override LayerType ChartType
    {
      get
      {
        return LayerType.StackedColumnChart;
      }
    }

    internal override float StackGap
    {
      get
      {
        return 5E-05f;
      }
    }

    internal override float AnnotationAnchorHeight
    {
      get
      {
        return 0.5f;
      }
    }

    internal StackedColumnsChart()
    {
    }

    internal override float GetMaxExtent(float scale, int count)
    {
      return scale * (float) MathEx.Hypot((double) this.InstanceWidth, (double) count * (double) this.InstanceHeight + (double) (count - 1) * (double) this.StackGap + (double) this.Altitude);
    }

    protected override double ComputeMaxAbsValue(List<InstanceData> instanceList, bool timeBased, int first)
    {
      MaxAbsSumCalculator absSumCalculator = new MaxAbsSumCalculator(instanceList, timeBased);
      absSumCalculator.Compute(first);
      return absSumCalculator.Max;
    }
  }
}
