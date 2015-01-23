namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal class D3D11RasterizerState : RasterizerState
    {
        private DirectX.Direct3D11.RasterizerState rasterizerState;

        public D3D11RasterizerState(RasterizerStateDescription desc)
            : base(desc)
        {
        }

        internal void SetState(DirectX.Direct3D11.D3DDevice device, DirectX.Direct3D11.DeviceContext context)
        {
            if (this.rasterizerState == null)
            {
                this.Register((Renderer)device.Tag);
                DirectX.Direct3D11.CullMode cullMode;
                switch (this.State.CullMode)
                {
                    case CullMode.Front:
                        cullMode = DirectX.Direct3D11.CullMode.Front;
                        break;
                    case CullMode.Back:
                        cullMode = DirectX.Direct3D11.CullMode.Back;
                        break;
                    default:
                        cullMode = DirectX.Direct3D11.CullMode.None;
                        break;
                }
                DirectX.Direct3D11.FillMode fillMode;
                switch (this.State.FillMode)
                {
                    case FillMode.Wireframe:
                        fillMode = DirectX.Direct3D11.FillMode.Wireframe;
                        break;
                    default:
                        fillMode = DirectX.Direct3D11.FillMode.Solid;
                        break;
                }
                DirectX.Direct3D11.RasterizerDescription description =
                    new DirectX.Direct3D11.RasterizerDescription()
                    {
                        AntiAliasedLineEnable = this.State.AntialiasedLineEnable,
                        CullMode = cullMode,
                        FrontCounterclockwise = true,
                        FillMode = fillMode,
                        MultisampleEnable = this.State.MultisampleEnable,
                        ScissorEnable = this.State.ScissorEnable,
                        DepthClipEnable = this.State.DepthClipEnable
                    };
                
                this.rasterizerState = device.CreateRasterizerState(description);
            }
            context.RS.State = this.rasterizerState;
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
            if (this.rasterizerState == null)
                return;
            this.rasterizerState.Dispose();
            this.rasterizerState = null;
        }
    }
}
