namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class FrameRenderParameters : DisposableResource
    {
        public RenderParameterMatrix4x4F[] DepthOffsetViewProj { get; private set; }

        public RenderParameterMatrix4x4F ViewProj { get; private set; }

        public RenderParameterMatrix4x4F View { get; private set; }

        public RenderParameterMatrix4x4F Projection { get; private set; }

        public RenderParameterVector4F CameraPos { get; private set; }

        public RenderParameterVector4F DirectionalLightPosition { get; private set; }

        public RenderParameterVector4F PointLightPositionAndFactor { get; private set; }

        public RenderParameterVector4F LightFactors { get; private set; }

        public RenderParameterFloat ElapsedTime { get; private set; }

        public RenderParameterFloat VisualTime { get; private set; }

        public RenderParameterFloat VisualTimeScale { get; private set; }

        public RenderParameterFloat FlatteningFactor { get; private set; }

        internal RenderParameters RenderParameters { get; private set; }

        internal FrameRenderParameters()
        {
            this.DepthOffsetViewProj = new RenderParameterMatrix4x4F[5];
            for (int index = 0; index < this.DepthOffsetViewProj.Length; ++index)
                this.DepthOffsetViewProj[index] = new RenderParameterMatrix4x4F("DepthOffsetViewProj");
            this.ViewProj = new RenderParameterMatrix4x4F("ViewProj");
            this.View = new RenderParameterMatrix4x4F("View");
            this.Projection = new RenderParameterMatrix4x4F("Projection");
            this.CameraPos = new RenderParameterVector4F("CameraPos");
            this.DirectionalLightPosition = new RenderParameterVector4F("LightDir");
            this.PointLightPositionAndFactor = new RenderParameterVector4F("PointLightAndFactor");
            this.LightFactors = new RenderParameterVector4F("LightFactors");
            this.ElapsedTime = new RenderParameterFloat("ElapsedTime");
            this.VisualTime = new RenderParameterFloat("GlobalVisualTime");
            this.VisualTimeScale = new RenderParameterFloat("GlobalVisualTimeScale");
            this.FlatteningFactor = new RenderParameterFloat("FlatteningFactor");
            this.RenderParameters = RenderParameters.Create(new IRenderParameter[16]
            {
                this.DepthOffsetViewProj[0],
                this.DepthOffsetViewProj[1],
                this.DepthOffsetViewProj[2],
                this.DepthOffsetViewProj[3],
                this.DepthOffsetViewProj[4],
                this.ViewProj,
                this.View,
                this.Projection,
                this.CameraPos,
                this.DirectionalLightPosition,
                this.PointLightPositionAndFactor,
                this.LightFactors,
                this.ElapsedTime,
                this.VisualTime,
                this.VisualTimeScale,
                this.FlatteningFactor
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing || this.RenderParameters == null)
                return;
            this.RenderParameters.Dispose();
            this.RenderParameters = (RenderParameters)null;
        }

        internal void CopyTo(FrameRenderParameters to)
        {
            to.DepthOffsetViewProj[0].Value = this.DepthOffsetViewProj[0].Value;
            to.DepthOffsetViewProj[1].Value = this.DepthOffsetViewProj[1].Value;
            to.DepthOffsetViewProj[2].Value = this.DepthOffsetViewProj[2].Value;
            to.DepthOffsetViewProj[3].Value = this.DepthOffsetViewProj[3].Value;
            to.DepthOffsetViewProj[4].Value = this.DepthOffsetViewProj[4].Value;
            to.ViewProj.Value = this.ViewProj.Value;
            to.View.Value = this.View.Value;
            to.Projection.Value = this.Projection.Value;
            to.CameraPos.Value = this.CameraPos.Value;
            to.DirectionalLightPosition.Value = this.DirectionalLightPosition.Value;
            to.PointLightPositionAndFactor.Value = this.PointLightPositionAndFactor.Value;
            to.LightFactors.Value = this.LightFactors.Value;
            to.ElapsedTime.Value = this.ElapsedTime.Value;
            to.VisualTime.Value = this.VisualTime.Value;
            to.VisualTimeScale.Value = this.VisualTimeScale.Value;
            to.FlatteningFactor.Value = this.FlatteningFactor.Value;
        }
    }
}
