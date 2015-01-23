// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.GlobeStep
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.Engine
{
  internal class GlobeStep : EngineStep
  {
    internal static string DefaultCacheDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Microsoft\\PowerMap\\";
    private TileRenderer tileRenderer = new TileRenderer(true);
    private Dictionary<string, Tile> tilesToRender = new Dictionary<string, Tile>();
    private const int RefinementWaitTime = 100;
    private const int MaxRefinementRetryCount = 600;
    private int previousOpCount;
    private TileCache tileCache;
    private GlobeUIController uiController;
    private ImageSet currentImageSet;
    private Color4F? waterColor;

    public ImageSet CurrentImageSet
    {
      get
      {
        return this.currentImageSet;
      }
      set
      {
        this.currentImageSet = value;
        this.waterColor = new Color4F?();
      }
    }

    public TileCacheStatistics TileStatistics
    {
      get
      {
        return this.tileCache.Statistics;
      }
    }

    public TileRenderer Renderer
    {
      get
      {
        return this.tileRenderer;
      }
    }

    public GlobeStep(IVisualizationEngineDispatcher dispatcher, Dispatcher eventDispatcher, ImageSet imageset, Action<Exception> onInternalError)
      : base(dispatcher, eventDispatcher)
    {
      this.CurrentImageSet = imageset;
      this.tileCache = new TileCache(onInternalError);
      this.waterColor = this.tileCache.GetWaterColor(this.CurrentImageSet);
      VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Initialized globe rendering with image set {0}.", (object) this.CurrentImageSet.Name);
    }

    public override void OnInitialized()
    {
      base.OnInitialized();
      this.uiController = new GlobeUIController(this);
      this.AddUIController((UIController) this.uiController);
    }

    internal override bool PreExecute(SceneState state, int phase)
    {
      int initializedOperationsCount = this.tileCache.InitializedOperationsCount;
      bool flag = initializedOperationsCount != this.previousOpCount;
      this.previousOpCount = initializedOperationsCount;
      return flag;
    }

    internal override void Execute(Renderer renderer, SceneState state, int phase)
    {
      try
      {
        if (phase == 0)
        {
          bool flag = this.RefineTiles(renderer, state);
          if (state.OfflineRender)
          {
            for (int index = 0; !flag && index++ < 600; flag = this.RefineTiles(renderer, state))
            {
              Thread.Sleep(100);
              this.tileCache.Update(renderer, state);
            }
            if (!flag)
              VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Timeout waiting for the globe tiles to be fully refined.");
          }
          this.tileCache.Update(renderer, state);
          if (!this.waterColor.HasValue)
            this.waterColor = this.tileCache.GetWaterColor(this.CurrentImageSet);
          state.WaterColor = this.waterColor;
          renderer.Profiler.BeginSection("[Globe]");
          this.tileRenderer.RenderDepth(renderer, state);
          this.tileRenderer.Render(renderer, state, false);
          if (state.TranslucentGlobe)
            renderer.ClearCurrentDepthTarget(1f);
          state.GlobeDepth = this.tileRenderer.DepthTexture;
          renderer.Profiler.EndSection();
        }
        else
        {
          if (state.TranslucentGlobe)
          {
            renderer.Profiler.BeginSection("[Globe Alpha]");
            this.tileRenderer.Render(renderer, state, true);
            renderer.Profiler.EndSection();
          }
          this.tileRenderer.ClearRenderables();
        }
        if (Environment.OSVersion.Platform != PlatformID.Win32NT || !(Environment.OSVersion.Version >= new Version(6, 2, 9200, 0)))
          return;
        renderer.Flush();
      }
      catch (Exception ex)
      {
        VisualizationTraceSource.Current.Fail("Exception while executing Globe Step.", ex);
      }
    }

    private bool RefineTiles(Renderer renderer, SceneState state)
    {
      this.tilesToRender.Clear();
      bool flag = false;
      for (int x = 0; x < 2; ++x)
      {
        for (int y = 0; y < 2; ++y)
        {
          Tile tile = this.tileCache.GetTile(this.CurrentImageSet.BaseLevel, x, y, this.CurrentImageSet, (Tile) null);
          if (tile != null)
          {
            if (tile.IsTileInFrustum(state.GetViewFrustum(), state.FlatteningFactor))
            {
              bool hasIncompleteData;
              tile.Resolve(this.tilesToRender, state, out hasIncompleteData);
              flag = flag | hasIncompleteData;
            }
          }
          else
            flag = true;
        }
      }
      foreach (Tile tile in this.tilesToRender.Values)
        this.tileRenderer.AddRenderable(tile.Renderable);
      return !flag;
    }

    public Dictionary<int, TileExtent> GetTileExtents()
    {
      Dictionary<int, List<Tile>> dictionary1 = new Dictionary<int, List<Tile>>();
      foreach (Tile tile in this.tilesToRender.Values)
      {
        if (!dictionary1.ContainsKey(tile.Level))
          dictionary1.Add(tile.Level, new List<Tile>());
        dictionary1[tile.Level].Add(tile);
      }
      Dictionary<int, TileExtent> dictionary2 = new Dictionary<int, TileExtent>();
      foreach (int key in dictionary1.Keys)
      {
        TileExtent tileExtent = TileExtent.Empty;
        foreach (Tile tile in dictionary1[key])
          tileExtent = !(tileExtent == TileExtent.Empty) ? tileExtent.Union(tile.Extent) : tile.Extent;
        dictionary2.Add(key, tileExtent);
      }
      return dictionary2;
    }

    public override void Dispose()
    {
      if (this.tileRenderer != null)
      {
        this.tileRenderer.Dispose();
        this.tileRenderer = (TileRenderer) null;
      }
      if (this.tileCache == null)
        return;
      this.tileCache.Dispose();
      this.tileCache = (TileCache) null;
    }
  }
}
