// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.BubbleChart
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;
using System;

namespace Microsoft.Data.Visualization.Engine
{
  internal class BubbleChart : Chart
  {
    internal override float FixedDimension
    {
      get
      {
        return (float) this.instanceHeight;
      }
    }

    internal override Vector2F FixedScale
    {
      get
      {
        return Vector2F.YVector;
      }
    }

    internal override LayerType ChartType
    {
      get
      {
        return LayerType.BubbleChart;
      }
    }

    internal override bool UsesAbsDimension
    {
      get
      {
        return true;
      }
    }

    internal override bool IsBubble
    {
      get
      {
        return true;
      }
    }

    internal BubbleChart()
    {
      this.instanceWidth = (double) Bubble.Width;
      this.instanceHeight = (double) Bubble.Height;
      this.Altitude = Bubble.Altitude;
      this.ShadowScale = Bubble.ShadowScale;
    }

    internal override Vector2F GetVariableScale(double scale)
    {
      return new Vector2F((float) (1.0 / Math.Sqrt(1.0 / scale) * this.instanceWidth), 0.0f);
    }

    internal override void AddPrivateVisuals(ChartLayer layer)
    {
      BubbleChart.AddNullAndZeroVisuals(layer);
    }

    protected static void AddNullAndZeroVisuals(ChartLayer layer)
    {
      layer.AddVisual(layer.NullMarkerId, (InstancedVisual) new RoundNullMarkerVisual());
      layer.AddVisual(layer.ZeroMarkerId, (InstancedVisual) new HollowCylinderVisual());
    }
  }
}
