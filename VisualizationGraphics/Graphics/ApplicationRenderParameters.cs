namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class ApplicationRenderParameters : DisposableResource
    {
        public RenderParameterVector2F PixelDimensions { get; private set; }

        public RenderParameterVector2F FrameBufferPixelDimensions { get; private set; }

        internal RenderParameters RenderParameters { get; private set; }

        internal ApplicationRenderParameters()
        {
            this.PixelDimensions = new RenderParameterVector2F("PixelDimensions");
            this.FrameBufferPixelDimensions = new RenderParameterVector2F("FrameBufferPixelDimensions");
            this.RenderParameters = RenderParameters.Create(new IRenderParameter[2]
            {
                this.PixelDimensions,
                this.FrameBufferPixelDimensions
            });
        }

        internal void CopyTo(ApplicationRenderParameters to)
        {
            to.PixelDimensions.Value = this.PixelDimensions.Value;
            to.FrameBufferPixelDimensions.Value = this.FrameBufferPixelDimensions.Value;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing || this.RenderParameters == null)
                return;
            this.RenderParameters.Dispose();
            this.RenderParameters = null;
        }
    }
}
