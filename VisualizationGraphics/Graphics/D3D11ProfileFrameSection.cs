using System;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11ProfileFrameSection : ProfileFrameSection
    {
        private DirectX.Direct3D11.D3DQuery disjoingQuery;
        private DirectX.Direct3D11.D3DQuery beginQuery;
        private DirectX.Direct3D11.D3DQuery barrierQuery;
        private IntPtr disjointQueryData;
        private IntPtr beginQueryData;
        private IntPtr barrierQueryData;
        private DirectX.Direct3D11.QueryDataTimestampDisjoint? disjointInfo;
        private ulong? frequency;
        private ulong? beginCounter;
        private ulong? endCounter;
        private bool isReady;
        private bool hasBegun;

        internal DirectX.Direct3D11.D3DDevice Device { get; private set; }

        internal object RendererLock { get; private set; }

        internal DirectX.Direct3D11.DeviceContext Context { get; private set; }

        public override bool IsReady
        {
            get
            {
                this.CheckIsReady();
                return this.isReady;
            }
        }

        public override float? Frequency
        {
            get
            {
                if (!this.frequency.HasValue)
                    this.frequency = this.GetFrequency();
                ulong? freq = this.frequency;
                if (!freq.HasValue)
                    return new float?();
                return freq.GetValueOrDefault();
            }
        }

        public override unsafe float? Duration
        {
            get
            {
                try
                {
                    lock (this.RendererLock)
                    {
                        if (!this.beginCounter.HasValue)
                        {
                            if (this.Context.GetData(this.beginQuery, this.beginQueryData, 
                                this.beginQuery.DataSize, 
                                DirectX.Direct3D11.AsyncGetDataOptions.DoNotFlush))
                                this.beginCounter = *((ulong*)this.beginQueryData.ToPointer());
                        }
                    }
                    if (this.beginCounter.HasValue)
                    {
                        if (this.endCounter.HasValue)
                            return (this.endCounter.Value - this.beginCounter.Value)/(this.Frequency.Value/1000f);
                    }
                }
                catch (DirectX.DirectXException ex)
                {
                }
                catch (COMException ex)
                {
                }
                return new float?();
            }
        }

        public D3D11ProfileFrameSection(string name, Renderer renderer)
            : base(name)
        {
            this.Device = ((D3D11Renderer)renderer).Device;
            this.Context = ((D3D11Renderer)renderer).Context;
            this.RendererLock = ((D3D11Renderer)renderer).ContextLock;
            this.Register(renderer);
        }

        public override bool Begin()
        {
            if (this.hasBegun)
                return false;
            this.hasBegun = true;
            lock (this.RendererLock)
            {
                this.CreateQuery();
                this.Context.Begin(this.disjoingQuery);
                this.Context.End(this.beginQuery);
            }
            return true;
        }

        public override bool End()
        {
            if (!this.hasBegun)
                return false;
            this.hasBegun = false;
            try
            {
                lock (this.RendererLock)
                {
                    this.Context.End(this.barrierQuery);
                    this.Context.End(this.disjoingQuery);
                }
            }
            catch (DirectX.DirectXException ex)
            {
            }
            catch (COMException ex)
            {
            }
            return true;
        }

        protected override void ResetState(ProfileFrameSection parent)
        {
            this.beginCounter = new ulong?();
            this.endCounter = new ulong?();
            this.frequency = new ulong?();
            this.disjointInfo = new DirectX.Direct3D11.QueryDataTimestampDisjoint?();
            this.isReady = false;
            this.Discard = false;
        }

        private void CreateQuery()
        {
            if (this.disjoingQuery == null)
            {
                this.Device = ((D3D11Renderer)this.Device.Tag).Device;
                this.Context = ((D3D11Renderer)this.Device.Tag).Context;
                this.disjoingQuery = this.Device.CreateQuery(
                    new DirectX.Direct3D11.QueryDescription()
                    {
                        Query = DirectX.Direct3D11.Query.TimestampDisjoint,
                        MiscellaneousQueryOptions = DirectX.Direct3D11.MiscellaneousQueryOptions.None
                    });
                this.beginQuery = this.Device.CreateQuery(
                    new DirectX.Direct3D11.QueryDescription()
                    {
                        Query = DirectX.Direct3D11.Query.Timestamp,
                        MiscellaneousQueryOptions = DirectX.Direct3D11.MiscellaneousQueryOptions.None
                    });
                this.barrierQuery = this.Device.CreateQuery(
                    new DirectX.Direct3D11.QueryDescription()
                    {
                        Query = DirectX.Direct3D11.Query.Timestamp,
                        MiscellaneousQueryOptions = DirectX.Direct3D11.MiscellaneousQueryOptions.None
                    });
            }
            if (this.disjointQueryData == IntPtr.Zero)
                this.disjointQueryData = Marshal.AllocCoTaskMem((int)this.disjoingQuery.DataSize);
            if (this.beginQueryData == IntPtr.Zero)
                this.beginQueryData = Marshal.AllocCoTaskMem((int)this.beginQuery.DataSize);
            if (!(this.barrierQueryData == IntPtr.Zero))
                return;
            this.barrierQueryData = Marshal.AllocCoTaskMem((int)this.barrierQuery.DataSize);
        }

        private unsafe ulong? GetFrequency()
        {
            try
            {
                lock (this.RendererLock)
                {
                    if (!this.disjointInfo.HasValue)
                    {
                        if (this.Context.GetData(
                            this.disjoingQuery, this.disjointQueryData, 
                            this.disjoingQuery.DataSize, 
                            DirectX.Direct3D11.AsyncGetDataOptions.DoNotFlush))
                        {
                            this.disjointInfo = new DirectX.Direct3D11.QueryDataTimestampDisjoint?(
                                *(DirectX.Direct3D11.QueryDataTimestampDisjoint*)this.disjointQueryData.ToPointer());
                            if (!this.disjointInfo.Value.Disjoint)
                                return new ulong?(this.disjointInfo.Value.Frequency);
                        }
                    }
                }
            }
            catch (DirectX.DirectXException ex)
            {
            }
            catch (COMException ex)
            {
            }
            return new ulong?();
        }

        private unsafe void CheckIsReady()
        {
            try
            {
                lock (this.RendererLock)
                {
                    if (this.isReady || this.barrierQuery == null || 
                        !this.Context.GetData(
                        this.barrierQuery, this.barrierQueryData, 
                        this.barrierQuery.DataSize, DirectX.Direct3D11.AsyncGetDataOptions.DoNotFlush))
                        return;
                    this.endCounter = new ulong?((ulong)*(long*)this.barrierQueryData.ToPointer());
                    this.isReady = this.Frequency.HasValue || this.disjointInfo.HasValue;
                }
            }
            catch (DirectX.DirectXException ex)
            {
            }
            catch (COMException ex)
            {
            }
        }

        internal override int GetEstimatedSystemMemoryUsage()
        {
            return (this.beginQuery != null ? (int)this.beginQuery.DataSize : 0) +
                (this.barrierQuery != null ? (int)this.barrierQuery.DataSize : 0) + 
                (this.disjoingQuery != null ? (int)this.disjoingQuery.DataSize : 0);
        }

        internal override int GetEstimatedVideoMemoryUsage()
        {
            return this.GetEstimatedSystemMemoryUsage();
        }

        protected override bool Reset()
        {
            this.ReleaseGraphicsResources();
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                this.ReleaseGraphicsResources();
            Marshal.FreeCoTaskMem(this.disjointQueryData);
            this.disjointQueryData = IntPtr.Zero;
            Marshal.FreeCoTaskMem(this.beginQueryData);
            this.beginQueryData = IntPtr.Zero;
            Marshal.FreeCoTaskMem(this.barrierQueryData);
            this.barrierQueryData = IntPtr.Zero;
        }

        private void ReleaseGraphicsResources()
        {
            if (this.disjoingQuery != null)
            {
                this.disjoingQuery.Dispose();
                this.disjoingQuery = null;
            }
            if (this.beginQuery != null)
            {
                this.beginQuery.Dispose();
                this.beginQuery = null;
            }
            if (this.barrierQuery != null)
            {
                this.barrierQuery.Dispose();
                this.barrierQuery = null;
            }
            this.Device = null;
            this.Context = null;
        }
    }
}
