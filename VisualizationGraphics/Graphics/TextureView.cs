namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class TextureView : GraphicsResource
    {
        public PixelFormat Format { get; private set; }

        public Texture Texture { get; private set; }

        internal TextureView(PixelFormat format, Texture texture)
        {
            this.Format = format;
            this.Texture = texture;
        }
    }
}
