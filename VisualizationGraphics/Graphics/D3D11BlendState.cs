namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11BlendState : BlendState
    {
        private DirectX.Direct3D11.BlendState blendState;

        public D3D11BlendState(BlendStateDescription desc)
            : base(desc)
        {
        }

        internal void SetState(DirectX.Direct3D11.D3DDevice device, DirectX.Direct3D11.DeviceContext context)
        {
            if (this.blendState == null)
            {
                this.Register((Renderer)device.Tag);
                DirectX.Direct3D11.BlendDescription description = new DirectX.Direct3D11.BlendDescription();
                description.AlphaToCoverageEnable = this.State.AlphaToCoverageEnable;
                description.RenderTarget[0] = new DirectX.Direct3D11.RenderTargetBlendDescription()
                {
                    BlendEnable = this.State.BlendEnable,
                    SourceBlend = this.GetBlend(this.State.SourceBlend),
                    SourceBlendAlpha = this.GetBlend(this.State.SourceBlendAlpha),
                    DestinationBlend = this.GetBlend(this.State.DestBlend),
                    DestinationBlendAlpha = this.GetBlend(this.State.DestBlendAlpha),
                    BlendOperation = this.GetBlendOp(this.State.BlendOp),
                    BlendOperationAlpha = this.GetBlendOp(this.State.BlendOpAlpha),
                    RenderTargetWriteMask = this.GetColorMask(this.State.WriteMask)
                };
                this.blendState = device.CreateBlendState(description);
            }
            context.OM.BlendState = new DirectX.Direct3D11.OutputMergerBlendState(
                this.blendState,
                new DirectX.Graphics.ColorRgba(this.State.BlendFactor.Rgba()), 
                uint.MaxValue);
        }

        private DirectX.Direct3D11.Blend GetBlend(BlendFactor factor)
        {
            switch (factor)
            {
                case BlendFactor.Zero:
                    return DirectX.Direct3D11.Blend.Zero;
                case BlendFactor.SourceAlpha:
                    return DirectX.Direct3D11.Blend.SourceAlpha;
                case BlendFactor.InvSourceAlpha:
                    return DirectX.Direct3D11.Blend.InverseSourceAlpha;
                case BlendFactor.BlendFactor:
                    return DirectX.Direct3D11.Blend.BlendFactor;
                case BlendFactor.InvBlendFactor:
                    return DirectX.Direct3D11.Blend.InverseBlendFactor;
                default:
                    return DirectX.Direct3D11.Blend.One;
            }
        }

        private DirectX.Direct3D11.BlendOperation GetBlendOp(BlendOperation operation)
        {
            switch (operation)
            {
                case BlendOperation.Subtract:
                    return DirectX.Direct3D11.BlendOperation.Subtract;
                case BlendOperation.Min:
                    return DirectX.Direct3D11.BlendOperation.Min;
                case BlendOperation.Max:
                    return DirectX.Direct3D11.BlendOperation.Max;
                default:
                    return DirectX.Direct3D11.BlendOperation.Add;
            }
        }

        private DirectX.Direct3D11.ColorWriteEnableComponents GetColorMask(RenderTargetWriteMask mask)
        {
            switch (mask)
            {
                case RenderTargetWriteMask.None:
                    return DirectX.Direct3D11.ColorWriteEnableComponents.None;
                case RenderTargetWriteMask.Color:
                    return DirectX.Direct3D11.ColorWriteEnableComponents.Red |
                        DirectX.Direct3D11.ColorWriteEnableComponents.Green |
                        DirectX.Direct3D11.ColorWriteEnableComponents.Blue;
                case RenderTargetWriteMask.Alpha:
                    return DirectX.Direct3D11.ColorWriteEnableComponents.Alpha;
                default:
                    return DirectX.Direct3D11.ColorWriteEnableComponents.All;
            }
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
            if (this.blendState == null)
                return;
            this.blendState.Dispose();
            this.blendState = null;
        }
    }
}
