namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class BlendState : GraphicsResource
    {
        protected BlendStateDescription State { get; set; }

        internal BlendState(BlendStateDescription desc)
        {
            this.State = desc.Clone();
        }

        public static BlendState Create(BlendStateDescription desc)
        {
            return new D3D11BlendState(desc);
        }

        public BlendStateDescription GetStateDescription()
        {
            return this.State.Clone();
        }
    }
}
