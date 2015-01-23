using Microsoft.Data.Visualization.DirectX.Direct3D11;
using Microsoft.Data.Visualization.DirectX.Graphics;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11Texture : Texture
    {
        public static SampleDescription NoAntiAliasingQuality = new SampleDescription(1U, 0U);
        public static SampleDescription AntiAliasingQuality = new SampleDescription(4U, 0U);
        private static List<D3D11Texture> sharedTextures = new List<D3D11Texture>();
        private static object sharedLock = new object();
        private Dictionary<D3DDevice, D3DResource> textureMap = new Dictionary<D3DDevice, D3DResource>();
        private Dictionary<D3DDevice, ShaderResourceView> viewMap = new Dictionary<D3DDevice, ShaderResourceView>();
        private object textureUpdateLock = new object();
        private Texture2DDescription description = new Texture2DDescription();
        private bool releaseTextureData;
        private bool isSharedStatic;
        private IntPtr sharedHandle;
        private D3DDevice creationDevice;
        private int pixelSizeInBytes;

        internal override IntPtr NativeResource
        {
            get
            {
                return this.GetTexture(this.creationDevice).NativeInterface;
            }
        }

        internal D3D11Texture(D3D11Renderer renderer, Texture2D d3d11Texture)
            : base(new Image(IntPtr.Zero, (int)d3d11Texture.Description.Width, (int)d3d11Texture.Description.Height, PixelFormat.Unknown), (int)d3d11Texture.Description.MipLevels != 1, false, TextureUsage.RenderTarget)
        {
            if (d3d11Texture != null)
                this.textureMap.Add(renderer.Device, d3d11Texture);
            this.creationDevice = renderer.Device;
            this.pixelSizeInBytes = 0;
            this.Register((Renderer)renderer);
        }

        public D3D11Texture(Image textureData, bool mipMapping, bool allowView, TextureUsage usage, D3DDevice device)
            : base(textureData, mipMapping, allowView, usage)
        {
            this.Register((Renderer)device.Tag);
            bool flag1 = usage == TextureUsage.SharedRenderTarget || usage == TextureUsage.SharedStatic;
            int num = flag1 ? 1 : 0;
            this.description.Width = (uint)textureData.Width;
            this.description.Height = (uint)textureData.Height;
            this.description.MipLevels = mipMapping ? 0U : 1U;
            this.description.ArraySize = 1U;
            this.description.Format = D3D11Texture.GetFormat(textureData.Format, allowView);
            this.description.BindingOptions = BindingOptions.ShaderResource;
            this.description.CpuAccessOptions = CpuAccessOptions.None;
            bool flag2 = D3D11Texture.HardwareHasRequiredMSAASupport(this.description.Format, device);
            this.description.SampleDescription = usage == TextureUsage.MultiSampledRenderTarget && flag2 ? D3D11Texture.AntiAliasingQuality : D3D11Texture.NoAntiAliasingQuality;
            this.description.MiscellaneousResourceOptions = flag1 ? MiscellaneousResourceOptions.Shared : MiscellaneousResourceOptions.None;
            if (mipMapping)
            {
                this.description.BindingOptions |= BindingOptions.RenderTarget;
                this.description.MiscellaneousResourceOptions |= MiscellaneousResourceOptions.GenerateMips;
            }
            switch (this.Usage)
            {
                case TextureUsage.Dynamic:
                    this.description.Usage = DirectX.Direct3D11.Usage.Dynamic;
                    break;
                case TextureUsage.RenderTarget:
                case TextureUsage.MultiSampledRenderTarget:
                case TextureUsage.SharedRenderTarget:
                    this.description.Usage = DirectX.Direct3D11.Usage.Default;
                    this.description.BindingOptions |= BindingOptions.RenderTarget;
                    break;
                default:
                    this.description.Usage = DirectX.Direct3D11.Usage.Default;
                    break;
            }
            this.pixelSizeInBytes = textureData.PixelSizeInBytes;
            this.CreateTextureResource(device);
        }

        internal static bool HardwareHasRequiredMSAASupport(Format format, D3DDevice device)
        {
            return device.GetMultisampleQualityLevels(format, D3D11Texture.AntiAliasingQuality.Count) > D3D11Texture.AntiAliasingQuality.Quality;
        }

        private void CreateTextureResource(D3DDevice device)
        {
            SubresourceData[] subresourceDataArray = null;
            lock (this.textureUpdateLock)
            {
                if (this.TextureData.Data != IntPtr.Zero)
                    subresourceDataArray = new SubresourceData[1]
          {
            new SubresourceData()
            {
              SystemMemory = this.TextureData.Data,
              SystemMemoryPitch = (uint) (this.TextureData.Width * this.TextureData.PixelSizeInBytes),
              SystemMemorySlicePitch = 0U
            }
          };
                D3DResource texture;
                try
                {
                    if (this.TextureData.Height == 1)
                        texture = device.CreateTexture1D(
                            new Texture1DDescription()
                            {
                                Width = this.description.Width,
                                MipLevels = this.description.MipLevels,
                                ArraySize = 1U,
                                Format = this.description.Format,
                                BindingOptions = this.description.BindingOptions,
                                CpuAccessOptions = this.description.CpuAccessOptions,
                                MiscellaneousResourceOptions = this.description.MiscellaneousResourceOptions,
                                Usage = this.description.Usage
                            }, subresourceDataArray);
                    else
                        texture = device.CreateTexture2D(this.description, subresourceDataArray);
                }
                catch (Exception ex)
                {
                    D3DDeviceExtension.NotifyError(device, string.Format("A texture failed to be created. Texture size: {0}, {1}; Texture format: {2}", (object)this.description.Width, (object)this.description.Height, (object)this.description.Format), ex);
                    throw;
                }
                this.textureMap.Add(device, texture);
                this.creationDevice = device;
                if (this.releaseTextureData && this.TextureData != null)
                    this.TextureData.Dispose();
                this.TextureData = null;
            }
            if (this.Usage != TextureUsage.SharedStatic)
                return;
            lock (D3D11Texture.sharedLock)
                D3D11Texture.sharedTextures.Add(this);
            this.sharedHandle = this.textureMap[this.creationDevice].SharedHandle;
            this.isSharedStatic = true;
        }

        public override bool Update(Image textureData, bool disposeImage)
        {
            if (this.textureMap != null && textureData != null)
            {
                IntPtr data = textureData.Data;
                if (textureData.Width != this.Width || textureData.Height != this.Height || textureData.Format != this.Format)
                {
                    ((Renderer)this.creationDevice.Tag).Notify(string.Format("Invalid texture update: {0}x{1}:{2}", (object)textureData.Width, (object)textureData.Height, (object)textureData.Format));
                    if (disposeImage)
                        textureData.Dispose();
                    return false;
                }
                else
                {
                    lock (this.textureUpdateLock)
                    {
                        if (this.TextureData != null && this.releaseTextureData)
                            this.TextureData.Dispose();
                        this.TextureData = textureData;
                        this.releaseTextureData = disposeImage;
                    }
                    return true;
                }
            }
            else
            {
                if (disposeImage && textureData != null)
                {
                    IntPtr data = textureData.Data;
                    textureData.Dispose();
                    textureData = null;
                }
                return false;
            }
        }

        public override TextureView GetTextureView(PixelFormat viewFormat, Renderer renderer)
        {
            if (this.Disposed)
            {
                renderer.NotifyError("Attempting to use a disposed texture (GetTextureView)", (Exception)null);
                return (TextureView)null;
            }
            else
            {
                D3DDevice device = ((D3D11Renderer)renderer).Device;
                if (!this.textureMap.ContainsKey(device))
                    return null;
                ShaderResourceViewDescription description = new ShaderResourceViewDescription();
                description.ViewDimension = ShaderResourceViewDimension.Texture2D;
                description.Format = D3D11Texture.GetFormat(viewFormat, false);
                description.Texture2D = new Texture2DShaderResourceView()
                {
                    MipLevels = this.GetTexture(device).Description.MipLevels,
                    MostDetailedMip = this.GetTexture(device).Description.MipLevels - 1U
                };
                ShaderResourceView view;
                try
                {
                    lock (((D3D11Renderer)renderer).ContextLock)
                        view = ((D3D11Renderer)renderer).Device.CreateShaderResourceView(this.textureMap[((D3D11Renderer)renderer).Device], description);
                }
                catch (Exception ex)
                {
                    view = null;
                }
                if (view == null)
                    return null;
                return new D3D11TextureView(viewFormat, view, this);
            }
        }

        internal ShaderResourceView GetResourceView(D3DDevice device, DeviceContext context)
        {
            if (this.Disposed)
            {
                D3DDeviceExtension.NotifyError(device, "Attempting to use a disposed texture (GetResourceView)", (Exception)null);
                return null;
            }
            else
            {
                D3DResource d3Dresource = (D3DResource)null;
                if (this.textureMap.ContainsKey(device))
                    d3Dresource = this.textureMap[device];
                else if (this.Usage == TextureUsage.SharedStatic)
                {
                    for (int index = 0; index < 2; ++index)
                    {
                        try
                        {
                            d3Dresource = device.OpenSharedResource<D3DResource>(this.sharedHandle);
                            if (index > 0)
                            {
                                D3DDeviceExtension.NotifyMessage(device, "The visualization engine re-attempted to open a shared texture and the call succeeded");
                                break;
                            }
                            else
                                break;
                        }
                        catch (Exception ex)
                        {
                            D3DDeviceExtension.NotifyError(device, "A shared texture resource failed to open. This may be caused by either a bug in the video driver or by resource starvation (OOM). Attempt #" + (object)index, ex);
                            if (index == 1)
                                return null;
                        }
                    }
                    if (d3Dresource != null)
                        this.textureMap.Add(device, d3Dresource);
                }
                else
                {
                    D3DDeviceExtension.NotifyMessage(device, "The visualization engine is attempting to create a resource view for a texture that previously failed to be created.");
                    return null;
                }
                bool flag = false;
                lock (this.textureUpdateLock)
                {
                    if (this.textureMap != null)
                    {
                        if (this.TextureData != null)
                        {
                            context.UpdateSubresource(d3Dresource, 0U, this.TextureData.Data, (uint)(this.TextureData.Width * this.TextureData.PixelSizeInBytes), 0U);
                            if (this.MipMapping)
                                flag = true;
                            if (this.releaseTextureData)
                                this.TextureData.Dispose();
                            this.TextureData = null;
                        }
                    }
                }
                ShaderResourceView shaderResourceView = null;
                if (this.viewMap.ContainsKey(device))
                    shaderResourceView = this.viewMap[device];
                if (shaderResourceView == null && this.textureMap != null)
                {
                    if (this.AllowView)
                        shaderResourceView = device.CreateShaderResourceView(d3Dresource, new ShaderResourceViewDescription()
                        {
                            Format = D3D11Texture.GetFormat(this.Format, false),
                            ViewDimension = ShaderResourceViewDimension.Texture2D,
                            Texture2D = new Texture2DShaderResourceView()
                            {
                                MipLevels = this.GetTexture(device).Description.MipLevels,
                                MostDetailedMip = this.GetTexture(device).Description.MipLevels - 1U
                            }
                        });
                    else
                        shaderResourceView = device.CreateShaderResourceView(d3Dresource);
                    if (this.MipMapping)
                        flag = true;
                    this.viewMap[device] = shaderResourceView;
                }
                if (shaderResourceView != null && flag)
                    context.GenerateMips(shaderResourceView);
                return shaderResourceView;
            }
        }

        internal Texture2D GetTexture(D3DDevice device)
        {
            if (this.textureMap.ContainsKey(device))
                return this.textureMap[device] as Texture2D;
            return null;
        }

        internal static Format GetFormat(PixelFormat format, bool typeless)
        {
            switch (format)
            {
                case PixelFormat.Rgba32Bpp:
                    return !typeless ? 
                        DirectX.Graphics.Format.R8G8B8A8UNorm : 
                        DirectX.Graphics.Format.R8G8B8A8Typeless;
                case PixelFormat.Bgra32Bpp:
                    return !typeless ? 
                        DirectX.Graphics.Format.B8G8R8A8UNorm : 
                        DirectX.Graphics.Format.B8G8R8A8Typeless;
                case PixelFormat.Bgr32Bpp:
                    return !typeless ? 
                        DirectX.Graphics.Format.B8G8R8X8UNorm : 
                        DirectX.Graphics.Format.B8G8R8X8Typeless;
                case PixelFormat.Gray8Bpp:
                case PixelFormat.Alpha8Bpp:
                    return !typeless ? 
                        DirectX.Graphics.Format.A8UNorm : 
                        DirectX.Graphics.Format.R8Typeless;
                case PixelFormat.R16Unorm16bpp:
                    return !typeless ? 
                        DirectX.Graphics.Format.R16UNorm : 
                        DirectX.Graphics.Format.R16Typeless;
                case PixelFormat.Rg16Unorm32Bpp:
                    return !typeless ? 
                        DirectX.Graphics.Format.R16G16UNorm : 
                        DirectX.Graphics.Format.R16G16Typeless;
                case PixelFormat.Float16Bpp:
                    return !typeless ?
                        DirectX.Graphics.Format.R16Float : 
                        DirectX.Graphics.Format.R16Typeless;
                case PixelFormat.Float32Bpp:
                    return !typeless ? 
                        DirectX.Graphics.Format.R32Float : 
                        DirectX.Graphics.Format.R32Typeless;
                default:
                    return DirectX.Graphics.Format.Unknown;
            }
        }

        internal static void ReleaseResources(D3DDevice device)
        {
            if (device == null)
                return;
            lock (D3D11Texture.sharedLock)
            {
                foreach (D3D11Texture texture in D3D11Texture.sharedTextures)
                {
                    if (texture.textureMap.ContainsKey(device))
                    {
                        D3DResource t = texture.textureMap[device];
                        if (t != null)
                            t.Dispose();
                        texture.textureMap.Remove(device);
                    }
                    if (texture.viewMap.ContainsKey(device))
                    {
                        ShaderResourceView v = texture.viewMap[device];
                        if (v != null)
                            v.Dispose();
                        texture.viewMap.Remove(device);
                    }
                    ((Renderer)device.Tag).Resources.UnregisterResource(texture);
                }
            }
        }

        internal override int GetEstimatedSystemMemoryUsage()
        {
            if (this.TextureData != null)
                return this.TextureData.Width * this.TextureData.Height * this.TextureData.PixelSizeInBytes;
            return 0;
        }

        internal override int GetEstimatedVideoMemoryUsage()
        {
            if (this.textureMap.Count > 0)
                return this.Width * this.Height * this.pixelSizeInBytes;
            return 0;
        }

        protected override bool Reset()
        {
            this.ReleaseGraphicsResources();
            D3D11Renderer d3D11Renderer = (D3D11Renderer)this.creationDevice.Tag;
            lock (this.textureUpdateLock)
            {
                if (this.TextureData == null)
                {
                    this.TextureData = new Image(IntPtr.Zero, this.Width, this.Height, this.Format);
                    this.releaseTextureData = true;
                }
            }
            this.CreateTextureResource(d3D11Renderer.Device);
            if ((this.description.BindingOptions & BindingOptions.RenderTarget) != BindingOptions.RenderTarget)
                return this.TextureData != null;
            else
                return true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            if (this.releaseTextureData && this.TextureData != null)
            {
                this.TextureData.Dispose();
                this.TextureData = null;
            }
            this.ReleaseGraphicsResources();
        }

        private void ReleaseGraphicsResourcesWorker()
        {
            foreach (D3DResource texture in this.textureMap.Values)
            {
                if (texture != null)
                    texture.Dispose();
            }
            this.textureMap.Clear();
            foreach (ShaderResourceView view in this.viewMap.Values)
            {
                if (view != null)
                    view.Dispose();
            }
            this.viewMap.Clear();
        }

        private void ReleaseGraphicsResources()
        {
            if (this.isSharedStatic)
            {
                lock (D3D11Texture.sharedLock)
                {
                    this.ReleaseGraphicsResourcesWorker();
                    D3D11Texture.sharedTextures.Remove(this);
                }
            }
            else
                this.ReleaseGraphicsResourcesWorker();
        }
    }
}
