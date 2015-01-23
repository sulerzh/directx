using Microsoft.Data.Visualization.DirectX;
using Microsoft.Data.Visualization.DirectX.Direct3D;
using Microsoft.Data.Visualization.DirectX.Direct3D11;
using Microsoft.Data.Visualization.DirectX.Graphics;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11Renderer : Renderer
    {
        private object renderTargetLock = new object();
        private object flipLock = new object();
        private object d3dLock = new object();
        private IntPtr windowHandle = IntPtr.Zero;
        private FrameProfiler profiler = new FrameProfiler(false);
        private ResourceManager resourceManager = new ResourceManager();
        private const int MaxDeviceResetAttempts = 30;
        private const int IntervalBetweenDeviceResetAttempts = 2000;
        private D3D11RendererCore core;
        private int currentFrameSinceAppStarted;
        private bool msaaEnabled;
        private BufferedRenderTarget offscreenRenderTarget;
        private bool resetGraphicsDevice;
        private SwapChain swapChain;
        private D3D11RenderTarget swapChainRenderTarget;
        private bool forceOffscreenTarget;
        private bool isOfflineVideoRendering;

        public override bool Initialized
        {
            get
            {
                return this.core != null;
            }
        }

        public override bool Msaa
        {
            get
            {
                return this.msaaEnabled;
            }
            set
            {
                SwapChain swapChain = this.swapChain;
                if (value == this.msaaEnabled || value && !D3D11Texture.HardwareHasRequiredMSAASupport(Format.R8G8B8A8UNorm, this.core.Device))
                    return;
                this.msaaEnabled = value;
                lock (this.renderTargetLock)
                {
                    this.offscreenRenderTarget.Dispose();
                    this.CreateOffscreenRenderTarget(this.FrameWidth, this.FrameHeight);
                }
            }
        }

        public override int FrameCount
        {
            get
            {
                return this.currentFrameSinceAppStarted;
            }
        }

        public override int FrameLatency
        {
            get
            {
                return BufferedRenderTarget.BufferLatency;
            }
        }

        public override int FrameWidth
        {
            get
            {
                return this.core.BackBufferRenderTarget.RenderTargetTexture.Width;
            }
        }

        public override int FrameHeight
        {
            get
            {
                return this.core.BackBufferRenderTarget.RenderTargetTexture.Height;
            }
        }

        public override string CorruptionErrorMessage
        {
            get
            {
                ErrorCode deviceRemovedReason = this.core.Device.DeviceRemovedReason;
                if (deviceRemovedReason == ErrorCode.Success)
                    return null;
                else
                    return deviceRemovedReason.ToString();
            }
        }

        internal DirectX.Direct3D11.DeviceContext Context
        {
            get
            {
                return this.core.Context;
            }
        }

        internal object ContextLock
        {
            get
            {
                return this.d3dLock;
            }
        }

        internal DirectX.Direct3D11.D3DDevice Device
        {
            get
            {
                return this.core.Device;
            }
        }

        public override ApplicationRenderParameters AppParameters
        {
            get
            {
                if (this.core == null)
                    return null;
                else
                    return this.core.AppParameters;
            }
        }

        public override FrameRenderParameters FrameParameters
        {
            get
            {
                if (this.core == null)
                    return null;
                return this.core.FrameParameters;
            }
        }

        public override FrameProfiler Profiler
        {
            get
            {
                return this.profiler;
            }
        }

        public override ResourceManager Resources
        {
            get
            {
                return this.resourceManager;
            }
        }

        public override bool Initialize(IntPtr renderWindowHandle, int windowWidth, int windowHeight, bool msaa, bool forceOffscreenComposition)
        {
            return this.InitializeDevice(renderWindowHandle, windowWidth, windowHeight, msaa, forceOffscreenComposition, false);
        }

        public override bool Initialize(int renderTargetWidth, int renderTargetHeight, bool msaa, bool isForOffline)
        {
            return this.InitializeDevice(IntPtr.Zero, renderTargetWidth, renderTargetHeight, msaa, false, isForOffline);
        }

        private bool InitializeDevice(IntPtr renderWindowHandle, int renderTargetWidth, int renderTargetHeight, bool msaa, bool forceOffscreenComposition, bool isForOfflineVideo)
        {
            this.isOfflineVideoRendering = isForOfflineVideo;
            try
            {
                FeatureLevel[] featureLevels = new FeatureLevel[] { FeatureLevel.Ten };
                DirectX.Direct3D11.CreateDeviceOptions options = DirectX.Direct3D11.CreateDeviceOptions.None;
                if (msaa)
                {
                    DirectX.Direct3D11.D3DDevice device = DirectX.Direct3D11.D3DDevice.CreateDevice(null, DirectX.Direct3D11.DriverType.Hardware, (string)null, options, featureLevels);
                    this.msaaEnabled = D3D11Texture.HardwareHasRequiredMSAASupport(Format.R8G8B8A8UNorm, device);
                    device.Dispose();
                    this.Notify("MSAA support check successful. Now initializing graphics device...");
                }
                if (renderWindowHandle != IntPtr.Zero)
                {
                    this.Notify("Initializing visualization engine with the the following creation options: " + options.ToString());
                    DirectX.Direct3D11.D3DDevice deviceAndSwapChain = 
                        DirectX.Direct3D11.D3DDevice.CreateDeviceAndSwapChain(
                        null, DirectX.Direct3D11.DriverType.Hardware, null, options, featureLevels,
                        new SwapChainDescription()
                        {
                            BufferCount = 1U,
                            BufferDescription =
                                new ModeDescription((uint) renderTargetWidth, (uint) renderTargetHeight,
                                    Format.R8G8B8A8UNorm, new Rational(60U, 1U)),
                            BufferUsage = UsageOptions.RenderTargetOutput,
                            OutputWindowHandle = renderWindowHandle,
                            SampleDescription =
                                !this.msaaEnabled || forceOffscreenComposition
                                    ? D3D11Texture.NoAntiAliasingQuality
                                    : D3D11Texture.AntiAliasingQuality,
                            Windowed = true
                        });
                    deviceAndSwapChain.Tag = this;
                    this.UpdateRendererCore(deviceAndSwapChain);
                    this.forceOffscreenTarget = forceOffscreenComposition;
                    this.swapChain = deviceAndSwapChain.SwapChain;
                    this.CreateSwapChainRenderTarget(renderTargetWidth, renderTargetHeight);
                    this.windowHandle = renderWindowHandle;
                    this.Notify("Visualization Engine initialization successful.");
                }
                else
                {
                    DirectX.Direct3D11.D3DDevice device = DirectX.Direct3D11.D3DDevice.CreateDevice(null, DirectX.Direct3D11.DriverType.Hardware, null, options, featureLevels);
                    device.Tag = this;
                    this.UpdateRendererCore(device);
                }
                if ((this.swapChain == null || forceOffscreenComposition) && this.offscreenRenderTarget == null)
                    this.CreateOffscreenRenderTarget(renderTargetWidth, renderTargetHeight);
            }
            catch (Exception ex)
            {
                this.NotifyError("The engine initialization failed.", ex);
                return false;
            }
            return true;
        }

        private void UpdateRendererCore(DirectX.Direct3D11.D3DDevice device)
        {
            if (this.core != null)
                this.core.UpdateDevice(device, device.ImmediateContext);
            else
                this.core = new D3D11RendererCore(device, device.ImmediateContext);
        }

        private void CreateSwapChainRenderTarget(int renderTargetWidth, int renderTargetHeight)
        {
            if (this.swapChainRenderTarget != null)
                this.swapChainRenderTarget.Dispose();
            this.swapChainRenderTarget = new D3D11RenderTarget(new D3D11Texture(this, this.swapChain.GetBuffer<DirectX.Direct3D11.Texture2D>(0U)), RenderTargetDepthStencilMode.Enabled);
            if (this.forceOffscreenTarget)
                return;
            this.core.SetRenderTarget(this.swapChainRenderTarget);
        }

        private void CreateOffscreenRenderTarget(int renderTargetWidth, int renderTargetHeight)
        {
            this.offscreenRenderTarget = new BufferedRenderTarget(renderTargetWidth, renderTargetHeight, PixelFormat.Bgra32Bpp, (Renderer)this, this.isOfflineVideoRendering);
            this.core.BackBufferRenderTarget = (D3D11RenderTarget)this.offscreenRenderTarget.TargetBuffer;
            lock (this.d3dLock)
                this.core.SetRenderTarget(this.core.BackBufferRenderTarget);
        }

        public override void UpdateRenderTargetSize(int renderTargetWidth, int renderTargetHeight)
        {
            try
            {
                DirectX.Direct3D11.Viewport viewport = this.core.BackBufferRenderTarget.GetViewport();
                if (renderTargetWidth == (int)viewport.Width && renderTargetHeight == (int)viewport.Height || (renderTargetWidth <= 0 || renderTargetHeight <= 0))
                    return;
                if (this.swapChain != null)
                {
                    if (this.swapChainRenderTarget.RenderTargetTexture != null)
                        this.swapChainRenderTarget.RenderTargetTexture.Dispose();
                    this.swapChainRenderTarget.Dispose();
                    this.swapChain.ResizeBuffers(1U, (uint)renderTargetWidth, (uint)renderTargetHeight, Format.R8G8B8A8UNorm, SwapChainOptions.None);
                    this.CreateSwapChainRenderTarget(renderTargetWidth, renderTargetHeight);
                }
                if (this.offscreenRenderTarget == null)
                    return;
                lock (this.renderTargetLock)
                {
                    this.offscreenRenderTarget.Dispose();
                    this.CreateOffscreenRenderTarget(renderTargetWidth, renderTargetHeight);
                }
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override bool TryGetRenderTargetFrame(out RenderTarget frame)
        {
            if (this.offscreenRenderTarget == null)
            {
                frame = null;
                return false;
            }
            else
            {
                Monitor.Enter(this.renderTargetLock);
                try
                {
                    return this.offscreenRenderTarget.TryGetReadyBuffer(out frame);
                }
                catch (DirectXException ex)
                {
                    this.CheckDeviceRemoved(ex);
                }
                catch (COMException ex)
                {
                    this.CheckDeviceRemoved(ex);
                }
                frame = null;
                return false;
            }
        }

        public override void ReleaseRenderTargetFrame()
        {
            if (this.offscreenRenderTarget == null)
                return;
            try
            {
                Monitor.Exit(this.renderTargetLock);
            }
            catch (SynchronizationLockException ex)
            {
                this.Notify("ReleaseRenderTargetFrame() was called without a previous call to TryGetRenderTargetFrame()");
            }
        }

        public override void SetStreamBuffer(StreamBuffer buffer)
        {
            lock (this.d3dLock)
                this.SetStreamBufferNoLock(buffer);
        }

        public override void SetStreamBufferNoLock(StreamBuffer buffer)
        {
            try
            {
                this.core.SetStreamBuffer(buffer);
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void CopyRenderTargetData(RenderTarget source, Rect sourceArea, ReadableBitmap destination)
        {
            try
            {
                lock (this.d3dLock)
                    ((D3D11ReadableBitmap)destination).CopyFromRenderTarget(source, sourceArea, this.Device, this);
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void CopyRenderTargetData(RenderTarget source, RenderTarget destination, bool flush)
        {
            if (source != null)
            {
                if (destination != null)
                {
                    try
                    {
                        lock (this.d3dLock)
                        {
                            DirectX.Direct3D11.Texture2D srcTexture = ((D3D11Texture)source.RenderTargetTexture).GetTexture(this.Device);
                            DirectX.Direct3D11.Texture2D dstTexture = ((D3D11Texture)destination.RenderTargetTexture).GetTexture(this.Device);
                            if (srcTexture == null || dstTexture == null)
                            {
                                this.Notify(string.Format("Error copying render target data: {0} --> {1}", srcTexture, dstTexture));
                                return;
                            }
                            else
                            {
                                if (srcTexture.Description.SampleDescription.Count > 1U)
                                {
                                    if (dstTexture.Description.SampleDescription.Count > 1U)
                                        return;
                                    this.Context.ResolveSubresource(dstTexture, 0U, srcTexture, 0U, srcTexture.Description.Format);
                                }
                                else
                                    this.Context.CopySubresourceRegion(dstTexture, 0U, 0U, 0U, 0U, srcTexture, 0U);
                                if (!flush)
                                    return;
                                this.Context.Flush();
                                return;
                            }
                        }
                    }
                    catch (DirectXException ex)
                    {
                        this.CheckDeviceRemoved(ex);
                        return;
                    }
                    catch (COMException ex)
                    {
                        this.CheckDeviceRemoved(ex);
                        return;
                    }
                }
            }
            this.Notify(string.Format("Error copying render target data (invalid input): {0} --> {1}", source, destination));
        }

        public override bool BeginFrame(Color4F? clearColor)
        {
            if (this.resetGraphicsDevice)
            {
                this.resetGraphicsDevice = false;
                this.HandleDeviceRemoved();
            }
            try
            {
                this.profiler.BeginFrame((Renderer)this, this.currentFrameSinceAppStarted);
                this.profiler.BeginSection("[Renderer] BeginFrame");
                lock (this.d3dLock)
                {
                    for (int slot = 0; slot < 8; ++slot)
                        this.SetTexture(slot, null);
                    if (this.offscreenRenderTarget != null)
                    {
                        if (!this.offscreenRenderTarget.Flip())
                        {
                            this.profiler.EndSection();
                            this.profiler.DiscardFrame();
                            return false;
                        }
                        else
                        {
                            this.core.BackBufferRenderTarget = (D3D11RenderTarget)this.offscreenRenderTarget.TargetBuffer;
                            this.core.SetRenderTarget(this.core.BackBufferRenderTarget);
                        }
                    }
                    ++this.currentFrameSinceAppStarted;
                    this.BeginRenderTargetFrame(this.core.BackBufferRenderTarget, clearColor);
                }
                this.profiler.EndSection();
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            return true;
        }

        public override void EndFrame()
        {
            try
            {
                lock (this.d3dLock)
                {
                    if (this.offscreenRenderTarget != null)
                        this.offscreenRenderTarget.EndFrame();
                    this.SetStreamBuffer(null);
                    if (this.swapChain != null)
                    {
                        if (this.forceOffscreenTarget)
                            this.core.SetRenderTarget(this.swapChainRenderTarget);
                        this.RaiseOnPresent(this.offscreenRenderTarget != null ? this.offscreenRenderTarget.PeekLatestFrame() : (RenderTarget)null);
                        this.swapChain.Present(0U, PresentOptions.None);
                        this.profiler.EndFrame();
                        if (!this.forceOffscreenTarget)
                            return;
                        this.core.SetRenderTarget(this.core.BackBufferRenderTarget);
                    }
                    else
                    {
                        this.profiler.EndFrame();
                        this.core.Context.Flush();
                    }
                }
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void BeginRenderTargetFrame(RenderTarget[] renderTargets, Color4F? clearColor, bool attachMainRenderTarget)
        {
            try
            {
                lock (this.d3dLock)
                    this.core.BeginRenderTargetFrame(renderTargets, clearColor, attachMainRenderTarget);
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void BeginRenderTargetFrame(RenderTarget renderTarget, Color4F? clearColor)
        {
            this.BeginRenderTargetFrame(new RenderTarget[] { renderTarget }, clearColor, false);
        }

        public override void EndRenderTargetFrame()
        {
            try
            {
                lock (this.d3dLock)
                    this.core.EndRenderTargetFrame();
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void ClearCurrentDepthTarget(float clearDepth)
        {
            try
            {
                lock (this.d3dLock)
                    this.core.ClearCurrentDepthTarget(clearDepth);
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void ClearCurrentStencilTarget(int clearStencil)
        {
            try
            {
                lock (this.d3dLock)
                    this.core.ClearCurrentStencilTarget(clearStencil);
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void SetRasterizerState(RasterizerState state)
        {
            lock (this.d3dLock)
                this.SetRasterizerStateNoLock(state);
        }

        public override void SetRasterizerStateNoLock(RasterizerState state)
        {
            try
            {
                lock (this.d3dLock)
                    this.core.SetRasterizerState(state);
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void SetDepthStencilState(DepthStencilState state)
        {
            lock (this.d3dLock)
                this.SetDepthStencilStateNoLock(state);
        }

        public override void SetDepthStencilStateNoLock(DepthStencilState state)
        {
            try
            {
                this.core.SetDepthStencilState(state);
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void SetBlendState(BlendState state)
        {
            lock (this.d3dLock)
                this.SetBlendStateNoLock(state);
        }

        public override void SetBlendStateNoLock(BlendState state)
        {
            try
            {
                this.core.SetBlendState(state);
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override Texture CreateTexture(Image textureData, bool mipMapping, bool allowView, TextureUsage usage)
        {
            D3D11Texture d3D11Texture = null;
            try
            {
                d3D11Texture = new D3D11Texture(textureData, mipMapping, allowView, usage, this.core.Device);
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            return d3D11Texture;
        }

        public override void SetTexture(int slot, Texture texture)
        {
            lock (this.d3dLock)
                this.SetTextureNoLock(slot, texture);
        }

        public override void SetTextureNoLock(int slot, Texture texture)
        {
            try
            {
                this.core.SetTexture(slot, texture);
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void SetVertexTexture(int slot, Texture texture)
        {
            lock (this.d3dLock)
                this.SetVertexTextureNoLock(slot, texture);
        }

        public override void SetVertexTextureNoLock(int slot, Texture texture)
        {
            try
            {
                this.core.SetVertexTexture(slot, texture);
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void SetVertexTextureNoLock(int slot, Texture[] texture)
        {
            try
            {
                for (int index = 0; index < texture.Length; ++index)
                    this.core.SetVertexTexture(slot + index, texture[index]);
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void SetTextureFromBackBuffer(int slot)
        {
            if (this.core.CurrentRenderTarget == this.core.BackBufferRenderTarget)
                return;
            Texture renderTargetTexture = this.core.BackBufferRenderTarget.RenderTargetTexture;
            this.SetTexture(slot, renderTargetTexture);
        }

        public override void SetTextureView(int slot, TextureView textureView)
        {
            try
            {
                lock (this.d3dLock)
                {
                    DirectX.Direct3D11.ShaderResourceView view = textureView == null ? null : ((D3D11TextureView)textureView).GetResourceView(this.core.Device, this.core.Context);
                    this.core.Context.PS.SetShaderResources((uint) slot, new DirectX.Direct3D11.ShaderResourceView[] { view });
                }
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void SetEffect(Effect effect)
        {
            lock (this.d3dLock)
                this.SetEffectNoLock(effect);
        }

        public override void SetEffectNoLock(Effect effect)
        {
            try
            {
                this.core.SetEffect(effect);
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void SetVertexSource(VertexBuffer[] vertexBuffers)
        {
            lock (this.d3dLock)
                this.SetVertexSourceNoLock(vertexBuffers);
        }

        public override void SetVertexSourceNoLock(VertexBuffer[] vertexBuffers)
        {
            try
            {
                this.core.SetVertexSource(vertexBuffers);
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void SetIndexSource(IndexBuffer indexBuffer)
        {
            lock (this.d3dLock)
                this.SetIndexSourceNoLock(indexBuffer);
        }

        public override void SetIndexSourceNoLock(IndexBuffer indexBuffer)
        {
            try
            {
                this.core.SetIndexSource(indexBuffer);
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void Draw(int startVertex, int vertexCount, PrimitiveTopology topology)
        {
            lock (this.d3dLock)
                this.DrawNoLock(startVertex, vertexCount, topology);
        }

        public override void DrawNoLock(int startVertex, int vertexCount, PrimitiveTopology topology)
        {
            try
            {
                this.core.Draw(startVertex, vertexCount, topology);
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void DrawIndexed(int startIndex, int indexCount, PrimitiveTopology topology)
        {
            lock (this.d3dLock)
                this.DrawIndexedNoLock(startIndex, indexCount, topology);
        }

        public override void DrawIndexedNoLock(int startIndex, int indexCount, PrimitiveTopology topology)
        {
            try
            {
                this.core.DrawIndexed(startIndex, indexCount, topology);
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void DrawInstanced(int startVertex, int vertexCountPerInstance, int startInstance, int instanceCount, PrimitiveTopology topology)
        {
            lock (this.d3dLock)
                this.DrawInstancedNoLock(startVertex, vertexCountPerInstance, startInstance, instanceCount, topology);
        }

        public override void DrawInstancedNoLock(int startVertex, int vertexCountPerInstance, int startInstance, int instanceCount, PrimitiveTopology topology)
        {
            try
            {
                this.core.DrawInstanced(startVertex, vertexCountPerInstance, startInstance, instanceCount, topology);
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void DrawIndexedInstanced(int startIndex, int indexCountPerInstance, int startInstance, int instanceCount, PrimitiveTopology topology)
        {
            lock (this.d3dLock)
                this.DrawIndexedInstancedNoLock(startIndex, indexCountPerInstance, startInstance, instanceCount, topology);
        }

        public override void DrawIndexedInstancedNoLock(int startIndex, int indexCountPerInstance, int startInstance, int instanceCount, PrimitiveTopology topology)
        {
            try
            {
                this.core.DrawIndexedInstanced(startIndex, indexCountPerInstance, startInstance, instanceCount, topology);
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void RenderLockEnter()
        {
            Monitor.Enter(this.d3dLock);
        }

        public override void RenderLockExit()
        {
            Monitor.Exit(this.d3dLock);
        }

        internal override void SetEndFrame(RenderTarget renderTarget)
        {
            try
            {
                ((D3D11RenderTarget)renderTarget).SetEndFrame(this.core.Device, this.core.Context);
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        public override void Flush()
        {
            try
            {
                lock (this.d3dLock)
                    this.core.Context.Flush();
            }
            catch (DirectXException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
            catch (COMException ex)
            {
                this.CheckDeviceRemoved(ex);
            }
        }

        internal void CheckDeviceRemoved(DirectXException e)
        {
            this.CheckDeviceRemoved(e, e.ErrorCode);
        }

        internal void CheckDeviceRemoved(COMException e)
        {
            this.CheckDeviceRemoved(e, e.ErrorCode);
        }

        internal RendererCoreState GetCoreState()
        {
            return this.core.GetCoreState();
        }

        private void CheckDeviceRemoved(Exception e, int errorCode)
        {
            switch ((ErrorCode)errorCode)
            {
                case ErrorCode.GraphicsErrorDeviceHung:
                case ErrorCode.GraphicsErrorDeviceRemoved:
                case ErrorCode.GraphicsErrorDeviceReset:
                case ErrorCode.GraphicsErrorDriverInternalError:
                    if (!this.resetGraphicsDevice)
                        this.NotifyError("The graphics device is in an invalid state.", e);
                    this.resetGraphicsDevice = true;
                    break;
                default:
                    throw e;
            }
        }

        private void HandleDeviceRemoved()
        {
            this.core.Device.Dispose();
            this.profiler.Dispose();
            this.profiler = new FrameProfiler(this.profiler.Enabled);
            bool flag = false;
            int resetAttemps = 0;
            while (!flag && resetAttemps++ < MaxDeviceResetAttempts)
            {
                flag = this.InitializeDevice(this.windowHandle, this.FrameWidth, this.FrameHeight, this.msaaEnabled, false, this.isOfflineVideoRendering);
                if (!flag)
                    Thread.Sleep(IntervalBetweenDeviceResetAttempts);
            }
            if (!flag)
                throw new DirectXException("The device failed to reinitialize", -2005270522);
            this.resourceManager.ResetResources();
            this.offscreenRenderTarget.Dispose();
            this.CreateOffscreenRenderTarget(this.FrameWidth, this.FrameHeight);
            this.core.Reset();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            if (this.core != null && this.core.Context != null)
            {
                this.core.Context.ClearState();
                this.core.Context.Flush();
            }
            if (this.offscreenRenderTarget != null)
            {
                this.offscreenRenderTarget.Dispose();
                this.offscreenRenderTarget = null;
            }
            if (this.profiler != null)
            {
                this.profiler.Dispose();
                this.profiler = null;
            }
            if (this.swapChain != null)
            {
                if (this.swapChainRenderTarget != null)
                {
                    if (this.swapChainRenderTarget.RenderTargetTexture != null)
                        this.swapChainRenderTarget.RenderTargetTexture.Dispose();
                    this.swapChainRenderTarget.Dispose();
                    this.swapChainRenderTarget = null;
                }
                this.swapChain.Dispose();
                this.swapChain = null;
            }
            if (this.core == null)
                return;
            this.core.Dispose();
            D3D11Texture.ReleaseResources(this.core.Device);
            if (this.core.Context != null)
                this.core.Context.Dispose();
            if (this.core.Device == null)
                return;
            this.core.Device.Dispose();
        }
    }
}
