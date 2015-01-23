// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.ColumnChart
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;

namespace Microsoft.Data.Visualization.Engine
{
  internal class ColumnChart : Chart
  {
    internal override float FixedDimension
    {
      get
      {
        return (float) this.instanceWidth;
      }
    }

    internal override Vector2F FixedScale
    {
      get
      {
        return Vector2F.XVector;
      }
    }

    internal override LayerType ChartType
    {
      get
      {
        return LayerType.ColumnChart;
      }
    }

    internal ColumnChart()
    {
      this.instanceWidth = (double) Column.Width;
      this.instanceHeight = (double) Column.Height;
      this.Altitude = Column.Altitude;
      this.ShadowScale = Column.ShadowScale;
    }

    internal override Vector2F GetVariableScale(double scale)
    {
      return new Vector2F(0.0f, (float) (scale * this.instanceHeight));
    }

    internal override void AddPrivateVisuals(ChartLayer layer)
    {
      layer.AddVisual(layer.NullMarkerId, (InstancedVisual) new SquareNullMarkerVisual());
      layer.AddVisual(layer.ZeroMarkerId, (InstancedVisual) new HollowCubeVisual());
    }
  }
}
