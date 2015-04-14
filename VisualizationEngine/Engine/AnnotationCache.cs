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
                {
                    return this.cache[instanceId];
                }
                this.cache.Add(instanceId, null);
            }
            Task task = Task.Factory.StartNew(() =>
            {
                if (this.imageSource == null)
                    return;
                Image annotationImage = this.imageSource.GetAnnotationImage(new InstanceId(0U, instanceId), false);
                if (!blockUntilComplete)
                    Monitor.Enter(this.cacheLock);
                if (annotationImage == null)
                {
                    this.cache.Remove(instanceId);
                }
                else if (this.cache.ContainsKey(instanceId))
                {
                    this.InvalidateTexture(instanceId, false);
                    this.cache[instanceId] = this.GetTexture(annotationImage, renderer);
                }
                else
                {
                    annotationImage.Dispose();
                }

                if (blockUntilComplete)
                    return;
                Monitor.Exit(this.cacheLock);
            });
            if (blockUntilComplete)
            {
                if (task.Wait(MaxAnnotationFetchTime))
                {
                    lock (this.cacheLock)
                    {
                        Texture tex;
                        if (this.cache.TryGetValue(instanceId, out tex))
                            return tex;
                        return null;
                    }
                }
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "The annotation cache was unable to synchronously retrieve an annotation image.");
            }
            return null;
        }

        public Texture GetMostRecentTexture(InstanceId instanceId, Renderer renderer, bool blockUntilComplete, out bool hadExactTexture)
        {
            lock (this.cacheLock)
            {
                hadExactTexture = true;
                Texture tex = this.GetTexture(instanceId, renderer, blockUntilComplete);
                if (tex == null)
                {
                    hadExactTexture = false;
                    foreach (InstanceId key in this.idProvider.GetRelatedIdsOverTime(instanceId))
                    {
                        if (this.cache.ContainsKey(key) && this.cache[key] != null)
                        {
                            tex = this.cache[key];
                            break;
                        }
                    }
                }
                return tex;
            }
        }

        public bool InvalidateTexture(InstanceId instanceId)
        {
            lock (this.cacheLock)
            {
                return this.InvalidateTexture(instanceId, true);
            }
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
                return null;
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
                foreach (Texture tex in this.cache.Values)
                {
                    if (tex != null)
                        tex.Dispose();
                }
            }
        }
    }
}
