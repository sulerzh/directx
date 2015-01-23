using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class Texture : GraphicsResource
    {
        public bool MipMapping { get; private set; }

        public TextureUsage Usage { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public PixelFormat Format { get; private set; }

        public bool AllowView { get; private set; }

        internal abstract IntPtr NativeResource { get; }

        protected Image TextureData { get; set; }

        internal Texture(Image textureData, bool mipMapping, bool allowView, TextureUsage usage)
        {
            this.TextureData = textureData;
            this.MipMapping = mipMapping;
            this.AllowView = allowView;
            this.Usage = usage;
            this.Width = textureData.Width;
            this.Height = textureData.Height;
            this.Format = textureData.Format;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!(disposing & this.TextureData != null))
                return;
            this.TextureData.Dispose();
            this.TextureData = null;
        }

        public abstract bool Update(Image textureData, bool disposeImage);

        public abstract TextureView GetTextureView(PixelFormat viewFormat, Renderer renderer);
    }
}
