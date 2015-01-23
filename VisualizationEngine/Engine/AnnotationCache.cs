// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.AnnotationCache
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Visualization.Engine
{
  internal class AnnotationCache : DisposableResource
  {
    private Dictionary<InstanceId, Texture> cache = new Dictionary<InstanceId, Texture>();
    private object cacheLock = new object();
    private const int MaxAnnotationFetchTime = 5000;
    private IAnnotationImageSource imageSource;
    private IInstanceIdRelationshipProvider idProvider;

    public AnnotationCache(IAnnotationImageSource annotationImageSource, IInstanceIdRelationshipProvider idRelationshipProvider)
    {
      this.idProvider = idRelationshipProvider;
      this.SetImageSource(annotationImageSource);
    }

    public void SetImageSource(IAnnotationImageSource annotationImageSource)
    {
      this.imageSource = annotationImageSource;
    }

    public Texture GetTexture(InstanceId instanceId, Renderer renderer, bool blockUntilComplete)
    {
      lock (this.cacheLock)
      {
        if (this.cache.ContainsKey(instanceId))
          return this.cache[instanceId];
        this.cache.Add(instanceId, (Texture) null);
      }
      Task task = Task.Factory.StartNew((Action) (() =>
      {
        if (this.imageSource == null)
          return;
        Image annotationImage = this.imageSource.GetAnnotationImage(new InstanceId(0U, instanceId), false);
        if (!blockUntilComplete)
          Monitor.Enter(this.cacheLock);
        if (annotationImage == null)
          this.cache.Remove(instanceId);
        else if (this.cache.ContainsKey(instanceId))
        {
          this.InvalidateTexture(instanceId, false);
          this.cache[instanceId] = this.GetTexture(annotationImage, renderer);
        }
        else
          annotationImage.Dispose();
        if (blockUntilComplete)
          return;
        Monitor.Exit(this.cacheLock);
      }));
      if (blockUntilComplete)
      {
        if (task.Wait(5000))
        {
          lock (this.cacheLock)
          {
            Texture local_1;
            if (this.cache.TryGetValue(instanceId, out local_1))
              return local_1;
            else
              return (Texture) null;
          }
        }
        else
          VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "The annotation cache was unable to synchronously retrieve an annotation image.");
      }
      return (Texture) null;
    }

    public Texture GetMostRecentTexture(InstanceId instanceId, Renderer renderer, bool blockUntilComplete, out bool hadExactTexture)
    {
      lock (this.cacheLock)
      {
        hadExactTexture = true;
        Texture local_0 = this.GetTexture(instanceId, renderer, blockUntilComplete);
        if (local_0 == null)
        {
          hadExactTexture = false;
          foreach (InstanceId item_0 in this.idProvider.GetRelatedIdsOverTime(instanceId))
          {
            if (this.cache.ContainsKey(item_0) && this.cache[item_0] != null)
            {
              local_0 = this.cache[item_0];
              break;
            }
          }
        }
        return local_0;
      }
    }

    public bool InvalidateTexture(InstanceId instanceId)
    {
      lock (this.cacheLock)
        return this.InvalidateTexture(instanceId, true);
    }

    private bool InvalidateTexture(InstanceId instanceId, bool invalidatePendingRequests)
    {
      bool flag = false;
      foreach (InstanceId key in this.idProvider.GetRelatedIdsOverTime(instanceId))
      {
        if (this.cache.ContainsKey(key))
        {
          Texture texture = this.cache[key];
          if (texture != null)
            texture.Dispose();
          if (texture != null || invalidatePendingRequests)
          {
            this.cache.Remove(key);
            flag = true;
          }
        }
      }
      return flag;
    }

    private Texture GetTexture(Image image, Renderer renderer)
    {
      if (image == null)
        return (Texture) null;
      Texture texture = renderer.CreateTexture(image, false, false, TextureUsage.Static);
      image.Dispose();
      return texture;
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing)
        return;
      lock (this.cacheLock)
      {
        foreach (Texture item_0 in this.cache.Values)
        {
          if (item_0 != null)
            item_0.Dispose();
        }
      }
    }
  }
}
