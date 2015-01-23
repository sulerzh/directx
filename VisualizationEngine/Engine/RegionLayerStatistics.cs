// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.RegionLayerStatistics
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

namespace Microsoft.Data.Visualization.Engine
{
  public class RegionLayerStatistics
  {
    public int MinRegionVertexCount { get; internal set; }

    public int MaxRegionVertexCount { get; internal set; }

    public int AverageRegionVertexCount { get; internal set; }

    public int RegionCount { get; internal set; }

    internal RegionLayerStatistics()
    {
      this.MinRegionVertexCount = int.MaxValue;
      this.MaxRegionVertexCount = 0;
      this.AverageRegionVertexCount = 0;
    }

    public override string ToString()
    {
      return string.Format("MinRegionVertexCount: {0}\r\nMaxRegionVertexCount: {1}\r\nAverageRegionVertexCount: {2}\r\nRegionCount: {3}\r\n", (object) (this.MinRegionVertexCount == int.MaxValue ? 0 : this.MinRegionVertexCount), (object) this.MaxRegionVertexCount, (object) this.AverageRegionVertexCount, (object) this.RegionCount);
    }
  }
}
