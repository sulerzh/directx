using System;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11ProfileSection : ProfileSection
    {
        private DirectX.Direct3D11.D3DDevice device;
        private DirectX.Direct3D11.DeviceContext context;
        private DirectX.Direct3D11.D3DQuery queryBegin;
        private DirectX.Direct3D11.D3DQuery queryEnd;
        private IntPtr queryDataBegin;
        private IntPtr queryDataEnd;
        private object rendererLock;
        private ulong? beginCounter;
        private ulong? endCounter;
        private float? duration;
        private bool hasBegun;
        private D3D11ProfileFrameSection parentSection;

        public override float? Duration
        {
            get
            {
                if (!this.duration.HasValue)
                    this.duration = this.GetDuration();
                return this.duration;
            }
        }

        public D3D11ProfileSection(string sectionName, ProfileFrameSection parent)
            : base(sectionName)
        {
            this.device = ((D3D11ProfileFrameSection)parent).Device;
            this.context = ((D3D11ProfileFrameSection)parent).Context;
            this.rendererLock = ((D3D11ProfileFrameSection)parent).RendererLock;
            this.parentSection = (D3D11ProfileFrameSection)parent;
            this.Register((Renderer)this.device.Tag);
        }

        public override bool Begin()
        {
            if (this.hasBegun)
                return false;
            this.hasBegun = true;
            lock (this.rendererLock)
            {
                this.CreateQueries();
                this.context.End(this.queryBegin);
            }
            return true;
        }

        public override bool End()
        {
            if (!this.hasBegun)
                return false;
            this.hasBegun = false;
            lock (this.rendererLock)
                this.context.End(this.queryEnd);
            return true;
        }

        protected override void ResetState(ProfileFrameSection parent)
        {
            this.beginCounter = new ulong?();
            this.endCounter = new ulong?();
            this.duration = new float?();
            this.parentSection = (D3D11ProfileFrameSection)parent;
        }

        private void CreateQueries()
        {
            if (this.queryBegin == null)
            {
                this.device = ((D3D11Renderer)this.device.Tag).Device;
                this.context = ((D3D11Renderer)this.device.Tag).Context;
                this.queryBegin = this.device.CreateQuery(
                    new DirectX.Direct3D11.QueryDescription()
                {
                    Query = DirectX.Direct3D11.Query.Timestamp,
                    MiscellaneousQueryOptions = DirectX.Direct3D11.MiscellaneousQueryOptions.None
                });
                this.queryEnd = this.device.CreateQuery(
                    new DirectX.Direct3D11.QueryDescription()
                {
                    Query = DirectX.Direct3D11.Query.Timestamp,
                    MiscellaneousQueryOptions = DirectX.Direct3D11.MiscellaneousQueryOptions.None
                });
            }
            if (this.queryDataBegin == IntPtr.Zero)
                this.queryDataBegin = Marshal.AllocCoTaskMem((int)this.queryBegin.DataSize);
            if (!(this.queryDataEnd == IntPtr.Zero))
                return;
            this.queryDataEnd = Marshal.AllocCoTaskMem((int)this.queryEnd.DataSize);
        }

        private unsafe float? GetDuration()
        {
            if (!this.parentSection.Frequency.HasValue)
                return new float?();
            lock (this.rendererLock)
            {
                if (!this.beginCounter.HasValue && 
                    this.context.GetData(
                    this.queryBegin, this.queryDataBegin,
                    this.queryBegin.DataSize, 
                    DirectX.Direct3D11.AsyncGetDataOptions.DoNotFlush))
                    this.beginCounter = new ulong?(*(ulong*)this.queryDataBegin.ToPointer());
                if (!this.endCounter.HasValue)
                {
                    if (this.context.GetData(
                        this.queryEnd, this.queryDataEnd,
                        this.queryEnd.DataSize, 
                        DirectX.Direct3D11.AsyncGetDataOptions.DoNotFlush))
                        this.endCounter = new ulong?(*(ulong*)this.queryDataEnd.ToPointer());
                }
            }
            if (this.beginCounter.HasValue && this.endCounter.HasValue)
                return new float?((float)(this.endCounter.Value - this.beginCounter.Value) / (this.parentSection.Frequency.Value / 1000f));
            else
                return new float?();
        }

        internal override int GetEstimatedSystemMemoryUsage()
        {
            return (this.queryBegin != null ? (int)this.queryBegin.DataSize : 0) +
                (this.queryEnd != null ? (int)this.queryEnd.DataSize : 0);
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
            Marshal.FreeCoTaskMem(this.queryDataBegin);
            this.queryDataBegin = IntPtr.Zero;
            Marshal.FreeCoTaskMem(this.queryDataEnd);
            this.queryDataEnd = IntPtr.Zero;
        }

        private void ReleaseGraphicsResources()
        {
            if (this.queryBegin != null)
            {
                this.queryBegin.Dispose();
                this.queryBegin = null;
            }
            if (this.queryEnd != null)
            {
                this.queryEnd.Dispose();
                this.queryEnd = null;
            }
            this.device = null;
            this.context = null;
        }
    }
}
