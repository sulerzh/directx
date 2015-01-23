// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.TileCacheStatistics
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

namespace Microsoft.Data.Visualization.Engine
{
  public class TileCacheStatistics
  {
    public int TileCount { get; internal set; }

    public int TextureCacheCount { get; internal set; }

    public int VertexCacheCount { get; internal set; }

    public int TextureCreationCount { get; internal set; }

    public int VertexCreationCount { get; internal set; }

    public int TextureReturnedToQueueCount { get; internal set; }

    public int VertexReturnedToQueueCount { get; internal set; }

    public int TileQueueCount { get; internal set; }

    public int TileInitializationCount { get; internal set; }

    public int TilePurgeCount { get; internal set; }

    public override string ToString()
    {
      return string.Format("TileCacheCount: {9}\r\nTextureCacheCount: {0}\r\nVertexCacheCount: {1}\r\nTextureCreationCount: {2}\r\nVertexCreationCount: {3}\r\nTextureReturnedToQueueCount: {4}\r\nVertexReturnedToQueueCount: {5}\r\nTileQueueCount: {6}\r\nTileInitializationCount: {7}\r\nTilePurgeCount: {8}", (object) this.TextureCacheCount, (object) this.VertexCacheCount, (object) this.TextureCreationCount, (object) this.VertexCreationCount, (object) this.TextureReturnedToQueueCount, (object) this.VertexReturnedToQueueCount, (object) this.TileQueueCount, (object) this.TileInitializationCount, (object) this.TilePurgeCount, (object) this.TileCount);
    }
  }
}
