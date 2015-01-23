using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class ResourceManager
    {
        private HashSet<GraphicsResource> resources = new HashSet<GraphicsResource>();
        private object hashLock = new object();
        private List<GraphicsResource> toAdd = new List<GraphicsResource>();
        private List<GraphicsResource> toRemove = new List<GraphicsResource>();
        private const int EstimatedFixedMemoryUsagePerResource = 256;
        private bool resetting;

        public int Count
        {
            get
            {
                return this.resources.Count;
            }
        }

        public long EstimatedSystemMemoryUsage
        {
            get
            {
                long result = 0L;
                lock (this.hashLock)
                {
                    foreach (GraphicsResource res in this.resources)
                    {
                        try
                        {
                            result += (long)res.GetEstimatedSystemMemoryUsage();
                        }
                        catch (Exception ex)
                        {
                        }
                        result += EstimatedFixedMemoryUsagePerResource;
                    }
                }
                return result;
            }
        }

        public long EstimatedVideoMemoryUsage
        {
            get
            {
                long result = 0L;
                lock (this.hashLock)
                {
                    foreach (GraphicsResource res in this.resources)
                        result += (long)res.GetEstimatedVideoMemoryUsage();
                }
                return result;
            }
        }

        internal bool RegisterResource(GraphicsResource resource)
        {
            lock (this.hashLock)
            {
                if (!this.resetting)
                    return this.resources.Add(resource);
                this.toAdd.Add(resource);
                return true;
            }
        }

        internal bool UnregisterResource(GraphicsResource resource)
        {
            lock (this.hashLock)
            {
                if (!this.resetting)
                    return this.resources.Remove(resource);
                this.toAdd.Add(resource);
                return true;
            }
        }

        internal void ResetResources()
        {
            lock (this.hashLock)
            {
                this.resetting = true;
                foreach (GraphicsResource res in this.resources)
                    res.ResetResource();
                this.resetting = false;
                foreach (GraphicsResource res in this.toAdd)
                    this.resources.Add(res);
                foreach (GraphicsResource res in this.toRemove)
                    this.resources.Remove(res);
                this.toAdd.Clear();
                this.toRemove.Clear();
            }
        }
    }
}
