// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.TileTexturePool
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
  internal class TileTexturePool : DisposableResource
  {
    private object texturePoolLock = new object();
    private Queue<Texture> tileTexturePool;
    private TileCacheStatistics cacheStats;

    public int Count
    {
      get
      {
        return this.tileTexturePool.Count;
      }
    }

    public TileTexturePool(TileCacheStatistics stats)
    {
      this.cacheStats = stats;
    }

    public Texture GetTextureFromPool(Renderer renderer)
    {
      lock (this.texturePoolLock)
      {
        if (this.tileTexturePool.Count > 0)
          return this.tileTexturePool.Dequeue();
      }
      using (Image textureData = new Image(IntPtr.Zero, 256, 256, PixelFormat.Rgba32Bpp))
      {
        Texture texture = renderer.CreateTexture(textureData, false, false, TextureUsage.SharedStatic);
        texture.OnReset += new EventHandler(this.texture_OnReset);
        ++this.cacheStats.TextureCreationCount;
        return texture;
      }
    }

    public void ReturnTextureToPool(Texture texture)
    {
      lock (this.texturePoolLock)
      {
        this.tileTexturePool.Enqueue(texture);
        ++this.cacheStats.TextureReturnedToQueueCount;
      }
    }

    public void Initialize(Renderer renderer, int poolSize)
    {
      this.tileTexturePool = new Queue<Texture>(poolSize);
      lock (this.texturePoolLock)
      {
        using (Image resource_0 = new Image(IntPtr.Zero, 256, 256, PixelFormat.Rgba32Bpp))
        {
          for (int local_1 = 0; local_1 < poolSize; ++local_1)
          {
            Texture local_2 = renderer.CreateTexture(resource_0, false, false, TextureUsage.SharedStatic);
            local_2.OnReset += new EventHandler(this.texture_OnReset);
            this.tileTexturePool.Enqueue(local_2);
            renderer.SetTexture(0, local_2);
          }
        }
      }
      renderer.SetTexture(0, (Texture) null);
    }

    private void texture_OnReset(object sender, EventArgs e)
    {
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing)
        return;
      foreach (DisposableResource disposableResource in this.tileTexturePool)
        disposableResource.Dispose();
    }
  }
}
