namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class RasterizerState : GraphicsResource
    {
        protected RasterizerStateDescription State { get; set; }

        internal RasterizerState(RasterizerStateDescription desc)
        {
            this.State = desc.Clone();
        }

        public static RasterizerState Create(RasterizerStateDescription desc)
        {
            return new D3D11RasterizerState(desc);
        }

        public RasterizerStateDescription GetState()
        {
            return this.State.Clone();
        }
    }
}
