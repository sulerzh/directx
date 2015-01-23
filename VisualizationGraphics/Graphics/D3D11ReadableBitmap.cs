using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11ReadableBitmap : ReadableBitmap
    {
        private SortedDictionary<int, DirectX.Direct3D11.Texture2D> textureBuffer =
            new SortedDictionary<int, DirectX.Direct3D11.Texture2D>(new BiggerComesFirst());
        private Queue<DirectX.Direct3D11.Texture2D> availableTextures = new Queue<DirectX.Direct3D11.Texture2D>();
        private D3D11Renderer renderer;
        private DirectX.Direct3D11.Texture2D lockedResource;

        public D3D11ReadableBitmap(int width, int height, PixelFormat format)
            : base(width, height, format)
        {
        }

        public override void ResetBuffer()
        {
            foreach (int index in this.textureBuffer.Keys)
                this.availableTextures.Enqueue(this.textureBuffer[index]);
            this.textureBuffer.Clear();
        }

        public override IntPtr LockData(out int sourceFrame, out int pitch)
        {
            return this.LockData(false, out sourceFrame, out pitch);
        }

        public override IntPtr LockDataImmediate(out int pitch)
        {
            int sourceFrame;
            return this.LockData(true, out sourceFrame, out pitch);
        }

        public IntPtr LockData(bool waitUntilAvailable, out int sourceFrame, out int pitch)
        {
            sourceFrame = 0;
            pitch = 0;
            if (this.renderer == null)
                return IntPtr.Zero;
            List<int> list = new List<int>();
            IntPtr reslut = IntPtr.Zero;
            try
            {
                foreach (int index in this.textureBuffer.Keys)
                {
                    DirectX.Direct3D11.Texture2D texture2D = this.textureBuffer[index];
                    lock (this.renderer.ContextLock)
                    {
                        DirectX.Direct3D11.MappedSubresource mapOpsion =
                            this.renderer.Context.Map(
                                texture2D, 0U,
                                DirectX.Direct3D11.Map.Read, waitUntilAvailable
                                    ? DirectX.Direct3D11.MapOptions.None
                                    : DirectX.Direct3D11.MapOptions.DoNotWait);
                        if (mapOpsion.Data != IntPtr.Zero)
                        {
                            if (reslut != IntPtr.Zero)
                            {
                                list.Add(index);
                                this.renderer.Context.Unmap(texture2D, 0U);
                            }
                            else
                            {
                                sourceFrame = index;
                                pitch = (int)mapOpsion.RowPitch;
                                reslut = mapOpsion.Data;
                                this.lockedResource = texture2D;
                            }
                        }
                    }
                }
            }
            catch (DirectX.DirectXException ex)
            {
                this.renderer.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.renderer.CheckDeviceRemoved(ex);
            }
            for (int index = 0; index < list.Count; ++index)
            {
                this.availableTextures.Enqueue(this.textureBuffer[list[index]]);
                this.textureBuffer.Remove(list[index]);
            }
            return reslut;
        }

        public override void Unlock()
        {
            try
            {
                if (this.lockedResource == null || this.renderer == null)
                    return;
                lock (this.renderer.ContextLock)
                    this.renderer.Context.Unmap(this.lockedResource, 0U);
                this.lockedResource = null;
            }
            catch (DirectX.DirectXException ex)
            {
                this.renderer.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.renderer.CheckDeviceRemoved(ex);
            }
        }

        internal void CopyFromRenderTarget(RenderTarget source, Rect sourceArea, DirectX.Direct3D11.D3DDevice device, D3D11Renderer d3d11Renderer)
        {
            if (this.renderer == null)
                this.Register(d3d11Renderer);
            this.renderer = d3d11Renderer;
            DirectX.Direct3D11.Box sourceBox = new DirectX.Direct3D11.Box()
            {
                Left = (uint)Math.Max(0, sourceArea.X),
                Right = (uint)Math.Min(sourceArea.X + sourceArea.Width, source.RenderTargetTexture.Width),
                Top = (uint)Math.Max(0, sourceArea.Y),
                Bottom = (uint)Math.Min(sourceArea.Y + sourceArea.Height, source.RenderTargetTexture.Height),
                Front = 0U,
                Back = 1U
            };
            lock (this.renderer.ContextLock)
            {
                DirectX.Direct3D11.Texture2D texture = ((D3D11Texture)source.RenderTargetTexture).GetTexture(device);
                this.renderer.Context.CopySubresourceRegion(
                    this.GetTexture(device), 0U, 0U, 0U, 0U, texture, 0U, sourceBox);
            }
        }

        private DirectX.Direct3D11.Texture2D GetTexture(DirectX.Direct3D11.D3DDevice device)
        {
            if (this.textureBuffer.ContainsKey(this.renderer.FrameCount))
                return this.textureBuffer[this.renderer.FrameCount];
            while (this.textureBuffer.Count > 2)
            {
                int sourceFrame;
                int pitch;
                if (this.LockData(true, out sourceFrame, out pitch) != IntPtr.Zero)
                    this.Unlock();
            }
            DirectX.Direct3D11.Texture2D texture2D = this.availableTextures.Count <= 0 ? 
                this.CreateStagingTexture(device) :
                this.availableTextures.Dequeue();
            this.textureBuffer.Add(this.renderer.FrameCount, texture2D);
            return texture2D;
        }

        private DirectX.Direct3D11.Texture2D CreateStagingTexture(DirectX.Direct3D11.D3DDevice device)
        {
            return device.CreateTexture2D(
                new DirectX.Direct3D11.Texture2DDescription()
            {
                Width = (uint)this.Width,
                Height = (uint)this.Height,
                MipLevels = 1U,
                ArraySize = 1U,
                Format = D3D11Texture.GetFormat(this.Format, false),
                SampleDescription = new DirectX.Graphics.SampleDescription(1U, 0U),
                BindingOptions = DirectX.Direct3D11.BindingOptions.None,
                CpuAccessOptions = DirectX.Direct3D11.CpuAccessOptions.Read,
                MiscellaneousResourceOptions = DirectX.Direct3D11.MiscellaneousResourceOptions.None,
                Usage = DirectX.Direct3D11.Usage.Staging
            }, null);
        }

        internal override int GetEstimatedSystemMemoryUsage()
        {
            int texNum = this.availableTextures.Count + this.textureBuffer.Count;
            Image image = new Image(IntPtr.Zero, 0, 0, this.Format);
            int pixelSizeInBytes = image.PixelSizeInBytes;
            image.Dispose();
            return this.Width * this.Height * pixelSizeInBytes * texNum;
        }

        internal override int GetEstimatedVideoMemoryUsage()
        {
            return 0;
        }

        protected override bool Reset()
        {
            this.ReleaseGraphicsResources();
            return true;
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
            foreach (int index in this.textureBuffer.Keys)
            {
                this.textureBuffer[index].Dispose();
            }
            this.textureBuffer.Clear();
            foreach (var texture in this.availableTextures)
            {
                texture.Dispose();
            }
            this.availableTextures.Clear();
            this.renderer = null;
            this.lockedResource = null;
        }

        private class BiggerComesFirst : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                if (x > y)
                    return -1;
                return x >= y ? 0 : 1;
            }
        }
    }
}
