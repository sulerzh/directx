using Microsoft.Data.Visualization.DirectX.Direct3D11;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11TextureView : TextureView
    {
        private ShaderResourceView resourceView;

        public D3D11TextureView(PixelFormat format, ShaderResourceView view, D3D11Texture texture)
            : base(format, texture)
        {
            this.resourceView = view;
            this.Register((Renderer)view.Device.Tag);
        }

        internal ShaderResourceView GetResourceView(D3DDevice device, DeviceContext context)
        {
            if (this.resourceView != null && this.Texture.MipMapping)
                context.GenerateMips(this.resourceView);
            return this.resourceView;
        }

        internal override int GetEstimatedSystemMemoryUsage()
        {
            return 0;
        }

        internal override int GetEstimatedVideoMemoryUsage()
        {
            return 0;
        }

        protected override bool Reset()
        {
            this.ReleaseGraphicsResources();
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            this.ReleaseGraphicsResources();
        }

        private void ReleaseGraphicsResources()
        {
            if (this.resourceView == null)
                return;
            this.resourceView.Dispose();
            this.resourceView = null;
        }
    }
}
