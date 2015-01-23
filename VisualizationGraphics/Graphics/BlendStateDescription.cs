namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class BlendStateDescription
    {
        public bool AlphaToCoverageEnable { get; set; }

        public bool BlendEnable { get; set; }

        public BlendFactor SourceBlend { get; set; }

        public BlendFactor DestBlend { get; set; }

        public BlendOperation BlendOp { get; set; }

        public BlendFactor SourceBlendAlpha { get; set; }

        public BlendFactor DestBlendAlpha { get; set; }

        public BlendOperation BlendOpAlpha { get; set; }

        public RenderTargetWriteMask WriteMask { get; set; }

        public Color4F BlendFactor { get; set; }

        public BlendStateDescription()
        {
            this.AlphaToCoverageEnable = false;
            this.BlendEnable = true;
            this.SourceBlend = Graphics.BlendFactor.SourceAlpha;
            this.DestBlend = Graphics.BlendFactor.InvSourceAlpha;
            this.BlendOp = BlendOperation.Add;
            this.SourceBlendAlpha = Graphics.BlendFactor.SourceAlpha;
            this.DestBlendAlpha = Graphics.BlendFactor.InvSourceAlpha;
            this.BlendOpAlpha = BlendOperation.Add;
            this.WriteMask = RenderTargetWriteMask.All;
            this.BlendFactor = new Color4F(0.0f, 0.0f, 0.0f, 0.0f);
        }

        internal BlendStateDescription Clone()
        {
            return (BlendStateDescription)this.MemberwiseClone();
        }
    }
}
