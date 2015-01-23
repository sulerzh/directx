// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.ClusteredColumnChart
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.MathExtensions;
using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal class ClusteredColumnChart : ColumnChart
  {
    internal override double HorizontalSpacing
    {
      get
      {
        return 2.0 * this.instanceWidth;
      }
    }

    internal override LayerType ChartType
    {
      get
      {
        return LayerType.ClusteredColumnChart;
      }
    }

    internal ClusteredColumnChart()
    {
    }

    internal override float GetMaxExtent(float scale, int count)
    {
      double d = Math.Atan(this.HorizontalSpacing / 2.0) * (double) count / 2.0;
      double num = 1.0 + (double) this.Altitude + (double) this.InstanceWidth;
      double y = 1.0 + num * num + num * Math.Cos(d) * 2.0;
      return scale * (float) MathEx.Hypot((double) this.InstanceWidth, y);
    }
  }
}
