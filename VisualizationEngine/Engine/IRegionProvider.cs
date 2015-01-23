// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.IRegionProvider
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Visualization.Engine
{
  public interface IRegionProvider : IDisposable
  {
    bool IsDirty { get; set; }

    string SerializedRegionProvider { get; }

    IEnumerable<string> Sources { get; }

    Task<List<RegionData>> GetRegionAsync(double lat, double lon, int lod, EntityType entityType, CancellationToken token, bool getAllPolygons = true, bool getMetadata = false, bool upLevel = true);

    void Reset();
  }
}
