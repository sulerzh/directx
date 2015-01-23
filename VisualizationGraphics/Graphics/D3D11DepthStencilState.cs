namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11DepthStencilState : DepthStencilState
    {
        private DirectX.Direct3D11.DepthStencilState depthStencilState;

        public D3D11DepthStencilState(DepthStencilStateDescription desc)
            : base(desc)
        {
        }

        public void SetState(DirectX.Direct3D11.D3DDevice device, DirectX.Direct3D11.DeviceContext context)
        {
            if (this.depthStencilState == null)
            {
                this.Register((Renderer)device.Tag);
                this.depthStencilState = device.CreateDepthStencilState(new DirectX.Direct3D11.DepthStencilDescription()
                {
                    DepthEnable = this.State.DepthEnable,
                    DepthWriteMask = this.State.DepthWriteEnable ?
                    DirectX.Direct3D11.DepthWriteMask.Enabled : DirectX.Direct3D11.DepthWriteMask.None,
                    DepthFunction = D3D11DepthStencilState.GetCompare(this.State.DepthFunction),
                    StencilEnable = this.State.StencilEnable,
                    StencilReadMask = (DirectX.Direct3D11.StencilReadMask)this.State.StencilReadMask,
                    StencilWriteMask = (DirectX.Direct3D11.StencilWriteMask)this.State.StencilWriteMask,
                    FrontFace = new DirectX.Direct3D11.DepthStencilOperationDescription()
                    {
                        StencilFunction = D3D11DepthStencilState.GetCompare(this.State.StencilFrontFace.Function),
                        StencilFailOperation = this.GetStencilOperation(this.State.StencilFrontFace.FailOperation),
                        StencilDepthFailOperation = this.GetStencilOperation(this.State.StencilFrontFace.DepthFailOperation),
                        StencilPassOperation = this.GetStencilOperation(this.State.StencilFrontFace.PassOperation)
                    },
                    BackFace = new DirectX.Direct3D11.DepthStencilOperationDescription()
                    {
                        StencilFunction = D3D11DepthStencilState.GetCompare(this.State.StencilBackFace.Function),
                        StencilFailOperation = this.GetStencilOperation(this.State.StencilBackFace.FailOperation),
                        StencilDepthFailOperation = this.GetStencilOperation(this.State.StencilBackFace.DepthFailOperation),
                        StencilPassOperation = this.GetStencilOperation(this.State.StencilBackFace.PassOperation)
                    }
                });
            }
            context.OM.SetDepthStencilStateAndReferenceValue(
                this.depthStencilState,
                (uint)this.State.StencilReferenceValue);
        }

        internal static DirectX.Direct3D11.ComparisonFunction GetCompare(ComparisonFunction function)
        {
            switch (function)
            {
                case ComparisonFunction.Never:
                    return DirectX.Direct3D11.ComparisonFunction.Never;
                case ComparisonFunction.Less:
                    return DirectX.Direct3D11.ComparisonFunction.Less;
                case ComparisonFunction.Equal:
                    return DirectX.Direct3D11.ComparisonFunction.Equal;
                case ComparisonFunction.LessEqual:
                    return DirectX.Direct3D11.ComparisonFunction.LessEqual;
                case ComparisonFunction.Greater:
                    return DirectX.Direct3D11.ComparisonFunction.Greater;
                case ComparisonFunction.NotEqual:
                    return DirectX.Direct3D11.ComparisonFunction.NotEqual;
                case ComparisonFunction.GreaterEqual:
                    return DirectX.Direct3D11.ComparisonFunction.GreaterEqual;
                default:
                    return DirectX.Direct3D11.ComparisonFunction.Always;
            }
        }

        private DirectX.Direct3D11.StencilOperation GetStencilOperation(StencilOperation operation)
        {
            switch (operation)
            {
                case StencilOperation.Zero:
                    return DirectX.Direct3D11.StencilOperation.Zero;
                case StencilOperation.Replace:
                    return DirectX.Direct3D11.StencilOperation.Replace;
                case StencilOperation.Increment:
                    return DirectX.Direct3D11.StencilOperation.Increment;
                case StencilOperation.Decrement:
                    return DirectX.Direct3D11.StencilOperation.Decrement;
                case StencilOperation.IncrementSaturate:
                    return DirectX.Direct3D11.StencilOperation.IncrementSat;
                case StencilOperation.DecrementSaturate:
                    return DirectX.Direct3D11.StencilOperation.DecrementSat;
                case StencilOperation.Invert:
                    return DirectX.Direct3D11.StencilOperation.Replace;
                default:
                    return DirectX.Direct3D11.StencilOperation.Keep;
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
            if (this.depthStencilState == null)
                return;
            this.depthStencilState.Dispose();
            this.depthStencilState = null;
        }
    }
}
