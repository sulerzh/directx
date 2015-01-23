using Microsoft.Data.Visualization.DirectX.Direct3D11;
using Microsoft.Data.Visualization.DirectX.Graphics;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11RenderTarget : RenderTarget
    {
        private IntPtr queryData = IntPtr.Zero;
        private RenderTargetView renderTargetView;
        private DepthStencilView depthStencilView;
        private DeviceContext deviceContext;
        private Viewport viewport;
        private D3DQuery readyQuery;

        public D3D11RenderTarget(Texture renderTargetTexture, RenderTargetDepthStencilMode depthStencilMode)
            : base(renderTargetTexture, depthStencilMode)
        {
        }

        internal RenderTargetView GetRenderTargetView(D3DDevice device, DeviceContext context)
        {
            if (this.renderTargetView == null)
            {
                this.Register((Renderer)device.Tag);
                this.renderTargetView = device.CreateRenderTargetView(((D3D11Texture)this.RenderTargetTexture).GetTexture(device));
            }
            return this.renderTargetView;
        }

        internal DepthStencilView GetDepthStencilView(D3DDevice device, DeviceContext context)
        {
            if ((this.DepthStencilMode == RenderTargetDepthStencilMode.Enabled || this.DepthStencilMode == RenderTargetDepthStencilMode.FloatDepthEnabled) && this.depthStencilView == null)
            {
                using (Texture2D texture2D = device.CreateTexture2D(new Texture2DDescription()
                {
                    ArraySize = 1U,
                    BindingOptions = BindingOptions.DepthStencil,
                    CpuAccessOptions = CpuAccessOptions.None,
                    Format =
                        this.DepthStencilMode == RenderTargetDepthStencilMode.FloatDepthEnabled
                            ? Format.D32Float
                            : Format.D24UNormS8UInt,
                    Width = (uint)this.RenderTargetTexture.Width,
                    Height = (uint)this.RenderTargetTexture.Height,
                    MipLevels = 1U,
                    SampleDescription =
                        ((D3D11Texture)this.RenderTargetTexture).GetTexture(device).Description.SampleDescription
                }))
                {
                    this.depthStencilView = device.CreateDepthStencilView(texture2D);
                }
            }
            return this.depthStencilView;
        }

        internal Viewport GetViewport()
        {
            if (this.viewport.Width == 0f)
            {
                this.viewport = new Viewport()
                {
                    Width = this.RenderTargetTexture.Width,
                    Height = this.RenderTargetTexture.Height,
                    MinDepth = 0.0f,
                    MaxDepth = 1f,
                    TopLeftX = 0.0f,
                    TopLeftY = 0.0f
                };
            }
            return this.viewport;
        }

        internal void SetEndFrame(D3DDevice device, DeviceContext context)
        {
            if (this.readyQuery == null)
            {
                this.Register((Renderer)device.Tag);
                this.readyQuery = device.CreateQuery(new QueryDescription()
                {
                    Query = Query.Event,
                    MiscellaneousQueryOptions = MiscellaneousQueryOptions.None
                });
                this.deviceContext = context;
            }
            context.End(this.readyQuery);
        }

        public override unsafe bool IsReady()
        {
            if (this.readyQuery == null)
                return true;
            if (this.queryData == IntPtr.Zero)
                this.queryData = Marshal.AllocCoTaskMem(4);
            if (this.deviceContext.GetData(this.readyQuery, this.queryData, 4U, AsyncGetDataOptions.DoNotFlush))
                return *(int*)this.queryData.ToPointer() != 0;
            return false;
        }

        internal override int GetEstimatedSystemMemoryUsage()
        {
            if (this.readyQuery == null)
                return 0;
            else
                return (int)this.readyQuery.DataSize;
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
            if (this.queryData != IntPtr.Zero)
                Marshal.FreeCoTaskMem(this.queryData);
            if (!disposing)
                return;
            this.ReleaseGraphicsResources();
        }

        private void ReleaseGraphicsResources()
        {
            if (this.renderTargetView != null)
            {
                this.renderTargetView.Dispose();
                this.renderTargetView = null;
            }
            if (this.depthStencilView != null)
            {
                this.depthStencilView.Dispose();
                this.depthStencilView = null;
            }
            if (this.readyQuery == null)
                return;
            this.readyQuery.Dispose();
            this.readyQuery = null;
        }
    }
}
