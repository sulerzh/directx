using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class ReadableBitmap : GraphicsResource
    {
        public int Width { get; private set; }

        public int Height { get; private set; }

        public PixelFormat Format { get; private set; }

        protected ReadableBitmap(int width, int height, PixelFormat format)
        {
            this.Width = width;
            this.Height = height;
            this.Format = format;
        }

        public static ReadableBitmap Create(int bitmapWidth, int bitmapHeight, PixelFormat format)
        {
            return new D3D11ReadableBitmap(bitmapWidth, bitmapHeight, format);
        }

        public abstract void ResetBuffer();

        public abstract IntPtr LockData(out int sourceFrame, out int pitch);

        public abstract IntPtr LockDataImmediate(out int pitch);

        public abstract void Unlock();
    }
}
