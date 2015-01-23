namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class DepthStencilState : GraphicsResource
    {
        protected DepthStencilStateDescription State { get; set; }

        internal DepthStencilState(DepthStencilStateDescription desc)
        {
            this.State = desc.Clone();
        }

        public static DepthStencilState Create(DepthStencilStateDescription desc)
        {
            return (DepthStencilState)new D3D11DepthStencilState(desc);
        }

        public DepthStencilStateDescription GetStateDescription()
        {
            return this.State.Clone();
        }
    }
}
