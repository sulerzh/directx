// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.TileCache
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Visualization.Engine
{
  public class TileCache : DisposableResource
  {
    private static object texturePoolLock = new object();
    private static TileCacheStatistics stats = new TileCacheStatistics();
    private static readonly ConcurrentDictionary<ImagerySet, string> dirsToDelete = new ConcurrentDictionary<ImagerySet, string>();
    private static readonly ReaderWriterLockSlim ReaderWriterLock = new ReaderWriterLockSlim();
    private static readonly DirectoryInfo dinfo = new DirectoryInfo(Tile.ImagingBaseDirectory);
    private readonly int MaxUnusedCacheSize = 300;
    private readonly int MaxCacheSize = 600;
    private Dictionary<string, Tile> queue = new Dictionary<string, Tile>();
    private Dictionary<string, Tile> tiles = new Dictionary<string, Tile>();
    private object tileLock = new object();
    private object queueLock = new object();
    private object vertexPoolLock = new object();
    private List<Tuple<long, Tile>> purgeList = new List<Tuple<long, Tile>>();
    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private readonly ReaderWriterLockSlim disposeReaderWriterLock = new ReaderWriterLockSlim();
    private const int MaxVertexBufferUpdatePerFrame = 15;
    private const int MaxTextureUpdatePerFrame = 2;
    private const int TileServerCount = 4;
    private const int ThreadCountPerServer = 2;
    private const int MaxLevelForCacheHold = 3;
    private const int MinFrameCountForPurge = 10;
    private const int MaxTilesPurgedPerFrame = 20;
    private const int TimeBeforeThreadAbortOnShutdown = 3000;
    private const int MinFileSize = 100;
    private readonly Action<Exception> onInternalError;
    private int initializedOpCount;
    private int currentImageSetId;
    private volatile bool running;
    private static volatile int tileCount;
    private Queue<VertexBuffer> tileVertexPool;
    private static TileTexturePool tileTexturePool;
    private static int tileTexturePoolReferenceCount;
    private static int tileCacheDeletionCount;
    private Thread[] queueThreads;
    private SemaphoreSlim[] tileSemaphore;
    private SemaphoreSlim tilePrepareSemaphore;
    private SemaphoreSlim textureLoadSemaphore;
    private EventWaitHandle[] queueThreadShutdown;
    private Vector3D cameraPosition;
    private int frameCount;
    private Renderer renderer;

    public static bool IsShrinkTaskRunning { get; set; }

    public static long Size
    {
      get
      {
        return Enumerable.Sum<FileInfo>(TileCache.dinfo.EnumerateFiles("*", SearchOption.AllDirectories), (Func<FileInfo, long>) (fi => fi.Length));
      }
    }

    public int InitializedOperationsCount
    {
      get
      {
        return this.initializedOpCount;
      }
    }

    public TileCacheStatistics Statistics
    {
      get
      {
        return TileCache.stats;
      }
    }

    public TileCache(Action<Exception> OnInternalError)
    {
      this.onInternalError = OnInternalError;
      this.InitializeSemaphores();
      this.StartQueue();
      Interlocked.Increment(ref TileCache.tileCacheDeletionCount);
      if (Environment.Is64BitProcess)
        return;
      this.MaxCacheSize /= 2;
      this.MaxUnusedCacheSize /= 2;
    }

    public static Task Shrink(long currentSizeInBytes, long targetSizeInBytes, CancellationToken token)
    {
      TileCache.IsShrinkTaskRunning = true;
      return Task.Factory.StartNew((Action) (() =>
      {
        try
        {
          if (currentSizeInBytes <= targetSizeInBytes)
            return;
          TileCache.ReaderWriterLock.EnterWriteLock();
          long num1 = currentSizeInBytes - targetSizeInBytes;
          Random random = new Random((int) (DateTime.Now.Ticks & (long) int.MaxValue));
          HashSet<string> hashSet = new HashSet<string>();
          int num2 = num1 == currentSizeInBytes ? 1 : 6;
          while (num1 > 0L && num2-- > 0)
          {
            string searchPattern;
            if (num2 == 0)
            {
              searchPattern = "*";
            }
            else
            {
              searchPattern = string.Format("*{0}.*", (object) random.Next(0, 10));
              if (!hashSet.Contains(searchPattern))
                hashSet.Add(searchPattern);
              else
                continue;
            }
            foreach (FileInfo fileInfo in TileCache.dinfo.EnumerateFiles(searchPattern, SearchOption.AllDirectories))
            {
              try
              {
                fileInfo.Delete();
                num1 -= fileInfo.Length;
                if (num1 > 0L)
                {
                  if (token.IsCancellationRequested)
                    break;
                }
                else
                  break;
              }
              catch (Exception ex)
              {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Failed to delete tile while shrinking cache due to exception: {0}", (object) ex);
              }
            }
          }
        }
        catch (Exception ex)
        {
          VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Cache shrinking failed with exception: {0}", (object) ex);
        }
        finally
        {
          TileCache.ReaderWriterLock.ExitWriteLock();
          TileCache.IsShrinkTaskRunning = false;
        }
      }), token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    internal unsafe Color4F? GetWaterColor(ImageSet imageSet)
    {
      Tile tile = this.GetTile(imageSet.BaseLevel, 0, 0, imageSet, (Tile) null);
      if (tile == null || !System.IO.File.Exists(tile.TextureFilename))
        return new Color4F?();
      Color4F? nullable = new Color4F?();
      try
      {
        using (Stream stream = (Stream) System.IO.File.OpenRead(tile.TextureFilename))
        {
          if (stream.Length > 0L)
          {
            using (Image image = new Image(stream))
              nullable = new Color4F?(Color4F.FromUint(*(uint*) image.Data.ToPointer()));
          }
        }
      }
      catch (Exception ex)
      {
        return new Color4F?();
      }
      return nullable;
    }

    internal Tile GetTile(int level, int x, int y, ImageSet dataset, Tile parent)
    {
      Tile tile = (Tile) null;
      string tileKey = ImageSet.GetTileKey(dataset, level, x, y);
      lock (this.tileLock)
      {
        if (!this.tiles.ContainsKey(tileKey))
        {
          tile = dataset.GetNewTile(level, x, y, parent, this);
          this.tiles.Add(tileKey, tile);
          ++TileCache.tileCount;
        }
        else
        {
          tile = this.tiles[tileKey];
          if (tile.Parent != null)
          {
            if (tile.Parent.Disposed)
              tile.SetParent(parent);
          }
        }
      }
      return tile;
    }

    internal void AddTileToQueue(Tile tile)
    {
      lock (this.queueLock)
      {
        if (tile.ReadyToRender)
        {
          if (tile.TextureReady)
            goto label_8;
        }
        if (!this.queue.ContainsKey(tile.Key))
        {
          this.queue[tile.Key] = tile;
          ThreadPool.QueueUserWorkItem((WaitCallback) (o =>
          {
            if (this.Disposed || this.cancellationTokenSource.IsCancellationRequested)
              return;
            bool flag1 = false;
            bool flag2 = false;
            try
            {
              flag2 = this.disposeReaderWriterLock.TryEnterReadLock(-1);
              if (this.Disposed || this.cancellationTokenSource.IsCancellationRequested)
                return;
              this.tilePrepareSemaphore.Wait(this.cancellationTokenSource.Token);
              lock (this.tileLock)
              {
                if (!this.tiles.ContainsKey(tile.Key) || tile.Disposed)
                  return;
                if ((long) this.frameCount - tile.LastUsed < 3L && (tile.InViewFrustum || tile.Parent != null && tile.Parent.InViewFrustum) && !this.cancellationTokenSource.IsCancellationRequested)
                {
                  ++this.initializedOpCount;
                  tile.PrepareForRender();
                }
                else
                  flag1 = true;
              }
            }
            catch (OperationCanceledException ex)
            {
              VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Operation canceled while adding a tile to loading queue in TileCache.");
            }
            catch (ThreadAbortException ex)
            {
              VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Thread aborted while adding a tile to loading queue in TileCache.");
            }
            catch (Exception ex1)
            {
              try
              {
                VisualizationTraceSource.Current.Fail("Exception occurred while adding a tile to the loading queue in TileCache.", ex1);
                if (this.onInternalError == null)
                  return;
                this.onInternalError(ex1);
              }
              catch (Exception ex2)
              {
              }
            }
            finally
            {
              try
              {
                if (flag1)
                  this.tilePrepareSemaphore.Release();
                if (flag2)
                  this.disposeReaderWriterLock.ExitReadLock();
              }
              catch (Exception ex)
              {
              }
            }
          }));
        }
      }
label_8:
      this.tileSemaphore[tile.ServerID].Release();
    }

    public void Update(Renderer rend, SceneState state)
    {
      if (this.tileVertexPool == null)
      {
        this.InitializeVertexPool(rend);
        lock (TileCache.texturePoolLock)
        {
          if (TileCache.tileTexturePool == null)
          {
            TileCache.tileTexturePool = new TileTexturePool(TileCache.stats);
            TileCache.tileTexturePool.Initialize(rend, this.MaxCacheSize);
            TileCache.tileTexturePoolReferenceCount = 1;
          }
          else
            ++TileCache.tileTexturePoolReferenceCount;
        }
      }
      this.cameraPosition = state.CameraPosition;
      this.renderer = rend;
      this.frameCount = this.renderer.FrameCount;
      this.PurgeLRU();
      this.DecimateQueue();
      int num = 0;
      lock (this.queueLock)
        num = this.queue.Count;
      if (this.tilePrepareSemaphore.CurrentCount == 0 && num > 0)
        this.tilePrepareSemaphore.Release(15);
      if (this.textureLoadSemaphore.CurrentCount == 0 && num > 0)
        this.textureLoadSemaphore.Release(2);
      TileCache.stats.TileCount = TileCache.tileCount;
      TileCache.stats.TileQueueCount = num;
      TileCache.stats.TextureCacheCount = TileCache.tileTexturePool.Count;
      TileCache.stats.VertexCacheCount = this.tileVertexPool.Count;
    }

    public VertexBuffer GetVertexBufferFromPool()
    {
      VertexBuffer vertexBuffer = (VertexBuffer) null;
      lock (this.vertexPoolLock)
      {
        if (this.tileVertexPool.Count > 0)
          vertexBuffer = this.tileVertexPool.Dequeue();
      }
      if (vertexBuffer == null)
      {
        int vertexCount = 289;
        TileVertex[] data = new TileVertex[vertexCount];
        ++TileCache.stats.VertexCreationCount;
        vertexBuffer = VertexBuffer.Create<TileVertex>(data, vertexCount, false);
        vertexBuffer.OnReset += new EventHandler(this.vertexBuffer_OnReset);
      }
      vertexBuffer.OnReset += new EventHandler(this.tile_OnReset);
      return vertexBuffer;
    }

    public void ReturnVertexBufferToPool(VertexBuffer vb)
    {
      lock (this.vertexPoolLock)
      {
        vb.OnReset -= new EventHandler(this.tile_OnReset);
        this.tileVertexPool.Enqueue(vb);
        ++TileCache.stats.VertexReturnedToQueueCount;
      }
    }

    public Texture GetTextureFromPool(Renderer renderer)
    {
      Texture textureFromPool = TileCache.tileTexturePool.GetTextureFromPool(renderer);
      textureFromPool.OnReset += new EventHandler(this.tile_OnReset);
      return textureFromPool;
    }

    public void ReturnTextureToPool(Texture texture)
    {
      texture.OnReset -= new EventHandler(this.tile_OnReset);
      TileCache.tileTexturePool.ReturnTextureToPool(texture);
    }

    private void tile_OnReset(object sender, EventArgs e)
    {
      foreach (DisposableResource disposableResource in this.tiles.Values)
        disposableResource.Dispose();
      this.tiles.Clear();
    }

    private void InitializeVertexPool(Renderer renderer)
    {
      this.tileVertexPool = new Queue<VertexBuffer>(this.MaxCacheSize);
      int vertexCount = 289;
      TileVertex[] data = new TileVertex[vertexCount];
      lock (this.vertexPoolLock)
      {
        for (int local_2 = 0; local_2 < this.MaxCacheSize; ++local_2)
        {
          VertexBuffer local_3 = VertexBuffer.Create<TileVertex>(data, vertexCount, false);
          local_3.OnReset += new EventHandler(this.vertexBuffer_OnReset);
          renderer.SetVertexSource(local_3);
          this.tileVertexPool.Enqueue(local_3);
        }
      }
      renderer.SetVertexSource((VertexBuffer) null);
    }

    private void vertexBuffer_OnReset(object sender, EventArgs e)
    {
    }

    private void InitializeSemaphores()
    {
      this.tileSemaphore = new SemaphoreSlim[4];
      for (int index = 0; index < this.tileSemaphore.Length; ++index)
        this.tileSemaphore[index] = new SemaphoreSlim(0);
      this.tilePrepareSemaphore = new SemaphoreSlim(0);
      this.textureLoadSemaphore = new SemaphoreSlim(0);
    }

    private void PurgeLRU()
    {
      if (TileCache.tileCount < this.MaxCacheSize)
        return;
      this.purgeList.Clear();
      lock (this.tileLock)
      {
        foreach (Tile item_0 in this.tiles.Values)
        {
          long local_1 = item_0.LastUsed;
          if (local_1 < (long) (this.frameCount - 10) && (item_0.Level > 3 || item_0.ImageSet.ImageSetID != this.currentImageSetId))
            this.purgeList.Add(new Tuple<long, Tile>(local_1, item_0));
        }
        if (this.purgeList.Count <= this.MaxUnusedCacheSize && TileCache.tileCount <= this.MaxCacheSize)
          return;
        this.purgeList.Sort((Comparison<Tuple<long, Tile>>) ((a, b) => a.Item1.CompareTo(b.Item1)));
        int local_5 = Math.Min(this.purgeList.Count, Math.Max(TileCache.tileCount - this.MaxCacheSize, Math.Min(20, this.purgeList.Count)));
        for (int local_6 = 0; local_6 < local_5; ++local_6)
        {
          bool local_8 = this.tiles.Remove(this.purgeList[local_6].Item2.Key);
          --TileCache.tileCount;
          this.purgeList[local_6].Item2.Dispose();
          if (local_8)
            ++TileCache.stats.TilePurgeCount;
        }
      }
    }

    private void TileProcessingThread(object param)
    {
      Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", false);
      int index1 = ((Tuple<bool, int, int>) param).Item2;
      int index2 = ((Tuple<bool, int, int>) param).Item3;
      WebClient webClient = WebRequestHelper.CreateWebClient();
      try
      {
        while (this.running)
        {
          double num = double.MaxValue;
          string index3 = (string) null;
          Tile tile = (Tile) null;
          this.tileSemaphore[index1].Wait(this.cancellationTokenSource.Token);
          if (!this.running)
            break;
          lock (this.queueLock)
          {
            foreach (Tile item_0 in this.queue.Values)
            {
              bool local_7 = index1 < 0 || item_0.ServerID == index1;
              bool local_8 = item_0.InViewFrustum || item_0.Parent != null && item_0.Parent.InViewFrustum;
              if (!item_0.InViewFrustum || item_0.LastUsed != (long) this.frameCount)
              {
                int temp_46 = item_0.Level;
              }
              if (!item_0.RequestPending && local_8 && local_7)
              {
                double local_10 = Math.Max(0.0, (new Vector3D(item_0.BoundingSphere.Origin) - this.cameraPosition).Length() - item_0.BoundingSphere.Radius);
                if (local_10 < num)
                {
                  Tile temp_74 = this.queue[item_0.Key];
                  num = local_10;
                  index3 = item_0.Key;
                }
              }
            }
            if (index3 != null)
            {
              tile = this.queue[index3];
              tile.SetRequestPending(true);
            }
          }
          if (tile != null)
          {
            try
            {
              this.LoadTileResources(tile, webClient);
            }
            catch (OperationCanceledException ex)
            {
              VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Operation canceled while loading tile resources in TileCache.");
              break;
            }
            lock (this.queueLock)
            {
              tile.SetRequestPending(false);
              this.queue.Remove(tile.Key);
            }
          }
        }
      }
      catch (OperationCanceledException ex)
      {
        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Operation canceled while processing tile in TileCache.");
      }
      catch (ThreadAbortException ex)
      {
        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Thread aborted while processing tiles in TileCache.");
      }
      catch (Exception ex1)
      {
        try
        {
          VisualizationTraceSource.Current.Fail("Internal Error occurred while processing tiles in TileCache.", ex1);
          if (this.onInternalError == null)
            return;
          this.onInternalError(ex1);
        }
        catch (Exception ex2)
        {
        }
      }
      finally
      {
        try
        {
          webClient.Dispose();
          this.queueThreadShutdown[index2].Set();
        }
        catch (Exception ex)
        {
        }
      }
    }

    private void DecimateQueue()
    {
      List<string> list = new List<string>();
      lock (this.queueLock)
      {
        foreach (Tile item_0 in this.queue.Values)
        {
          if (!item_0.RequestPending && (long) this.frameCount - item_0.LastUsed > 10L)
            list.Add(item_0.Key);
        }
        foreach (string item_1 in list)
          this.queue.Remove(item_1);
      }
    }

    private void StartQueue()
    {
      this.running = true;
      this.queueThreads = new Thread[8];
      this.queueThreadShutdown = new EventWaitHandle[8];
      for (int index = 0; index < this.queueThreadShutdown.Length; ++index)
        this.queueThreadShutdown[index] = new EventWaitHandle(false, EventResetMode.ManualReset);
      for (int index1 = 0; index1 < 4; ++index1)
      {
        for (int index2 = 0; index2 < 2; ++index2)
        {
          int index3 = index1 * 2 + index2;
          this.queueThreads[index3] = new Thread(new ParameterizedThreadStart(this.TileProcessingThread));
          this.queueThreads[index3].Priority = ThreadPriority.BelowNormal;
          this.queueThreads[index3].Name = "Network I/O Engine Thread #" + (object) index3;
          this.queueThreads[index3].Start((object) new Tuple<bool, int, int>(false, index1, index3));
        }
      }
    }

    private void InitializeTile(Tile tile)
    {
      if (!this.running)
        return;
      this.textureLoadSemaphore.Wait(this.cancellationTokenSource.Token);
      lock (this.tileLock)
      {
        if (!this.tiles.ContainsKey(tile.Key) || tile.Disposed || !tile.InitializeResources(this.renderer))
          return;
        tile.SetTextureReady();
        this.currentImageSetId = tile.ImageSet.ImageSetID;
        ++this.initializedOpCount;
        ++TileCache.stats.TileInitializationCount;
      }
    }

    private void LoadTileResources(Tile tile, WebClient webClient)
    {
      try
      {
        TileCache.ReaderWriterLock.EnterReadLock();
        string textureDirectory = tile.TextureDirectory;
        if (!Directory.Exists(textureDirectory))
          Directory.CreateDirectory(textureDirectory);
        if (tile.ImageSet.ImagerySet == ImagerySet.Aerial || tile.ImageSet.ImagerySet == ImagerySet.AerialWithLabels)
          TileCache.dirsToDelete.TryAdd(tile.ImageSet.ImagerySet, tile.ImageSetDirectory);
        string textureFilename = tile.TextureFilename;
        if (System.IO.File.Exists(textureFilename))
        {
          if (new FileInfo(textureFilename).Length < 100L)
          {
            try
            {
              System.IO.File.Delete(textureFilename);
            }
            catch (Exception ex)
            {
              VisualizationTraceSource.Current.Fail(string.Format("Exception while deleting an invalid file ({0}).", (object) textureFilename), ex);
            }
          }
        }
        if (!System.IO.File.Exists(textureFilename))
        {
          try
          {
            string textureUrl = tile.TextureURL;
            webClient.DownloadFile(textureUrl, textureFilename);
          }
          catch (WebException ex)
          {
          }
          catch (IOException ex)
          {
            VisualizationTraceSource.Current.Fail(string.Format("Failed to download tile ({0}).", (object) textureFilename), (Exception) ex);
          }
        }
        try
        {
          if (System.IO.File.Exists(textureFilename) && new FileInfo(textureFilename).Length < 100L)
            return;
          this.InitializeTile(tile);
        }
        catch (OperationCanceledException ex)
        {
          throw;
        }
        catch (Exception ex)
        {
          VisualizationTraceSource.Current.Fail(string.Format("Exception while loading a tile from the cached file ({0}).", (object) textureFilename), ex);
        }
      }
      finally
      {
        TileCache.ReaderWriterLock.ExitReadLock();
      }
    }

    private void ShutdownQueue()
    {
      if (!this.running)
        return;
      this.running = false;
      this.cancellationTokenSource.Cancel();
      this.textureLoadSemaphore.Release(10000);
      this.tilePrepareSemaphore.Release(10000);
      for (int index = 0; index < this.tileSemaphore.Length; ++index)
        this.tileSemaphore[index].Release(4);
      if (WaitHandle.WaitAll((WaitHandle[]) this.queueThreadShutdown, 3000))
        return;
      for (int index = 0; index < this.queueThreadShutdown.Length; ++index)
      {
        if (!this.queueThreadShutdown[index].WaitOne(0))
        {
          this.queueThreads[index].Abort();
          VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "The download thread #{1} exceeded the maximum allowed ({0} ms) time before it was forced to shut down.", (object) 3000, (object) index);
        }
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        bool flag = false;
        try
        {
          this.ShutdownQueue();
          base.Dispose(disposing);
          flag = this.disposeReaderWriterLock.TryEnterWriteLock(-1);
          for (int index = 0; index < this.tileSemaphore.Length; ++index)
            this.tileSemaphore[index].Dispose();
          this.textureLoadSemaphore.Dispose();
          this.tilePrepareSemaphore.Dispose();
          for (int index = 0; index < this.queueThreadShutdown.Length; ++index)
            this.queueThreadShutdown[index].Dispose();
          foreach (DisposableResource disposableResource in this.tiles.Values)
            disposableResource.Dispose();
          if (this.tileVertexPool != null)
          {
            foreach (DisposableResource disposableResource in this.tileVertexPool)
              disposableResource.Dispose();
          }
          lock (TileCache.texturePoolLock)
          {
            --TileCache.tileTexturePoolReferenceCount;
            if (TileCache.tileTexturePoolReferenceCount == 0)
            {
              TileCache.tileTexturePool.Dispose();
              TileCache.tileTexturePool = (TileTexturePool) null;
            }
          }
        }
        catch (ThreadAbortException ex)
        {
          VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Thread aborted while disposing tile cache.");
        }
        catch (Exception ex)
        {
          VisualizationTraceSource.Current.Fail("Failed to dispose tile cache.", ex);
        }
        finally
        {
          if (flag)
          {
            try
            {
              this.disposeReaderWriterLock.ExitWriteLock();
            }
            catch (Exception ex)
            {
              VisualizationTraceSource.Current.Fail("Failed to ExitWriteLock disposeReaderWriterLock.", ex);
            }
          }
        }
      }
      try
      {
        if (Interlocked.Decrement(ref TileCache.tileCacheDeletionCount) != 0)
          return;
        TileCache.ReaderWriterLock.EnterWriteLock();
        foreach (KeyValuePair<ImagerySet, string> keyValuePair in TileCache.dirsToDelete)
        {
          try
          {
            Directory.Delete(keyValuePair.Value, true);
          }
          catch (Exception ex)
          {
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Failed to delete imagery cache directory. Error: {0}", (object) ex.Message);
          }
        }
        TileCache.ReaderWriterLock.ExitWriteLock();
      }
      catch (Exception ex)
      {
        VisualizationTraceSource.Current.Fail("Failed in tile cache directory deletion.", ex);
      }
    }
  }
}
