namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class RasterizerStateDescription
    {
        public FillMode FillMode { get; set; }

        public CullMode CullMode { get; set; }

        public bool MultisampleEnable { get; set; }

        public bool ScissorEnable { get; set; }

        public bool AntialiasedLineEnable { get; set; }

        public bool DepthClipEnable { get; set; }

        public RasterizerStateDescription()
        {
            this.FillMode = FillMode.Solid;
            this.CullMode = CullMode.None;
            this.MultisampleEnable = false;
            this.ScissorEnable = true;
            this.AntialiasedLineEnable = true;
            this.DepthClipEnable = true;
        }

        internal RasterizerStateDescription Clone()
        {
            return (RasterizerStateDescription)this.MemberwiseClone();
        }
    }
}
