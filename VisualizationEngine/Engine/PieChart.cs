// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.PieChart
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
  internal class PieChart : BubbleChart
  {
    internal override LayerType ChartType
    {
      get
      {
        return LayerType.PieChart;
      }
    }

    internal override int PrivateVisualsCount
    {
      get
      {
        return 1 + base.PrivateVisualsCount;
      }
    }

    internal PieChart()
    {
    }

    internal override void AddPrivateVisuals(ChartLayer layer)
    {
      layer.AddVisual((short) 0, (InstancedVisual) new PieSliceVisual(32));
      BubbleChart.AddNullAndZeroVisuals(layer);
    }

    protected override double ComputeMaxAbsValue(List<InstanceData> instanceList, bool timeBased, int first)
    {
      MaxSumOfAbsCalculator sumOfAbsCalculator = new MaxSumOfAbsCalculator(instanceList, timeBased);
      sumOfAbsCalculator.Compute(first);
      return sumOfAbsCalculator.Max;
    }
  }
}
